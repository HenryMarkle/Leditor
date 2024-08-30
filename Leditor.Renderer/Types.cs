namespace Leditor.Renderer;

using Leditor.Renderer.Generic;

using Leditor.Data;
using Leditor.Data.Tiles;
using Leditor.Data.Props;
using Leditor.Data.Materials;
using Leditor.Data.Props.Definitions;

using System.Numerics;
using Leditor.Data.Props.Legacy;

public class Registry
{
    public Dex<TileDefinition>? Tiles { get; set; }
    public Dex<InitPropBase>? Props { get; set; }
    public Dex<MaterialDefinition>? Materials { get; set; }

    public CastLibrary[] CastLibraries { get; set; } = [];
}

public class Folders
{
    public string[] Tiles           { get; init; } = [];
    public string[] Props           { get; init; } = [];
    public string[] Materials       { get; init; } = [];

    public string? LegacyCastMembers { get; init; }

    public string? Executable { get; init; }

    public string? Projects { get; set; }

    public string Shaders { get; protected set; } = "";

    protected string _resources = "";
    public required string Resources 
    { 
        get => _resources; 
        set
        {
            Shaders = Path.Combine(_resources, "Shaders");
            _resources = value;
        } 
    }

    public readonly struct CheckResult
    {
        public bool Executable          { get; init; }
        public bool LegacyCastMembers   { get; init; }

        public string[] NotFoundTiles     { get; init; }
        public string[] NotFoundProps     { get; init; }
        public string[] NotFoundMaterials { get; init; }

        public bool Projects { get; init; }

        public bool Resources { get; init; }
        public bool Shaders { get; init; }
    }

    public CheckResult CheckIntegrity() => new() {
        Executable = Directory.Exists(Executable),
        LegacyCastMembers = Directory.Exists(LegacyCastMembers),

        NotFoundTiles = Tiles.Where(p => !Directory.Exists(p)).ToArray(),
        NotFoundProps = Props.Where(p => !Directory.Exists(p)).ToArray(),
        NotFoundMaterials = Materials.Where(p => !Directory.Exists(p)).ToArray(),

        Projects = File.Exists(Projects),
        Resources = Directory.Exists(Resources),
        Shaders = Directory.Exists(Shaders)
    };
}

public class Files
{
    public string? Logs { get; set; }
}

public class Textures
{

}