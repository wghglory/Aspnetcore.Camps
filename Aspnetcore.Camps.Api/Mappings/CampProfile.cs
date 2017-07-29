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
                    opt => opt.ResolveUsing(entity => entity.EventDate.AddDays(entity.Length - 1)))
//                .ForMember(vm => vm.Url, opt => opt.MapFrom(entity => $"/api/camps/{entity.Moniker}"))  //not good for refactoring
                .ForMember(c => c.Url, opt => opt.ResolveUsing<CampUrlResolver>());
        }
    }
}