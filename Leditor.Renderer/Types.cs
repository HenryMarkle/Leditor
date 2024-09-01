namespace Leditor.Renderer;

using Leditor.Renderer.Generic;

using Leditor.Data;
using Leditor.Data.Tiles;
using Leditor.Data.Props;
using Leditor.Data.Materials;
using Leditor.Data.Props.Definitions;

using System.Numerics;
using Leditor.Data.Props.Legacy;
using Raylib_cs;

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

    public string Textures { get; protected set; } = "";
    public string Shaders { get; protected set; } = "";

    protected string _resources = "";
    public required string Resources 
    { 
        get => _resources; 
        set
        {
            _resources = value;

            Shaders = Path.Combine(_resources, "Shaders");
            Textures = Path.Combine(_resources, "Textures");
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
        public bool Textures { get; init; }
        public bool Shaders { get; init; }

        public override string ToString()
        {
            var tiles = NotFoundTiles.Length > 0 
                ? "\n\t" + string.Join("\n\t", NotFoundTiles) 
                : "OK";
            
            var props = NotFoundProps.Length > 0 
                ? "\n\t" + string.Join("\n\t", NotFoundProps) 
                : "OK";
            
            var mater = NotFoundMaterials.Length > 0 
                ? "\n\t" + string.Join("\n\t", NotFoundMaterials) 
                : "OK";

            static string IsOk(bool v) => v ? "OK" : "MISSING";

            return $"Executable: {IsOk(Executable)}\n"
            + $"Cast: {IsOk(LegacyCastMembers)}\n"
            + $"Tile: {tiles}\n"
            + $"Prop: {props}\n"
            + $"Material: {mater}\n"
            + $"Projects: {IsOk(Projects)}\n"
            + $"Resources: {IsOk(Resources)}\n"
            + $"Textures: {IsOk(Textures)}\n"
            + $"Shaders: {IsOk(Shaders)}";
        }
    }

    public CheckResult CheckIntegrity() => new() {
        Executable = Directory.Exists(Executable),
        LegacyCastMembers = Directory.Exists(LegacyCastMembers),

        NotFoundTiles = Tiles.Where(p => !Directory.Exists(p)).ToArray(),
        NotFoundProps = Props.Where(p => !Directory.Exists(p)).ToArray(),
        NotFoundMaterials = Materials.Where(p => !Directory.Exists(p)).ToArray(),

        Projects = Directory.Exists(Projects),
        Resources = Directory.Exists(Resources),
        Textures = Directory.Exists(Textures),
        Shaders = Directory.Exists(Shaders)
    };
}

public class Files
{
    public string? Logs { get; set; }
}

public class Textures : IDisposable
{
    public bool Disposed { get; protected set; }
    public bool Initialized { get; protected set;}

    public Texture2D Folder { get; protected set; }
    public Texture2D File { get; protected set; }

    public void Inititialize(string assetsFolder)
    {
        if (Initialized) return;

        Folder = Raylib.LoadTexture(Path.Combine(assetsFolder, "folder icon.png"));
        File = Raylib.LoadTexture(Path.Combine(assetsFolder, "file icon.png"));

        Initialized = true;
    }

    public void Dispose()
    {
        if (Disposed || !Initialized) return;
        Disposed = true;

        Raylib.UnloadTexture(Folder);
        Raylib.UnloadTexture(File);
    }
}