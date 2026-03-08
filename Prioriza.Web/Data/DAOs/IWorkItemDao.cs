using Prioriza.Web.Data.Entities;

namespace Prioriza.Web.Data.DAOs;

public interface IWorkItemDao
{
    Task<IEnumerable<WorkItem>> GetAllByProjectAsync(int projectId);
    Task<WorkItem?> GetByIdAsync(int id);
    Task<WorkItem> CreateAsync(WorkItem workItem);
    Task<WorkItem> UpdateAsync(WorkItem workItem);
    Task ToggleCompletedAsync(int id);
    Task DeleteAsync(int id);
}