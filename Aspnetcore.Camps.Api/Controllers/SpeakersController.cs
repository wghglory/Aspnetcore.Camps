using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aspnetcore.Camps.Api.Filters;
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
    [ValidateModel]
    [Authorize]
    [ApiVersion("1.0")]
    [ApiVersion("1.1")]
    public class SpeakersController : BaseController
    {
        protected readonly ILogger<SpeakersController> Logger;
        protected readonly IMapper Mapper;
        protected readonly ICampRepository Repository;
        protected readonly UserManager<CampUser> UserMgr;

        public SpeakersController(ICampRepository repository,
            ILogger<SpeakersController> logger,
            IMapper mapper,
            UserManager<CampUser> userMgr)
        {
            Repository = repository;
            Logger = logger;
            Mapper = mapper;
            UserMgr = userMgr;
        }

        [HttpGet]
        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        public IActionResult Get(string moniker, bool includeTalks = false)
        {
            var speakers = includeTalks
                ? Repository.GetSpeakersByMonikerWithTalks(moniker)
                : Repository.GetSpeakersByMoniker(moniker);

            return Ok(Mapper.Map<IEnumerable<SpeakerViewModel>>(speakers));
        }

        [HttpGet]
        [AllowAnonymous]
        [MapToApiVersion("1.1")]
        public virtual IActionResult GetWithCount(string moniker, bool includeTalks = false)
        {
            var speakers = includeTalks
                ? Repository.GetSpeakersByMonikerWithTalks(moniker)
                : Repository.GetSpeakersByMoniker(moniker);

            return Ok(new {count = speakers.Count(), results = Mapper.Map<IEnumerable<SpeakerViewModel>>(speakers)});
        }

        [HttpGet("{id}", Name = "GetSpeaker")]
        public IActionResult Get(string moniker, int id, bool includeTalks = false)
        {
            var speaker = includeTalks ? Repository.GetSpeakerWithTalks(id) : Repository.GetSpeaker(id);
            if (speaker == null) return NotFound();
            if (speaker.Camp.Moniker != moniker) return BadRequest("Speaker not in specified Camp");

            return Ok(Mapper.Map<SpeakerViewModel>(speaker));
        }

        [HttpPost]
        public async Task<IActionResult> Post(string moniker, [FromBody] SpeakerViewModel model)
        {
            try
            {
                var camp = Repository.GetCampByMoniker(moniker);
                if (camp == null) return BadRequest("Could not find camp");

                var speaker = Mapper.Map<Speaker>(model);
                speaker.Camp = camp;

                var campUser = await UserMgr.FindByNameAsync(this.User.Identity.Name);
                if (campUser != null)
                {
                    Repository.Add(speaker);

                    if (await Repository.SaveAllAsync())
                    {
                        var url = Url.Link("GetSpeaker", new {moniker = camp.Moniker, id = speaker.Id});
                        return Created(url, Mapper.Map<SpeakerViewModel>(speaker));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception thrown while adding speaker: {ex}");
            }

            return BadRequest("Could not add new speaker");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string moniker, int id, [FromBody] SpeakerViewModel model)
        {
            try
            {
                var speaker = Repository.GetSpeaker(id);
                if (speaker == null) return NotFound();
                if (speaker.Camp.Moniker != moniker) return BadRequest("Speaker and Camp do not match");

                if (speaker.User.UserName != this.User.Identity.Name) return Forbid();

                Mapper.Map(model, speaker);

                if (await Repository.SaveAllAsync())
                {
                    return Ok(Mapper.Map<SpeakerViewModel>(speaker));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception thrown while updating speaker: {ex}");
            }

            return BadRequest("Could not update speaker");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string moniker, int id)
        {
            try
            {
                var speaker = Repository.GetSpeaker(id);
                if (speaker == null) return NotFound();
                if (speaker.Camp.Moniker != moniker) return BadRequest("Speaker and Camp do not match");

                if (speaker.User.UserName != this.User.Identity.Name) return Forbid();

                Repository.Delete(speaker);

                if (await Repository.SaveAllAsync())
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception thrown while deleting speaker: {ex}");
            }

            return BadRequest("Could not delete speaker");
        }
    }
}