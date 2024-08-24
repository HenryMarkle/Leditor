namespace Leditor.Renderer;

using Serilog;

using Raylib_cs;
using static Raylib_cs.Raylib;
using Leditor.Data.Tiles;

public class Program
{
    public static bool FileExistsInFolder(string folder, string file)
    {
        if (Environment.OSVersion.Platform == PlatformID.Unix)
        {
            var foundPath = Directory
                .GetFiles(folder)
                .FirstOrDefault(f => f.Equals($"{file}.png", StringComparison.OrdinalIgnoreCase));
        
            return foundPath is not null;
        }
        
        return File.Exists(Path.Combine(folder, file));
    }

    public static string? GetFilePathInFolder(string folder, string file)
    {
        if (Environment.OSVersion.Platform == PlatformID.Unix)
        {
            var foundPath = Directory
                .GetFiles(folder)
                .FirstOrDefault(f => f.Equals($"{file}.png", StringComparison.OrdinalIgnoreCase));
        
            return foundPath;
        }
        
        return Path.Combine(folder, file);
    }
    private static string GetEmbeddedResource(string resourceName)
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        using Stream stream = assembly.GetManifestResourceStream(resourceName)!;
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    public static void Main(string[] args)
    {
        #region Path Pointers
        
        var executableDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) 
            ?? throw new Exception("unable to retreive current executable's path");

        string dataDir;
        
        #if DEBUG
        dataDir = Path.Combine(executableDir, "..", "..", "..", "..", "Data");
        #else
        dataDir = Path.Combine(executableDir, "Data");
        #endif

        var folders = new Folders
        {
            Executable = executableDir,

            LegacyCastMembers = Path.Combine(dataDir, "Cast"),

            Tiles = [ Path.Combine(dataDir, "Graphics") ],
            Props = [ Path.Combine(dataDir, "Props") ],
            Materials = [ Path.Combine(dataDir, "Materials") ]
        };

        var files = new Files {
            Logs = Path.Combine(executableDir, "logs", "logs.log")
        };
        
        #endregion

        using var logger = new LoggerConfiguration()
            .WriteTo.Map(
                _ => Environment.CurrentManagedThreadId,
                (id, wt) => wt.File(
                    Path.Combine(executableDir, "logs", $"logs-{id}.log"),
                    fileSizeLimitBytes: 50000000,
                    rollOnFileSizeLimit: true
                )
            )
            .WriteTo.Console()
            .CreateLogger();


        logger.Information("Initializing renderer");

        #region Folder Check

        logger.Information("Checking paths");

        var foldersCheckResult = folders.CheckIntegrity();

        if (!foldersCheckResult.Executable) logger.Fatal("Executable's path not found");
        if (!foldersCheckResult.LegacyCastMembers) logger.Fatal("Cast folder not found");
        
        foreach (var folder in foldersCheckResult.NotFoundTiles)
            logger.Error("Tiles folder not found: \"{folder}\"", folder);

        foreach (var folder in foldersCheckResult.NotFoundProps)
            logger.Error("Props folder not found: \"{folder}\"", folder);

        foreach (var folder in foldersCheckResult.NotFoundMaterials)
            logger.Error("Materials folder not found: \"{folder}\"", folder);

        #endregion

        logger.Information("Loading tiles");

        TileDefinition[] tiles = [];

        var tileLoader = new TileLoader(folders, logger);
        var propLoader = new TileLoader(folders, logger);
        var materialLoader = new TileLoader(folders, logger);

        var tileTextureLoadComplete = false;
        var propTextureLoadComplete = false;
        var materialTextureLoadComplete = false;
        
        SetTargetFPS(60);
        
        //---------------------------------------------------------
        InitWindow(2000 * 2/3, 1200 * 2/3, "Henry's Renderer");
        //---------------------------------------------------------

        while (!WindowShouldClose())
        {
            BeginDrawing();
            {
                ClearBackground(Color.Gray);

                if (!tileTextureLoadComplete)
                {
                    if (tileLoader.IsReady && tileLoader.LoadNext())
                    {
                        tiles = tileLoader.GetTiles();
                        tileTextureLoadComplete = true;
                    }

                    EndDrawing();
                    continue;
                }

                DrawText("Load Complete", 10, 10, 40, Color.White);
            }
            EndDrawing();
        }

        CloseWindow();

        foreach (var tile in tiles) UnloadTexture(tile.Texture);

        logger.Information("Program terminated");
    }
}