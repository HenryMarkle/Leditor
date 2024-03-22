using Leditor.Data.Props.Settings;
using Raylib_cs;

namespace Leditor.Data.Props.Definitions;

public sealed class Rope(
    string name,
    int depth,
    int segmentLength,
    int collisionDepth,
    float segmentRadius,
    float gravity,
    float friction,
    float airFriction,
    bool stiff,
    float edgeDirection,
    float rigid,
    float selfPush,
    float sourcePush,
    Texture2D texture
) : PropDefinition(name, depth, texture)
{
    public override PropSettings NewSettings(int renderOrder = 0, int seed = 0, int renderTime = 0)
        => new RopeSettings(renderOrder, seed, renderTime);
    
    //

    public int SegmentLength { get; } = segmentLength;
    public int CollisionDepth { get; } = collisionDepth;
    public float SegmentRadius { get; } = segmentRadius;
    public float Gravity { get; } = gravity;
    public float Friction { get; } = friction;
    public float AirFriction { get; } = airFriction;
    public bool Stiff { get; } = stiff;
    public float EdgeDirection { get; } = edgeDirection;
    public float Rigid { get; } = rigid;
    public float SelfPush { get; } = selfPush;
    public float SourcePush { get; } = sourcePush;
}