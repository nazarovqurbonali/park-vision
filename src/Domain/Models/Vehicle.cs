namespace Domain.Models;

public class Vehicle
{
    public Guid Id { get; set; }
    public Rect BoundingBox { get; set; }
    public int FrameCount { get; set; }
    public int SaveCount { get; set; }
}