namespace Leditor.Renderer;

using Serilog;

using ImGuiNET;
using rlImGui_cs;

using Raylib_cs;
using static Raylib_cs.Raylib;

using Leditor.Data.Tiles;
using Leditor.Data.Props.Legacy;
using Leditor.Data.Props.Definitions;
using Leditor.Serialization;
using Leditor.Serialization.Frame;

using Leditor.Renderer.RL;
using Leditor.Renderer.Generic;
using Leditor.Renderer.Pages;
using Leditor.Data;
using Leditor.Data.Materials;

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
        string resDir;
        
        #if DEBUG
        dataDir = Path.Combine(executableDir, "..", "..", "..", "..", "Data");
        resDir = Path.Combine(executableDir, "..", "..", "..", "..", "Resources");
        #else
        dataDir = Path.Combine(executableDir, "Data");
        resDir = Path.Combine(executableDir, "Resources");
        #endif

        var folders = new Folders
        {
            Executable = executableDir,

            LegacyCastMembers = Path.Combine(dataDir, "Cast"),

            Tiles = [ Path.Combine(dataDir, "Graphics") ],
            Props = [ Path.Combine(dataDir, "Props") ],
            Materials = [ Path.Combine(dataDir, "Materials") ],

            Projects = Path.Combine(dataDir, "LevelEditorProjects"),
            Resources = resDir
        };

        var files = new Files {
            Logs = Path.Combine(executableDir, "logs", "logs.log")
        };
        
        #endregion

        using var logger = new LoggerConfiguration()
            .WriteTo.File(
                Path.Combine(executableDir, "logs", $"logs.log"),
                fileSizeLimitBytes: 50000000,
                rollOnFileSizeLimit: true,
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug
            )
            .WriteTo.Console(Serilog.Events.LogEventLevel.Debug)
            .CreateLogger();


        logger.Information("Initializing renderer");

        #region Folder Check

        logger.Information("Checking paths");

        var foldersCheckResult = folders.CheckIntegrity();

        logger.Information("Check result:\n\n{Result}\n", foldersCheckResult);

        if (!foldersCheckResult.Executable) logger.Fatal("Executable's path not found");
        if (!foldersCheckResult.LegacyCastMembers) logger.Fatal("Cast folder not found");
        if (!foldersCheckResult.Projects)
        {
            Console.WriteLine(folders.Projects);
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

        logger.Information("Loading resources");

        var embeddedMaterials = Utils.GetEmbeddedMaterials();

        Registry registry = new();

        var castLibrariesLoader = new CastLoader(folders.LegacyCastMembers, logger);

        var tileLoader = new DefinitionLoader<TileDefinition>(folders.Tiles, TileImporter.ParseInitAsync_NoCategories, logger);
        var propLoader = new DefinitionLoader<InitPropBase>(folders.Props, PropImporter.ParseLegacyInitAsync_NoCategories, logger);
        var mateLoader = new MaterialLoader(folders.Materials, MaterialImporter.GetMaterialInitAsync, logger);

        Task<TileDefinition[]>? embeddedTilesTask = null;
        Task? embeddedMaterialsTask = null;

        var embeddedTasksDone = false;

        int page = 0;

        Context context = new()
        { 
            Page = page, 
            Registry = registry ,
        };

        context.PageChanged += (_, next) => { page = next; };

        context.LevelLoaded += (_, _) => { page = 1; };
        context.LevelLoadingStarted += (_, _) => { page = 2; };
        context.LevelLoadingFailed += (_, _) => { page = 0; };

        
        SetTargetFPS(60);
        SetWindowState(ConfigFlags.ResizableWindow);

        #if RELEASE
        SetTraceLogLevel(TraceLogLevel.Error);
        #else
        SetTraceLogLevel(TraceLogLevel.Warning);
        #endif

        //---------------------------------------------------------
        InitWindow(2000 * 2/3, 1200 * 2/3, "Henry's Renderer");
        //---------------------------------------------------------

        logger.Information("Loading shaders");

        Shaders shaders = Shaders.LoadFrom(folders.Shaders);

        context.Engine = new()
        {
            Logger = logger,
            Shaders = shaders,
            Registry = registry
        };

        #region Load Assets

        Textures textures = new();
        textures.Inititialize(folders.Textures);
        context.Textures = textures;
        
        #endregion

        #region Init Pages

        StartPage startPage = new(context)
        {
            ProjectsFolder = folders.Projects,
            Logger = logger,
        };

        MainPage mainPage = new(context)
        {
            Logger = logger,
        };

        LevelLoadingPage levelLoadingPage = new();

        RenderingPage renderingPage = new(context)
        {
            Logger = logger
        };

        #endregion

        rlImGui.Setup(true, true);
        
        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        ImGui.GetIO().ConfigDockingWithShift = true;
        ImGui.GetIO().ConfigDockingTransparentPayload = true;

        long frame = 0;

        while (!WindowShouldClose())
        {
            BeginDrawing();
            {
                // This section may need abstracting..
                #region Loading

                loading:

                if (!castLibrariesLoader.IsCompleted)
                {
                    if (castLibrariesLoader.LoadNext()) registry.CastLibraries = castLibrariesLoader.GetLibs().ToDictionary(l => l.Name);
                }

                if (!tileLoader.IsCompleted)
                {
                    if (tileLoader.IsReady && tileLoader.LoadNext())
                    {
                        registry.Tiles = new Dex<TileDefinition>(tileLoader.GetResult());
                        registry.Tiles.Register([
                            new TileDefinition(
                                "shortCutHorizontal", 
                                (1, 1), 
                                TileType.VoxelStruct, 
                                0, 
                                new int[1,1,3], 
                                [1, 9], 
                                [], 
                                1
                            ) { Texture = LoadTexture(Path.Combine(folders.Tiles[0], "shortCutHorizontal.png")) },
                            new TileDefinition(
                                "shortCutVertical", 
                                (1, 1), 
                                TileType.VoxelStruct, 
                                0, 
                                new int[1,1,3], 
                                [1, 9], 
                                [], 
                                1
                            ) { Texture = LoadTexture(Path.Combine(folders.Tiles[0], "shortCutVertical.png")) },
                            new TileDefinition(
                                "shortCutTile", 
                                (1, 1), 
                                TileType.VoxelStruct, 
                                0, 
                                new int[1,1,3], 
                                [1, 9], 
                                [], 
                                1
                            ) { Texture = LoadTexture(Path.Combine(folders.Tiles[0], "shortCutTile.png")) },
                            new TileDefinition(
                                "shortCutArrows",
                                (3, 3),
                                TileType.VoxelStruct,
                                1,
                                new int[1,1,3],
                                [1, 7, 12],
                                [],
                                -1
                            ) { Texture = LoadTexture(Path.Combine(folders.Tiles[0], "shortCutArrows.png")) },
                            new TileDefinition(
                                "shortCutDots",
                                (3, 3),
                                TileType.VoxelStruct,
                                1,
                                new int[1,1,3],
                                [1, 7, 12],
                                [],
                                -1
                            ) { Texture = LoadTexture(Path.Combine(folders.Tiles[0], "shortCutDots.png")) },
                            new TileDefinition(
                                "shortCut",
                                (3, 3),
                                TileType.VoxelStruct,
                                1,
                                new int[1,1,3],
                                [1, 7, 12],
                                [],
                                -1
                            ) { Texture = LoadTexture(Path.Combine(folders.Tiles[0], "shortCut.png")) },
                        ]);
                    }
                }

                if (!propLoader.IsCompleted)
                {
                    if (propLoader.IsReady && propLoader.LoadNext())
                    {
                        registry.Props = new Dex<InitPropBase>(propLoader.GetResult());
                    }
                }

                if (!mateLoader.IsCompleted)
                {
                    if (mateLoader.IsReady && mateLoader.LoadNext())
                    {
                        registry.Materials = new Dex<MaterialDefinition>(mateLoader.GetResult());
                    }
                }

                if (
                    !castLibrariesLoader.IsCompleted || 
                    !tileLoader.IsCompleted || 
                    !propLoader.IsCompleted || 
                    !mateLoader.IsCompleted
                )
                {
                    Draw.LoadingScreen(
                        (float)castLibrariesLoader.Progress / castLibrariesLoader.TotalProgress,
                        (float)tileLoader.Progress / tileLoader.TotalProgress, 
                        (float)propLoader.Progress / propLoader.TotalProgress, 
                        (float)mateLoader.Progress / mateLoader.TotalProgress
                    );

                    if (++frame % 100 != 0) goto loading;

                    EndDrawing();
                    continue;
                }
                else if (embeddedMaterialsTask is null)
                {
                    embeddedMaterialsTask = Utils.FetchEmbeddedTextures(embeddedMaterials, registry.CastLibraries);
                }
                else if (embeddedTilesTask is null)
                {
                    embeddedTilesTask = Utils.LoadEmbeddedTiles(
                        Path.Combine(
                            folders.LegacyCastMembers, 
                            "Drought_393439_Drought Needed Init.txt"
                        ), 
                        registry.CastLibraries
                    );
                }
                else if (!embeddedMaterialsTask.IsCompleted || !embeddedTilesTask.IsCompleted)
                {
                    Draw.PleaseWaitScreen();
                    EndDrawing();
                    continue;
                }
                else if (!embeddedTasksDone)
                {
                    registry.Tiles?.Register(embeddedTilesTask.Result);
                    registry.Materials?.Register(embeddedMaterials);
                    embeddedTasksDone = true;
                }

                #endregion

                #region Pages

                switch (page)
                {
                    case 0: startPage.Draw(); break;
                    case 1: mainPage.Draw(); break;
                    case 2: levelLoadingPage.Draw(); break;
                    case 3: renderingPage.Draw(); break;

                    default: startPage.Draw(); break;
                }

                #endregion
            }
            EndDrawing();

            unchecked
            {
                context.Frame = ++frame;
            }
        }

        #region Cleanup

        logger.Information("Unloading resources");

        foreach (var lib in registry.CastLibraries.Values)
        {
            foreach (var (_, member) in lib.Members) UnloadTexture(member.Texture);
        }

        if (registry.Tiles is not null) foreach (var tile in registry.Tiles.Definitions) UnloadTexture(tile.Texture);
        if (registry.Props is not null) foreach (var prop in registry.Props.Definitions) UnloadTexture(prop.Texture);
        if (registry.Materials is not null) foreach (var mater in registry.Materials.Definitions) if (mater.RenderType is MaterialRenderType.CustomUnified) UnloadTexture(mater.Texture);

        context.Engine.Dispose();
        shaders.Dispose();
        
        #endregion

        rlImGui.Shutdown();
        CloseWindow();


        logger.Information("Program terminated");
    }
}