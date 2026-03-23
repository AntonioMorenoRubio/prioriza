using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    
    /// <summary>
    /// Porcentaje de tareas completadas sobre el total.
    /// Devuelve 0 si no hay tareas.
    /// Requiere eager loading de Tasks en el DAO.
    /// </summary>
    [NotMapped]
    public int Progreso => Tasks.Any()
        ? (int)(Tasks.Count(t => t.IsCompleted) * 100.0 / Tasks.Count)
        : 0;

    /// <summary>
    /// Número de tareas aún pendientes de completar.
    /// </summary>
    [NotMapped]
    public int TareasPendientes => Tasks.Count(t => !t.IsCompleted);
}