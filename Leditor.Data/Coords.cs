using System.Numerics;

namespace Leditor.Data;

public readonly record struct Coords(int X, int Y, int Z)
{
    public static implicit operator Coords((int x, int y, int z) tuple) => new(tuple.x, tuple.y, tuple.z);
    public static implicit operator (int, int, int)(Coords c) => (c.X, c.Y, c.Z);

    public static implicit operator Vector3(Coords c) => new(c.X, c.Y, c.Z);
    public static implicit operator Coords(Vector3 v) => ((int)v.X, (int)v.Y, (int)v.Z);

    public static Coords operator +(Coords c1, Coords c2) => new(c1.X + c2.X, c1.Y + c2.Y, c1.Z + c2.Z);
    public static Coords operator -(Coords c1, Coords c2) => new(c1.X - c2.X, c1.Y - c2.Y, c1.Z - c2.Z);
    public static Coords operator *(Coords c1, Coords c2) => new(c1.X * c2.X, c1.Y * c2.Y, c1.Z * c2.Z);
    public static Coords operator /(Coords c1, Coords c2) => new(c1.X / c2.X, c1.Y / c2.Y, c1.Z / c2.Z);

    public static Coords operator *(Coords c1, int i) => new(c1.X * i, c1.Y * i, c1.Z * i);
    public static Coords operator /(Coords c1, int i) => new(c1.X / i, c1.Y / i, c1.Z / i);

    public void Deconstruct(out int x, out int y, out int z)
    {
        x = X;
        y = Y;
        z = Z;
    }
        
    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }
}