namespace AquaDex.Core.DTOs;

public class CreateBiteAlertDto
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int? SpeciesId { get; set; }
    public string? Message { get; set; }
}