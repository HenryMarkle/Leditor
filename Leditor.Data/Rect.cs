using System.Numerics;

namespace Leditor.Data;

/// <summary>
/// Lingo-style rect data structure
/// </summary>
public struct Rect
{
    public float Left;
    public float Top;
    public float Right;
    public float Bottom;

    public Rect(float left, float top, float right, float bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public Rect(Vector2 topLeft, Vector2 bottomRight)
    {
        Left = topLeft.X;
        Top = topLeft.Y;
        Right = bottomRight.X;
        Bottom = bottomRight.Y;
    }

    public static Rect operator +(Rect r1, int i) => new(r1.Left + i, r1.Top + i, r1.Right + i, r1.Bottom + i);
    public static Rect operator -(Rect r1, int i) => new(r1.Left - i, r1.Top - i, r1.Right - i, r1.Bottom - i);
    public static Rect operator *(Rect r1, int i) => new(r1.Left * i, r1.Top * i, r1.Right * i, r1.Bottom * i);
    public static Rect operator /(Rect r1, int i) => new(r1.Left / i, r1.Top / i, r1.Right / i, r1.Bottom / i);
    
    public static Rect operator +(Rect r1, float i) => new(r1.Left + i, r1.Top + i, r1.Right + i, r1.Bottom + i);
    public static Rect operator -(Rect r1, float i) => new(r1.Left - i, r1.Top - i, r1.Right - i, r1.Bottom - i);
    public static Rect operator *(Rect r1, float i) => new(r1.Left * i, r1.Top * i, r1.Right * i, r1.Bottom * i);
    public static Rect operator /(Rect r1, float i) => new(r1.Left / i, r1.Top / i, r1.Right / i, r1.Bottom / i);
    
    public static Rect operator +(Rect r1, Rect r2) => new(r1.Left + r2.Left, r1.Top + r2.Top, r1.Right + r2.Right, r1.Bottom + r2.Bottom);
    public static Rect operator -(Rect r1, Rect r2) => new(r1.Left - r2.Left, r1.Top - r2.Top, r1.Right - r2.Right, r1.Bottom - r2.Bottom);
    public static Rect operator *(Rect r1, Rect r2) => new(r1.Left * r2.Left, r1.Top * r2.Top, r1.Right * r2.Right, r1.Bottom * r2.Bottom);
    public static Rect operator /(Rect r1, Rect r2) => new(r1.Left / r2.Left, r1.Top / r2.Top, r1.Right / r2.Right, r1.Bottom / r2.Bottom);

    public void Deconstruct(out float left, out float top, out float right, out float bottom)
    {
        left = Left;
        top = Top;
        right = Right;
        bottom = Bottom;
    }
}
