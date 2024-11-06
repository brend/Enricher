namespace Enricher.Models;

using Enricher.Structure;

public class Product
{
    public int Id { get; set; }
    public int Id_Text_Name { get; set; }
    [Enrichable(nameof(Id_Text_Name))]
    public string? Name { get; set; }
}