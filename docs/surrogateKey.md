# Using a Surrogate Key

Sometimes we don't want to expose primary key. Of course, sometimes we can expose primary key compared to more important information like identity number or SSN. I feel it may be good to use a surrogate key if the API is for public users. For admin management system, just expose the primary key.

CampsController: change getById to getByMoniker

```csharp
[HttpGet("{moniker}", Name = "CampGet")]
public IActionResult Get(string moniker, bool includeSpeakers = false)
{
    try
    {
        Camp camp = includeSpeakers
            ? _repo.GetCampByMonikerWithSpeakers(moniker)
            : _repo.GetCampByMoniker(moniker);

        if (camp == null) return NotFound($"Camp {moniker} was not found");
        // map Camp entity to CampViewModel
        return Ok(_mapper.Map<CampViewModel>(camp));
    }
    catch
    {
        // ignored
    }

    return BadRequest();
}
```

Since we don't have getById, we have to change how to build url in CampResolver:

```csharp
public class CampUrlResolver : IValueResolver<Camp, CampViewModel, string>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CampUrlResolver(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string Resolve(Camp source, CampViewModel destination, string destMember, ResolutionContext context)
    {
        var url = (IUrlHelper) _httpContextAccessor.HttpContext.Items[BaseController.Urlhelper];
        // replace return url.Link("CampGet", new {id = source.Id});
        return url.Link("CampGet", new {moniker = source.Moniker});
    }
}
```

Now the result is like:

```json
[
    {
        "url": "http://localhost:5000/api/Camps/ATL2019",
        ...
    },
    {
        "url": "http://localhost:5000/api/Camps/ATL2016",
        ...
    }
]
```