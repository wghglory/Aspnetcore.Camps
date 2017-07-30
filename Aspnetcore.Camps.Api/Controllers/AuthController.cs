using System;
using System.Threading.Tasks;
using Aspnetcore.Camps.Api.Filters;
using Aspnetcore.Camps.Api.Mappings;
using Aspnetcore.Camps.Api.ViewModels;
using Aspnetcore.Camps.Model;
using Aspnetcore.Camps.Model.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Aspnetcore.Camps.Api.Controllers
{
    public class AuthController : Controller
    {
        private readonly CampContext _context;
        private readonly SignInManager<CampUser> _signInMgr;
        private readonly ILogger<AuthController> _logger;

        public AuthController(CampContext context, SignInManager<CampUser> signInMgr, ILogger<AuthController> logger)
        {
            _context = context;
            _signInMgr = signInMgr;
            _logger = logger;
        }

        [HttpPost("api/auth/login")]
        [ValidateModel]
        public async Task<IActionResult> Login([FromBody] CredentialModel model)
        {
            try
            {
                var result = await _signInMgr.PasswordSignInAsync(model.UserName, model.Password, false, false);
                if (result.Succeeded)
                {
                    return Ok();
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception thrown: {e}");
                throw;
            }

            return BadRequest("fail to login");
        }

        public async void SignOut()
        {
            await _signInMgr.SignOutAsync();
        }
    }
}