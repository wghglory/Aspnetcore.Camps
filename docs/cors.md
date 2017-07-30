# CORS

Cross Origin Resource Sharing

- Browsers prevent API requests across domains
- Enable CORS gets around this limitation
    Doesn't affect non-browser development

Startup.cs:

```csharp
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
}

public void Configure(IApplicationBuilder app)
{
    /*// globally enable cors. usually won't use this
    app.UseCors(cfg =>
    {
        cfg.AllowAnyHeader()
            .AllowAnyMethod()
            .WithOrigins("http://wildermuth.com");
    });*/
}
```

CampsController:

```csharp
[EnableCors("AnyGET")]
[Route("api/[controller]")]
[ValidateModel]
public class CampsController : BaseController
{
    // ....
    [HttpGet("")]
    public IActionResult Get() { }

    [EnableCors("Wildermuth")]
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CampViewModel model) { }
}
```