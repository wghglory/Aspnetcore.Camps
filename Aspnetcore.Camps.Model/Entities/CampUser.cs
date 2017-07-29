using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Aspnetcore.Camps.Model.Entities
{
    public class CampUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}