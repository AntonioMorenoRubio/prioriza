using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Prioriza.Web.Data.DAOs;
using Prioriza.Web.Data.Entities;

namespace Prioriza.Web.Controllers;

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

    // GET /Projects
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        var projects = await _projectDao.GetAllByUserAsync(userId);
        return View(projects);
    }

    // GET /Projects/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var project = await _projectDao.GetByIdAsync(id);
        if (project is null || project.UserId != _userManager.GetUserId(User))
            return NotFound();

        return View(project);
    }

    // GET /Projects/Create
    public IActionResult Create() => View();

    // POST /Projects/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Project project)
    {
        if (!ModelState.IsValid)
            return View(project);

        project.UserId = _userManager.GetUserId(User)!;
        await _projectDao.CreateAsync(project);
        return RedirectToAction(nameof(Index));
    }

    // GET /Projects/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var project = await _projectDao.GetByIdAsync(id);
        if (project is null || project.UserId != _userManager.GetUserId(User))
            return NotFound();

        return View(project);
    }

    // POST /Projects/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Project project)
    {
        if (id != project.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(project);

        var existing = await _projectDao.GetByIdAsync(id);
        if (existing is null || existing.UserId != _userManager.GetUserId(User))
            return NotFound();

        project.UserId = existing.UserId;
        await _projectDao.UpdateAsync(project);
        return RedirectToAction(nameof(Index));
    }

    // GET /Projects/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var project = await _projectDao.GetByIdAsync(id);
        if (project is null || project.UserId != _userManager.GetUserId(User))
            return NotFound();

        return View(project);
    }

    // POST /Projects/Delete/5
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var project = await _projectDao.GetByIdAsync(id);
        if (project is null || project.UserId != _userManager.GetUserId(User))
            return NotFound();

        await _projectDao.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}