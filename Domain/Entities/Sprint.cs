using Domain.Enums;

namespace Domain.Models;

public class Sprint : Entity
{
    public string Name { get; private set; } = null!;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public SprintStatus Status { get; private set; }
    public string ProjectId { get; private set; } = null!;

    public Sprint() { }

    public Sprint(string id, string projectId, string name, DateTime start, DateTime end) : base(id)
    {
        ProjectId = projectId;
        Name = name;
        StartDate = start;
        EndDate = end;
        Status = SprintStatus.Planned;
    }

    public void Start()
    {
        if (Status != SprintStatus.Planned) return;
        Status = SprintStatus.Active;
    }

    public void Complete()
    {
        if (Status != SprintStatus.Active) return;
        Status = SprintStatus.Completed;
    }
}
