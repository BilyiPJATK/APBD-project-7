namespace Tutorial_07.Entities;

public class ClientTrip
{
    public int IdClient { get; set; }
    public int IdTrip { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }
    public List<Country> Countries { get; set; } = [];
    public int RegisteredAt { get; set; }
    public int? PaymentDate { get; set; }
}