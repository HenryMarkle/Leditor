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
    public string[] Tiles           { get; init; }
    public string[] Props           { get; init; }
    public string[] Materials       { get; init; }

    public string LegacyCastMembers { get; init; }

    public string Executable { get; init; }
}

public class Files
{
    public string Logs { get; set; }
}

public class Shaders
{

}

public class Textures
{

}