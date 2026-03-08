using Prioriza.Web.Data.Entities;

namespace Prioriza.Web.Data.DAOs;

public interface IProjectDao
{
    Task<IEnumerable<Project>> GetAllByUserAsync(string userId);
    Task<Project?> GetByIdAsync(int id);
    Task<Project> CreateAsync(Project project);
    Task<Project> UpdateAsync(Project project);
    Task DeleteAsync(int id);
}