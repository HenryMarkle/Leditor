using System.Diagnostics;
using static Raylib_CsLo.RayMath;

using System.Numerics;

namespace Leditor;

public class RopeModel
{
    private Prop Rope { get; init; }
    private InitRopeProp Init { get; set; }

    private int _segmentCount = 0;

    private int SegmentCount
    {
        get => _segmentCount;
        set
        {
            _segmentCount = value;
            
            
        }
    }
    private PropRopeSettings Settings { get; init; }

    private Vector2[] _segmentVelocities;
    private Vector2[] _lastPositions;
    private readonly short[] _rigidityArray = [-2, 2, -3, 3, -4, 4];
    private readonly (short, short)[] _pushList =
    [
        (0, 0), (-1, 0), (-1, -1), (0, -1), (1, -1), (1, 0), (1, 1), (0, 1), (1, 1), (-1, 1)
    ];

    public RopeModel(Prop prop, InitRopeProp init, int segmentCount = 15)
    {
        Rope = prop;
        Init = init;
        SegmentCount = segmentCount;
        Settings = (PropRopeSettings)prop.Extras.Settings;

        List<Vector2> velocities = [];
        List<Vector2> lastPositions = [];
        
        foreach (var segment in Rope.Extras.RopePoints)
        {
            velocities.Add(new Vector2(0, 0));
            lastPositions.Add(segment);
        }

        _segmentVelocities = [..velocities];
        _lastPositions = [..lastPositions];
    }
    
    public void Reset(PropQuads quads)
    {
        var (pointA, pointB) = Utils.RopeEnds(quads);
    
        var distance = Vector2Distance(pointA, pointB);
        
        // var segmentCount = distance / Init.SegmentLength;

        if (SegmentCount < 3) SegmentCount = 3;

        var step = distance / Init.SegmentLength;

        List<Vector2> newPoints = [];
        List<Vector2> newLastPositions = [];
        
        for (var i = 0; i < SegmentCount; i++)
        {
            var mv = MoveToPoint(pointA, pointB, (i - 0.5f) * step);
            
            newPoints.Add(pointA + mv); 
            newLastPositions.Add(pointA + mv);
        }

        var newVelocities = new Vector2[newPoints.Count];

        Rope.Extras.RopePoints = [..newPoints];
        _segmentVelocities = newVelocities;
        _lastPositions = [..newLastPositions];
    }

    private Vector2 MoveToPoint(Vector2 pointA, Vector2 pointB, float theMovement)
    {
        var tempB = Vector2Subtract(pointB, pointA);
        var dist = Vector2Distance(new(0, 0), tempB);
        
        Vector2 dirVec;
        
        if (dist > 0)
        {
            dirVec = tempB / dist;
        }
        else
        {
            dirVec = new(0, 1);
        }

        return dirVec * theMovement;
    }

    private float Restrict(float value, float max, float min)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    private float Lerp(float a, float b, float value)
    {
        var restricted = Restrict(value, 0, 1);

        if (b < a)
        {
            (a, b) = (b, a);

            restricted = 1.0f - restricted;
        }

        return Restrict(a + (b-a)*restricted, a, b);
    }

    private void ConnectSegments(int indexA, int indexB)
    {
        var segments = Rope.Extras.RopePoints;

        var a = segments[indexA];
        var b = segments[indexB];

        var dir = MoveToPoint(a, b, 1);
        var dist = Vector2Distance(a, b);

        if (Init.Stiff || dist > Init.SegmentLength)
        {
            var mv = dir * (dist - Init.SegmentLength);

            var halfMv = mv * 0.5f;
            
            segments[indexA] += halfMv;
            _segmentVelocities[indexA] += halfMv;
            segments[indexB] -= halfMv;
            _segmentVelocities[indexB] -= halfMv;
        }
    }

    
    private void ApplyRigidity(int indexA)
    {
        var segments = Rope.Extras.RopePoints;
        
        foreach (var b2 in _rigidityArray)
        {
            var b = indexA + b2;

            if (!(b > -1 && b < segments.Length)) return;
            
            var dir = MoveToPoint(segments[indexA], segments[b], 1);
            var val = dir * Init.Rigid * Init.SegmentLength /
                      (Vector2Distance(segments[indexA], segments[b]) + 0.1f + Math.Abs(b2));
            
            _segmentVelocities[indexA] -= val;
            _segmentVelocities[b] += val;
        }
    }
    private Vector2 SmoothedPos(int indexA, ref PropQuads quads)
    {
        var segments = Rope.Extras.RopePoints;
        var (pA, pB) = Utils.RopeEnds(quads);
        
        if (indexA == 0)
        {
            if (Settings.Release != PropRopeRelease.Left)
            {
                return pA;
            }
            else
            {
                return segments[indexA];
            }
        }
        else if (indexA == segments.Length - 1)
        {
            if (Settings.Release != PropRopeRelease.Right)
            {
                return pB;
            }
            else
            {
                return segments[indexA];
            }
        }
        else
        {
            var sp = (segments[indexA - 1] + segments[indexA + 1]) / 2f;
            return (segments[indexA] + sp) / 2f;
        }
    }

    private int afaMvLvlEdit(int x, int y, int layer)
    {
        // x--;
        // y--;
        // check if in matrix
        if (x < 0 || x >= GLOBALS.Level.Width || y < 0 || y >= GLOBALS.Level.Height) return 0;

        return GLOBALS.Level.GeoMatrix[y, x, layer].Geo;
    }
    
    private int afaMvLvlEdit(Vector2 position, int layer)
    {
        var (x, y) = ((int)position.X, (int)position.Y);
        
        // check if in matrix
        if (x < 0 || x >= GLOBALS.Level.Width || y < 0 || y >= GLOBALS.Level.Height) return 1;

        return GLOBALS.Level.GeoMatrix[y, x, layer].Geo;
    }

    private Vector2 GiveGridPos(Vector2 p)
    {
        return new Vector2((int)(p.X / 16f/* + 4.999f*/), (int)(p.Y / 16f/* + 4.999f*/));
    }

    private struct TmpPoint
    {
        internal Vector2 velocity;
        internal Vector2 loc;
        internal Vector2 lastLoc;
        internal Vector2 frc;
        internal Vector2 sizePnt;
    }
    
    private void SharedCheckVCollision(ref TmpPoint a, float friction, int layer)
    {
        const int bounce = 0;

        if (a.frc.Y > 0)
        {
            var lastGridPos = GiveGridPos(a.lastLoc);
            var feetPos = GiveGridPos(a.loc + new Vector2(0, a.sizePnt.Y+0.01f));
            var lastFeetPos = GiveGridPos(a.lastLoc + new Vector2(0, a.sizePnt.Y));
            var leftPos = GiveGridPos(a.loc + new Vector2(-a.sizePnt.X+1, a.sizePnt.Y+0.01f));
            var rightPos = GiveGridPos(a.loc + new Vector2(+a.sizePnt.X-1, a.sizePnt.Y+0.01f));

            for (var q = lastFeetPos.Y; q < feetPos.Y+1; q++)
            {
                for (var c = leftPos.X; c < rightPos.X+1; c++)
                {
                    if (afaMvLvlEdit((int)c, (int)q, layer) == 1 && afaMvLvlEdit((int)c, (int)q - 1, layer) != 1)
                    {
                        if (lastGridPos.Y >= q && afaMvLvlEdit((int)lastGridPos.X, (int)lastFeetPos.Y, layer) == 1)
                        {
                            
                        }
                        else
                        {
                            a.loc.Y = a.loc.Y - a.velocity.Y*.8f /*(q - 1) * 16f - a.sizePnt.Y*/;
                            a.frc.X *= friction;
                            a.frc.Y = -a.frc.Y * bounce;
                            return;
                        }
                    }
                }
            }
        }
        else if (a.frc.Y < 0)
        {
            var lastGridPos = GiveGridPos(a.lastLoc);
            var headPos = GiveGridPos(a.loc - new Vector2(0, a.sizePnt.Y+0.01f));
            var lastHeadPos = GiveGridPos(a.lastLoc - new Vector2(0, a.sizePnt.Y));
            var leftPos = GiveGridPos(a.loc + new Vector2(-a.sizePnt.X+1, a.sizePnt.Y+0.01f));
            var rightPos = GiveGridPos(a.loc + new Vector2(a.sizePnt.X-1, a.sizePnt.Y+0.01f));

            for (var d = (int)Math.Floor(headPos.Y); d <= (int)lastHeadPos.Y; d++)
            {
                var q = lastHeadPos.Y - d - headPos.Y;

                for (var c = (int)Math.Floor(leftPos.X); c <= rightPos.X; c++)
                {
                    if (afaMvLvlEdit((int)c, (int)q, layer) == 1 && afaMvLvlEdit((int)c, (int)q + 1, layer) != 1)
                    {
                        if (lastGridPos.Y <= q && afaMvLvlEdit((int)lastGridPos.X, (int)lastGridPos.Y, layer) != 1)
                        {
                            
                        }
                        else
                        {
                            a.loc.Y = q * 16 + a.sizePnt.Y;
                            a.frc.X *= friction;
                            a.frc.Y = -a.frc.Y * bounce;
                            return;
                        }
                    }
                }
            }
        }
    }

    private Vector2 GiveTileMiddle(Vector2 pos)
    {
        return new Vector2(pos.X*16f - 8, pos.Y*16f - 8);
    }

    private void PushSegmentOutOfTerrain(int indexA, int layer)
    {
        var segments = Rope.Extras.RopePoints;
        
        var p = new TmpPoint
        {
            velocity = _segmentVelocities[indexA],
            loc = segments[indexA], 
            lastLoc = _lastPositions[indexA], 
            frc = _segmentVelocities[indexA], 
            sizePnt = new Vector2(Init.SegmentRadius, Init.SegmentRadius)
        };

        SharedCheckVCollision(ref p, Init.Friction, layer); //

        segments[indexA] = p.loc;
        _segmentVelocities[indexA] = p.frc;

        var gridPos = GiveGridPos(segments[indexA]);

        foreach (var (x, y) in _pushList)
        {
            if (afaMvLvlEdit(x, y, layer) == 1)
            {
                var midPos = GiveTileMiddle(gridPos + new Vector2(x, y));

                var terrainPosX = Restrict(segments[indexA].X, midPos.X - 10, midPos.X + 10);
                var terrainPosY = Restrict(segments[indexA].Y, midPos.Y - 10, midPos.Y + 10);
                
                var terrainPos = new Vector2(terrainPosX, terrainPosY);
                terrainPos = (terrainPos * 10f + midPos) / 11f;
                var dir = MoveToPoint(segments[indexA], terrainPos, 1);
                var dist = Vector2Distance(segments[indexA], terrainPos);

                if (dist < Init.SegmentRadius)
                {
                    var mov = dir * (dist - Init.SegmentRadius);
                    segments[indexA] += mov;
                    _segmentVelocities[indexA] += mov;
                }
            }
        }
    }

    private bool DiagWI(Vector2 pA, Vector2 pB, float dig)
    {
        var rectHeight = Math.Abs(pA.Y - pB.Y);
        var rectWidth = Math.Abs(pA.X - pB.X);
        return rectHeight*rectHeight + rectWidth * rectWidth < dig * dig;
    }
    
    public void Update(PropQuads quads, int layer)
    {
        var (pA, pB) = Utils.RopeEnds(quads);
        var segments = Rope.Extras.RopePoints;
        
        if (Init.EdgeDirection > 0)
        {
            var dir = MoveToPoint(pA, pB, 1.0f);

            if (Settings.Release != PropRopeRelease.Left)
            {
                for (var i = 0; i < segments.Length / 2; i++)
                {
                    var fac = (float)Math.Pow(1.0f - (i - 1f)/segments.Length/2f, 2);

                    _segmentVelocities[i] += dir * fac * Init.EdgeDirection;
                }

                var idealFirstPos = pA + dir * segments.Length;
                segments[0] = new Vector2(
                    Lerp(segments[0].X, idealFirstPos.X, Init.EdgeDirection),
                    Lerp(segments[0].Y, idealFirstPos.Y, Init.EdgeDirection)
                );
            }

            if (Settings.Release != PropRopeRelease.Right)
            {
                for (var i = 0; i < segments.Length / 2; i++)
                {
                    var fac = (float)Math.Pow(1.0f - (i - 1f)/segments.Length/2, 2);

                    // assumed that velocity array was being accessed from the end
                    var a = segments.Length - 1 - i;
                    _segmentVelocities[a] -= dir * fac * Init.EdgeDirection;
                }

                var idealFirstPos = pB - dir*segments.Length;
                segments[^1] = new Vector2(
                    Lerp(segments[^1].X, idealFirstPos.X, Init.EdgeDirection),
                    Lerp(segments[^1].Y, idealFirstPos.Y, Init.EdgeDirection)
                    );
                
            }

        }
        
        if (Settings.Release != PropRopeRelease.Left)
        {
            segments[0] = pA;
            _segmentVelocities[0] = new Vector2(0, 0);
        }

        if (Settings.Release != PropRopeRelease.Right)
        {
            segments[^1] = pB;
            _segmentVelocities[^1] = new Vector2(0, 0);
        }

        for (var i = 0; i < segments.Length; i++)
        {
            _lastPositions[i] = segments[i];
            segments[i] += _segmentVelocities[i];
            _segmentVelocities[i] *= Init.AirFriction;
            _segmentVelocities[i].Y += Init.Gravity;
        }

        for (var i = 1; i < segments.Length; i++)
        {
            ConnectSegments(i, i-1);

            if (Init.Rigid > 0)
            {
                ApplyRigidity(i);
            }
        }
        
        for (var i = segments.Length - 2; i > -1; i--)
        {
            ConnectSegments(i, i+1);

            if (Init.Rigid > 0)
            {
                ApplyRigidity(i);
            }
        }

        if (Init.SelfPush > 0)
        {
            for (var a = 0; a < segments.Length; a++)
            {
                for (var b = 0; b < segments.Length; b++)
                {
                    if (a != b && DiagWI(segments[a], segments[b], Init.SelfPush))
                    {
                        var dir2 = MoveToPoint(segments[a], segments[b], 1);
                        var dist = Vector2Distance(segments[a], segments[b]);
                        var mov = (dir2 * (dist - Init.SelfPush)) * 0.5f;

                        segments[a] += mov;
                        _segmentVelocities[a] += mov;

                        segments[b] -= mov;
                        _segmentVelocities[b] -= mov;
                    }
                }
            }
        }

        if (Init.SourcePush > 0)
        {
            for (var a = 0; a < segments.Length; a++)
            {
                _segmentVelocities[a] += MoveToPoint(pA, pB, Init.SourcePush) * Restrict((a-1f)/ (segments.Length - 1) -0.7f, 0, 1);
                _segmentVelocities[a] += MoveToPoint(pA, pB, Init.SourcePush) * Restrict((1f - (a - 1f) / (segments.Length - 1)) -0.7f, 0, 1);
            }
        }
        
        for (var i = 0 + (Settings.Release != PropRopeRelease.Left ? 1 : 0); i < segments.Length - (Settings.Release != PropRopeRelease.Right ? 1 : 0); i++)
        {
            PushSegmentOutOfTerrain(i, layer);
        }
    }
}
