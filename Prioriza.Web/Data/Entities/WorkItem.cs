using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Prioriza.Web.Data.Entities;

[Table("WorkItems")]
public class WorkItem
{
    [Key] 
    public int Id { get; set; }

    [MaxLength(200)] 
    public required string Title { get; set; }
    [MaxLength(200)] 
    public string Description { get; set; } = string.Empty;

    public bool IsCompleted { get; set; } = false;

    public int ProjectId { get; set; }

    public Project Project { get; set; } = null!;
    public Priority? Priority { get; set; }
    public DateOnly? DueDate { get; set; }

    [NotMapped]
    public bool IsUrgent => DueDate.HasValue && DueDate.Value <= DateOnly.FromDateTime(DateTime.Today.AddDays(2));

    [NotMapped]
    public bool IsOneWeekToDueDate => DueDate.HasValue && DueDate.Value <= DateOnly.FromDateTime(DateTime.Today.AddDays(7));
    /// <summary>
    /// Quadrants from Eisenhower's Matrix.
    /// Requires Priority and DueDate derived operations.
    /// </summary>
    [NotMapped]
    public EisenhowerQuadrant EisenhowerQuadrant => (Priority == Entities.Priority.Alta, IsUrgent) switch
    {
        (true, true) => EisenhowerQuadrant.DoFirst,
        (true, false) => EisenhowerQuadrant.Schedule,
        (false, true) => EisenhowerQuadrant.Delegate,
        (false, false) => EisenhowerQuadrant.Delete
    };
}