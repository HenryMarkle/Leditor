namespace Leditor.Renderer;

using Serilog;

using ImGuiNET;
using rlImGui_cs;

using Raylib_cs;
using static Raylib_cs.Raylib;

using Leditor.Data.Tiles;
using Leditor.Data.Props.Definitions;
using Leditor.Serialization;

using Leditor.Renderer.RL;
using Leditor.Renderer.Generic;
using Leditor.Renderer.Pages;

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
            Materials = [ Path.Combine(dataDir, "Materials") ],

            Projects = Path.Combine(dataDir, "LevelEditorProjects")
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
        if (!foldersCheckResult.Projects)
        {
            #if DEBUG
            try
            {
                Directory.CreateDirectory(folders.Projects);
            }
            catch (Exception e)
            {
                logger.Fatal(e, $"Failed to create prjects folder \"{folders.Projects}\"");
                throw new Exception("Failed to create projects folder", e);
            }
            #else
            Directory.CreateDirectory(folders.Projects);
            #endif
        }
        
        foreach (var folder in foldersCheckResult.NotFoundTiles)
            logger.Error("Tiles folder not found: \"{folder}\"", folder);

        foreach (var folder in foldersCheckResult.NotFoundProps)
            logger.Error("Props folder not found: \"{folder}\"", folder);

        foreach (var folder in foldersCheckResult.NotFoundMaterials)
            logger.Error("Materials folder not found: \"{folder}\"", folder);

        #endregion

        logger.Information("Loading tiles");

        TileDefinition[] tiles = [];
        PropDefinition[] props = [];

        var tileLoader = new DefinitionLoader<TileDefinition>(folders.Tiles, TileImporter.ParseInitAsync_NoCategories, logger);
        var propLoader = new DefinitionLoader<PropDefinition>(folders.Props, PropImporter.ParseInitAsync_NoCategories, logger);

        var tileTextureLoadComplete = false;
        var propTextureLoadComplete = false;
        var materialTextureLoadComplete = false;

        #region Init Pages

        int page = 0;

        Context context = new() { Page = page };

        context.PageChanged += (_, next) => { page = next; };



        MainPage mainPage = new()
        {
            ProjectsFolder = folders.Projects,
            Logger = logger,
            Context = context
        };

        #endregion
        
        SetTargetFPS(60);

        #if RELEASE
        SetTraceLogLevel(TraceLogLevel.Error);
        #endif

        //---------------------------------------------------------
        InitWindow(2000 * 2/3, 1200 * 2/3, "Henry's Renderer");
        //---------------------------------------------------------

        rlImGui.Setup(true, true);
        
        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        ImGui.GetIO().ConfigDockingWithShift = true;
        ImGui.GetIO().ConfigDockingTransparentPayload = true;

        long frame = 0;

        while (!WindowShouldClose())
        {
            BeginDrawing();
            {
                #region Loading

                if (!tileTextureLoadComplete)
                {
                    if (tileLoader.IsReady && tileLoader.LoadNext())
                    {
                        tiles = tileLoader.GetResult();
                        tileTextureLoadComplete = true;
                    }
                }

                if (!propTextureLoadComplete)
                {
                    if (propLoader.IsReady && propLoader.LoadNext())
                    {
                        props = propLoader.GetResult();
                        propTextureLoadComplete = true;
                    }

                    Draw.LoadingScreen(
                        (float)tileLoader.Progress / tileLoader.TotalProgress, 
                        (float)propLoader.Progress / propLoader.TotalProgress, 
                        0
                    );

                    EndDrawing();
                    continue;
                }

                #endregion

                Draw.MainScreen();
            }
            EndDrawing();

            unchecked
            {
                frame++;
            }
        }

        #region Cleanup

        logger.Information("Unloading resources");

        foreach (var tile in tiles) UnloadTexture(tile.Texture);
        foreach (var prop in props) UnloadTexture(prop.Texture);
        
        #endregion

        rlImGui.Shutdown();
        CloseWindow();


        logger.Information("Program terminated");
    }
}