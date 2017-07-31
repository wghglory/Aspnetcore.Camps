using System.Text;
using System.Threading.Tasks;
using Aspnetcore.Camps.Api.Controllers;
using Aspnetcore.Camps.Api.ViewModels;
using Aspnetcore.Camps.Model;
using Aspnetcore.Camps.Model.Entities;
using Aspnetcore.Camps.Model.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;

namespace Aspnetcore.Camps.Api
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            Env = env;
        }

        public IConfigurationRoot Configuration { get; }
        public IHostingEnvironment Env { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration);

            services.AddDbContext<CampContext>(
                    o => o.UseMySql(Configuration.GetConnectionString("MysqlConnection")))
                .AddIdentity<CampUser, IdentityRole>();

            services.AddScoped<ICampRepository, CampRepository>();

            services.AddTransient<CampDbInitializer>();
            services.AddTransient<CampIdentityInitializer>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddAutoMapper();

            // api verisioning
            services.AddApiVersioning(cfg =>
            {
                cfg.DefaultApiVersion = new ApiVersion(1, 1);
                cfg.AssumeDefaultVersionWhenUnspecified = true;
                cfg.ReportApiVersions = true;

                // support both
                cfg.ApiVersionReader = ApiVersionReader.Combine(new QueryStringApiVersionReader("ver"),
                    new HeaderApiVersionReader(
                        "Camp-Version")); //QueryStringOrHeaderApiVersionReader is removed(https://github.com/Microsoft/aspnet-api-versioning/releases)

                // way 1: add versioning on controller and actions
                
                //---
                
                // way 2: Using Versioning Conventions
                cfg.Conventions.Controller<TalksController>()
                    .HasApiVersion(new ApiVersion(1, 0))
                    .HasApiVersion(new ApiVersion(1, 1))
                    .HasApiVersion(new ApiVersion(2, 0))
                    .Action(m => m.Post(default(string), default(int), default(TalkViewModel)))
                    .MapToApiVersion(new ApiVersion(2, 0));
            });


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

            services.AddAuthorization(cfg =>
            {
                cfg.AddPolicy("SuperUsers", p => p.RequireClaim("SuperUser", "True"));
            });

            services.AddMvc(opt =>
                {
//                    // use Windows Visual Studio to enable ssl 
//                    // http will redirect to https
//                    if (!Env.IsProduction())
//                    {
//                        opt.SslPort = 44388;
//                    }
//                    opt.Filters.Add(new RequireHttpsAttribute()); //support ssl
                })
                .AddJsonOptions(opt =>
                {
                    opt.SerializerSettings.ReferenceLoopHandling =
                        ReferenceLoopHandling.Ignore;
                });
        }

        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            CampDbInitializer campDbInitializer,
            CampIdentityInitializer campIdentityInitializer)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            /*// globally enable cors. usually won't use this
            app.UseCors(cfg =>
            {
                cfg.AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins("http://wildermuth.com");
            });*/

            app.UseIdentity();

            app.UseJwtBearerAuthentication(new JwtBearerOptions()
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = Configuration["Tokens:Issuer"],
                    ValidAudience = Configuration["Tokens:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Tokens:Key"])),
                    ValidateLifetime = true
                }
            });

            app.UseMvc();

            campDbInitializer.Seed().Wait();
            campIdentityInitializer.Seed().Wait();
        }
    }
}