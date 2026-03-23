using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Prioriza.Web.Data.DAOs;
using Prioriza.Web.Data.Entities;

namespace Prioriza.Web.Controllers;

/// <summary>
/// Gestiona las operaciones CRUD sobre las tareas (WorkItems).
/// Las tareas no tienen vistas propias: todas las acciones son POST
/// y redirigen de vuelta a la página de detalle del proyecto.
/// </summary>
[Authorize]
public class WorkItemsController : Controller
{
    private readonly IWorkItemDao _workItemDao;
    private readonly IProjectDao _projectDao;
    private readonly UserManager<ApplicationUser> _userManager;

    public WorkItemsController(
        IWorkItemDao workItemDao,
        IProjectDao projectDao,
        UserManager<ApplicationUser> userManager)
    {
        _workItemDao = workItemDao;
        _projectDao = projectDao;
        _userManager = userManager;
    }
    
    /// <summary>
    /// Obtiene un proyecto verificando que pertenece al usuario autenticado.
    /// Devuelve null si el proyecto no existe o pertenece a otro usuario.
    /// </summary>
    private async Task<Project?> GetOwnedProjectAsync(int projectId)
    {
        var project = await _projectDao.GetByIdAsync(projectId);
        if (project is null || project.UserId != _userManager.GetUserId(User))
            return null;
        return project;
    }

    /// <summary>
    /// Obtiene una tarea verificando que pertenece al usuario autenticado.
    /// La propiedad se verifica a través del proyecto, ya que WorkItem
    /// no tiene FK directa al usuario.
    /// Devuelve null si la tarea o su proyecto no existen, o si el proyecto
    /// pertenece a otro usuario.
    /// </summary>
    private async Task<WorkItem?> GetOwnedWorkItemAsync(int id)
    {
        var workItem = await _workItemDao.GetByIdAsync(id);
        if (workItem is null)
            return null;

        var project = await _projectDao.GetByIdAsync(workItem.ProjectId);
        if (project is null || project.UserId != _userManager.GetUserId(User))
            return null;

        return workItem;
    }

    /// <summary>
    /// POST /WorkItems/Create
    /// Crea una nueva tarea dentro de un proyecto del usuario.
    /// El ProjectId se asigna desde la ruta, no desde el formulario,
    /// para evitar que se manipule desde el cliente.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int projectId, WorkItem workItem)
    {
        if (await GetOwnedProjectAsync(projectId) is null)
            return NotFound();
        
        ModelState.Remove(nameof(WorkItem.ProjectId));
        ModelState.Remove(nameof(WorkItem.Project));
        ModelState.Remove(nameof(WorkItem.Description));

        if (!ModelState.IsValid)
            return RedirectToAction("Details", "Projects", new { id = projectId });

        workItem.ProjectId = projectId;
        workItem.Description ??= string.Empty;
        await _workItemDao.CreateAsync(workItem);
        return RedirectToAction("Details", "Projects", new { id = projectId });
    }

    /// <summary>
    /// POST /WorkItems/Edit
    /// Actualiza los datos de una tarea existente.
    /// Verifica que tanto el proyecto como la tarea pertenecen al usuario.
    /// El ProjectId se preserva del registro original.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int projectId, WorkItem workItem)
    {
        if (await GetOwnedProjectAsync(projectId) is null)
            return NotFound();

        var existing = await GetOwnedWorkItemAsync(workItem.Id);
        if (existing is null)
            return NotFound();
        
        ModelState.Remove(nameof(WorkItem.ProjectId));
        ModelState.Remove(nameof(WorkItem.Project));

        if (!ModelState.IsValid)
            return RedirectToAction("Details", "Projects", new { id = projectId });

        existing.ProjectId = projectId;
        existing.Title = workItem.Title;
        existing.Description = workItem.Description;
        existing.Priority = workItem.Priority;
        existing.DueDate = workItem.DueDate;
        await _workItemDao.UpdateAsync(existing);
        return RedirectToAction("Details", "Projects", new { id = projectId });
    }

    /// <summary>
    /// POST /WorkItems/ToggleCompleted
    /// Invierte el estado de completado de una tarea.
    /// Usado desde la lista de tareas del proyecto sin necesidad de abrir un formulario.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleCompleted(int id, int projectId)
    {
        if (await GetOwnedWorkItemAsync(id) is null)
            return NotFound();

        await _workItemDao.ToggleCompletedAsync(id);
        return RedirectToAction("Details", "Projects", new { id = projectId });
    }

    /// <summary>
    /// POST /WorkItems/Delete
    /// Elimina una tarea tras verificar que pertenece al usuario autenticado.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int projectId)
    {
        if (await GetOwnedWorkItemAsync(id) is null)
            return NotFound();

        await _workItemDao.DeleteAsync(id);
        return RedirectToAction("Details", "Projects", new { id = projectId });
    }
}