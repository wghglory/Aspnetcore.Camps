# Asp.net Core Api

When all code is ready, we first run `dotnet ef migrations add Initial` to generate Migration. Usually after this we need to run `dotnet ef database update` so we will have a database with _EFMigrationHistory and other empty tables. But in this project we don't need run `dotnet ef database update` command since we explictly call `Database.Migrate()` in DbContext:

```csharp
using Microsoft.EntityFrameworkCore;

namespace Aspnetcore.CityInfo.Api.Entities
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
           : base(options)
        {
            Database.Migrate();  // same as `dotnet ef database update`
        }

        public DbSet<City> Cities { get; set; }
        public DbSet<PointOfInterest> PointsOfInterest { get; set; }
    }
}
```

## 2 Code-First Seed Ways

1. AppDbContextExtensions.cs
1. DbInitializer.cs

## Caveat

In startup.cs `ConfigureServices`, add below to avoid **self referencing issue**:

```csharp
services.AddMvc().AddJsonOptions(
    options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);
```

## Api Testing

import test/City.postman.json to postman and run test.

## NLog

.Net Core default supports log information in output window. If you want to log everything in file, install `NLog.Extensions.Logging`.

## Further Optimization

1. Create ViewModels for read, update, delete instead of exposing entity directly when calling api (safer, lightweight)
1. Use AutoMapper to convert between ViewModels and Entities