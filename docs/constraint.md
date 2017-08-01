# What Are the Constraints of REST

- Client-Server
- Stateless Server
- Cache
- Uniform Interface
- Layered System
- Code-On-Demand

## What Are ETags

- Header used as a unique key for resource
  ```
  HTTP/1.1 200 OK
  Content-Type: text/xml; charset=utf-8
  Date: Thu, 23 May 2013 21:52:14 GMT
  ETag: W/"4893023942098"
  Content-Length: 639
  ```
- Requests should test using `If-None-Match` (**HTTP/1.1 304 N**)
  ```
  GET /api/nutrition/foods/2 HTTP/1.1
  Accept: application/json, text/xml
  Host: localhost:8863
  If-None-Match: "4893023942098"
  ```
- For PUT/PATCH it is different (**HTTP/1.1 412 Precondition Failed**, **_Etag完全相同时才允许更新数据_**)
  ```
  PATCH /api/user/diaries/2013-5-12 HTTP/1.1
  Accept: application/json, text/xml
  Host: localhost:8863
  If-Match: "4893023942098"
  ```

---

### If-None-Match Demo

```csharp
public class TalksController : BaseController
{
    private readonly ILogger<TalksController> _logger;
    private readonly IMapper _mapper;
    private readonly ICampRepository _repo;
    private readonly IMemoryCache _cache;

    public TalksController(ICampRepository repo, ILogger<TalksController> logger, IMapper mapper, IMemoryCache cache)
    {
        _repo = repo;
        _logger = logger;
        _mapper = mapper;
        _cache = cache;
    }

    [HttpGet("{id}", Name = "GetTalk")]
    public IActionResult Get(string moniker, int speakerId, int id)
    {
        if (Request.Headers.ContainsKey("If-None-Match"))
        {
            var oldEtag = Request.Headers["If-None-Match"].First();
            if (_cache.Get($"Talk-{id}-{oldEtag}") != null)
            {
                // return StatusCode(304);
                return StatusCode((int) HttpStatusCode.NotModified);
            }
        }

        var talk = _repo.GetTalk(id);

        if (talk.Speaker.Id != speakerId || talk.Speaker.Camp.Moniker != moniker)
            return BadRequest("Invalid talk for the speaker selected");

        // now not working for mysql
        var etag = Convert.ToBase64String(talk.RowVersion);
        Response.Headers.Add("ETag", etag); // response header should contain ETag when you request this api
        _cache.Set($"Talk-{talk.Id}-{etag}", talk);

        return Ok(_mapper.Map<TalkViewModel>(talk));
    }
}
```

Startup.cs add cache in `ConfigureServices` method

```csharp
// cache
services.AddMemoryCache();  //store cache to current machine's memory
```

### Etag用于数据更新前检查数据是否已经变化(handle concurrency)

```diff
+ using Microsoft.Extensions.Caching.Memory;

namespace Aspnetcore.Camps.Api.Controllers
{
    [Route("api/camps/{moniker}/speakers/{speakerId}/talks")]
    [ValidateModel]
    public class TalksController : BaseController
    {
        private readonly ILogger<TalksController> _logger;
        private readonly IMapper _mapper;
        private readonly ICampRepository _repo;
+        private readonly IMemoryCache _cache;

        public TalksController(ICampRepository repo, ILogger<TalksController> logger, IMapper mapper,
            IMemoryCache cache)
        {
            _repo = repo;
            _logger = logger;
            _mapper = mapper;
+            _cache = cache;
        }

        [HttpGet("{id}", Name = "GetTalk")]
        public IActionResult Get(string moniker, int speakerId, int id)
        {
+            if (Request.Headers.ContainsKey("If-None-Match"))
+            {
+                var oldEtag = Request.Headers["If-None-Match"].First();
+                if (_cache.Get($"Talk-{id}-{oldEtag}") != null)
+                {
+                    // return StatusCode(304);
+                    return StatusCode((int) HttpStatusCode.NotModified);
+                }
+            }
+
            var talk = _repo.GetTalk(id);

            if (talk.Speaker.Id != speakerId || talk.Speaker.Camp.Moniker != moniker)
                return BadRequest("Invalid talk for the speaker selected");

+            AddETag(talk);

            return Ok(_mapper.Map<TalkViewModel>(talk));
        }

+        private void AddETag(Talk talk)
+        {
+            // now not working for mysql
+            var etag = Convert.ToBase64String(talk.RowVersion);
+            Response.Headers.Add("ETag", etag); // response header should contain ETag when you request this api
+            _cache.Set($"Talk-{talk.Id}-{etag}", talk);
+        }

        [HttpPost()]
        public async Task<IActionResult> Post(string moniker, int speakerId, [FromBody] TalkViewModel model)
        {
            try
            {
                var speaker = _repo.GetSpeaker(speakerId);
                if (speaker != null)
                {
                    var talk = _mapper.Map<Talk>(model);

                    talk.Speaker = speaker;
                    _repo.Add(talk);

                    if (await _repo.SaveAllAsync())
                    {
+                        AddETag(talk);

                        return Created(
                            Url.Link("GetTalk", new {moniker = moniker, speakerId = speakerId, id = talk.Id}),
                            _mapper.Map<TalkViewModel>(talk));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save new talk: {ex}");
            }

            return BadRequest("Failed to save new talk");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string moniker, int speakerId, int id, [FromBody] TalkViewModel model)
        {
            try
            {
                var talk = _repo.GetTalk(id);
                if (talk == null) return NotFound();

+                if (Request.Headers.ContainsKey("If-Match"))
+                {
+                    var etag = Request.Headers["If-Match"].First();
+                    if (etag != Convert.ToBase64String(talk.RowVersion))
+                    {
+                        return StatusCode((int) HttpStatusCode.PreconditionFailed);  //412
+                    }
+                }


                _mapper.Map(model, talk);

                if (await _repo.SaveAllAsync())
                {
+                    AddETag(talk);

                    return Ok(_mapper.Map<TalkViewModel>(talk));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to update talk: {ex}");
            }

            return BadRequest("Failed to update talk");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string moniker, int speakerId, int id)
        {
            try
            {
                var talk = _repo.GetTalk(id);
                if (talk == null) return NotFound();

+                if (Request.Headers.ContainsKey("If-Match"))
+                {
+                    var etag = Request.Headers["If-Match"].First();
+                    if (etag != Convert.ToBase64String(talk.RowVersion))
+                    {
+                        return StatusCode((int)HttpStatusCode.PreconditionFailed);
+                    }
+                }

                _repo.Delete(talk);

                if (await _repo.SaveAllAsync())
                {
                    return Ok();
                }
            }

            return BadRequest("Failed to delete talk");
        }
    }
}
```

### Links

I'm not a big fan of returning Api links since you may return too much data. But sometimes you may need them.

```csharp
namespace Aspnetcore.Camps.Api.ViewModels
{
    public class LinkModel
    {
        public string Href { get; set; }
        public string Rel { get; set; }
        public string Verb { get; set; } = "GET";
    }
}
```

TalkViewModel

```diff
namespace Aspnetcore.Camps.Api.ViewModels
{
    public class TalkViewModel
    {
        public string Url { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Abstract { get; set; }

        [Required]
        public string Category { get; set; }

        public string Level { get; set; }
        public string Prerequisites { get; set; }
        public DateTime StartingTime { get; set; } = DateTime.Now;
        public string Room { get; set; }

        // 如果要返回一些api patch/put等接口地址
+        public ICollection<LinkModel> Links { get; set; }
    }
}
```

CampsProfile.cs

```csharp
CreateMap<Talk, TalkViewModel>()
    .ForMember(s => s.Url, opt => opt.ResolveUsing<TalkUrlResolver>())
    .ForMember(s => s.Links, opt => opt.ResolveUsing<TalkLinksResolver>())
```

```csharp
using System.Collections.Generic;
using Aspnetcore.Camps.Api.Controllers;
using Aspnetcore.Camps.Api.ViewModels;
using Aspnetcore.Camps.Model.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Aspnetcore.Camps.Api.Mappings
{
    public class TalkLinksResolver : IValueResolver<Talk, TalkViewModel, ICollection<LinkModel>>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TalkLinksResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ICollection<LinkModel> Resolve(Talk source, TalkViewModel destination, ICollection<LinkModel> destMember,
            ResolutionContext context)
        {
            var url = (IUrlHelper) _httpContextAccessor.HttpContext.Items[BaseController.Urlhelper];

            return new List<LinkModel>()
            {
                new LinkModel()
                {
                    Rel = "Self",
                    Href = url.Link("GetTalk",
                        new {moniker = source.Speaker.Camp.Moniker, speakerId = source.Speaker.Id, id = source.Id})
                },
                new LinkModel()
                {
                    Rel = "Update",
                    Href = url.Link("UpdateTalk",
                        new {moniker = source.Speaker.Camp.Moniker, speakerId = source.Speaker.Id, id = source.Id}),
                    Verb = "PUT"
                },
                new LinkModel()
                {
                    Rel = "Speaker",
                    Href = url.Link("GetSpeaker", new {moniker = source.Speaker.Camp.Moniker, id = source.Speaker.Id})
                },
            };
        }
    }
}
```

Request <https://localhost:44388/api/camps/ATL2016/speakers/1/talks/>

Result

```json
[
    {
        "url": "https://localhost:44388/api/camps/ATL2016/speakers/1/talks/1",
        "title": "How to do ASP.NET Core",
        "abstract": "How to do ASP.NET Core",
        "category": "Web Development",
        "level": "100",
        "prerequisites": "C# Experience",
        "startingTime": "2017-07-30T14:30:00",
        "room": "245",
        "links": [
            {
                "href": "https://localhost:44388/api/camps/ATL2016/speakers/1/talks/1",
                "rel": "Self",
                "verb": "GET"
            },
            {
                "href": "https://localhost:44388/api/camps/ATL2016/speakers/1/talks/1",
                "rel": "Update",
                "verb": "PUT"
            },
            {
                "href": "https://localhost:44388/api/camps/ATL2016/speakers/1",
                "rel": "Speaker",
                "verb": "GET"
            }
        ]
    },
    {
        "url": "https://localhost:44388/api/camps/ATL2016/speakers/1/talks/2",
        "title": "How to do Bootstrap 4",
        "abstract": "How to do Bootstrap 4",
        "category": "Web Development",
        "level": "100",
        "prerequisites": "CSS Experience",
        "startingTime": "2017-07-30T13:00:00",
        "room": "246",
        "links": [
            {
                "href": "https://localhost:44388/api/camps/ATL2016/speakers/1/talks/2",
                "rel": "Self",
                "verb": "GET"
            },
            {
                "href": "https://localhost:44388/api/camps/ATL2016/speakers/1/talks/2",
                "rel": "Update",
                "verb": "PUT"
            },
            {
                "href": "https://localhost:44388/api/camps/ATL2016/speakers/1",
                "rel": "Speaker",
                "verb": "GET"
            }
        ]
    }
]
```