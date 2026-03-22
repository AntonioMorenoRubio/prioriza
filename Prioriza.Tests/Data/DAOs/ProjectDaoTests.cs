using Microsoft.EntityFrameworkCore;
using Prioriza.Web.Data;
using Prioriza.Web.Data.DAOs;
using Prioriza.Web.Data.Entities;

namespace Prioriza.Tests.Data.DAOs;

public class ProjectDaoTests
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetAllByUserAsync_ReturnOnlyUserProjects()
    {
        // Arrange
        await using var context = CreateContext();
        context.Projects.AddRange(
            new Project { Id = 1, Name = "Proyecto A", UserId = "user-1" },
            new Project { Id = 2, Name = "Proyecto B", UserId = "user-1" },
            //Diff. User, shouldn't include
            new Project { Id = 3, Name = "Proyecto C", UserId = "user-2" }
        );
        await context.SaveChangesAsync();
        IProjectDao dao = new ProjectDao(context);

        // Act
        var result = await dao.GetAllByUserAsync("user-1");

        // Assert
        var enumerable = result.ToList();
        Assert.Equal(2, enumerable.Count());
        Assert.All(enumerable, p => Assert.Equal("user-1", p.UserId));
    }

    [Fact]
    public async Task GetAllByUserAsync_IncludesTasks()
    {
        // Arrange
        await using var context = CreateContext();
        var project = new Project { Id = 1, Name = "Con tareas", UserId = "user-1" };
        context.Projects.Add(project);
        context.WorkItems.AddRange(
            new WorkItem { Id = 1, Title = "Tarea 1", ProjectId = 1 },
            new WorkItem { Id = 2, Title = "Tarea 2", ProjectId = 1 }
        );
        await context.SaveChangesAsync();
        var dao = new ProjectDao(context);

        // Act
        var result = await dao.GetAllByUserAsync("user-1");

        // Assert
        Assert.Equal(2, result.First().Tasks.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsProject_WhenExists()
    {
        // Arrange
        await using var context = CreateContext();
        context.Projects.Add(new Project { Id = 1, Name = "Test", UserId = "user-1" });
        await context.SaveChangesAsync();
        var dao = new ProjectDao(context);

        // Act
        var result = await dao.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        await using var context = CreateContext();
        var dao = new ProjectDao(context);

        var result = await dao.GetByIdAsync(99);

        Assert.Null(result);
    }
    
    [Fact]
    public async Task CreateAsync_PersistsProject()
    {
        await using (var context = CreateContext())
        {
            await new ProjectDao(context).CreateAsync(
                new Project { Name = "Nuevo", UserId = "user-1" });
        }

        await using (var context = CreateContext())
        {
            Assert.Equal(1, await context.Projects.CountAsync());
            Assert.Equal("Nuevo", (await context.Projects.FirstAsync()).Name);
        }
    }

    [Fact]
    public async Task UpdateAsync_ChangesName()
    {
        await using (var context = CreateContext())
        {
            context.Projects.Add(new Project
                { Id = 1, Name = "Original", UserId = "user-1" });
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var project = await context.Projects.FindAsync(1);
            project!.Name = "Actualizado";
            await new ProjectDao(context).UpdateAsync(project);
        }

        await using (var context = CreateContext())
        {
            var updated = await context.Projects.FindAsync(1);
            Assert.Equal("Actualizado", updated!.Name);
        }
    }

    [Fact]
    public async Task DeleteAsync_RemovesProject()
    {
        await using (var context = CreateContext())
        {
            context.Projects.Add(new Project
                { Id = 1, Name = "A borrar", UserId = "user-1" });
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            await new ProjectDao(context).DeleteAsync(1);
        }

        await using (var context = CreateContext())
        {
            Assert.Equal(0, await context.Projects.CountAsync());
        }
    }

    [Fact]
    public async Task DeleteAsync_DoesNotThrow_WhenNotExists()
    {
        await using var context = CreateContext();
        var ex = await Record.ExceptionAsync(() => new ProjectDao(context).DeleteAsync(99));
        Assert.Null(ex);
    }
}