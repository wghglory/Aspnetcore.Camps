# ViewModel and AutoMapper

## Why ViewModel

For larger application, never expose your entity directly in your api.

1. Some properties are not necessary to return
1. Some properties may be objects, and it's better to flatten them
1. Not safe to expose Id or other important information

So that's why we need a viewModel! To save more time, use `AutoMapper`

## AutoMapper

1. Install `AutoMapper.Extensions.Microsoft.DependencyInjection` from Nuget.

1. In Startup.cs `ConfigureServices(IServiceCollection services)` method add: `services.AddAutoMapper();`

1. We need to tell AutoMapper how to do the mapping. To do this, AutoMapper will look for any class that inherits from `AutoMapper.Profile`

    ```csharp
    using Aspnetcore.Camps.Api.ViewModels;
    using Aspnetcore.Camps.Model.Entities;
    using AutoMapper;

    namespace Aspnetcore.Camps.Api.Mappings
    {
        public class CampProfile : Profile
        {
            public CampProfile()
            {
                CreateMap<Camp, CampViewModel>();
            }
        }
    }
    ```

1. In Controller

    ```csharp
    private readonly IMapper _mapper;

    public CampsController(ICampRepository repo, ILogger<CampsController> logger, IMapper mapper)
    {
        _repo = repo;
        _logger = logger;
        _mapper = mapper;
    }

    [HttpGet("")]
    public IActionResult Get()
    {
        var camps = _repo.GetAllCamps();
        // map Camp entity to CampViewModel
        return Ok(_mapper.Map<IEnumerable<CampViewModel>>(camps));
    }
    ```

### Convention-based Mapping

Let's take a look at `CampViewModel.cs`

```csharp
using System;
using System.ComponentModel.DataAnnotations;

namespace Aspnetcore.Camps.Api.ViewModels
{
    public class CampViewModel
    {
        public string Url { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(20)]
        public string Moniker { get; set; }

        [Required]
        [MinLength(5)]
        [MaxLength(100)]
        public string Name { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [Required]
        [MinLength(25)]
        [MaxLength(4096)]
        public string Description { get; set; }

        // below is convention-based flattened location.
        // Camp has navigation property "Location", and if we add `Location` before the properties in Location entity
        // Automapper knows this rule, and it will do the mappings automatically
        public string LocationAddress1 { get; set; }
        public string LocationAddress2 { get; set; }
        public string LocationAddress3 { get; set; }
        public string LocationCityTown { get; set; }
        public string LocationStateProvince { get; set; }
        public string LocationPostalCode { get; set; }
        public string LocationCountry { get; set; }
    }
}
```

Here is `Camp` entity:

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aspnetcore.Camps.Model.Entities
{
    public class Camp
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Moniker cannot be empty")]
        public string Moniker { get; set; }

        public string Name { get; set; }
        public DateTime EventDate { get; set; } = DateTime.MinValue;
        public int Length { get; set; }
        public string Description { get; set; }
        public virtual Location Location { get; set; }

        public ICollection<Speaker> Speakers { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [ConcurrencyCheck]
        public byte[] RowVersion { get; set; }
    }
}
```

Some properties like `Name, Moniker`, they are exactly the same in ViewModel and Entity. It's very easy for AutoMapper to do mappings for this kind of property.

In Camp entity, `Location` property is a complex one. In CampViewModel, we want to flatten it. If we only put the same property name as `Location` properties. e.g. Both have `Address1`. AutoMapper cannot do the mapping. But there is a convention. If **`ViewModel property = Complex Object name + its property`**, AutoMapper will automatically map properties. e.g. `LocationAddress1 = Location + Address1`

### Customized Mapping

Camp entity has these 2 properties:

```csharp
public DateTime EventDate { get; set; } = DateTime.MinValue;  // start date
public int Length { get; set; }   // how many days the event last 
```

CampViewModel has these 2:

```csharp
public DateTime StartDate { get; set; }
public DateTime EndDate { get; set; }
```

We want to map `EventDate` to `StartDate` and calculate `EndDate` based on `Length`. So let's go back to `CampProfile`

```csharp
using Aspnetcore.Camps.Api.ViewModels;
using Aspnetcore.Camps.Model.Entities;
using AutoMapper;

namespace Aspnetcore.Camps.Api.Mappings
{
    public class CampProfile : Profile
    {
        public CampProfile()
        {
            CreateMap<Camp, CampViewModel>()
                .ForMember(vm => vm.StartDate, opt => opt.MapFrom(entity => entity.EventDate))
                .ForMember(vm => vm.EndDate,
                    opt => opt.ResolveUsing(entity => entity.EventDate.AddDays(entity.Length - 1)));
        }
    }
}
```

So `ViewModel's StartDate` will map from `entity's EventDate`. `ViewModel's EndDate` should equal to `entity's EventDate + Length - 1`.

> If `Length = 1`, this means the start/end day is same day.

Now the get request result should be like below:

```json
{
    "url": null,
    "moniker": "ATL2016",
    "name": "Your First Code Camp",
    "startDate": "2017-09-12T00:00:00",
    "endDate": "2017-09-12T00:00:00",
    "description": "This is the first code camp",
    "locationAddress1": "123 Main Street",
    "locationAddress2": null,
    "locationAddress3": null,
    "locationCityTown": "Atlanta",
    "locationStateProvince": "GA",
    "locationPostalCode": "30303",
    "locationCountry": "USA"
}
```