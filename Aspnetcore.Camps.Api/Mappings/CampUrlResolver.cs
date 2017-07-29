using Aspnetcore.Camps.Api.Controllers;
using Aspnetcore.Camps.Api.ViewModels;
using Aspnetcore.Camps.Model.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Aspnetcore.Camps.Api.Mappings
{
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
//            return url.Link("CampGet", new {id = source.Id});
            return url.Link("CampGet", new {moniker = source.Moniker});
        }
    }
}