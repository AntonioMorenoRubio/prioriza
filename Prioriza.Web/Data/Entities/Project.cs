using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Prioriza.Web.Data.Entities;

[Table("Projects")]
public class Project
{
    [Key] public int Id { get; set; }
    [MaxLength(100)] public required string Name { get; set; }
    [MaxLength(200)] public string Description { get; set; } = String.Empty;
    [MaxLength(450)] public required string UserId { get; set; }
    public bool IsInbox { get; set; } = false;
    public ApplicationUser User { get; set; } = null!;
    public ICollection<WorkItem> Tasks { get; set; } = new List<WorkItem>();
}