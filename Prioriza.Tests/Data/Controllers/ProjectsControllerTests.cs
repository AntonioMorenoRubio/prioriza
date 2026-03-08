using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Prioriza.Web.Controllers;
using Prioriza.Web.Data.DAOs;
using Prioriza.Web.Data.Entities;

namespace Prioriza.Tests.Data.Controllers;

public class ProjectsControllerTests
{
    private readonly Mock<IProjectDao> _daoMock = new();
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;

    public ProjectsControllerTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!, null!, null!, null!, null!
        );
    }

    private ProjectsController BuildController(string userId = "user-1")
    {
        _userManagerMock
            .Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(userId);

        var controller = new ProjectsController(_daoMock.Object, _userManagerMock.Object)
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

    // ── Index ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Index_ReturnsViewWithProjectsOfCurrentUser()
    {
        var projects = new List<Project>
        {
            new() { Id = 1, Name = "P1", UserId = "user-1", Tasks = new List<WorkItem>() },
            new() { Id = 2, Name = "P2", UserId = "user-1", Tasks = new List<WorkItem>()  }
        };
        _daoMock.Setup(d => d.GetAllByUserAsync("user-1")).ReturnsAsync(projects);

        var result = await BuildController("user-1").Index();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(projects, view.Model);
    }

    // ── Details ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Details_ReturnsView_WhenProjectBelongsToUser()
    {
        var project = new Project { Id = 1, Name = "Test", UserId = "user-1", Tasks = new List<WorkItem>() };
        _daoMock.Setup(d => d.GetByIdAsync(1)).ReturnsAsync(project);

        var result = await BuildController("user-1").Details(1);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(project, view.Model);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenProjectIsNull()
    {
        _daoMock.Setup(d => d.GetByIdAsync(99)).ReturnsAsync((Project?)null);

        var result = await BuildController().Details(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenProjectBelongsToAnotherUser()
    {
        var project = new Project { Id = 1, Name = "Ajeno", UserId = "user-2", Tasks = new List<WorkItem>() };
        _daoMock.Setup(d => d.GetByIdAsync(1)).ReturnsAsync(project);

        var result = await BuildController("user-1").Details(1);

        Assert.IsType<NotFoundResult>(result);
    }

    // ── Create ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_Post_RedirectsToIndex_WhenModelValid()
    {
        var project = new Project { Name = "Nuevo", UserId = "", Tasks = new List<WorkItem>() };
        _daoMock.Setup(d => d.CreateAsync(It.IsAny<Project>())).ReturnsAsync(project);

        var result = await BuildController().Create(project);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ProjectsController.Index), redirect.ActionName);
    }

    [Fact]
    public async Task Create_Post_ReturnsView_WhenModelInvalid()
    {
        var controller = BuildController();
        controller.ModelState.AddModelError("Name", "Required");

        var result = await controller.Create(new Project { Id = 1, Name = "Test", UserId = "user-1", Tasks = new List<WorkItem>() });

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Create_Post_AssignsUserIdFromSession()
    {
        Project? created = null;
        _daoMock
            .Setup(d => d.CreateAsync(It.IsAny<Project>()))
            .Callback<Project>(p => created = p)
            .ReturnsAsync(new Project { Id = 1, Name = "Test", UserId = "user-1", Tasks = new List<WorkItem>() });

        await BuildController("user-1").Create(new Project { Id = 1, Name = "Test", UserId = "user-1", Tasks = new List<WorkItem>() });

        Assert.Equal("user-1", created?.UserId);
    }

    // ── Edit ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Edit_Get_ReturnsView_WhenOwner()
    {
        var project = new Project { Id = 1, Name = "Test", UserId = "user-1", Tasks = new List<WorkItem>()  };
        _daoMock.Setup(d => d.GetByIdAsync(1)).ReturnsAsync(project);

        var result = await BuildController("user-1").Edit(1);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(project, view.Model);
    }

    [Fact]
    public async Task Edit_Get_ReturnsNotFound_WhenNotOwner()
    {
        var project = new Project { Id = 1, Name = "Ajeno", UserId = "user-2", Tasks = new List<WorkItem>()  };
        _daoMock.Setup(d => d.GetByIdAsync(1)).ReturnsAsync(project);

        var result = await BuildController("user-1").Edit(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Post_RedirectsToIndex_WhenValid()
    {
        var existing = new Project { Id = 1, Name = "Original", UserId = "user-1", Tasks = new List<WorkItem>()  };
        _daoMock.Setup(d => d.GetByIdAsync(1)).ReturnsAsync(existing);
        _daoMock.Setup(d => d.UpdateAsync(It.IsAny<Project>())).ReturnsAsync(existing);

        var result = await BuildController("user-1")
            .Edit(1, new Project { Id = 1, Name = "Editado", UserId = "user-1", Tasks = new List<WorkItem>() });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ProjectsController.Index), redirect.ActionName);
    }

    [Fact]
    public async Task Edit_Post_ReturnsBadRequest_WhenIdMismatch()
    {
        var result = await BuildController().Edit(1, new Project { Id = 99, Name = "Test", UserId = "user-1", Tasks = new List<WorkItem>() });

        Assert.IsType<BadRequestResult>(result);
    }

    // ── Delete ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteConfirmed_RedirectsToIndex_WhenOwner()
    {
        var project = new Project { Id = 1, Name = "A borrar", UserId = "user-1", Tasks = new List<WorkItem>()  };
        _daoMock.Setup(d => d.GetByIdAsync(1)).ReturnsAsync(project);

        var result = await BuildController("user-1").DeleteConfirmed(1);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ProjectsController.Index), redirect.ActionName);
        _daoMock.Verify(d => d.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteConfirmed_ReturnsNotFound_WhenNotOwner()
    {
        var project = new Project { Id = 1, Name = "Ajeno", UserId = "user-2", Tasks = new List<WorkItem>()  };
        _daoMock.Setup(d => d.GetByIdAsync(1)).ReturnsAsync(project);

        var result = await BuildController("user-1").DeleteConfirmed(1);

        Assert.IsType<NotFoundResult>(result);
        _daoMock.Verify(d => d.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
}