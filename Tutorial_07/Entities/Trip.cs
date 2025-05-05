namespace Tutorial_07.Entities;

public class Trip : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }
    public List<Country> Countries { get; set; } = [];
    public List<Client> Participants { get; set; } = [];
}