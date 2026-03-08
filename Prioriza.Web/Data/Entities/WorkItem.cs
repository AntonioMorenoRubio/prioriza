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

    public required string UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;
}