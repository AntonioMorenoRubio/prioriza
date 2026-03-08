using Prioriza.Web.Data.Entities;

namespace Prioriza.Web.Data.DAOs;

/// <summary>
/// Contrato para operaciones de acceso a datos de <see cref="WorkItem"/>.
/// </summary>
public interface IWorkItemDao
{
    /// <summary>
    /// Devuelve todas las tareas de un proyecto concreto.
    /// </summary>
    Task<IEnumerable<WorkItem>> GetAllByProjectAsync(int projectId);

    /// <summary>
    /// Devuelve una tarea por su Id.
    /// Devuelve null si no existe.
    /// </summary>
    Task<WorkItem?> GetByIdAsync(int id);

    /// <summary>
    /// Persiste una nueva tarea en la base de datos y la devuelve con su Id asignado.
    /// </summary>
    Task<WorkItem> CreateAsync(WorkItem workItem);

    /// <summary>
    /// Actualiza los datos de una tarea existente.
    /// </summary>
    Task<WorkItem> UpdateAsync(WorkItem workItem);

    /// <summary>
    /// Invierte el estado de completado de una tarea.
    /// Si estaba completada pasa a pendiente, y viceversa.
    /// No hace nada si el Id no existe.
    /// </summary>
    Task ToggleCompletedAsync(int id);

    /// <summary>
    /// Elimina una tarea por su Id.
    /// No hace nada si el Id no existe.
    /// </summary>
    Task DeleteAsync(int id);
}