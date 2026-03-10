using Microsoft.EntityFrameworkCore;
using Prioriza.Web.Data;
using Prioriza.Web.Data.DAOs;
using Prioriza.Web.Data.Entities;

namespace Prioriza.Tests.Data.DAOs;

public class WorkItemDaoTests
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    private ApplicationDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_dbName).Options);

    [Fact]
    public async Task CreateAsync_PersistsWorkItem()
    {
        var workItem = new WorkItem { Title = "Tarea 1", ProjectId = 1 };

        await using (var context = CreateContext())
        {
            var dao = new WorkItemDao(context);
            await dao.CreateAsync(workItem);
        }

        await using (var context = CreateContext())
        {
            var saved = await context.WorkItems.FirstOrDefaultAsync(w => w.Title == "Tarea 1");
            Assert.NotNull(saved);
            Assert.Equal(1, saved.ProjectId);
        }
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsWorkItem_WhenExists()
    {
        await using (var context = CreateContext())
        {
            context.WorkItems.Add(new WorkItem { Id = 1, Title = "Tarea 1", ProjectId = 1 });
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var dao = new WorkItemDao(context);
            var result = await dao.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal("Tarea 1", result.Title);
        }
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        await using var context = CreateContext();
        var dao = new WorkItemDao(context);
        var result = await dao.GetByIdAsync(99);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllByProjectAsync_ReturnsOnlyProjectWorkItems()
    {
        await using (var context = CreateContext())
        {
            context.WorkItems.AddRange(
                new WorkItem { Title = "Tarea A", ProjectId = 1 },
                new WorkItem { Title = "Tarea B", ProjectId = 1 },
                new WorkItem { Title = "Tarea C", ProjectId = 2 }
            );
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var dao = new WorkItemDao(context);
            var result = (await dao.GetAllByProjectAsync(1)).ToList();
            Assert.Equal(2, result.Count);
            Assert.All(result, w => Assert.Equal(1, w.ProjectId));
        }
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        await using (var context = CreateContext())
        {
            context.WorkItems.Add(new WorkItem { Id = 1, Title = "Original", ProjectId = 1 });
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var dao = new WorkItemDao(context);
            var workItem = await dao.GetByIdAsync(1);
            workItem!.Title = "Actualizado";
            await dao.UpdateAsync(workItem);
        }

        await using (var context = CreateContext())
        {
            var updated = await context.WorkItems.FindAsync(1);
            Assert.Equal("Actualizado", updated!.Title);
        }
    }

    [Fact]
    public async Task ToggleCompletedAsync_TogglesFromFalseToTrue()
    {
        await using (var context = CreateContext())
        {
            context.WorkItems.Add(new WorkItem { Id = 1, Title = "Tarea", ProjectId = 1, IsCompleted = false });
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var dao = new WorkItemDao(context);
            await dao.ToggleCompletedAsync(1);
        }

        await using (var context = CreateContext())
        {
            var result = await context.WorkItems.FindAsync(1);
            Assert.True(result!.IsCompleted);
        }
    }

    [Fact]
    public async Task ToggleCompletedAsync_TogglesFromTrueToFalse()
    {
        await using (var context = CreateContext())
        {
            context.WorkItems.Add(new WorkItem { Id = 1, Title = "Tarea", ProjectId = 1, IsCompleted = true });
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var dao = new WorkItemDao(context);
            await dao.ToggleCompletedAsync(1);
        }

        await using (var context = CreateContext())
        {
            var result = await context.WorkItems.FindAsync(1);
            Assert.False(result!.IsCompleted);
        }
    }

    [Fact]
    public async Task ToggleCompletedAsync_DoesNothing_WhenNotExists()
    {
        await using var context = CreateContext();
        var dao = new WorkItemDao(context);
        var ex = await Record.ExceptionAsync(() => dao.ToggleCompletedAsync(99));
        Assert.Null(ex);
    }

    [Fact]
    public async Task DeleteAsync_RemovesWorkItem()
    {
        await using (var context = CreateContext())
        {
            context.WorkItems.Add(new WorkItem { Id = 1, Title = "Tarea", ProjectId = 1 });
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var dao = new WorkItemDao(context);
            await dao.DeleteAsync(1);
        }

        await using (var context = CreateContext())
        {
            var result = await context.WorkItems.FindAsync(1);
            Assert.Null(result);
        }
    }

    [Fact]
    public async Task DeleteAsync_DoesNothing_WhenNotExists()
    {
        await using var context = CreateContext();
        var dao = new WorkItemDao(context);
        var ex = await Record.ExceptionAsync(() => dao.DeleteAsync(99));
        Assert.Null(ex);
    }
}