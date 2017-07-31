using System;
using System.Collections.Generic;
using System.Linq;
using Aspnetcore.Camps.Api.ViewModels;
using Aspnetcore.Camps.Model.Entities;
using Aspnetcore.Camps.Model.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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

        [AllowAnonymous]
        [MapToApiVersion("2.0")]
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