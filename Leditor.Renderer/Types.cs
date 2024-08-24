namespace Leditor.Renderer;

using Leditor.Data.Tiles;
using Leditor.Data.Props;
using Leditor.Data.Materials;

public class Registry
{
    public TileDex Tiles            { get; init; }
    public PropDex Props            { get; init; }
    public MaterialDex Materials    { get; init; }

    public Registry(TileDex tileDex, PropDex propDex, MaterialDex materialDex) {
        Tiles = tileDex;
        Props = propDex;
        Materials = materialDex;
    }
}

public class Folders
{
    public string[] Tiles           { get; init; } = [];
    public string[] Props           { get; init; } = [];
    public string[] Materials       { get; init; } = [];

    public string? LegacyCastMembers { get; init; }

    public string? Executable { get; init; }

    public readonly struct CheckResult
    {
        public bool Executable          { get; init; }
        public bool LegacyCastMembers   { get; init; }

        public string[] NotFoundTiles     { get; init; }
        public string[] NotFoundProps     { get; init; }
        public string[] NotFoundMaterials { get; init; }
    }

    public CheckResult CheckIntegrity() => new() {
        Executable = Directory.Exists(Executable),
        LegacyCastMembers = Directory.Exists(LegacyCastMembers),

        NotFoundTiles = Tiles.Where(p => !Directory.Exists(p)).ToArray(),
        NotFoundProps = Props.Where(p => !Directory.Exists(p)).ToArray(),
        NotFoundMaterials = Materials.Where(p => !Directory.Exists(p)).ToArray(),
    };
}

public class Files
{
    public string? Logs { get; set; }
}

public class Shaders
{

}

public class Textures
{

}