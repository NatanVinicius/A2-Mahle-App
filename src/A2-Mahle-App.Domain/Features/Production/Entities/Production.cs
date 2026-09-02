namespace A2MahleApp.Domain.Features.Production.Entities;

public sealed class Production
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public int Produced { get; set; }

    public int Approved { get; set; }

    public int Rejected { get; set; }
}
