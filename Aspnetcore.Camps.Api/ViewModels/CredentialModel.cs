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