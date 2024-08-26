namespace Leditor.Data;

using System.Numerics;

public struct CameraQuad(
    (int angle, float radius) topLeft, 
    (int angle, float radius) topRight, 
    (int angle, float radius) bottomRight, 
    (int angle, float radius) bottomLeft
) {
    public (int angle, float radius) TopLeft { get; set; } = topLeft; 
    public (int angle, float radius) TopRight { get; set; } = topRight;
    public (int angle, float radius) BottomRight { get; set; } = bottomRight; 
    public (int angle, float radius) BottomLeft { get; set; } = bottomLeft;
    
    public void Reset()
    {
        TopLeft     = (0, 0);
        TopRight    = (0, 0);
        BottomRight = (0, 0);
        BottomLeft  = (0, 0);
    }
};

public struct RenderCamera {
    public Vector2 Coords { get; set; }
    public CameraQuad Quad { get; set; }
}