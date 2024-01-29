using static Raylib_CsLo.RayMath;

namespace Leditor;

public class RopeModel(Prop prop, InitRopeProp init)
{
    private Prop Rope { get; init; }
    private InitRopeProp Init { get; set; }

    public void Reset()
    {
        var quads = Rope.Quads;

        var pointA = Vector2Divide(Vector2Add(quads.TopLeft, quads.BottomLeft), new(2f, 2f));
        var pointB = Vector2Divide(Vector2Add(quads.TopRight, quads.BottomRight), new(2f, 2f));
        
        var distance = Vector2Distance(pointA, pointB);
        
        var segmentCount = distance / Init.SegmentLength;
    }
    
    public void Update() {}
}
