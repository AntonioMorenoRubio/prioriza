using Prioriza.Web.Data.Entities;

namespace Prioriza.Tests.Data.Entities;

public class WorkItemTests
{
    // ── IsUrgent ─────────────────────────────────────────────────────────────

    [Fact]
    public void IsUrgent_IsTrue_WhenDueDateWithinTwoDays()
    {
        var task = new WorkItem
        {
            Title = "Test",
            ProjectId = 1,
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        };
        Assert.True(task.IsUrgent);
    }

    [Fact]
    public void IsUrgent_IsFalse_WhenDueDateBeyondTwoDays()
    {
        var task = new WorkItem
        {
            Title = "Test",
            ProjectId = 1,
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5))
        };
        Assert.False(task.IsUrgent);
    }

    [Fact]
    public void IsUrgent_IsFalse_WhenNoDueDate()
    {
        var task = new WorkItem { Title = "Test", ProjectId = 1 };
        Assert.False(task.IsUrgent);
    }

    // ── IsOneWeekToDueDate ───────────────────────────────────────────────────

    [Fact]
    public void IsOneWeekToDueDate_IsTrue_WhenDueDateWithinSevenDays()
    {
        var task = new WorkItem
        {
            Title = "Test",
            ProjectId = 1,
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5))
        };
        Assert.True(task.IsOneWeekToDueDate);
    }

    [Fact]
    public void IsOneWeekToDueDate_IsFalse_WhenDueDateBeyondSevenDays()
    {
        var task = new WorkItem
        {
            Title = "Test",
            ProjectId = 1,
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(10))
        };
        Assert.False(task.IsOneWeekToDueDate);
    }
    
    // ── EisenhowerQuadrant ───────────────────────────────────────────────────

    [Fact]
    public void EisenhowerQuadrant_IsDoFirst_WhenHighPriorityAndUrgent()
    {
        var task = new WorkItem
        {
            Title = "Test",
            ProjectId = 1,
            Priority = Priority.Alta,
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        };
        Assert.Equal(EisenhowerQuadrant.DoFirst, task.EisenhowerQuadrant);
    }

    [Fact]
    public void EisenhowerQuadrant_IsSchedule_WhenHighPriorityAndNotUrgent()
    {
        var task = new WorkItem
        {
            Title = "Test",
            ProjectId = 1,
            Priority = Priority.Alta,
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(10))
        };
        Assert.Equal(EisenhowerQuadrant.Schedule, task.EisenhowerQuadrant);
    }

    [Fact]
    public void EisenhowerQuadrant_IsDelegate_WhenLowPriorityAndUrgent()
    {
        var task = new WorkItem
        {
            Title = "Test",
            ProjectId = 1,
            Priority = Priority.Baja,
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        };
        Assert.Equal(EisenhowerQuadrant.Delegate, task.EisenhowerQuadrant);
    }

    [Fact]
    public void EisenhowerQuadrant_IsDelete_WhenLowPriorityAndNotUrgent()
    {
        var task = new WorkItem
        {
            Title = "Test",
            ProjectId = 1,
            Priority = Priority.Baja,
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(10))
        };
        Assert.Equal(EisenhowerQuadrant.Delete, task.EisenhowerQuadrant);
    }

    [Fact]
    public void EisenhowerQuadrant_IsDelete_WhenNoPriorityAndNoDueDate()
    {
        var task = new WorkItem { Title = "Test", ProjectId = 1 };
        Assert.Equal(EisenhowerQuadrant.Delete, task.EisenhowerQuadrant);
    }
}