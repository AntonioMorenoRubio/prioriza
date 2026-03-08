using Microsoft.EntityFrameworkCore;
using Prioriza.Web.Data.Entities;

namespace Prioriza.Web.Data.DAOs;

public class WorkItemDao(ApplicationDbContext context) : IWorkItemDao
{
    public async Task<IEnumerable<WorkItem>> GetAllByProjectAsync(int projectId)
    {
        return await context.WorkItems
            .Where(w => w.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<WorkItem?> GetByIdAsync(int id)
    {
        return await context.WorkItems
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<WorkItem> CreateAsync(WorkItem workItem)
    {
        context.WorkItems.Add(workItem);
        await context.SaveChangesAsync();
        return workItem;
    }

    public async Task<WorkItem> UpdateAsync(WorkItem workItem)
    {
        context.WorkItems.Update(workItem);
        await context.SaveChangesAsync();
        return workItem;
    }

    public async Task ToggleCompletedAsync(int id)
    {
        var workItem = await context.WorkItems.FindAsync(id);
        if (workItem is null) return;

        workItem.IsCompleted = !workItem.IsCompleted;
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var workItem = await context.WorkItems.FindAsync(id);
        if (workItem is null) return;

        context.WorkItems.Remove(workItem);
        await context.SaveChangesAsync();
    }
}