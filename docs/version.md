# Versioning Your API

## What is Versioning API

Once you publish an API, it's set in stone

- Users/Customers rely on the API not changing
- But requirements will change
- Need a way to evolve the API without breaking existing clients
- API Versioning isn't Product Versioning

## The Problem with API Versioning

- In typical projects
  - Versioning is accomplished with different versions of the package

- API Versioning is harder
  - Your API needs to support both new and old users
  - Side-by-side deployment isn't feasible most of the time
  - Need to support both versions in same code base

## API Versioning Schemes

### Versioning in the URI

- URI Path <https://foo.org/api/v2/Customers>
- Query String <https://foo.org/api/Customers?v=2.0>: I feel this is better than URI path since no need to change URI schema

### Versioning with Headers

GET /api/camps HTTP/1.1
Host: localhost:44388
Content-Type: application/json
X-Version: 2.0

### Versioning with Accept Header

GET /api/camps HTTP/1.1
Host: localhost:44388
Content-Type: application/json
Accept: application/json;version=2.0

### Versioning with Content Type (I don't like, complicated)

GET /api/camps HTTP/1.1
Host: localhost:44388
Content-Type: application/vnd.yourapp.camp.v1+json
Accept: application/vnd.yourapp.camp.v1+jso

---

```bash
dotnet add package Microsoft.AspNetCore.Mvc.Versioning   # 1.2.1
dotnet restore
```

## Adding ApiVersioning

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // api verisioning
    services.AddApiVersioning(cfg =>
    {
        cfg.DefaultApiVersion = new ApiVersion(1, 1);
        cfg.AssumeDefaultVersionWhenUnspecified = true;
        cfg.ReportApiVersions = true;

        // support both
        cfg.ApiVersionReader = ApiVersionReader.Combine(new QueryStringApiVersionReader("ver"),
            new HeaderApiVersionReader(
                "Camp-Version")); //QueryStringOrHeaderApiVersionReader is removed(https://github.com/Microsoft/aspnet-api-versioning/releases)

        // way 1: add versioning on controller and actions

        //---

        // way 2: Using Versioning Conventions
        cfg.Conventions.Controller<TalksController>()
            .HasApiVersion(new ApiVersion(1, 0))
            .HasApiVersion(new ApiVersion(1, 1))
            .HasApiVersion(new ApiVersion(2, 0))
            .Action(m => m.Post(default(string), default(int), default(TalkViewModel)))
            .MapToApiVersion(new ApiVersion(2, 0));
    });
}
```

## Using Versioning Attributes

SpeakersController.cs

```csharp
namespace Aspnetcore.Camps.Api.Controllers
{
    [Route("api/camps/{moniker}/speakers")]
    [ValidateModel]
    [Authorize]
    [ApiVersion("1.0")]
    [ApiVersion("1.1")]
    public class SpeakersController : BaseController
    {
        //...
        [HttpGet]
        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        public IActionResult Get(string moniker, bool includeTalks = false)
        {
            var speakers = includeTalks
                ? _repository.GetSpeakersByMonikerWithTalks(moniker)
                : _repository.GetSpeakersByMoniker(moniker);

            return Ok(_mapper.Map<IEnumerable<SpeakerViewModel>>(speakers));
        }

        [HttpGet]
        [MapToApiVersion("1.1")]
        public virtual IActionResult GetWithCount(string moniker, bool includeTalks = false)
        {
            var speakers = includeTalks
                ? _repository.GetSpeakersByMonikerWithTalks(moniker)
                : _repository.GetSpeakersByMoniker(moniker);

            return Ok(new {count = speakers.Count(), results = _mapper.Map<IEnumerable<SpeakerViewModel>>(speakers)});
        }
        //...
    }
}
```

Requests by URI:

- <https://localhost:44388/api/camps/ATL2016/speakers> default 1.1, GetWithCount
- <https://localhost:44388/api/camps/ATL2016/speakers?ver=1.0> 1.0, Get

Requests from Headers:

- <https://localhost:44388/api/camps/ATL2016/speakers> Header:Camp-Version 1.1, GetWithCount
- <https://localhost:44388/api/camps/ATL2016/speakers> Header:Camp-Version 1.0, Get

## Adding a Versioned Controller

```csharp
namespace Aspnetcore.Camps.Api.Controllers
{
    [Route("api/camps/{moniker}/speakers")]
    [ApiVersion("2.0")]
    public class Speakers2Controller : SpeakersController
    {
        public Speakers2Controller(ICampRepository repository,
            ILogger<SpeakersController> logger,
            IMapper mapper,
            UserManager<CampUser> userMgr)
            : base(repository, logger, mapper, userMgr)
        {
        }

        public override IActionResult GetWithCount(string moniker, bool includeTalks = false)
        {
            var speakers = includeTalks
                ? Repository.GetSpeakersByMonikerWithTalks(moniker)
                : Repository.GetSpeakersByMoniker(moniker);

            return Ok(new
            {
                currentTime = DateTime.UtcNow,
                count = speakers.Count(),
                results = Mapper.Map<IEnumerable<Speaker2ViewModel>>(speakers)
            });
        }
    }
}
```

## Versioning the Payload

Speaker2ViewModel.cs

```csharp
namespace Aspnetcore.Camps.Api.ViewModels
{
    public class Speaker2Model : SpeakerViewModel
    {
        public string BadgeName { get; set; }
    }
}
```

CampMappingProfile.cs

```csharp
CreateMap<Speaker, Speaker2ViewModel>()
    .IncludeBase<Speaker, SpeakerViewModel>()
    .ForMember(s => s.BadgeName, opt => opt.ResolveUsing(s => $"{s.Name} (@{s.TwitterName})"));
```

Request with Header:

<https://localhost:44388/api/camps/ATL2016/speakers>
Header: Camp-Version
Value: 2.0

Request with URI:

<https://localhost:44388/api/camps/ATL2016/speakers?ver=2.0>

Both results will target on Speaker2Controller.

## Using Versioning Conventions

Startup.cs

```csharp
// way 2: Using Versioning Conventions
cfg.Conventions.Controller<TalksController>()
    .HasApiVersion(new ApiVersion(1, 0))
    .HasApiVersion(new ApiVersion(1, 1))
    .HasApiVersion(new ApiVersion(2, 0))
    .Action(m => m.Post(default(string), default(int), default(TalkViewModel)))
    .MapToApiVersion(new ApiVersion(2, 0));
```

Now the post request matching that parameters (string,int,TalkViewModel) is version 2.0