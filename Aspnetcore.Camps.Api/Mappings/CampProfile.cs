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
                }))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); // ignore null fields of source when mapping

            CreateMap<Speaker, SpeakerViewModel>()
                .ForMember(s => s.Url, opt => opt.ResolveUsing<SpeakerUrlResolver>())
                .ReverseMap()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Talk, TalkViewModel>()
                .ForMember(s => s.Url, opt => opt.ResolveUsing<TalkUrlResolver>())
                .ReverseMap()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}