using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Prioriza.Web.Data.Entities;

namespace Prioriza.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<WorkItem> WorkItems { get; set; }
    public DbSet<Project> Projects { get; set; }
}
