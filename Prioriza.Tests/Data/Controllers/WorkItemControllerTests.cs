using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Prioriza.Web.Controllers;
using Prioriza.Web.Data.DAOs;
using Prioriza.Web.Data.Entities;

namespace Prioriza.Tests.Data.Controllers;

public class WorkItemsControllerTests
{
    private readonly Mock<IWorkItemDao> _workItemDaoMock = new();
    private readonly Mock<IProjectDao> _projectDaoMock = new();
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;

    public WorkItemsControllerTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!, null!, null!, null!, null!
        );
    }

    private WorkItemsController BuildController(string userId = "user-1")
    {
        _userManagerMock
            .Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(userId);

        var controller = new WorkItemsController(
            _workItemDaoMock.Object,
            _projectDaoMock.Object,
            _userManagerMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId)
                    }, "mock"))
                }
            }
        };
        return controller;
    }

    // ── Create ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_RedirectsToProjectDetails_WhenValid()
    {
        var project = new Project { Id = 1, Name = "P1", UserId = "user-1" };
        _projectDaoMock.Setup(d => d.GetByIdAsync(1)).ReturnsAsync(project);
        _workItemDaoMock.Setup(d => d.CreateAsync(It.IsAny<WorkItem>()))
            .ReturnsAsync(new WorkItem { Id = 1, Title = "Tarea", ProjectId = 1 });

        var result = await BuildController().Create(1, new WorkItem { Title = "Tarea", ProjectId = 1 });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal("Projects", redirect.ControllerName);
        Assert.Equal(1, redirect.RouteValues!["id"]);
    }

    [Fact]
    public async Task Create_ReturnsNotFound_WhenProjectNotOwned()
    {
        _projectDaoMock.Setup(d => d.GetByIdAsync(1)).ReturnsAsync((Project?)null);

        var result = await BuildController().Create(1, new WorkItem { Title = "Tarea", ProjectId = 1 });

        Assert.IsType<NotFoundResult>(result);
        _workItemDaoMock.Verify(d => d.CreateAsync(It.IsAny<WorkItem>()), Times.Never);
    }

    [Fact]
    public async Task Create_AssignsProjectIdFromRoute()
    {
        var project = new Project { Id = 1, Name = "P1", UserId = "user-1" };
        _projectDaoMock.Setup(d => d.GetByIdAsync(1)).ReturnsAsync(project);

        WorkItem? created = null;
        _workItemDaoMock
            .Setup(d => d.CreateAsync(It.IsAny<WorkItem>()))
            .Callback<WorkItem>(w => created = w)
            .ReturnsAsync(new WorkItem { Id = 1, Title = "Tarea", ProjectId = 1 });

        await BuildController().Create(1, new WorkItem { Title = "Tarea", ProjectId = 99 });

        Assert.Equal(1, created?.ProjectId);
    }

    // ── Edit ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Edit_RedirectsToProjectDetails_WhenValid()
    {
        var project = new Project { Id = 1, Name = "P1", UserId = "user-1" };
        var existing = new WorkItem { Id = 1, Title = "Original", ProjectId = 1 };
        _projectDaoMock.Setup(d => d.GetByIdAsync(1)).ReturnsAsync(project);
        _workItemDaoMock.Setup(d => d.GetByIdAsync(1)).ReturnsAsync(existing);
        _workItemDaoMock.Setup(d => d.UpdateAsync(It.IsAny<WorkItem>())).ReturnsAsync(existing);

        var result = await BuildController().Edit(1, new WorkItem { Id = 1, Title = "Editado", ProjectId = 1 });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal("Projects", redirect.ControllerName);
    }

    [Fact]
    public async Task Edit_ReturnsNotFound_WhenProjectNotOwned()
    {
        _projectDaoMock.Setup(d => d.GetByIdAsync(1)).ReturnsAsync((Project?)null);

        var result = await BuildController().Edit(1, new WorkItem { Id = 1, Title = "Tarea", ProjectId = 1 });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_ReturnsNotFound_WhenWorkItemNotOwned()
    {
        var project = new Project { Id = 1, Name = "P1", UserId = "user-1" };
        _projectDaoMock.Setup(d => d.GetByIdAsync(1)).ReturnsAsync(project);
        _workItemDaoMock.Setup(d => d.GetByIdAsync(1)).ReturnsAsync((WorkItem?)null);

        var result = await BuildController().Edit(1, new WorkItem { Id = 1, Title = "Tarea", ProjectId = 1 });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_PreservesIsCompleted()
    {
        var project = new Project { Id = 1, Name = "P1", UserId = "user-1" };
        var existing = new WorkItem { Id = 1, Title = "Original", ProjectId = 1, IsCompleted = true };
        _projectDaoMock.Setup(d => d.GetByIdAsync(1)).ReturnsAsync(project);
        _workItemDaoMock.Setup(d => d.GetByIdAsync(1)).ReturnsAsync(existing);

        WorkItem? updated = null;
        _workItemDaoMock
            .Setup(d => d.UpdateAsync(It.IsAny<WorkItem>()))
            .Callback<WorkItem>(w => updated = w)
            .ReturnsAsync(existing);

        await BuildController().Edit(1, new WorkItem { Id = 1, Title = "Editado", ProjectId = 1, IsCompleted = false });

        Assert.True(updated?.IsCompleted);
    }

    // ── ToggleCompleted ──────────────────────────────────────────────────────

    [Fact]
    public async Task ToggleCompleted_RedirectsToProjectDetails_WhenOwned()
    {
        var project = new Project { Id = 1, Name = "P1", UserId = "user-1" };
        var workItem = new WorkItem { Id = 1, Title = "Tarea", ProjectId = 1 };
        _workItemDaoMock.Setup(d => d.GetByIdAsync(1)).ReturnsAsync(workItem);
        _projectDaoMock.Setup(d => d.GetByIdAsync(1)).ReturnsAsync(project);

        var result = await BuildController().ToggleCompleted(1, 1);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        _workItemDaoMock.Verify(d => d.ToggleCompletedAsync(1), Times.Once);
    }

    [Fact]
    public async Task ToggleCompleted_ReturnsNotFound_WhenNotOwned()
    {
        _workItemDaoMock.Setup(d => d.GetByIdAsync(1)).ReturnsAsync((WorkItem?)null);

        var result = await BuildController().ToggleCompleted(1, 1);

        Assert.IsType<NotFoundResult>(result);
        _workItemDaoMock.Verify(d => d.ToggleCompletedAsync(It.IsAny<int>()), Times.Never);
    }

    // ── Delete ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_RedirectsToProjectDetails_WhenOwned()
    {
        var project = new Project { Id = 1, Name = "P1", UserId = "user-1" };
        var workItem = new WorkItem { Id = 1, Title = "Tarea", ProjectId = 1 };
        _workItemDaoMock.Setup(d => d.GetByIdAsync(1)).ReturnsAsync(workItem);
        _projectDaoMock.Setup(d => d.GetByIdAsync(1)).ReturnsAsync(project);

        var result = await BuildController().Delete(1, 1);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        _workItemDaoMock.Verify(d => d.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenNotOwned()
    {
        _workItemDaoMock.Setup(d => d.GetByIdAsync(1)).ReturnsAsync((WorkItem?)null);

        var result = await BuildController().Delete(1, 1);

        Assert.IsType<NotFoundResult>(result);
        _workItemDaoMock.Verify(d => d.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
}