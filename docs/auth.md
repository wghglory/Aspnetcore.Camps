# Authentication and Authorization

## Authentication

Concept: **Using Credentials to determine Identity**

Types for API:

- App Authentication (App Key + Secret is a typical scenario)
  - Using a secret to identify an app for your API
  - Not authenticating as the user, but as the developer!
- User Authentication is accessing your API as a User. (In order of security)
  1. Cookie Authentication
  1. Basic Authentication (Insecure and slow, use Tokens instead)
  1. Token Authentication
  1. OAuth

## Authorization

Concept: **Verifying an Identity has rights to a specific resource**

## ASP.NET Identity

Simple system for storing User identities, roles and claims

- Not appropriate for App Authentication
- Easy to do Cookie-Based Authentication
- Basis for Basic and Token Authentication
- For OAuth use more robust system like Identity Server <https://identityserver.com>

### Using Identity

Startup.cs

```csharp
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Aspnetcore.Camps.Api
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // create cors policy, any controller/action can use these policies
            services.AddCors(cfg =>
            {
                cfg.AddPolicy("Wildermuth", builder =>
                {
                    builder.AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithOrigins("http://wildermuth.com");
                });

                cfg.AddPolicy("AnyGET", builder =>
                {
                    builder.AllowAnyHeader()
                        .WithMethods("GET")
                        .AllowAnyOrigin();
                });
            });

            // add identity
            services.AddIdentity<CampUser, IdentityRole>().AddEntityFrameworkStores<CampContext>();

            // config identity option, return corresponding code
            services.Configure<IdentityOptions>(config =>
            {
                config.Cookies.ApplicationCookie.Events =
                    new CookieAuthenticationEvents()
                    {
                        OnRedirectToLogin = (ctx) =>
                        {
                            if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                            {
                                ctx.Response.StatusCode = 401;
                            }

                            return Task.CompletedTask;
                        },
                        OnRedirectToAccessDenied = (ctx) =>
                        {
                            if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                            {
                                ctx.Response.StatusCode = 403;
                            }

                            return Task.CompletedTask;
                        }
                    };
            });

            services.AddMvc()
                .AddJsonOptions(opt =>
                {
                    opt.SerializerSettings.ReferenceLoopHandling =
                        ReferenceLoopHandling.Ignore;
                });
        }

        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            CampDbInitializer campDbInitializer)
        {
            // ...
            app.UseIdentity();

            app.UseMvc();
        }
    }
}
```

CampsController:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;

namespace Aspnetcore.Camps.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ValidateModel]
    [EnableCors("AnyGET")]
    public class CampsController : BaseController{}
}
```

### Cookie Authentication

AuthController.cs

```csharp
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
    }
}
```

CredentialModel.cs

```csharp
using System.ComponentModel.DataAnnotations;

namespace Aspnetcore.Camps.Api.ViewModels
{
    public class CredentialModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
```

Now if you try to access <https://localhost:43388/api/camps> in postman, you will see 401 unAuthorized. If you send post request to <https://localhost:43388/api/auth/login> with `username:wghglory, password:P@ssw0rd!`, you will see 200. Now you can access the campsController.

> When you sign in successfully, asp.net will set a cookie with name `.AspNetCore.Identity.Application`. Then when you request other actions with [Authorize] filter, this cookie will also be sent. Asp.net will verify cookie. So this is Cookie/Basic Authentication.

### Implement Authorization for SpeakersController

```diff
namespace Aspnetcore.Camps.Api.Controllers
{
    [Route("api/camps/{moniker}/speakers")]
    [ValidateModel]
+    [Authorize]
    public class SpeakersController : BaseController
    {
        private readonly ILogger<SpeakersController> _logger;
        private readonly IMapper _mapper;
        private readonly ICampRepository _repository;
+       private readonly UserManager<CampUser> _userMgr;

        public SpeakersController(ICampRepository repository,
            ILogger<SpeakersController> logger,
            IMapper mapper,
+            UserManager<CampUser> userMgr)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
+            _userMgr = userMgr;
        }

        [HttpGet]
+        [AllowAnonymous]
        public IActionResult Get(string moniker, bool includeTalks = false)
        {
            var speakers = includeTalks
                ? _repository.GetSpeakersByMonikerWithTalks(moniker)
                : _repository.GetSpeakersByMoniker(moniker);

            return Ok(_mapper.Map<IEnumerable<SpeakerViewModel>>(speakers));
        }

        [HttpGet("{id}", Name = "GetSpeaker")]
+        [AllowAnonymous]
        public IActionResult Get(string moniker, int id, bool includeTalks = false)
        {
            var speaker = includeTalks ? _repository.GetSpeakerWithTalks(id) : _repository.GetSpeaker(id);
            if (speaker == null) return NotFound();
            if (speaker.Camp.Moniker != moniker) return BadRequest("Speaker not in specified Camp");

            return Ok(_mapper.Map<SpeakerViewModel>(speaker));
        }

        [HttpPost]
        public async Task<IActionResult> Post(string moniker, [FromBody] SpeakerViewModel model)
        {
            try
            {
                var camp = _repository.GetCampByMoniker(moniker);
                if (camp == null) return BadRequest("Could not find camp");

                var speaker = _mapper.Map<Speaker>(model);
                speaker.Camp = camp;

+                var campUser = await _userMgr.FindByNameAsync(this.User.Identity.Name);
+                if (campUser != null)
+                {
                    _repository.Add(speaker);

                    if (await _repository.SaveAllAsync())
                    {
                        var url = Url.Link("GetSpeaker", new {moniker = camp.Moniker, id = speaker.Id});
                        return Created(url, _mapper.Map<SpeakerViewModel>(speaker));
                    }
+                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown while adding speaker: {ex}");
            }

            return BadRequest("Could not add new speaker");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string moniker, int id, [FromBody] SpeakerViewModel model)
        {
            try
            {
                var speaker = _repository.GetSpeaker(id);
                if (speaker == null) return NotFound();
                if (speaker.Camp.Moniker != moniker) return BadRequest("Speaker and Camp do not match");

+                if (speaker.User.UserName != this.User.Identity.Name) return Forbid();

                _mapper.Map(model, speaker);

                if (await _repository.SaveAllAsync())
                {
                    return Ok(_mapper.Map<SpeakerViewModel>(speaker));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown while updating speaker: {ex}");
            }

            return BadRequest("Could not update speaker");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string moniker, int id)
        {
            try
            {
                var speaker = _repository.GetSpeaker(id);
                if (speaker == null) return NotFound();
                if (speaker.Camp.Moniker != moniker) return BadRequest("Speaker and Camp do not match");

+                if (speaker.User.UserName != this.User.Identity.Name) return Forbid();

                _repository.Delete(speaker);

                if (await _repository.SaveAllAsync())
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown while deleting speaker: {ex}");
            }

            return BadRequest("Could not delete speaker");
        }
    }
}
```