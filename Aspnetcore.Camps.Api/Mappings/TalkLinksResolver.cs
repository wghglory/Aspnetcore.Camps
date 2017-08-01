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