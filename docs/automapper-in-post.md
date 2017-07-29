# Reverse AutoMapping

In get request, We have one-way mapping from entity to viewModel. But in Post request, parameter accepts viewModel, so we need to map viewModel to entity, and then save it to db.

CampProfile.cs, call `ReverseMap()` and after it write reverse mappings.

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
            CreateMap<Camp, CampViewModel>() // Camp --> CampViewModel
                .ForMember(vm => vm.StartDate, opt => opt.MapFrom(entity => entity.EventDate))
                .ForMember(vm => vm.EndDate,
                    opt => opt.ResolveUsing(entity => entity.EventDate.AddDays(entity.Length - 1)))
//                .ForMember(vm => vm.Url, opt => opt.MapFrom(entity => $"/api/camps/{entity.Moniker}"))  //not good for refactoring
                .ForMember(c => c.Url, opt => opt.ResolveUsing<CampUrlResolver>())
                .ReverseMap() // CampViewModel --> Camp
                .ForMember(entity => entity.EventDate, opt => opt.MapFrom(vm => vm.StartDate))
                .ForMember(entity => entity.Length, opt => opt.ResolveUsing(vm => (vm.EndDate - vm.StartDate).Days + 1))
                .ForMember(entity => entity.Location, opt => opt.ResolveUsing(vm => new Location
                {
                    Address1 = vm.LocationAddress1,
                    Address2 = vm.LocationAddress2,
                    Address3 = vm.LocationAddress3,
                    CityTown = vm.LocationCityTown,
                    StateProvince = vm.LocationStateProvince,
                    PostalCode = vm.LocationPostalCode,
                    Country = vm.LocationCountry
                }));
        }
    }
}
```

CampsController:

```csharp
[HttpPost]
public async Task<IActionResult> Post([FromBody] CampViewModel model)
{
    try
    {
        _logger.LogInformation("Creating a new Code Camp");

        // reverse map, viewModel --> entity
        Camp camp = _mapper.Map<Camp>(model);

        _repo.Add(camp);
        if (await _repo.SaveAllAsync())
        {
            // after saving, we return the model just created
            var newUri = Url.Link("CampGet", new {Moniker = model.Moniker});
            return Created(newUri, _mapper.Map<CampViewModel>(camp));
        }
        else
        {
            _logger.LogWarning("Could not save Camp to the database");
        }
    }
    catch (Exception ex)
    {
        _logger.LogError($"Threw exception while saving Camp: {ex}");
    }

    return BadRequest();
}
```

Post request:

```json
{
    "name":"yuhan",
    "startDate": "2017-7-30",
    "enddate": "2017-8-10",
    "moniker": "ATL2017",
    "locationAddress1": "shanghai",
    "locationStateProvince": "Shanghai",
    "locationCountry": "China"
}
```

Post Response:

```json
{
    "url": "http://localhost:5000/api/Camps/ATL2017",
    "moniker": "ATL2017",
    "name": "yuhan",
    "startDate": "2017-07-30T00:00:00",
    "endDate": "2017-08-10T00:00:00",
    "description": null,
    "locationAddress1": "shanghai",
    "locationAddress2": null,
    "locationAddress3": null,
    "locationCityTown": null,
    "locationStateProvince": "Shanghai",
    "locationPostalCode": null,
    "locationCountry": "China"
}
```