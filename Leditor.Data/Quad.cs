﻿using System.Numerics;

namespace Leditor.Data;

public struct Quad
{
    public Vector2 TopLeft { get; set; }
    public Vector2 TopRight { get; set; }
    public Vector2 BottomRight { get; set; }
    public Vector2 BottomLeft { get; set; }

    public Quad()
    {
        TopLeft = new Vector2(0, 0);
        TopRight = new Vector2(0, 0);
        BottomRight = new Vector2(0, 0);
        BottomLeft = new Vector2(0, 0);
    }

    public Quad(Vector2 topLeft, Vector2 topRight, Vector2 bottomRight, Vector2 bottomLeft)
    {
        TopLeft = topLeft;
        TopRight = topRight;
        BottomRight = bottomRight;
        BottomLeft = bottomLeft;
    }

    public static implicit operator Vector2[](Quad q) => [ q.TopLeft, q.TopRight, q.BottomRight, q.BottomLeft ];
    public static implicit operator Quad(Vector2[] v) => new(v[0], v[1], v[2], v[3]);

    public static Quad operator +(Quad q, Vector2 v) => new(q.TopLeft + v, q.TopRight + v, q.BottomRight + v, q.BottomLeft + v);
    public static Quad operator -(Quad q, Vector2 v) => new(q.TopLeft - v, q.TopRight - v, q.BottomRight - v, q.BottomLeft - v);
    public static Quad operator *(Quad q, int number) => new(q.TopLeft * number, q.TopRight * number, q.BottomRight * number, q.BottomLeft * number);
    public static Quad operator /(Quad q, int number) => new(q.TopLeft / number, q.TopRight / number, q.BottomRight / number, q.BottomLeft / number);

    public readonly void Deconstruct(
        out Vector2 topLeft, 
        out Vector2 topRight, 
        out Vector2 bottomRight, 
        out Vector2 bottomLeft
    ) {
        topLeft = TopLeft;
        topRight = TopRight;
        bottomRight = BottomRight;
        bottomLeft = BottomLeft;
    }

    public readonly Vector2 Center()
    {
        var nearestX = Math.Min(Math.Min(TopLeft.X, TopRight.X), Math.Min(BottomLeft.X, BottomRight.X));
        var nearestY = Math.Min(Math.Min(TopLeft.Y, TopRight.Y), Math.Min(BottomLeft.Y, BottomRight.Y));

        var furthestX = Math.Max(Math.Max(TopLeft.X, TopRight.X), Math.Max(BottomLeft.X, BottomRight.X));
        var furthestY = Math.Max(Math.Max(TopLeft.Y, TopRight.Y), Math.Max(BottomLeft.Y, BottomRight.Y));

        return new Vector2(nearestX + (furthestX - nearestX)/2, nearestY + (furthestY - nearestY)/2);
    }

    /// <summary>
    /// Rotate points around a center
    /// </summary>
    /// <param name="angle">The angle in degrees</param>
    /// <param name="center">The center point</param>
    /// <returns>A new quad with the rotated points</returns>
    public readonly Quad Rotate(float angle, Vector2 center)
    {
        var radian = float.DegreesToRadians(angle);
        
        var sinRotation = (float)Math.Sin(radian);
        var cosRotation = (float)Math.Cos(radian);
        
        Vector2 newTopLeft, newTopRight, newBottomRight, newBottomLeft;

        { // Rotate the top left corner
            var x = TopLeft.X;
            var y = TopLeft.Y;

            var dx = x - center.X;
            var dy = y - center.Y;

            newTopLeft = new Vector2(
                center.X + dx * cosRotation - dy * sinRotation, 
                center.Y + dx * sinRotation + dy * cosRotation
            );
        }
        
        { // Rotate the top right corner
            var x = TopRight.X;
            var y = TopRight.Y;

            var dx = x - center.X;
            var dy = y - center.Y;

            newTopRight = new Vector2(center.X + dx * cosRotation - dy * sinRotation, center.Y + dx * sinRotation + dy * cosRotation);
        }
        
        { // Rotate the bottom right corner
            var x = BottomRight.X;
            var y = BottomRight.Y;

            var dx = x - center.X;
            var dy = y - center.Y;

            newBottomRight = new Vector2(center.X + dx * cosRotation - dy * sinRotation, center.Y + dx * sinRotation + dy * cosRotation);
        }
        
        { // Rotate the bottom left corner
            var x = BottomLeft.X;
            var y = BottomLeft.Y;

            var dx = x - center.X;
            var dy = y - center.Y;

            newBottomLeft = new Vector2(center.X + dx * cosRotation - dy * sinRotation, center.Y + dx * sinRotation + dy * cosRotation);
        }

        return new Quad(newTopLeft, newTopRight, newBottomRight, newBottomLeft);
    }

    /// <summary>
    /// Rotate points around their center
    /// </summary>
    /// <param name="angle">The angle in degrees</param>
    /// <returns>A new quad with the rotated points</returns>
    public readonly Quad Rotate(float angle) => Rotate(angle, Center());
}