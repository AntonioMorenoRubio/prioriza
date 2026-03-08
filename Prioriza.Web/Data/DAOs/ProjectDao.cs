using Microsoft.EntityFrameworkCore;
using Prioriza.Web.Data.Entities;

namespace Prioriza.Web.Data.DAOs;

public class ProjectDao(ApplicationDbContext context) : IProjectDao
{
    public async Task<IEnumerable<Project>> GetAllByUserAsync(string userId)
    {
        return await context.Projects
            .Where(p => p.UserId == userId && !p.IsInbox)
            .Include(p => p.Tasks)
            .ToListAsync();
    }

    public async Task<Project?> GetByIdAsync(int id)
    {
        return await context.Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
    
    public async Task<Project?> GetInboxByUserAsync(string userId)
    {
        return await context.Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.IsInbox);
    }

    public async Task<Project> CreateAsync(Project project)
    {
        context.Projects.Add(project);
        await context.SaveChangesAsync();
        return project;
    }

    public async Task<Project> UpdateAsync(Project project)
    {
        context.Projects.Update(project);
        await context.SaveChangesAsync();
        return project;
    }

    public async Task DeleteAsync(int id)
    {
        var project = await context.Projects.FindAsync(id);
        if (project is not null)
        {
            context.Projects.Remove(project);
            await context.SaveChangesAsync();
        }
    }
}