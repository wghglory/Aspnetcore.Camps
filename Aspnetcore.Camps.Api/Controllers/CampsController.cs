using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aspnetcore.Camps.Api.Filters;
using Aspnetcore.Camps.Api.ViewModels;
using Aspnetcore.Camps.Model.Entities;
using Aspnetcore.Camps.Model.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Aspnetcore.Camps.Api.Controllers
{
    [Route("api/[controller]")]
    [ValidateModel]
    public class CampsController : BaseController
    {
        private readonly ILogger<CampsController> _logger;
        private readonly ICampRepository _repo;
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
            // map IEnumerable<Camp> entity to IEnumerable<CampViewModel>
            return Ok(_mapper.Map<IEnumerable<CampViewModel>>(camps));
        }

        /*[HttpGet("{id}", Name = "GetCamp")]
        public IActionResult Get(int id, bool includeSpeakers = false)
        {
            try
            {
                Camp camp = includeSpeakers ? _repo.GetCampWithSpeakers(id) : _repo.GetCamp(id);

                if (camp == null) return NotFound($"Camp {id} was not found");
                // map Camp entity to CampViewModel
                return Ok(_mapper.Map<CampViewModel>(camp));
            }
            catch
            {
                // ignored
            }

            return BadRequest();
        }
        
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Camp model)
        {
            try
            {
                _logger.LogInformation("Creating a new Code Camp");

                _repo.Add(model);
                if (await _repo.SaveAllAsync())
                {
                    // after saving, we return the model just created
                    var newUri = Url.Link("GetCamp", new {id = model.Id});
                    return Created(newUri, model);
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
        
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Camp model)
        {
            try
            {
                var oldCamp = _repo.GetCamp(id);
                if (oldCamp == null) return NotFound($"Could not find a camp with an ID of {id}");

                // Map model to the oldCamp
                oldCamp.Name = model.Name ?? oldCamp.Name;
                oldCamp.Description = model.Description ?? oldCamp.Description;
                oldCamp.Location = model.Location ?? oldCamp.Location;
                oldCamp.Length = model.Length > 0 ? model.Length : oldCamp.Length;
                oldCamp.EventDate = model.EventDate != DateTime.MinValue ? model.EventDate : oldCamp.EventDate;

                if (await _repo.SaveAllAsync())
                {
                    return Ok(oldCamp);
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return BadRequest("Couldn't update Camp");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var oldCamp = _repo.GetCamp(id);
                if (oldCamp == null) return NotFound($"Could not find Camp with ID of {id}");

                _repo.Delete(oldCamp);
                if (await _repo.SaveAllAsync())
                {
                    return Ok();
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return BadRequest("Could not delete Camp");
        }
        */


        [HttpGet("{moniker}", Name = "GetCamp")]
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


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CampViewModel model)
        {
            try
            {
//                if (!ModelState.IsValid) return BadRequest(ModelState);   //not needed because of ValidateModel Filter

                _logger.LogInformation("Creating a new Code Camp");

                // reverse map, viewmodel --> entity
                Camp camp = _mapper.Map<Camp>(model);

                _repo.Add(camp);
                if (await _repo.SaveAllAsync())
                {
                    // after saving, we return the model just created
                    var newUri = Url.Link("GetCamp", new {Moniker = model.Moniker});
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

        [HttpPut("{moniker}")]
        public async Task<IActionResult> Put(string moniker, [FromBody] CampViewModel model)
        {
            try
            {
//                if (!ModelState.IsValid) return BadRequest(ModelState);  //not needed because of ValidateModel Filter

                var oldCamp = _repo.GetCampByMoniker(moniker);
                if (oldCamp == null) return NotFound($"Could not find a camp with an Moniker of {moniker}");

//                // Map model to the oldCamp
//                oldCamp.Name = model.Name ?? oldCamp.Name;
//                oldCamp.Description = model.Description ?? oldCamp.Description;
//                oldCamp.Location = model.Location ?? oldCamp.Location;
//                oldCamp.Length = model.Length > 0 ? model.Length : oldCamp.Length;
//                oldCamp.EventDate = model.EventDate != DateTime.MinValue ? model.EventDate : oldCamp.EventDate;

                _mapper.Map(model, oldCamp);

                if (await _repo.SaveAllAsync())
                {
                    return Ok(_mapper.Map<CampViewModel>(oldCamp));
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return BadRequest("Couldn't update Camp");
        }

        [HttpPatch("{moniker}")]
        public async Task<IActionResult> Patch(string moniker, [FromBody] JsonPatchDocument<CampViewModel> patchDoc)
        {
            try
            {
//                if (!ModelState.IsValid) return BadRequest(ModelState);  //not needed because of ValidateModel Filter

                var oldCamp = _repo.GetCampByMoniker(moniker);
                if (oldCamp == null) return NotFound($"Could not find a camp with an Moniker of {moniker}");

//                // Map model to the oldCamp
//                oldCamp.Name = model.Name ?? oldCamp.Name;
//                oldCamp.Description = model.Description ?? oldCamp.Description;
//                oldCamp.Location = model.Location ?? oldCamp.Location;
//                oldCamp.Length = model.Length > 0 ? model.Length : oldCamp.Length;
//                oldCamp.EventDate = model.EventDate != DateTime.MinValue ? model.EventDate : oldCamp.EventDate;

                CampViewModel model = _mapper.Map<CampViewModel>(oldCamp);

                patchDoc.ApplyTo(model, ModelState);

                TryValidateModel(model);

                if (!ModelState.IsValid) return BadRequest(ModelState);

                // note: `property: null` from viewmodel will make entity `property: null` even if entity previous property has a value
                _mapper.Map(model, oldCamp);

                if (await _repo.SaveAllAsync())
                {
                    return Ok(_mapper.Map<CampViewModel>(oldCamp));
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return BadRequest("Couldn't update Camp");
        }

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                var oldCamp = _repo.GetCampByMoniker(moniker);
                if (oldCamp == null) return NotFound($"Could not find Camp with Moniker of {moniker}");

                _repo.Delete(oldCamp);
                if (await _repo.SaveAllAsync())
                {
                    return Ok();
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return BadRequest("Could not delete Camp");
        }
    }
}