using Microsoft.EntityFrameworkCore;
using Prioriza.Web.Data.Entities;

namespace Prioriza.Web.Data.DAOs;

/// <inheritdoc />
public class WorkItemDao(ApplicationDbContext context) : IWorkItemDao
{
    /// <inheritdoc />
    public async Task<IEnumerable<WorkItem>> GetAllByProjectAsync(int projectId)
    {
        return await context.WorkItems
            .Where(w => w.ProjectId == projectId)
            .ToListAsync();
    }


    /// <inheritdoc />
    public async Task<WorkItem?> GetByIdAsync(int id)
    {
        return await context.WorkItems
            .FirstOrDefaultAsync(w => w.Id == id);
    }


    /// <inheritdoc />
    public async Task<WorkItem> CreateAsync(WorkItem workItem)
    {
        context.WorkItems.Add(workItem);
        await context.SaveChangesAsync();
        return workItem;
    }


    /// <inheritdoc />
    public async Task<WorkItem> UpdateAsync(WorkItem workItem)
    {
        context.WorkItems.Update(workItem);
        await context.SaveChangesAsync();
        return workItem;
    }


    /// <inheritdoc />
    /// <remarks>
    /// Carga la entidad antes de modificarla para que EF Core
    /// pueda rastrear el cambio y generar el UPDATE correcto.
    /// </remarks>
    public async Task ToggleCompletedAsync(int id)
    {
        var workItem = await context.WorkItems.FindAsync(id);
        if (workItem is null) return;

        workItem.IsCompleted = !workItem.IsCompleted;
        await context.SaveChangesAsync();
    }


    /// <inheritdoc />
    public async Task DeleteAsync(int id)
    {
        var workItem = await context.WorkItems.FindAsync(id);
        if (workItem is null) return;

        context.WorkItems.Remove(workItem);
        await context.SaveChangesAsync();
    }
}