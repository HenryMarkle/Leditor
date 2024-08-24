namespace Leditor.Renderer;

using Leditor.Renderer.Generic;

using Leditor.Data;
using Leditor.Data.Tiles;
using Leditor.Data.Props;
using Leditor.Data.Materials;
using Leditor.Data.Props.Definitions;

using System.Numerics;

public class Context
{
    public delegate void PageChangedEventHandler(int previous, int next);
    public event PageChangedEventHandler? PageChanged;

    private int _page;

    public int PrevPage { get; private set; }

    public int Page
    {
        get => _page;

        set
        {
            PrevPage = _page;
            _page = value;

            PageChanged?.Invoke(PrevPage, Page);
        }
    }
}

public class Registry
{
    public Dex<TileDefinition> Tiles { get; set; }
    public Dex<PropDefinition> Props { get; set; }
    public Dex<MaterialDefinition> Materials { get; set; }
}

public class Folders
{
    public string[] Tiles           { get; init; } = [];
    public string[] Props           { get; init; } = [];
    public string[] Materials       { get; init; } = [];

    public string? LegacyCastMembers { get; init; }

    public string? Executable { get; init; }

    public string? Projects { get; set; }

    public readonly struct CheckResult
    {
        public bool Executable          { get; init; }
        public bool LegacyCastMembers   { get; init; }

        public string[] NotFoundTiles     { get; init; }
        public string[] NotFoundProps     { get; init; }
        public string[] NotFoundMaterials { get; init; }

        public bool Projects { get; init; }
    }

    public CheckResult CheckIntegrity() => new() {
        Executable = Directory.Exists(Executable),
        LegacyCastMembers = Directory.Exists(LegacyCastMembers),

        NotFoundTiles = Tiles.Where(p => !Directory.Exists(p)).ToArray(),
        NotFoundProps = Props.Where(p => !Directory.Exists(p)).ToArray(),
        NotFoundMaterials = Materials.Where(p => !Directory.Exists(p)).ToArray(),

        Projects = File.Exists(Projects)
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