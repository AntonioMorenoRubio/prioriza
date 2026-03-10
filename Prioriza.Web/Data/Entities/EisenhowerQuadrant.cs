namespace Prioriza.Web.Data.Entities;

/// <summary>
/// Quadrants from Eisenhower's Matrix.
/// </summary>
public enum EisenhowerQuadrant
{
    DoFirst,    // Important + Urgent
    Schedule,   // Important + Not Urgent
    Delegate,      // Unimportant + Urgent
    Delete      // Unimportant + Not Urgent
}