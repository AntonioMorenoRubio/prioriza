using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Prioriza.Web.Data.DAOs;
using Prioriza.Web.Data.Entities;

namespace Prioriza.Web.Controllers;

/// <summary>
/// Gestiona las operaciones CRUD sobre los proyectos del usuario autenticado.
/// Todas las acciones requieren autenticación mediante [Authorize].
/// </summary>
[Authorize]
public class ProjectsController : Controller
{
    private readonly IProjectDao _projectDao;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProjectsController(IProjectDao projectDao, UserManager<ApplicationUser> userManager)
    {
        _projectDao = projectDao;
        _userManager = userManager;
    }

    /// <summary>
    /// Obtiene un proyecto verificando que pertenece al usuario autenticado.
    /// Devuelve null si el proyecto no existe o pertenece a otro usuario.
    /// Centraliza la comprobación de propiedad para evitar repetición en cada acción.
    /// </summary>
    private async Task<Project?> GetOwnedProjectAsync(int id)
    {
        var project = await _projectDao.GetByIdAsync(id);
        if (project is null || project.UserId != _userManager.GetUserId(User))
            return null;
        return project;
    }

    /// <summary>
    /// GET /Projects
    /// Muestra el listado de proyectos del usuario autenticado.
    /// Punto de entrada del dashboard principal.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        var projects = await _projectDao.GetAllByUserAsync(userId);
        return View(projects);
    }

    /// <summary>
    /// GET /Projects/Details/5
    /// Muestra el detalle de un proyecto con todas sus tareas.
    /// Devuelve 404 si el proyecto no existe o pertenece a otro usuario.
    /// </summary>
    public async Task<IActionResult> Details(int id)
    {
        var project = await GetOwnedProjectAsync(id);
        if (project is null)
            return NotFound();

        return View(project);
    }

    /// <summary>
    /// GET /Projects/Create
    /// Muestra el formulario de creación de un nuevo proyecto.
    /// </summary>
    public IActionResult Create() => View();

    /// <summary>
    /// POST /Projects/Create
    /// Persiste un nuevo proyecto asignándolo al usuario autenticado.
    /// IsInbox se fuerza a false para que nunca se cree un segundo Inbox por accidente.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Project project)
    {
        if (!ModelState.IsValid)
            return View(project);

        project.UserId = _userManager.GetUserId(User)!;
        project.IsInbox = false;
        await _projectDao.CreateAsync(project);
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// GET /Projects/Edit/5
    /// Muestra el formulario de edición de un proyecto existente.
    /// Devuelve 404 si el proyecto no existe o pertenece a otro usuario.
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        var project = await GetOwnedProjectAsync(id);
        if (project is null)
            return NotFound();

        return View(project);
    }

    /// <summary>
    /// POST /Projects/Edit/5
    /// Actualiza los datos de un proyecto existente.
    /// UserId e IsInbox se preservan del registro original para evitar
    /// que se manipulen desde el formulario.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Project project)
    {
        if (id != project.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(project);

        var existing = await GetOwnedProjectAsync(id);
        if (existing is null)
            return NotFound();

        project.UserId = existing.UserId;
        project.IsInbox = existing.IsInbox;
        await _projectDao.UpdateAsync(project);
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// GET /Projects/Delete/5
    /// Muestra la pantalla de confirmación antes de eliminar un proyecto.
    /// Devuelve 404 si el proyecto no existe o pertenece a otro usuario.
    /// </summary>
    public async Task<IActionResult> Delete(int id)
    {
        var project = await GetOwnedProjectAsync(id);
        if (project is null)
            return NotFound();

        return View(project);
    }

    /// <summary>
    /// POST /Projects/Delete/5
    /// Elimina el proyecto tras la confirmación del usuario.
    /// Las tareas asociadas se eliminan en cascada por configuración de la BD.
    /// </summary>
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var project = await GetOwnedProjectAsync(id);
        if (project is null)
            return NotFound();

        await _projectDao.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}