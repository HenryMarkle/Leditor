using static Raylib_cs.Raymath;
using Leditor.Data.Geometry;
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

    public Vector2[] BezierHandles { get; set; }

    public enum EditTypeEnum { Simulation, BezierPaths }
    public EditTypeEnum EditType { get; set; }

    public bool Gravity { get; set; }

    internal void UpdateSegments(Vector2[] segments) {
        var oldLength = Rope.Extras.RopePoints.Length;
        var newLength = segments.Length;

        var deficit = newLength - oldLength;

        if (deficit == 0) return;

        var newVelocities = new Vector2[newLength];
        var newLastPositions = new Vector2[newLength];
        
        if (deficit > 0) {
            for (var i = 0; i < oldLength; i++) {
                newVelocities[i] = _segmentVelocities[i];
                newLastPositions[i] = Rope.Extras.RopePoints[i];
            }

            for (var k = oldLength; k < newLength; k++) {
                newVelocities[k] = new Vector2(0, 0);
                newLastPositions[k] = segments[k];
            }
        } else {
            for (var j = 0; j < newLength; j ++) {
                newVelocities[j] = _segmentVelocities[j];
                newLastPositions[j] = Rope.Extras.RopePoints[j];
            }
        }

        SegmentCount = newLength;

        Rope.Extras.RopePoints = segments;
        _segmentVelocities = newVelocities;
        _lastPositions = newLastPositions;
    }

    private PropRopeSettings Settings { get; init; }

    private Vector2[] _segmentVelocities;
    private Vector2[] _lastPositions;
    private readonly short[] _rigidityArray = [-2, 2, -3, 3, -4, 4];
    private readonly (short, short)[] _pushList =
    [
        (0, 0), (-1, 0), (-1, -1), (0, -1), (1, -1), (1, 0), (1, 1), (0, 1), (1, 1), (-1, 1)
    ];

    public RopeModel(Prop prop, InitRopeProp init)
    {
        Rope = prop;
        Init = init;
        SegmentCount = prop.Extras.RopePoints.Length;
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

        EditType = EditTypeEnum.Simulation;
        BezierHandles = [];
        Gravity = true;
    }

    public void ResetBezierHandles() {
        var quad = Rope.Quad;
        
        BezierHandles = [ Utils.QuadsCenter(ref quad) ];
    }
    
    public void Reset(PropQuad quads)
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

    // ------------------------------------------------------------------------
    //
    //  The folowing code was copied, with permission, from
    //  https://github.com/pkhead/rained/blob/main/src/Rained/RopeModel.cs#L183
    //
    // ------------------------------------------------------------------------

    struct Segment
    {
        public Vector2 pos;
        public Vector2 lastPos;
        public Vector2 vel;
    }

    struct RopePoint
    {
        public Vector2 Loc;
        public Vector2 LastLoc;
        public Vector2 Frc;
        public Vector2 SizePnt;
    }

    private static Vector2 MoveToPoint(Vector2 a, Vector2 b, float t)
    {
        var diff = b - a;
        if (diff.LengthSquared() == 0) return Vector2.UnitY * t;
        return Vector2.Normalize(diff) * t;
    }

    // simplification of a specialized version of MoveToPoint where t = 1
    private static Vector2 Direction(Vector2 from, Vector2 to)
    {
        if (to == from) return Vector2.UnitY; // why is MoveToPoint defined like this??
        return Vector2.Normalize(to - from);
    }

    private static Vector2 GiveGridPos(Vector2 pos)
    {
        /*return new Vector2(
            MathF.Floor((pos.X / 20f) + 0.4999f),
            MathF.Floor((pos.Y / 20f) + 0.4999f)
        );*/
        return new Vector2(
            MathF.Floor(pos.X / 20f + 1f),
            MathF.Floor(pos.Y / 20f + 1f)
        );
    }

    private static Vector2 GiveMiddleOfTile(Vector2 pos)
    {
        return new Vector2(
            (pos.X * 20f) - 10f,
            (pos.Y * 20f) - 10f
        );
    }

    private static float Lerp(float A, float B, float val)
    {
        val = Math.Clamp(val, 0f, 1f);
        if (B < A)
        {
            (B, A) = (A, B);
            val = 1f - val;
        }
        return Math.Clamp(A + (B-A)*val, A, B);
    }

    private static bool DiagWI(Vector2 point1, Vector2 point2, float dig)
    {
        var RectHeight = MathF.Abs(point1.Y - point2.Y);
        var RectWidth = MathF.Abs(point1.X - point2.X);
        return (RectHeight * RectHeight) + (RectWidth * RectWidth) < dig*dig;
    }

    // wtf is this function name?
    private static int AfaMvLvlEdit(Vector2 p, int layer)
    {
        int x = (int)p.X - 1;
        int y = (int)p.Y - 1;
        
        var level = GLOBALS.Level.GeoMatrix;
        
        if (x >= 0 && x < level.GetLength(1) && y >= 0 && y < level.GetLength(0))
            return (int)level[y, x, layer].Type;
        else
            return 1;
    }

    public void Update(PropQuad quad, int layer) 
    {
        var (posA, posB) = Utils.RopeEnds(quad);
        var segments = Rope.Extras.RopePoints;

        if (Init.EdgeDirection > 0f)
        {
            var dir = Direction(posA, posB);
            if (Settings.Release != PropRopeRelease.Left)
            {
                // WARNING - indexing
                for (int A = 0; A <= segments.Length / 2f - 1; A++)
                {
                    var fac = 1f - A / (segments.Length / 2f);
                    fac *= fac;

                    _segmentVelocities[A] += dir*fac*Init.EdgeDirection;
                }

                var idealFirstPos = posA + dir * Init.SegmentLength;
                segments[0] = new Vector2(
                    Lerp(segments[0].X, idealFirstPos.X, Init.EdgeDirection),
                    Lerp(segments[0].Y, idealFirstPos.Y, Init.EdgeDirection)
                );
            }

            if (Settings.Release != PropRopeRelease.Right)
            {
                // WARNING - indexing
                for (int A1 = 0; A1 <= segments.Length / 2f - 1; A1++)
                {
                    var fac = 1f - A1 / (segments.Length / 2f);
                    fac *= fac;
                    var A = segments.Length + 1 - (A1+1) - 1;
                    _segmentVelocities[A] -= dir*fac*Init.EdgeDirection;
                }

                var idealFirstPos = posB - dir * Init.SegmentLength;
                segments[^1] = new Vector2(
                    Lerp(segments[^1].X, idealFirstPos.X, Init.EdgeDirection),
                    Lerp(segments[^1].Y, idealFirstPos.Y, Init.EdgeDirection)
                );
            }
        }

        if (Settings.Release != PropRopeRelease.Left)
        {
            segments[0] = posA;
            _segmentVelocities[0] = Vector2.Zero;
        }

        if (Settings.Release != PropRopeRelease.Right)
        {
            segments[^1] = posB;
            _segmentVelocities[^1] = Vector2.Zero;
        }

        for (int i = 0; i < segments.Length; i++)
        {
            _lastPositions[i] = segments[i];
            segments[i] += _segmentVelocities[i];
            _segmentVelocities[i] *= Init.AirFriction;
            if (Gravity) _segmentVelocities[i].Y += Init.Gravity;
        }

        for (int i = 1; i < segments.Length; i++)
        {
            ConnectRopePoints(i, i-1);
            if (Init.Rigid > 0)
                ApplyRigidity(i);
        }

        for (int i = 2; i <= segments.Length; i++)
        {
            var a = segments.Length - i + 1;
            ConnectRopePoints(a-1, a);
            
            if (Init.Rigid > 0)
                ApplyRigidity(i-1);
        }

        if (Init.SelfPush > 0)
        {
            for (int A = 0; A < segments.Length; A++)
            {
                for (int B = 0; B < segments.Length; B++)
                {
                    if (A != B && DiagWI(segments[A], segments[B], Init.SelfPush))
                    {
                        var dir = Direction(segments[A], segments[B]);
                        var dist = Vector2.Distance(segments[A], segments[B]);
                        var mov = dir * (dist - Init.SelfPush);

                        segments[A] += mov * 0.5f;
                        _segmentVelocities[A] += mov * 0.5f;
                        segments[B] -= mov * 0.5f;
                        _segmentVelocities[B] -= mov * 0.5f;
                    }
                }
            }
        }

        if (Init.SourcePush > 0)
        {
            for (int A = 0; A < segments.Length; A++)
            {
                _segmentVelocities[A] += MoveToPoint(posA, segments[A], Init.SourcePush) * Math.Clamp((A / (segments.Length - 1f)) - 0.7f, 0f, 1f);
                _segmentVelocities[A] += MoveToPoint(posB, segments[A], Init.SourcePush) * Math.Clamp((1f - (A / (segments.Length - 1f))) - 0.7f, 0f, 1f);

            }
        }

        for (int i = 1 + (Settings.Release != PropRopeRelease.Left ? 1:0); i <= segments.Length - (Settings.Release != PropRopeRelease.Right ? 1:0); i++)
        {
            PushRopePointOutOfTerrain(i-1, layer);
        }

        /*
        if(preview)then
            member("ropePreview").image.copyPixels(member("pxl").image,  member("ropePreview").image.rect, rect(0,0,1,1), {#color:color(255, 255, 255)})
            repeat with i = 1 to ropeModel.segments.count then
                adaptedPos = me.SmoothedPos(i)
                adaptedPos = adaptedPos - cameraPos*20.0
                adaptedPos = adaptedPos * previewScale
                member("ropePreview").image.copyPixels(member("pxl").image, rect(adaptedPos-point(1,1), adaptedPos+point(2,2)), rect(0,0,1,1), {#color:color(0, 0, 0)})
            end repeat
        end if
        */
    }

    private void ConnectRopePoints(int A, int B)
    {
        var segments = Rope.Extras.RopePoints;

        var dir = Direction(segments[A], segments[B]);
        var dist = Vector2.Distance(segments[A], segments[B]);

        if (Init.Stiff || dist > Init.SegmentLength)
        {
            var mov = dir * (dist - Init.SegmentLength);

            segments[A] += mov * 0.5f;
            _segmentVelocities[A] += mov * 0.5f;
            segments[B] -= mov * 0.5f;
            _segmentVelocities[B] -= mov * 0.5f;
        }
    }

    private void ApplyRigidity(int A)
    {
        var segments = Rope.Extras.RopePoints;

        void func(int B2)
        {
            var B = A+1 + B2;
            if (B > 0 && B <= segments.Length)
            {
                var dir = Direction(segments[A], segments[B-1]);
                _segmentVelocities[A] -= (dir * Init.Rigid * Init.SegmentLength)
                    / (Vector2.Distance(segments[A], segments[B-1]) + 0.1f + MathF.Abs(B2));
                _segmentVelocities[B-1] += (dir * Init.Rigid * Init.SegmentLength)
                    / (Vector2.Distance(segments[A], segments[B-1]) + 0.1f + MathF.Abs(B2)); 
            }
        };

        func(-2);
        func(2);
        func(-3);
        func(3);
        func(-4);
        func(4);
    }

    private Vector2 SmoothPos(PropQuad quad, int A)
    {
        var (posA, posB) = Utils.RopeEnds(quad);
        var segments = Rope.Extras.RopePoints;

        if (A == 0)
        {
            if (Settings.Release != PropRopeRelease.Left)
                return posA;
            else
                return segments[A];
        }
        else if (A == segments.Length - 1)
        {
            if (Settings.Release != PropRopeRelease.Right)
                return posB;
            else
                return segments[A];
        }
        else
        {
            var smoothpos = (segments[A-1] + segments[A+1]) / 2f;
            return (segments[A] + smoothpos) / 2f;
        }
    }

    // not in the lingo source code
    private Vector2 SmoothPosOld(PropQuad quad, int A)
    {
        var (posA, posB) = Utils.RopeEnds(quad);
        var segments = Rope.Extras.RopePoints;

        if (A == 0)
        {
            if (Settings.Release != PropRopeRelease.Left)
                return posA;
            else
                return _lastPositions[A];
        }
        else if (A == segments.Length - 1)
        {
            if (Settings.Release != PropRopeRelease.Right)
                return posB;
            else
                return _lastPositions[A];
        }
        else
        {
            var smoothpos = (_lastPositions[A-1] + _lastPositions[A+1]) / 2f;
            return (_lastPositions[A] + smoothpos) / 2f;
        }
    }

    private void PushRopePointOutOfTerrain(int A, int layer)
    {
        var segments = Rope.Extras.RopePoints;

        var p = new RopePoint()
        {
            Loc = segments[A],
            LastLoc = _lastPositions[A],
            Frc = _segmentVelocities[A],
            SizePnt = Vector2.One * Init.SegmentRadius
        };

        p = SharedCheckVCollision(p, Init.Friction, layer);
        segments[A] = p.Loc;
        _segmentVelocities[A] = p.Frc;

        var gridPos = GiveGridPos(segments[A]);
        
        loopFunc(new Vector2(0f, 0f));
        loopFunc(new Vector2(-1f, 0f));
        loopFunc(new Vector2(-1f, -1f));
        loopFunc(new Vector2(0f, -1));
        loopFunc(new Vector2(1f, -1));
        loopFunc(new Vector2(1f, 0f));
        loopFunc(new Vector2(1f, 1f));
        loopFunc(new Vector2(0f, 1f));
        loopFunc(new Vector2(-1f, 1f));

        void loopFunc(Vector2 dir)
        {
            if (AfaMvLvlEdit(gridPos+dir, layer) == 1)
            {
                var midPos = GiveMiddleOfTile(gridPos + dir);
                var terrainPos = new Vector2(
                    Math.Clamp(segments[A].X, midPos.X-10f, midPos.X+10f),
                    Math.Clamp(segments[A].Y, midPos.Y-10f, midPos.Y+10f)
                );
                terrainPos = ((terrainPos * 10f) + midPos) / 11f;

                var dir2 = Direction(segments[A], terrainPos);
                var dist = Vector2.Distance(segments[A], terrainPos);
                if (dist < Init.SegmentRadius)
                {
                    var mov = dir2 * (dist-Init.SegmentRadius);
                    segments[A] += mov;
                    _segmentVelocities[A] += mov;
                }
            }
        }
    }

    private RopePoint SharedCheckVCollision(RopePoint p, float friction, int layer)
    {
        var bounce = 0f;

        if (p.Frc.Y > 0f)
        {
            var lastGridPos = GiveGridPos(p.LastLoc);
            var feetPos = GiveGridPos(p.Loc + new Vector2(0f, p.SizePnt.Y + 0.01f));
            var lastFeetPos = GiveGridPos(p.LastLoc + new Vector2(0f, p.SizePnt.Y));
            var leftPos = GiveGridPos(p.Loc + new Vector2(-p.SizePnt.X + 1f, p.SizePnt.Y + 0.01f));
            var rightPos = GiveGridPos(p.Loc + new Vector2(p.SizePnt.X - 1f, p.SizePnt.Y + 0.01f));

            // WARNING - idk if lingo calculate the loop direction
            for (int q = (int)lastFeetPos.Y; q <= feetPos.Y; q++)
            {
                for (int c = (int)leftPos.X; c <= rightPos.X; c++)
                {
                    if (AfaMvLvlEdit(new(c, q), layer) == 1 && AfaMvLvlEdit(new Vector2(c, q-1f), layer) != 1)
                    {
                        if (lastGridPos.Y >= q && AfaMvLvlEdit(lastGridPos, layer) == 1)
                        {}
                        else
                        {
                            p.Loc.Y = ((q-1f)*20f) - p.SizePnt.Y;
                            p.Frc.X *= friction;
                            p.Frc.Y = -p.Frc.Y * bounce;
                            return p;
                        }
                    }
                }
            }
        }
        else if (p.Frc.Y < 0f)
        {
            var lastGridPos = GiveGridPos(p.LastLoc);
            var headPos = GiveGridPos(p.Loc - new Vector2(0f, p.SizePnt.Y + 0.01f));
            var lastHeadPos = GiveGridPos(p.LastLoc - new Vector2(0, p.SizePnt.Y));
            var leftPos = GiveGridPos(p.Loc + new Vector2(-p.SizePnt.X + 1f, p.SizePnt.Y + 0.01f));
            var rightPos = GiveGridPos(p.Loc + new Vector2(p.SizePnt.X - 1f, p.SizePnt.Y + 0.01f));

            // WARNING - idk if lingo calculates the loop direction
            for (int d = (int)headPos.Y; d <= lastHeadPos.Y; d++)
            {
                var q = lastHeadPos.Y - (d-headPos.Y);
                for (int c = (int)leftPos.X; c <= rightPos.X; c++)
                {
                    if (AfaMvLvlEdit(new(c, q), layer) == 1 && AfaMvLvlEdit(new(c, q+1f), layer) != 1)
                    {
                        if (lastGridPos.Y <= q && AfaMvLvlEdit(lastGridPos, layer) != 1)
                        {}
                        else
                        {
                            p.Loc.Y = (q*20f)+p.SizePnt.Y;
                            p.Frc.X *= friction;
                            p.Frc.Y = -p.Frc.Y * bounce;
                            return p;
                        }
                    }
                }
            }
        }

        return p;
    }
}
