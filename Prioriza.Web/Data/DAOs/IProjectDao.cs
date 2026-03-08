using Prioriza.Web.Data.Entities;

namespace Prioriza.Web.Data.DAOs;

/// <summary>
/// Contrato para operaciones de acceso a datos de <see cref="Project"/>.
/// </summary>
public interface IProjectDao
{
    /// <summary>
    /// Devuelve todos los proyectos del usuario, excluyendo el Inbox.
    /// </summary>
    Task<IEnumerable<Project>> GetAllByUserAsync(string userId);

    /// <summary>
    /// Devuelve un proyecto por su Id, incluyendo sus tareas.
    /// Devuelve null si no existe.
    /// </summary>
    Task<Project?> GetByIdAsync(int id);

    /// <summary>
    /// Devuelve el proyecto Inbox del usuario.
    /// Toda tarea creada sin proyecto explícito va aquí.
    /// Devuelve null si el usuario aún no tiene Inbox (no debería ocurrir).
    /// </summary>
    Task<Project?> GetInboxByUserAsync(string userId);

    /// <summary>
    /// Persiste un nuevo proyecto en la base de datos y lo devuelve con su Id asignado.
    /// </summary>
    Task<Project> CreateAsync(Project project);

    /// <summary>
    /// Actualiza los datos de un proyecto existente.
    /// </summary>
    Task<Project> UpdateAsync(Project project);

    /// <summary>
    /// Elimina un proyecto y sus tareas en cascada.
    /// No hace nada si el Id no existe.
    /// </summary>
    Task DeleteAsync(int id);
}