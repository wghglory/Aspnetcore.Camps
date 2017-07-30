using Aspnetcore.Camps.Api.Controllers;
using Aspnetcore.Camps.Api.ViewModels;
using Aspnetcore.Camps.Model.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Aspnetcore.Camps.Api.Mappings
{
    public class TalkUrlResolver : IValueResolver<Talk, TalkViewModel, string>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TalkUrlResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Resolve(Talk source, TalkViewModel destination, string destMember, ResolutionContext context)
        {
            var helper = (IUrlHelper) _httpContextAccessor.HttpContext.Items[BaseController.Urlhelper];
            return helper.Link("GetTalk",
                new {moniker = source.Speaker.Camp.Moniker, speakerId = source.Speaker.Id, id = source.Id});
        }
    }
}