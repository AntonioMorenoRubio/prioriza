using Microsoft.AspNetCore.Identity;

namespace Prioriza.Web.Data.Entities;

public class ApplicationUser : IdentityUser
{
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}