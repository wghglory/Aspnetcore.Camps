using Aspnetcore.Camps.Model.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Aspnetcore.Camps.Model
{
    public sealed class CampContext : IdentityDbContext
    {
        public IConfigurationRoot Configuration { get; set; }

        public CampContext(DbContextOptions options) : base(options)
        {
            Database.Migrate(); // same as `dotnet ef database update`
        }

        public DbSet<Camp> Camps { get; set; }
        public DbSet<Speaker> Speakers { get; set; }
        public DbSet<Talk> Talks { get; set; }

//        protected override void OnModelCreating(ModelBuilder builder)
//        {
//            base.OnModelCreating(builder);
//
//            builder.Entity<Entities.Camp>()
//                .Property(c => c.RowVersion)
//                .ValueGeneratedOnAddOrUpdate()
//                .IsConcurrencyToken();
//            builder.Entity<Speaker>()
//                .Property(c => c.RowVersion)
//                .ValueGeneratedOnAddOrUpdate()
//                .IsConcurrencyToken();
//            builder.Entity<Talk>()
//                .Property(c => c.RowVersion)
//                .ValueGeneratedOnAddOrUpdate()  // [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
//                .IsConcurrencyToken();  //[ConcurrencyCheck]
//        }
    }
}