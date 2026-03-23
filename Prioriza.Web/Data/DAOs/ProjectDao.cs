using Microsoft.EntityFrameworkCore;
using Prioriza.Web.Data.Entities;

namespace Prioriza.Web.Data.DAOs;

/// <inheritdoc />
public class ProjectDao(ApplicationDbContext context) : IProjectDao
{
    /// <inheritdoc />
    /// <remarks>
    /// Usa .Include para cargar las tareas de cada proyecto en la misma consulta,
    /// necesario para calcular el progreso en las vistas sin consultas adicionales.
    /// Filtra con !IsInbox para que el Inbox no aparezca en el listado de proyectos.
    /// </remarks>
    public async Task<IEnumerable<Project>> GetAllByUserAsync(string userId)
    {
        return await context.Projects
            .Where(p => p.UserId == userId && !p.IsInbox)
            .Include(p => p.Tasks)
            .ToListAsync();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Usa .Include para cargar las tareas junto con el proyecto.
    /// Necesario para mostrar la lista de tareas en la página de detalle
    /// sin hacer una segunda consulta a la base de datos.
    /// </remarks>
    public async Task<Project?> GetByIdAsync(int id)
    {
        return await context.Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
    
    /// <inheritdoc />
    public async Task<Project?> GetInboxByUserAsync(string userId)
    {
        return await context.Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.IsInbox);
    }

    /// <inheritdoc />
    public async Task<Project> CreateAsync(Project project)
    {
        context.Projects.Add(project);
        await context.SaveChangesAsync();
        return project;
    }

    /// <inheritdoc />
    public async Task<Project> UpdateAsync(Project project)
    {
        context.Projects.Update(project);
        await context.SaveChangesAsync();
        return project;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Carga la entidad antes de borrarla porque EF Core necesita
    /// una entidad rastreada para ejecutar el DELETE.
    /// </remarks>
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