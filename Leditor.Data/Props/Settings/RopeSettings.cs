using System.Numerics;

namespace Leditor.Data.Props.Settings;

public enum RopeRelease { Left, Right, None }

public class RopeSettings(
    int renderOrder = 0, 
    int seed = 0, 
    int renderTime = 0, 
    RopeRelease release = RopeRelease.None, 
    float? thickness = null, 
    int? applyColor = null)
    : PropSettings(renderOrder, seed, renderTime)
{
    public RopeRelease Release { get; set; } = release;
    public float? Thickness { get; set; } = thickness;
    public int? ApplyColor { get; set; } = applyColor;

    public Vector2[] Points { get; set; } = [];
    
    //

    public RopeSettings(
        IEnumerable<Vector2> points,
        int renderOrder = 0, 
        int seed = 0, 
        int renderTime = 0, 
        RopeRelease release = RopeRelease.None, 
        float? thickness = null, 
        int? applyColor = null) : this(renderOrder, seed, renderTime, release, thickness, applyColor)
    {
        Points = [..points];
    }
}