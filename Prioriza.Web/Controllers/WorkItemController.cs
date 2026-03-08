using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Prioriza.Web.Data.DAOs;
using Prioriza.Web.Data.Entities;

namespace Prioriza.Web.Controllers;

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
    
    private async Task<Project?> GetOwnedProjectAsync(int projectId)
    {
        var project = await _projectDao.GetByIdAsync(projectId);
        if (project is null || project.UserId != _userManager.GetUserId(User))
            return null;
        return project;
    }

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

    // POST /WorkItems/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int projectId, WorkItem workItem)
    {
        if (await GetOwnedProjectAsync(projectId) is null)
            return NotFound();

        if (!ModelState.IsValid)
            return RedirectToAction("Details", "Projects", new { id = projectId });

        workItem.ProjectId = projectId;
        await _workItemDao.CreateAsync(workItem);
        return RedirectToAction("Details", "Projects", new { id = projectId });
    }

    // POST /WorkItems/Edit
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int projectId, WorkItem workItem)
    {
        if (await GetOwnedProjectAsync(projectId) is null)
            return NotFound();

        var existing = await GetOwnedWorkItemAsync(workItem.Id);
        if (existing is null)
            return NotFound();

        if (!ModelState.IsValid)
            return RedirectToAction("Details", "Projects", new { id = projectId });

        workItem.ProjectId = projectId;
        await _workItemDao.UpdateAsync(workItem);
        return RedirectToAction("Details", "Projects", new { id = projectId });
    }

    // POST /WorkItems/ToggleCompleted
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleCompleted(int id, int projectId)
    {
        if (await GetOwnedWorkItemAsync(id) is null)
            return NotFound();

        await _workItemDao.ToggleCompletedAsync(id);
        return RedirectToAction("Details", "Projects", new { id = projectId });
    }

    // POST /WorkItems/Delete
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int projectId)
    {
        if (await GetOwnedWorkItemAsync(id) is null)
            return NotFound();

        await _workItemDao.DeleteAsync(id);
        return RedirectToAction("Details", "Projects", new { id = projectId });
    }
}