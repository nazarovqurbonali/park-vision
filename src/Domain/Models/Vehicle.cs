namespace Domain.Models;

public class Vehicle
{
    public int Id { get; set; }
    public Rect BoundingBox { get; set; }
    public int FrameCount { get; set; }
    public int SaveCount { get; set; }
}