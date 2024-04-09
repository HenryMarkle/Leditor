using System.Numerics;

namespace Leditor.Data;

public readonly record struct Color(byte R = 255, byte G = 255, byte B = 255, byte A = 255)
{
    public Color((byte, byte, byte) tuple) : this(tuple.Item1, tuple.Item2, tuple.Item3) {}
    public Color((byte, byte, byte, byte) tuple) : this(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4) {}

    public Color(Vector4 vector4) 
        : this((byte) vector4.X, (byte) vector4.Y, (byte) vector4.Z, (byte) vector4.W) { }
    
    public Color(Vector3 vector3) 
        : this((byte) vector3.X, (byte) vector3.Y, (byte) vector3.Z) { }
    
    //

    public static implicit operator Raylib_cs.Color(Color c) => new(c.R, c.G, c.B, c.A);
    public static implicit operator Color(Raylib_cs.Color c) => new(c.R, c.G, c.B, c.A);
    
    public static implicit operator Color((byte, byte, byte) tuple) 
        => new(tuple.Item1, tuple.Item2, tuple.Item3);
    
    public static implicit operator Color((byte, byte, byte, byte) tuple) 
        => new(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);

    public static implicit operator Vector4(Color c) => new(c.R, c.G, c.B, c.A);
    public static implicit operator Vector3(Color c) => new(c.R, c.G, c.B);

    public static implicit operator Color(Vector4 v) => new(v);
    public static implicit operator Color(Vector3 v) => new(v);
    
    //

    public void Deconstruct(out byte r, out byte g, out byte b)
    {
        r = R;
        g = G;
        b = B;
    }
    
    public void Deconstruct(out byte r, out byte g, out byte b, out byte a)
    {
        r = R;
        g = G;
        b = B;
        a = A;
    }
}