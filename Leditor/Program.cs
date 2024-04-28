global using Raylib_cs;
using System.Globalization;
using static Raylib_cs.Raylib;

using rlImGui_cs;

using ImGuiNET;
using System.Numerics;
using Leditor.Serialization;
using System.Text.Json;
using Serilog;
using System.Text;
using System.Threading;
using Drizzle.Lingo.Runtime;
using Drizzle.Logic;
using Leditor.Pages;
using Leditor.Renderer;

#nullable enable

namespace Leditor;

class Program
{
    // Used to load geo blocks menu item textures.
    // Do not alter the indices, and do NOT call before InitWindow()
    private static Texture2D[] LoadUiTextures() => [
        LoadTexture("assets/geo/ui/solid.png"),             // 0
        LoadTexture("assets/geo/ui/air.png"),               // 1
        // LoadTexture("assets/geo/ui/slopebr.png"),     
        LoadTexture("assets/geo/ui/slopebl.png"),           // 2
        LoadTexture("assets/geo/ui/multisolid.png"),        // 3
        LoadTexture("assets/geo/ui/multiair.png"),          // 4
        // LoadTexture("assets/geo/ui/slopetr.png"),        
        // LoadTexture("assets/geo/ui/slopetl.png"),        
        LoadTexture("assets/geo/ui/platform.png"),          // 5
        LoadTexture("assets/geo/ui/move.png"),              // 6
        LoadTexture("assets/geo/ui/rock.png"),              // 7
        LoadTexture("assets/geo/ui/spear.png"),             // 8
        LoadTexture("assets/geo/ui/crack.png"),             // 9
        LoadTexture("assets/geo/ui/ph.png"),                // 10
        LoadTexture("assets/geo/ui/pv.png"),                // 11
        LoadTexture("assets/geo/ui/glass.png"),             // 12
        LoadTexture("assets/geo/ui/backcopy.png"),          // 13
        LoadTexture("assets/geo/ui/entry.png"),             // 14
        LoadTexture("assets/geo/ui/shortcut.png"),          // 15
        LoadTexture("assets/geo/ui/den.png"),               // 16
        LoadTexture("assets/geo/ui/passage.png"),           // 17
        LoadTexture("assets/geo/ui/bathive.png"),           // 18
        LoadTexture("assets/geo/ui/waterfall.png"),         // 19
        LoadTexture("assets/geo/ui/scav.png"),              // 20
        LoadTexture("assets/geo/ui/wack.png"),              // 21
        LoadTexture("assets/geo/ui/garbageworm.png"),       // 22
        LoadTexture("assets/geo/ui/worm.png"),              // 23
        LoadTexture("assets/geo/ui/forbidflychains.png"),   // 24
        LoadTexture("assets/geo/ui/clearall.png"),          // 25
        LoadTexture("assets/geo/ui/save-to-memory.png"),    // 26
        LoadTexture("assets/geo/ui/load-from-memory.png"),  // 27
    ];

    // Used to load geo block textures.
    // Do not alter the indices, and do NOT call before InitWindow()
    private static Texture2D[] LoadGeoTextures() => [
        // 0: air
        LoadTexture("assets/geo/solid.png"),
        LoadTexture("assets/geo/cbl.png"),
        LoadTexture("assets/geo/cbr.png"),
        LoadTexture("assets/geo/ctl.png"),
        LoadTexture("assets/geo/ctr.png"),
        LoadTexture("assets/geo/platform.png"),
        LoadTexture("assets/geo/entryblock.png"),
        // 7: NONE
        // 8: NONE
        LoadTexture("assets/geo/thickglass.png"),
    ];


    // Used to load geo stackables textures.
    // And you guessed it: do not alter the indices, and do NOT call before InitWindow()
    static Texture2D[] LoadStackableTextures() => [
        LoadTexture("assets/geo/ph.png"),             // 0
        LoadTexture("assets/geo/pv.png"),             // 1
        LoadTexture("assets/geo/bathive.png"),        // 2
        LoadTexture("assets/geo/dot.png"),            // 3
        LoadTexture("assets/geo/crackbl.png"),        // 4
        LoadTexture("assets/geo/crackbr.png"),        // 5
        LoadTexture("assets/geo/crackc.png"),         // 6
        LoadTexture("assets/geo/crackh.png"),         // 7
        LoadTexture("assets/geo/cracklbr.png"),       // 8
        LoadTexture("assets/geo/cracktbr.png"),       // 9
        LoadTexture("assets/geo/cracktl.png"),        // 10
        LoadTexture("assets/geo/cracktlb.png"),       // 11
        LoadTexture("assets/geo/cracktlr.png"),       // 12
        LoadTexture("assets/geo/cracktr.png"),        // 13
        LoadTexture("assets/geo/cracku.png"),         // 14
        LoadTexture("assets/geo/crackv.png"),         // 15
        LoadTexture("assets/geo/garbageworm.png"),    // 16
        LoadTexture("assets/geo/scav.png"),           // 17
        LoadTexture("assets/geo/rock.png"),           // 18
        LoadTexture("assets/geo/waterfall.png"),      // 19
        LoadTexture("assets/geo/wack.png"),           // 20
        LoadTexture("assets/geo/worm.png"),           // 21
        LoadTexture("assets/geo/entryb.png"),         // 22
        LoadTexture("assets/geo/entryl.png"),         // 23
        LoadTexture("assets/geo/entryr.png"),         // 24
        LoadTexture("assets/geo/entryt.png"),         // 25
        LoadTexture("assets/geo/looseentry.png"),     // 26
        LoadTexture("assets/geo/passage.png"),        // 27
        LoadTexture("assets/geo/den.png"),            // 28
        LoadTexture("assets/geo/spear.png"),          // 29
        LoadTexture("assets/geo/forbidflychains.png"),// 30
        LoadTexture("assets/geo/crackb.png"),         // 31
        LoadTexture("assets/geo/crackr.png"),         // 32
        LoadTexture("assets/geo/crackt.png"),         // 33
        LoadTexture("assets/geo/crackl.png"),         // 34
    ];

    private static Texture2D[] LoadPropMenuCategoryTextures() =>
    [
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "prop category tiles.png")),
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "prop category ropes.png")),
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "prop category longs.png")),
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "prop category other.png")),
    ];

    private static Texture2D[] LoadGeoInterfaceTextures() =>
    [
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "slope_black.png")), // 0
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "pc_black.png")), // 1
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "spear_black.png")), // 2
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "entry_black.png")), // 3
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "slope.png")), // 4
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "pc.png")), // 5
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "spear.png")), // 6
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "entry.png")), // 7

        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "solid_black.png")), // 8
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "platform_black.png")), // 9
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "glass_black.png")), // 10
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "solid.png")), // 11
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "platform.png")), // 12
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "glass.png")), // 13

        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "pv.png")), // 14
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "ph.png")), // 15
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "crack.png")), // 16
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "pv_black.png")), // 17
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "ph_black.png")), // 18
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "crack_black.png")), // 19

        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "bathive.png")), // 20
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "forbidflychains.png")), // 21
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "waterfall.png")), // 22
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "worm.png")), // 23
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "rock.png")), // 24
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "bathive_black.png")), // 25
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "forbidflychains_black.png")), // 26
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "waterfall_black.png")), // 27
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "worm_black.png")), // 28
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "rock_black.png")), // 29

        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "looseentry.png")), // 30
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "dot.png")), // 31
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "passage.png")), // 32
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "den.png")), // 33
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "wack.png")), // 34
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "scav.png")), // 35
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "garbage.png")), // 36
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "looseentry_black.png")), // 37
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "dot_black.png")), // 38
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "passage_black.png")), // 39
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "den_black.png")), // 40
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "wack_black.png")), // 41
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "scav_black.png")), // 42
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "garbage_black.png")), // 43

        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "camera icon.png")) // 44
    ];

    // Used to load light/shadow brush images as textures.
    private static Texture2D[] LoadLightTextures(Serilog.Core.Logger logger) => Directory
        .GetFileSystemEntries(GLOBALS.Paths.LightAssetsDirectory)
        .Where(e => e.EndsWith(".png"))
        .Select((e) =>
        {
            logger.Debug($"loading light texture \"{e}\""); 
            return LoadTexture(e); })
        .ToArray();
    
    private record struct SaveProjectResult(bool Success, Exception? Exception = null);

    private static bool _globalSave;
    private static Task<string>? _saveFileDialog;
    private static Task<SaveProjectResult>? _saveResult;

    private static bool _askForPath;
    private static bool _failedToSave;

    private static bool _isGuiLocked;

    private static DrizzleRenderWindow? _renderWindow;
    
    private static async Task<SaveProjectResult> SaveProjectAsync(string path)
    {
        SaveProjectResult result;
        
        try
        {
            var strTask = Exporters.ExportAsync(GLOBALS.Level);

            var str = await strTask;
            
            await File.WriteAllTextAsync(path, str);

            result = new(true);
        }
        catch (Exception e)
        {
            result = new(false, e);
        }

        return result;
    }

    // Unused
    private static Texture2D[][] LoadPropTextures()
    {
        return GLOBALS.Props.Select(category =>
            category.Select(prop =>
                LoadTexture(Path.Combine(GLOBALS.Paths.PropsAssetsDirectory, prop.Name + ".png")) // cause a random crash
            ).ToArray()
        ).ToArray();
    }
    
    // Unused
    private static Texture2D[][] LoadPropTexturesFromRenderer()
    {
        return GLOBALS.Props.Select(category =>
            category.Select(prop =>
                LoadTexture(Path.Combine(Path.Combine(GLOBALS.Paths.RendererDirectory, "Data", "Props"), prop.Name + ".png")) // cause a random crash
            ).ToArray()
        ).ToArray();
    }

    private static (string[], (string, Color)[][]) LoadMaterialInit()
    {
        var path = GLOBALS.Paths.MaterialsInitPath;

        var text = File.ReadAllText(path);

        return Importers.GetMaterialInit(text);
    }

    private static ((string, Color)[], InitTile[][]) LoadTileInit()
    {
        var path = GLOBALS.Paths.TilesInitPath;

        var text = File.ReadAllText(path).ReplaceLineEndings();

        return Importers.GetTileInit(text);
    }

    private static ((string, Color)[], InitTile[][]) LoadTileInitFromRenderer()
    {
        var path = Path.Combine(GLOBALS.Paths.RendererDirectory, "Graphics", "Init.txt");

        var text = File.ReadAllText(path).ReplaceLineEndings();

        return Importers.GetTileInit(text);
    }

    private static async Task<((string, Data.Color)[], Data.Tiles.TileDefinition[][])> LoadTileInitFromRendererAsync()
    {
        return await TileImporter.ParseInitAsync(Path.Combine(GLOBALS.Paths.RendererDirectory, "Graphics",
            "Init.txt"));
    }

    private static async Task<TileInitLoadInfo[]> LoadTileInitPackages()
    {
        var packageDirectories = Directory.GetDirectories(GLOBALS.Paths.TilePackagesDirectory);
        
        IEnumerable<(string directory, string file)> initPaths = packageDirectories.Select(directory => (directory, Path.Combine(directory, "init.txt")));

        var initLoadTasks = initPaths
            .Where(p => File.Exists(p.Item2))
            .Select(p => 
                Task.Factory.StartNew(() => {
                        var (categories, tiles) = Importers.GetTileInit(File.ReadAllText(p.file).ReplaceLineEndings());

                        return new TileInitLoadInfo(categories, tiles, p.directory);
                })
                )
            .ToArray();

        List<TileInitLoadInfo> inits = [];

        foreach (var task in initLoadTasks) inits.Add(await task);

        return [..inits];
    }

    private static async Task<((string name, Data.Color color)[], Data.Tiles.TileDefinition[][], string directory)[]>
        LoadTileInitPacksAsync()
    {
        var packageDirectories = Directory.GetDirectories(GLOBALS.Paths.TilePackagesDirectory);
        
        IEnumerable<(string directory, string file)> initPaths = packageDirectories.Select(directory => (directory, Path.Combine(directory, "init.txt")));

        var initLoadTasks = initPaths
            .Where(p => File.Exists(p.Item2))
            .Select(p => 
                Task.Factory.StartNew(() => {
                    var task = TileImporter.ParseInitAsync(p.file);

                    task.Wait();

                    var (categories, tiles) = task.Result;
                    
                    return (categories, tiles, p.directory);
                })
            )
            .ToArray();

        List<((string name, Data.Color color)[], Data.Tiles.TileDefinition[][], string directory)> inits = [];

        foreach (var task in initLoadTasks) inits.Add(await task);

        return [..inits];
    }
    
    // TODO: add packed props

    private static ((string category, Color color)[] categories, InitPropBase[][] init) LoadPropInit()
    {
        var text = File.ReadAllText(GLOBALS.Paths.PropsInitPath).ReplaceLineEndings();
        return Importers.GetPropsInit(text);
    }
    
    // Unused
    private static ((string category, Color color)[] categories, InitPropBase[][] init) LoadPropInitFromRenderer()
    {
        var text = File.ReadAllText(Path.Combine(GLOBALS.Paths.RendererDirectory, "Props", "Init.txt")).ReplaceLineEndings();
        return Importers.GetPropsInit(text);
    }
    
    //

    private static TileLoader _tileLoader;
    private static PropLoader _propLoader;

    // MAIN FUNCTION
    private static void Main()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

        #if DEBUG
        GLOBALS.BuildConfiguration = "Build Configuration: Debug";
        #else
        GLOBALS.BuildConfiguration = "Build Configuration: Release";
        #endif

        // Initialize logging

        try
        {
            if (!Directory.Exists(Path.Combine(GLOBALS.Paths.ExecutableDirectory, "logs"))) Directory.CreateDirectory(Path.Combine(GLOBALS.Paths.ExecutableDirectory, "logs"));
        }
        catch
        {
            Console.WriteLine("Failed to create logs directory");
            // The program should halt here..
        }

        using var logger = new LoggerConfiguration().WriteTo.File(
            Path.Combine(GLOBALS.Paths.ExecutableDirectory, "logs/logs.txt"),
            fileSizeLimitBytes: 50000000,
            rollOnFileSizeLimit: true
            ).CreateLogger();

        GLOBALS.Logger = logger;

        logger.Information("program has started");

        // check cache directory

        if (!Directory.Exists(GLOBALS.Paths.CacheDirectory))
        {
            try
            {
                Directory.CreateDirectory(GLOBALS.Paths.CacheDirectory);
            }
            catch (Exception e)
            {
                logger.Error($"failed to create cache directory: {e}");
            }
        }

        // Check for the assets folder and subfolders
        
        // Check /assets. A common mistake when building is to forget to copy the 
        // assets folder

        var missingAssetsDirectory = !Directory.Exists(GLOBALS.Paths.AssetsDirectory);
        
        var failedIntegrity = missingAssetsDirectory;
        
        if (failedIntegrity) goto skip_file_check;
        
        foreach (var (directory, exists) in GLOBALS.Paths.DirectoryIntegrity)
        {
            if (!exists)
            {
                logger.Fatal($"Critical directory not found: \"{directory}\"");
                failedIntegrity = true;
            }
            
            if (failedIntegrity) goto skip_file_check;
        }

        foreach (var (file, exists) in GLOBALS.Paths.FileIntegrity)
        {
            if (!exists)
            {
                logger.Fatal($"critical file not found: \"{file}]\"");
                failedIntegrity = true;
            }
        }
        
        skip_file_check:

        // Import settings

        logger.Information("Importing settings");

        // Default settings

        var serOptions = new JsonSerializerOptions { WriteIndented = true };

        // load the settings.json file

        // TODO: Improvise
        try
        {
            if (File.Exists(GLOBALS.Paths.SettingsPath))
            {
                var settingsText = File.ReadAllText(Path.Combine(GLOBALS.Paths.ExecutableDirectory, "settings.json"));
                GLOBALS.Settings = JsonSerializer.Deserialize<Settings>(settingsText, serOptions) ?? throw new Exception("failed to deserialize settings.json");
            }
            else
            {
                logger.Debug("settings.json file not found; exporting default settings");
                var text = JsonSerializer.Serialize(GLOBALS.Settings, serOptions);
                File.WriteAllText(GLOBALS.Paths.SettingsPath, text);
            }
        }
        catch (Exception e)
        {
            logger.Error($"Failed to import settings from settings.json: {e}\nUsing default settings");

            try
            {
                var text = JsonSerializer.Serialize(GLOBALS.Settings, serOptions);
                File.WriteAllText(GLOBALS.Paths.SettingsPath, text);
            }
            catch (Exception e2)
            {
                logger.Error($"Failed to create default settings: {e2}");
            }
        }

        // Check for projects folder
        
        if (!failedIntegrity && !Directory.Exists(GLOBALS.Paths.ProjectsDirectory))
        {
            try
            {
                Directory.CreateDirectory(GLOBALS.Paths.ProjectsDirectory);
            }
            catch (Exception e)
            {
                logger.Fatal($"Failed to create a projects folder: {e}");
                return;
            }
        }
        
        logger.Information("Initializing data");

        logger.Information(GLOBALS.Version);
        logger.Information(GLOBALS.RaylibVersion);
        
        // Load Cache

        var loadRecentProjectsTask = failedIntegrity 
            ? Task.CompletedTask
            : Task.Factory.StartNew(() => {
                if (!File.Exists(GLOBALS.Paths.RecentProjectsPath)) return;
                
                var lines = File.ReadAllLines(GLOBALS.Paths.RecentProjectsPath);

                foreach (var path in lines)
                {
                    GLOBALS.RecentProjects.AddLast((path, Path.GetFileNameWithoutExtension(path)));
                    
                    if (GLOBALS.RecentProjects.Count > GLOBALS.RecentProjectsLimit) 
                        GLOBALS.RecentProjects.RemoveFirst();
                }
            });

        var allCacheTasks = Task.WhenAll([loadRecentProjectsTask]);

        var tilePackagesTask = Task.FromResult<TileInitLoadInfo[]>([]);

        (string name, Color color)[] loadedPackageTileCategories = [];
        InitTile[][] loadedPackageTiles = [];
        
        if (!failedIntegrity)
        {
            // Load tiles and props
            
            logger.Information("Indexing tiles and props");

            try
            {
                (GLOBALS.TileCategories, GLOBALS.Tiles) = LoadTileInitFromRenderer();
                _tileLoader = new([
                    GLOBALS.Paths.TilesAssetsDirectory, 
                    ..Directory.GetDirectories(GLOBALS.Paths.TilePackagesDirectory)
                ]);
            }
            catch (Exception e)
            {
                logger.Fatal($"Failed to load tiles init: {e}");
                failedIntegrity = true;
                throw new Exception(innerException: e, message: $"Failed to load tiles init: {e}");
            }
            
            try
            {
                var materialsInit = LoadMaterialInit();

                GLOBALS.MaterialCategories = [..GLOBALS.MaterialCategories, ..materialsInit.Item1];
                GLOBALS.Materials = [..GLOBALS.Materials, ..materialsInit.Item2];

                foreach (var category in materialsInit.Item2) {
                    foreach (var (material, color) in category) {
                        if (GLOBALS.MaterialColors.ContainsKey(material)) {
                            throw new Exception($"Duplicate material definition \"{material}\"");
                        } else {
                            GLOBALS.MaterialColors.Add(material, color);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Fatal($"Failed to load materials init: {e}");
                throw new Exception(innerException: e, message: $"Failed to load materials init: {e}");
            }

            try
            {
                (GLOBALS.PropCategories, GLOBALS.Props) = LoadPropInitFromRenderer();
                _propLoader = new([GLOBALS.Paths.PropsAssetsDirectory]);
            }
            catch (Exception e)
            {
                logger.Fatal($"Failed to load props init: {e}");
                throw new Exception(innerException: e, message: $"Failed to load props init: {e}");
            }
            
            //

            // Merge tile packages
            
            logger.Debug("Loading custom tiles");

            if (Directory.Exists(GLOBALS.Paths.TilePackagesDirectory))
            {
                logger.Debug("Loading pack tiles");
                
                try
                {
                    tilePackagesTask = LoadTileInitPackages();
                    
                    tilePackagesTask.Wait();

                    var packages = tilePackagesTask.Result;

                    if (packages.Length > 0)
                    {
                        loadedPackageTileCategories = packages.SelectMany(p => p.Categories).ToArray();
                        
                        loadedPackageTiles = packages.SelectMany(p => p.Tiles).ToArray();
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"Failed to load tile packages: {e}"); 
                }
            }
        }
        
        // Load Tile Textures

        var propTextures = new PropTexturesLoader();
        var lightTextures = new LightTexturesLoader();
        
        // 1. Get all texture paths
        
        var ropePropImagePaths = GLOBALS.RopeProps
            .Select(r => Path.Combine(GLOBALS.Paths.RendererDirectory, "Props", r.Name + ".png"))
            .ToArray();
        
        var longPropImagePaths = GLOBALS.LongProps
            .Select(l => Path.Combine(GLOBALS.Paths.RendererDirectory, "Props", l.Name + ".png"))
            .ToArray();
        
        var otherPropImagePaths = GLOBALS.Props.Select(category =>
            category.Select(prop => Path.Combine(GLOBALS.Paths.RendererDirectory, "Props", prop.Name + ".png")
            ).ToArray()
        ).ToArray();
        
        var lightImagePaths = failedIntegrity ? [] : Directory
            .GetFileSystemEntries(GLOBALS.Paths.LightAssetsDirectory)
            .Where(e => e.EndsWith(".png"))
            .Select(e =>
            {
                logger.Debug($"Loading light texture \"{e}\""); 
                return e; 
            })
            .ToArray();
        
        // 2. MERGE TILES
        
        logger.Debug("Merging custom tiles");
        
        GLOBALS.TileCategories = [..GLOBALS.TileCategories, ..loadedPackageTileCategories];
        GLOBALS.Tiles = [..GLOBALS.Tiles, ..loadedPackageTiles];
        
        // 3. Load the images

        var loadPropImagesTask = propTextures.PrepareFromPathsAsync(ropePropImagePaths, longPropImagePaths, otherPropImagePaths);
        var loadLightImagesTask = lightTextures.PrepareFromPathsAsync(lightImagePaths);
        
        // 4. Await loading in later stages
        
        //

        logger.Information("Initializing window");

        var icon = LoadImage(GLOBALS.Paths.IconPath);

        SetConfigFlags(ConfigFlags.ResizableWindow);
        SetConfigFlags(ConfigFlags.Msaa4xHint);
        
        #if DEBUG
        // TODO: Change this
        SetTraceLogLevel(TraceLogLevel.Info);
        #else
        SetTraceLogLevel(TraceLogLevel.Error);
        #endif
        
        //----------------------------------------------------------------------------
        // No texture loading prior to this point
        //----------------------------------------------------------------------------
        InitWindow(GLOBALS.MinScreenWidth, GLOBALS.MinScreenHeight, "Henry's Leditor");
        //
        
        SetWindowIcon(icon);
        SetWindowMinSize(GLOBALS.MinScreenWidth, GLOBALS.MinScreenHeight);
        SetExitKey(KeyboardKey.Null);
        
        if (failedIntegrity) goto skip_gl_init;

        // The splashscreen
        GLOBALS.Textures.SplashScreen = LoadTexture(GLOBALS.Paths.SplashScreenPath);

        // This is the level's light map, which will be used to draw textures on called "light/shadow brushes"
        GLOBALS.Textures.LightMap = LoadRenderTexture(
            GLOBALS.Level.Width * GLOBALS.Scale + 300,
            GLOBALS.Level.Height * GLOBALS.Scale + 300
        );

        //

        logger.Information("loading textures");

        // Load images to RAM concurrently first, then load them to VRAM in the main thread.
        // Do NOT load textures directly to VRAM on a separate thread.

        //
        try
        {
            logger.Debug("loading UI textures");
            GLOBALS.Textures.GeoMenu = LoadUiTextures();
            logger.Debug("loading geo textures");
            GLOBALS.Textures.GeoBlocks = LoadGeoTextures();
            GLOBALS.Textures.GeoStackables = LoadStackableTextures();
            logger.Debug("loading light brush textures");
            // Light textures need to be loaded on a separate thread, just like tile textures
            GLOBALS.Textures.LightBrushes = LoadLightTextures(logger);
        }
        catch (Exception e)
        {
            logger.Fatal($"{e}");
        }

        GLOBALS.Textures.MissingTile =
            LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "other", "missing tile.png"));

        GLOBALS.Textures.PropMenuCategories = LoadPropMenuCategoryTextures();


        GLOBALS.Textures.PropEditModes =
        [
            LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "move icon.png")),
            LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "rotate icon.png")),
            LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "scale icon.png")),
            LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "warp icon.png")),
            LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "edit point icon.png")),
        ];

        GLOBALS.Textures.PropGenerals =
        [
            LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "no collision icon.png"))
        ];

        GLOBALS.Textures.GeoInterface = LoadGeoInterfaceTextures();

        //

        GLOBALS.Textures.TileSpecs = LoadRenderTexture(200, 200);
        GLOBALS.Textures.PropDepth = LoadRenderTexture(290, 20);
        GLOBALS.Textures.DimensionsVisual = LoadRenderTexture(1400, 800);

        //

        logger.Information("loading shaders");

        GLOBALS.Shaders.TilePreview = LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "tile_preview2.frag"));

        // These two are used to display the light/shadow brush beneath the cursor
        GLOBALS.Shaders.ShadowBrush = LoadShader(null, Path.Combine(GLOBALS.Paths.AssetsDirectory, "shaders", "shadow_brush.fs"));
        GLOBALS.Shaders.LightBrush = LoadShader(null, Path.Combine(GLOBALS.Paths.AssetsDirectory, "shaders", "light_brush.fs"));

        // These two are used to actually draw/erase the shadow on the light map
        GLOBALS.Shaders.ApplyLightBrush = LoadShader(null, Path.Combine(GLOBALS.Paths.AssetsDirectory, "shaders", "apply_light_brush.fs"));
        GLOBALS.Shaders.ApplyShadowBrush = LoadShader(null, Path.Combine(GLOBALS.Paths.AssetsDirectory, "shaders", "apply_shadow_brush.fs"));

        GLOBALS.Shaders.Prop = LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop.frag"));

        GLOBALS.Shaders.StandardProp =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_standard.frag"));
        
        GLOBALS.Shaders.VariedStandardProp =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_varied_standard.frag"));
        
        GLOBALS.Shaders.SoftProp =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_soft.frag"));
        
        GLOBALS.Shaders.VariedSoftProp =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_varied_soft.frag"));
        
        GLOBALS.Shaders.SimpleDecalProp =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_simple_decal.frag"));
        
        GLOBALS.Shaders.VariedDecalProp =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_varied_decal.frag"));
        
        GLOBALS.Shaders.ColoredTileProp =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_colored.frag"));
        
        GLOBALS.Shaders.ColoredBoxTileProp =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_colored_box_type.frag"));

        GLOBALS.Shaders.LongProp =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_long.frag"));

        GLOBALS.Shaders.DefaultProp =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_default.frag"));

        GLOBALS.Shaders.PreviewColoredTileProp = LoadShader(null,
            Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_colored_preview.frag"));

        GLOBALS.Shaders.LightMapStretch =
            LoadShader(null,
                Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "lightmap_stretch.frag"));

        GLOBALS.Shaders.VFlip = LoadShaderFromMemory(null, @"#version 330
in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D inputTexture;

out vec4 FragColor;

void main() {
    vec2 op = vec2(fragTexCoord.x, 1 - fragTexCoord.y);
    FragColor = texture(inputTexture, op);
}");
        //

        SetTargetFPS(GLOBALS.Settings.Misc.FPS);

        GLOBALS.Camera = new Camera2D { Zoom = GLOBALS.Settings.GeneralSettings.DefaultZoom };
        
        skip_gl_init:

        float initialFrames = 0;

        Texture2D? screenshotTexture = null;

        logger.Information("Initializing pages");
        
        // Initialize pages

        GeoEditorPage geoPage = new() { Logger = logger };
        TileEditorPage tilePage = new() { Logger = logger };
        CamerasEditorPage camerasPage = new() { Logger = logger };
        LightEditorPage lightPage = new() { Logger = logger };
        DimensionsEditorPage dimensionsPage = new() { Logger = logger };
        NewLevelPage newLevelPage = new() { Logger = logger };
        DeathScreen deathScreen = new() { Logger = logger };
        EffectsEditorPage effectsPage = new() { Logger = logger };
        PropsEditorPage propsPage = new() { Logger = logger };
        MainPage mainPage = new() { Logger = logger };
        StartPage startPage = new() { Logger = logger };
        SaveProjectPage savePage = new() { Logger = logger };
        FailedTileCheckOnLoadPage failedTileCheckOnLoadPage = new() { Logger = logger };
        AssetsNukedPage assetsNukedPage = new() { Logger = logger };
        MissingAssetsPage missingAssetsPage = new() { Logger = logger };
        MissingTexturesPage missingTexturesPage = new() { Logger = logger };
        MissingPropTexturesPage missingPropTexturesPage = new() { Logger = logger };
        MissingInitFilePage missingInitFilePage = new() { Logger = logger };
        ExperimentalGeometryPage experimentalGeometryPage = new() { Logger = logger };
        SettingsPage settingsPage = new() { Logger = logger };

        // GLOBALS.Pager = new Pager(logger, new Context(logger, null));
        //
        // GLOBALS.Pager.Init(20);
        
        //
        // GLOBALS.Pager.RegisterDefault<StartPage>(0);
        // GLOBALS.Pager.RegisterException<DeathScreen>();
        // GLOBALS.Pager.Register<MainPage>(1);
        // GLOBALS.Pager.Register<SettingsPage>(9);
        // GLOBALS.Pager.Register<MissingInitFilePage>(17);
        // GLOBALS.Pager.Register<MissingPropTexturesPage>(19);
        // GLOBALS.Pager.Register<MissingTexturesPage>(16);
        // GLOBALS.Pager.Register<MissingAssetsPage>(15);
        // GLOBALS.Pager.Register<AssetsNukedPage>(14);
        // GLOBALS.Pager.Register<FailedTileCheckOnLoadPage>(13);
        // GLOBALS.Pager.Register<GeoEditorPage>(18);
        // GLOBALS.Pager.Register<ExperimentalGeometryPage>(2);
        // GLOBALS.Pager.Register<TileEditorPage>(3);
        // GLOBALS.Pager.Register<CamerasEditorPage>(4);
        // GLOBALS.Pager.Register<LightEditorPage>(5);
        // GLOBALS.Pager.Register<DimensionsEditorPage>(6);
        // GLOBALS.Pager.Register<EffectsEditorPage>(7);
        // GLOBALS.Pager.Register<PropsEditorPage>(8);
        
        // Lingo runtime assets path

        if (!failedIntegrity)
        {
            LingoRuntime.MovieBasePath = GLOBALS.Paths.RendererDirectory + Path.DirectorySeparatorChar;
            LingoRuntime.CastPath = Path.Combine(LingoRuntime.MovieBasePath, "Cast");
        }
        
        //
        var isLingoRuntimeInit = false;
        //
        
        logger.Information("Initializing events");
        
        // Page event handlers
        startPage.ProjectLoaded += propsPage.OnProjectLoaded;
        startPage.ProjectLoaded += mainPage.OnProjectLoaded;
        startPage.ProjectLoaded += dimensionsPage.OnProjectLoaded;
        
        mainPage.ProjectLoaded += propsPage.OnProjectLoaded;
        mainPage.ProjectLoaded += dimensionsPage.OnProjectLoaded;
        
        newLevelPage.ProjectCreated += propsPage.OnProjectCreated;
        //

        unsafe
        {
            GLOBALS.WindowHandle = new IntPtr(GetWindowHandle());
        }
        
        // Quick save task

        Task<(bool success, Exception? exception)>? quickSaveTask = null;
        
        //
        rlImGui.Setup(GLOBALS.Settings.GeneralSettings.DarkTheme, true);

        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        ImGui.GetIO().ConfigDockingWithShift = true;
        ImGui.GetIO().ConfigDockingTransparentPayload = true;
        
        
        // ImGui.LoadIniSettingsFromDisk(Path.Combine(GLOBALS.Paths.ExecutableDirectory, "imgui.ini"));
        //
        
        // Load fonts

        if (!failedIntegrity && !GLOBALS.Settings.GeneralSettings.DefaultFont)
        {
            logger.Debug("Loading fonts");
            try
            {
                var fontPaths = Directory
                    .GetFiles(GLOBALS.Paths.FontsDirectory)
                    .Where(p => p.EndsWith(".ttf"))
                    .ToList();

                var io = ImGui.GetIO();
                foreach (var fontPath in fontPaths)
                    io.Fonts.AddFontFromFileTTF(fontPath, 13);
                
                rlImGui.ReloadFonts();

                var firstFont = fontPaths.FirstOrDefault();
                
                
                GLOBALS.Font = LoadFont(firstFont);
            }
            catch (Exception e)
            {
                logger.Error($"Failed to load custom fonts: {e}");
            }
        }
        
        // Tile & Prop Textures

        Task.WaitAll([loadPropImagesTask, loadLightImagesTask]);

        var loadPropTexturesList = loadPropImagesTask.Result;
        var loadLightTexturesList = loadLightImagesTask.Result;
        
        using var loadPropTexturesEnumerator = loadPropTexturesList.GetEnumerator();
        using var loadLightTexturesEnumerator = loadLightTexturesList.GetEnumerator();

        var tileTexturesLoadProgress = 0;
        var propTexturesLoadProgress = 0;
        var lightTexturesLoadProgress = 0;

        var totalPropTexturesLoadProgress = loadPropTexturesList.Count;
        var totalLightTexturesLoadProgress = loadLightTexturesList.Count;

        var loadRate = GLOBALS.Settings.Misc.TileImageScansPerFrame;

        var isLoadingTexturesDone = false;
        
        logger.Information("Begin main loop");

        var gShortcuts = GLOBALS.Settings.Shortcuts.GlobalShortcuts;

        // Doesn't seem to work
        _tileLoader.Logger = logger;
        
        _tileLoader.Start();

        _propLoader.Start();
        _propLoader.IncludeDefined(("Ropes", new Data.Color(0, 0, 0)), GLOBALS.Ropes, GLOBALS.Paths.PropsAssetsDirectory);
        _propLoader.IncludeDefined(("Longs", new Data.Color(0, 0, 0)), GLOBALS.Longs, GLOBALS.Paths.PropsAssetsDirectory);

        var tileLoadProgress = 0;
        var propLoadProgress = 0;
        
        Task<Data.Tiles.TileDex>? tileDexTask = null;
        Task<Data.Props.PropDex>? propDexTask = null;

        var fatalException = false;
        
        while (!WindowShouldClose())
        {
            try
            {
                if (!failedIntegrity) goto skip_failed_integrity;
                #region FailedIntegrity
                
                if (missingAssetsDirectory)
                {
                    BeginDrawing();
                    ClearBackground(Color.Black);
                    
                    DrawText("Missing Assets Folder", 50, 50, 50, Color.White);
                    DrawText("The /assets folder seems to be completely missing.", 
                        50, 
                        200, 
                        20, 
                        Color.White
                    );
                    
                    DrawText("The program cannot function without it.", 
                        50, 
                        230, 
                        20, 
                        Color.White
                    );
                    
                    DrawText("If you've built the program yourself, you might have forgotten to ", 
                        50, 
                        260, 
                        20, 
                        Color.White
                    );
                    
                    DrawText("copy the assets from the project's source code.", 
                        50, 
                        290, 
                        20, 
                        Color.White
                    );
                    
                    DrawText(GLOBALS.Version, 10, GetScreenHeight() - 25, 20, Color.White);
                    EndDrawing();
                }
                GLOBALS.Page = 15;
                continue;
                #endregion
                skip_failed_integrity:
                
                
                
                #region Splashscreen
                if (initialFrames < 180 && GLOBALS.Settings.Misc.SplashScreen)
                {
                    initialFrames++;

                    BeginDrawing();

                    ClearBackground(new(0, 0, 0, 255));

                    DrawTexturePro(
                        GLOBALS.Textures.SplashScreen,
                        new(0, 0, GLOBALS.Textures.SplashScreen.Width, GLOBALS.Textures.SplashScreen.Height),
                        new(0, 0, GLOBALS.MinScreenWidth, GLOBALS.MinScreenHeight),
                        new(0, 0),
                        0,
                        new(255, 255, 255, 255)
                    );


                    if (initialFrames > 60) DrawText(GLOBALS.Version, 700, 50, 15, Color.White);

                    if (initialFrames > 70)
                    {
                        DrawText(GLOBALS.RaylibVersion, 700, 70, 15, Color.White);
                    }

                    if (initialFrames > 80)
                    {
                        DrawText(GLOBALS.BuildConfiguration, 700, 90, 15, Color.White);
                    }

                    if (initialFrames > 90)
                    {
                        if (failedIntegrity) 
                            DrawText("missing resources", 700, 320, 16, new(252, 38, 38, 255));
                    }

                    EndDrawing();

                    continue;
                }
                #endregion
                
                // Temporary solution
                if (fatalException) {
                    deathScreen.Draw();
                    continue;
                }

                if (!_tileLoader.Done)
                {
                    tileLoadProgress++;
                    
                    while (tileLoadProgress % loadRate != 0)
                    {
                        // Load complete
                        if (_tileLoader.Proceed())
                        {
                            tileDexTask = _tileLoader.Build();
                            
                            break;
                        } 
                            
                        tileLoadProgress++;
                    }
                    
                    var width = GetScreenWidth();
                    var height = GetScreenHeight();
                    
                    BeginDrawing();
                    Printers.DrawSplashScreen(true);

                    if (!_tileLoader.Started || !_tileLoader.PackLoadCompleted)
                    {
                        if (GLOBALS.Font is null)
                            DrawText("Loading Tiles", 100, height - 120, 20, Color.White);
                        else
                            DrawTextEx(GLOBALS.Font.Value, "Loading Tiles", new Vector2(100, height - 120), 20, 1, Color.White);
                    }
                    else if (!_tileLoader.TextureLoadCompleted)
                    {
                        if (GLOBALS.Font is null)
                            DrawText("Loading Tile Textures", 100, height - 120, 20, Color.White);
                        else
                            DrawTextEx(GLOBALS.Font.Value, "Loading Tile Textures", new Vector2(100, height - 120), 20, 1, Color.White);
                    }
                    else if (!_tileLoader.DexBuildCompleted)
                    {
                        if (GLOBALS.Font is null)
                            DrawText("Building Table", 100, height - 120, 20, Color.White);
                        else
                            DrawTextEx(GLOBALS.Font.Value, "Building Table", new Vector2(100, height - 120), 20, 1, Color.White);
                    }

                    Printers.DrawProgressBar(new Rectangle(100, height - 100, width - 200, 30), tileLoadProgress, _tileLoader.TotalProgress, false, Color.White);
                    EndDrawing();
                    continue;
                }
                if (GLOBALS.TileDex is null)
                {
                    if (tileDexTask?.IsCompleted == true) {
                        GLOBALS.TileDex = tileDexTask.Result;
                        _propLoader.IncludeTiles(GLOBALS.TileDex);
                    }
                    
                    var height = GetScreenHeight();
                    
                    BeginDrawing();
                    Printers.DrawSplashScreen(true);

                    if (GLOBALS.Font is null)
                        DrawText("Building Tile Dex", 100, height - 120, 20, Color.White);
                    else
                        DrawTextEx(GLOBALS.Font.Value, "Building Tile Dex", new Vector2(100, height - 120), 20, 1, Color.White);
                    
                    EndDrawing();
                    
                    continue;
                }

                goto skip_prop_load;

                if (!_propLoader.Done)
                {
                    propLoadProgress++;
                    
                    while (propLoadProgress % loadRate != 0)
                    {
                        // Load complete
                        if (_propLoader.Proceed())
                        {
                            propDexTask = _propLoader.Build();
                            
                            break;
                        } 
                            
                        propLoadProgress++;
                    }
                    
                    var width = GetScreenWidth();
                    var height = GetScreenHeight();
                    
                    BeginDrawing();
                    Printers.DrawSplashScreen(true);
                
                    if (!_propLoader.Started || !_propLoader.PackLoadCompleted)
                    {
                        if (GLOBALS.Font is null)
                            DrawText("Loading Props", 100, height - 120, 20, Color.White);
                        else
                            DrawTextEx(GLOBALS.Font.Value, "Loading Props", new Vector2(100, height - 120), 20, 1, Color.White);
                    }
                    else if (!_propLoader.TextureLoadCompleted)
                    {
                        if (GLOBALS.Font is null)
                            DrawText("Loading Prop Textures", 100, height - 120, 20, Color.White);
                        else
                            DrawTextEx(GLOBALS.Font.Value, "Loading Prop Textures", new Vector2(100, height - 120), 20, 1, Color.White);
                    }
                    else if (!_propLoader.DexBuildCompleted)
                    {
                        if (GLOBALS.Font is null)
                            DrawText("Building Table", 100, height - 120, 20, Color.White);
                        else
                            DrawTextEx(GLOBALS.Font.Value, "Building Table", new Vector2(100, height - 120), 20, 1, Color.White);
                    }
                
                    Printers.DrawProgressBar(new Rectangle(100, height - 100, width - 200, 30), propLoadProgress, _propLoader.TotalProgress, false, Color.White);
                    EndDrawing();
                    
                    continue;
                }
                if (GLOBALS.PropDex is null)
                {
                    if (propDexTask?.IsCompleted == true) GLOBALS.PropDex = propDexTask.Result;
                    
                    var height = GetScreenHeight();
                    
                    BeginDrawing();
                    Printers.DrawSplashScreen(true);
                
                    if (GLOBALS.Font is null)
                        DrawText("Building Prop Dex", 100, height - 120, 20, Color.White);
                    else
                        DrawTextEx(GLOBALS.Font.Value, "Building Prop Dex", new Vector2(100, height - 120), 20, 1, Color.White);
                    
                    EndDrawing();
                    continue;
                }
                
                skip_prop_load:
                // TODO: To be replaced
                if (!isLoadingTexturesDone)
                {
                    // Loading screen

                    loadLoop:
                    
                    if (loadPropTexturesEnumerator.MoveNext())
                    {
                        loadPropTexturesEnumerator.Current?.Invoke();
                        propTexturesLoadProgress++;

                        while (propTexturesLoadProgress % loadRate != 0 && loadPropTexturesEnumerator.MoveNext())
                        {
                            loadPropTexturesEnumerator.Current?.Invoke();
                            propTexturesLoadProgress++;
                        }

                        var width = GetScreenWidth();
                        var height = GetScreenHeight();

                        BeginDrawing();
                        ClearBackground(new(0, 0, 0, 255));

                        DrawTexturePro(
                            GLOBALS.Textures.SplashScreen,
                            new(0, 0, GLOBALS.Textures.SplashScreen.Width, GLOBALS.Textures.SplashScreen.Height),
                            new(0, 0, GLOBALS.MinScreenWidth, GLOBALS.MinScreenHeight),
                            new(0, 0),
                            0,
                            new(255, 255, 255, 255)
                        );

                        if (GLOBALS.Settings.GeneralSettings.DeveloperMode) DrawText("Developer mode active", 50, 300, 16, Color.Yellow);

                        DrawText(GLOBALS.Version, 700, 50, 15, Color.White);
                        DrawText(GLOBALS.RaylibVersion, 700, 70, 15, Color.White);
                        DrawText(GLOBALS.BuildConfiguration, 700, 90, 15, Color.White);
                        
                        
                        if (GLOBALS.Font is null)
                            DrawText("Loading prop textures", 100, height - 120, 20, Color.White);
                        else
                            DrawTextEx(GLOBALS.Font.Value, "Loading prop textures", new Vector2(100, height - 120), 20, 1, Color.White);


                        Printers.DrawProgressBar(new Rectangle(100, height - 100, width - 200, 30), propTexturesLoadProgress, totalPropTexturesLoadProgress, false, Color.White);

                        EndDrawing();
                        
                        continue;
                    }
                    if (loadLightTexturesEnumerator.MoveNext())
                    {
                        loadLightTexturesEnumerator.Current?.Invoke();
                        lightTexturesLoadProgress++;

                        while (lightTexturesLoadProgress % loadRate != 0 && loadLightTexturesEnumerator.MoveNext())
                        {
                            loadLightTexturesEnumerator.Current?.Invoke();
                            lightTexturesLoadProgress++;
                        }

                        var width = GetScreenWidth();
                        var height = GetScreenHeight();

                        BeginDrawing();
                        ClearBackground(new(0, 0, 0, 255));

                        DrawTexturePro(
                            GLOBALS.Textures.SplashScreen,
                            new(0, 0, GLOBALS.Textures.SplashScreen.Width, GLOBALS.Textures.SplashScreen.Height),
                            new(0, 0, GLOBALS.MinScreenWidth, GLOBALS.MinScreenHeight),
                            new(0, 0),
                            0,
                            new(255, 255, 255, 255)
                        );

                        if (GLOBALS.Settings.GeneralSettings.DeveloperMode) DrawText("Developer mode active", 50, 300, 16, Color.Yellow);

                        DrawText(GLOBALS.Version, 700, 50, 15, Color.White);
                        DrawText(GLOBALS.RaylibVersion, 700, 70, 15, Color.White);
                        DrawText(GLOBALS.BuildConfiguration, 700, 90, 15, Color.White);
                        
                        if (GLOBALS.Font is null)
                            DrawText("Loading light brushed", 100, height - 120, 20, Color.White);
                        else
                            DrawTextEx(GLOBALS.Font.Value, "Loading light brushes", new Vector2(100, height - 120), 20, 1, Color.White);

                        //Raylib_CsLo.RayGui.GuiProgressBar(new(100, height - 100, width - 200, 30), "", "", lightTexturesLoadProgress, 0, totalLightTexturesLoadProgress);
                        Printers.DrawProgressBar(new Rectangle(100, height - 100, width - 200, 30), lightTexturesLoadProgress, totalLightTexturesLoadProgress, false, Color.White);
                        
                        EndDrawing();
                        
                        continue;
                    }

                    GLOBALS.Textures.Props = propTextures.Others;
                    GLOBALS.Textures.RopeProps = propTextures.Ropes;
                    GLOBALS.Textures.LongProps = propTextures.Longs;
                    GLOBALS.Textures.LightBrushes = lightTextures.Textures;
                    
                    loadPropTexturesList.Clear();
                    loadLightTexturesList.Clear();

                    isLoadingTexturesDone = true;
                }
                else if (!allCacheTasks.IsCompleted)
                {
                    var width = GetScreenWidth();
                    var height = GetScreenHeight();

                    BeginDrawing();
                    ClearBackground(new(0, 0, 0, 255));

                    DrawTexturePro(
                        GLOBALS.Textures.SplashScreen,
                        new(0, 0, GLOBALS.Textures.SplashScreen.Width, GLOBALS.Textures.SplashScreen.Height),
                        new(0, 0, GLOBALS.MinScreenWidth, GLOBALS.MinScreenHeight),
                        new(0, 0),
                        0,
                        new(255, 255, 255, 255)
                    );

                    DrawText(GLOBALS.Version, 700, 50, 15, Color.White);
                    DrawText(GLOBALS.RaylibVersion, 700, 70, 15, Color.White);
                    DrawText(GLOBALS.BuildConfiguration, 700, 90, 15, Color.White);
                        
                    if (GLOBALS.Font is null)
                        DrawText("Loading cache", 100, height - 120, 20, Color.White);
                    else
                        DrawTextEx(GLOBALS.Font.Value, "Loading cache", new Vector2(100, height - 120), 20, 1, Color.White);

                    EndDrawing();
                    
                    continue;
                }
                // Initialize Renderer Runtime
                else if (GLOBALS.Settings.GeneralSettings.CacheRendererRuntime)
                {
                    if (!isLingoRuntimeInit)
                    {
                        GLOBALS.LingoRuntimeInitTask = Task.Factory.StartNew(() =>
                        {
                            SixLabors.ImageSharp.Configuration.Default.PreferContiguousImageBuffers = true;
                            GLOBALS.LingoRuntime.Init();
                            EditorRuntimeHelpers.RunStartup(GLOBALS.LingoRuntime);
                        });
                    
                        isLingoRuntimeInit = true;
                        
                        var width = GetScreenWidth();
                        var height = GetScreenHeight();
                        
                        BeginDrawing();
                        ClearBackground(new(0, 0, 0, 255));
                    
                        DrawTexturePro(
                            GLOBALS.Textures.SplashScreen,
                            new(0, 0, GLOBALS.Textures.SplashScreen.Width, GLOBALS.Textures.SplashScreen.Height),
                            new(0, 0, GLOBALS.MinScreenWidth, GLOBALS.MinScreenHeight),
                            new(0, 0),
                            0,
                            new(255, 255, 255, 255)
                        );
                    
                        DrawText(GLOBALS.Version, 700, 50, 15, Color.White);
                        DrawText(GLOBALS.RaylibVersion, 700, 70, 15, Color.White);
                        DrawText(GLOBALS.BuildConfiguration, 700, 90, 15, Color.White);
                            
                        if (GLOBALS.Font is null)
                            DrawText("Initializing Renderer Runtime", 100, height - 120, 20, Color.White);
                        else
                            DrawTextEx(GLOBALS.Font.Value, "Initializing Renderer Runtime", new Vector2(100, height - 120), 20, 1, Color.White);
                    
                    
                        //Raylib_CsLo.RayGui.GuiProgressBar(new(100, height - 100, width - 200, 30), "", "", tileTexturesLoadProgress, 0, totalTileTexturesLoadProgress);
                        EndDrawing();
                        continue;
                    }
                    
                    if (!GLOBALS.LingoRuntimeInitTask.IsCompletedSuccessfully)
                    {
                        var faulted = GLOBALS.LingoRuntimeInitTask.IsFaulted;
                        
                        var width = GetScreenWidth();
                        var height = GetScreenHeight();
                        
                        BeginDrawing();
                        ClearBackground(new(0, 0, 0, 255));
                    
                        DrawTexturePro(
                            GLOBALS.Textures.SplashScreen,
                            new(0, 0, GLOBALS.Textures.SplashScreen.Width, GLOBALS.Textures.SplashScreen.Height),
                            new(0, 0, GLOBALS.MinScreenWidth, GLOBALS.MinScreenHeight),
                            new(0, 0),
                            0,
                            new(255, 255, 255, 255)
                        );
                    
                        DrawText(GLOBALS.Version, 700, 50, 15, Color.White);
                        DrawText(GLOBALS.RaylibVersion, 700, 70, 15, Color.White);
                            
                        if (GLOBALS.Font is null)
                            DrawText("Loading tile textures", 100, height - 120, 20, Color.White);
                        else
                            DrawTextEx(GLOBALS.Font.Value, faulted ? "Failed to initialize renderer runtime" : "Initializing Renderer Runtime", new Vector2(100, height - 120), 20, 1, Color.White);
                    
                    
                        //Raylib_CsLo.RayGui.GuiProgressBar(new(100, height - 100, width - 200, 30), "", "", tileTexturesLoadProgress, 0, totalTileTexturesLoadProgress);
                        EndDrawing();
                    
                        if (faulted)
                        {
                            Console.WriteLine(GLOBALS.LingoRuntimeInitTask.Exception);
                            break;
                        }
                        continue;
                    }
                }

                // page preprocessing

                if (GLOBALS.Page == 2 && GLOBALS.Settings.GeometryEditor.LegacyInterface) GLOBALS.Page = 18;
                
                // Globals quick save

                if (_isGuiLocked && _globalSave)
                {
                    BeginDrawing();
                    
                    ClearBackground(Color.Black);
                    
                    DrawText("Please wait..", GetScreenWidth() / 2 - 100, GetScreenHeight() / 2 - 20, 30, new(255, 255, 255, 255));
                    
                    if (_askForPath)
                    {
                        
                        if (_saveFileDialog is null)
                        {
                            if (_saveResult!.IsCompleted)
                            {
                                GLOBALS.Page = 1;
                                _globalSave = false;
                                _isGuiLocked = false;
                            }
                        }
                        else
                        {
                            if (!_saveFileDialog.IsCompleted)
                            {
                                EndDrawing();
                                continue;
                            }
                            if (string.IsNullOrEmpty(_saveFileDialog.Result))
                            {
                                _globalSave = false;
                                _isGuiLocked = false;
                                EndDrawing();
                                continue;
                            }

                            var path = _saveFileDialog.Result;

                            if (_saveResult is null)
                            {
                                _saveResult = SaveProjectAsync(path);
                                EndDrawing();
                                continue;
                            }
                            if (!_saveResult.IsCompleted)
                            {
                                EndDrawing();
                                continue;
                            }

                            var result = _saveResult.Result;

                            if (!result.Success)
                            {
                                _globalSave = false;
                                _failedToSave = true;
                                _isGuiLocked = false;
                                EndDrawing();
                                #if DEBUG
                                if (result.Exception is not null) logger.Error($"Failed to save project: {result.Exception}");
                                #endif
                                _saveResult = null;
                                _saveFileDialog = null;
                                continue;
                            }
                            
                            // export light map
                            {
                                var image = LoadImageFromTexture(GLOBALS.Textures.LightMap.Texture);

                                unsafe
                                {
                                    ImageFlipVertical(&image);
                                }

                                var parent = Directory.GetParent(path)?.FullName ?? GLOBALS.ProjectPath;
                                var name = Path.GetFileNameWithoutExtension(path);

                                ExportImage(image, Path.Combine(parent, name + ".png"));

                                UnloadImage(image);
                            }

                            {
                                _globalSave = false;
                                var parent = Directory.GetParent(_saveFileDialog.Result)?.FullName;

                                GLOBALS.ProjectPath = parent ?? GLOBALS.ProjectPath;
                                GLOBALS.Level.ProjectName = Path.GetFileNameWithoutExtension(_saveFileDialog.Result);

                                _saveFileDialog = null;
                                _saveResult = null;
                                _isGuiLocked = false;

                                // Export level image to cache
                                if (!Directory.Exists(Path.Combine(GLOBALS.Paths.CacheDirectory, "levelpreviews"))) {
                                    Directory.CreateDirectory(Path.Combine(GLOBALS.Paths.CacheDirectory, "levelpreviews"));
                                }

                                using var levelImg = Printers.GenerateLevelReviewImage();

                                ExportImage(levelImg, Path.Combine(GLOBALS.Paths.CacheDirectory, "levelpreviews", GLOBALS.Level.ProjectName+".png"));


                                EndDrawing();
                            }
                        }
                    }
                    else
                    {
                        var path = Path.Combine(GLOBALS.ProjectPath, GLOBALS.Level.ProjectName + ".txt");
                        
                        if (_saveResult is null)
                        {
                            _saveResult = SaveProjectAsync(path);
                            EndDrawing();
                            continue;
                        }
                        if (!_saveResult.IsCompleted)
                        {
                            EndDrawing();
                            continue;
                        }

                        var result = _saveResult.Result;

                        if (!result.Success)
                        {
                            _globalSave = false;
                            _failedToSave = true;
                            _isGuiLocked = false;
                            EndDrawing();
                            #if DEBUG
                            if (result.Exception is not null) logger.Error($"Failed to save project: {result.Exception}");
                            #endif
                            _saveResult = null;
                            _saveFileDialog = null;
                            continue;
                        }
                        
                        // export light map
                        {
                            var image = LoadImageFromTexture(GLOBALS.Textures.LightMap.Texture);

                            unsafe
                            {
                                ImageFlipVertical(&image);
                            }

                            var parent = Directory.GetParent(path)?.FullName ?? GLOBALS.ProjectPath;
                            var name = Path.GetFileNameWithoutExtension(path);

                            ExportImage(image, Path.Combine(parent, name + ".png"));

                            UnloadImage(image);
                        }
                        
                        _globalSave = false;
                        _saveFileDialog = null;
                        _saveResult = null;
                        _isGuiLocked = false;

                        // Export level image to cache
                        if (!Directory.Exists(Path.Combine(GLOBALS.Paths.CacheDirectory, "levelpreviews"))) {
                            Directory.CreateDirectory(Path.Combine(GLOBALS.Paths.CacheDirectory, "levelpreviews"));
                        }

                        using var levelImg = Printers.GenerateLevelReviewImage();

                        ExportImage(levelImg, Path.Combine(GLOBALS.Paths.CacheDirectory, "levelpreviews", GLOBALS.Level.ProjectName+".png"));

                        EndDrawing();
                    }
                }
                else if (_renderWindow is not null)
                {
                    BeginDrawing();
                    
                    ClearBackground(Color.Black);
                    
                    rlImGui.Begin();
                    
                    // True == window is closed
                    if (_renderWindow.DrawWindow())
                    {
                        _renderWindow.Dispose();
                        _renderWindow = null;
                        
                        // Freeing as much memory as possible
                        GC.Collect(2, GCCollectionMode.Aggressive, true, true);
                        GC.WaitForFullGCComplete();
                    }

                    rlImGui.End();
                    
                    EndDrawing();
                }
                // turn into popup modal
                else if (GLOBALS.NavSignal == 4)
                {
                    BeginDrawing();
                    ClearBackground(Color.Gray);
                    rlImGui.Begin();
                    
                    if (!ImGui.Begin("About##AboutLeditor", 
                            ImGuiWindowFlags.NoCollapse | 
                            ImGuiWindowFlags.NoDocking | 
                            ImGuiWindowFlags.NoMove)) 
                        GLOBALS.NavSignal = 0;

                    ImGui.SetWindowPos((new Vector2(GetScreenWidth(), GetScreenHeight()) - ImGui.GetWindowSize())/2);
                    
                    var availableSpace = ImGui.GetContentRegionAvail();
                    
                    ImGui.Text("Created By: Henry Markle");
                    ImGui.Text("Renderer: Drizzle - embedded with the help of pkhead (chromosoze)");
                    
                    ImGui.Spacing();
                    
                    ImGui.Text(GLOBALS.Version);
                    ImGui.Text(GLOBALS.RaylibVersion);
                    ImGui.Text(GLOBALS.BuildConfiguration);
                    
                    ImGui.Spacing();

                    if (ImGui.Button("Henry's Leditor", availableSpace with { Y = 20 }))
                        OpenURL("https://github.com/HenryMarkle/Leditor");

                    if (ImGui.IsItemHovered())
                    {
                        if (ImGui.BeginTooltip())
                        {
                            ImGui.Text("https://github.com/HenryMarkle/Leditor");
                            ImGui.EndTooltip();
                        }
                    }
                    
                    if (ImGui.Button("Drizzle", availableSpace with { Y = 20 }))
                        OpenURL("https://github.com/HenryMarkle/Drizzle");
                    
                    if (ImGui.IsItemHovered())
                    {
                        if (ImGui.BeginTooltip())
                        {
                            ImGui.Text("https://github.com/HenryMarkle/Drizzle");
                            ImGui.EndTooltip();
                        }
                    }
                    
                    if (ImGui.Button("pkhead", availableSpace with { Y = 20 }))
                        OpenURL("https://github.com/pkhead");
                    
                    if (ImGui.IsItemHovered())
                    {
                        if (ImGui.BeginTooltip())
                        {
                            ImGui.Text("https://github.com/pkhead");
                            ImGui.EndTooltip();
                        }
                    }
                    
                    ImGui.Spacing();
                    
                    if (ImGui.Button("Close", availableSpace with { Y = 20 })) 
                        GLOBALS.NavSignal = 0;
                    ImGui.End();
                    
                    rlImGui.End();
                    EndDrawing();
                }
                else
                {
                    #region GlobalShortcuts
                    {
                        var ctrl = IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.RightControl);
                        var shift = IsKeyDown(KeyboardKey.LeftShift) || IsKeyDown(KeyboardKey.RightShift);
                        var alt = IsKeyDown(KeyboardKey.LeftAlt) || IsKeyDown(KeyboardKey.RightAlt);

                        // TODO: Move to using Pager
                        
                        if (!GLOBALS.LockNavigation) {
                            if (gShortcuts.ToMainPage.Check(ctrl, shift, alt))
                            {
#if DEBUG
                                logger.Debug("Going to page 1");
#endif
                                GLOBALS.Page = 1;
                            }

                            if (gShortcuts.ToGeometryEditor.Check(ctrl, shift, alt))
                            {
#if DEBUG
                                logger.Debug("Going to page 2");
#endif
                                GLOBALS.Page = 2;
                            }

                            if (gShortcuts.ToTileEditor.Check(ctrl, shift, alt))
                            {
#if DEBUG
                                logger.Debug("Going to page 3");
#endif
                                GLOBALS.Page = 3;
                            }

                            if (gShortcuts.ToCameraEditor.Check(ctrl, shift, alt))
                            {
#if DEBUG
                                logger.Debug("Going to page 4");
#endif
                                GLOBALS.Page = 4;
                            }

                            if (gShortcuts.ToLightEditor.Check(ctrl, shift, alt))
                            {
#if DEBUG
                                logger.Debug("Going to page 5");
#endif
                                GLOBALS.Page = 5;
                            }

                            if (gShortcuts.ToDimensionsEditor.Check(ctrl, shift, alt))
                            {
#if DEBUG
                                logger.Debug("Going to page 6");
#endif
                                GLOBALS.Page = 6;
                            }

                            if (gShortcuts.ToEffectsEditor.Check(ctrl, shift, alt))
                            {
#if DEBUG
                                logger.Debug("Going to page 7");
#endif
                                GLOBALS.Page = 7;
                            }

                            if (gShortcuts.ToPropsEditor.Check(ctrl, shift, alt))
                            {
#if DEBUG
                                logger.Debug("Going to page 8");
#endif
                                GLOBALS.Page = 8;
                            }

                            if (gShortcuts.ToSettingsPage.Check(ctrl, shift, alt))
                            {
#if DEBUG
                                logger.Debug("Going to page 9");
#endif
                                GLOBALS.Page = 9;
                            }
                        }

                        if (gShortcuts.Open.Check(ctrl, shift, alt)) GLOBALS.Page = 0;
                        else if (gShortcuts.QuickSave.Check(ctrl, shift, alt) || GLOBALS.NavSignal == 1)
                        {
                            if (string.IsNullOrEmpty(GLOBALS.ProjectPath))
                            {
                                GLOBALS.PreviousPage = GLOBALS.Page;
                                GLOBALS.Page = 12;
                            }
                            else
                            {
                                _askForPath = false;
                            }

                            _globalSave = true;
                            _isGuiLocked = true;
                            GLOBALS.NavSignal = 0;
                        }
                        else if (gShortcuts.QuickSaveAs.Check(ctrl, shift, alt) || GLOBALS.NavSignal == 2)
                        {
                            // _askForPath = true;
                            // _saveFileDialog = Utils.SetFilePathAsync();
                            // _isGuiLocked = true;
                            // _globalSave = true;
                            // GLOBALS.NavSignal = 0;

                            GLOBALS.PreviousPage = GLOBALS.Page;
                            GLOBALS.Page = 12;
                        }
                        else if (gShortcuts.Render.Check(ctrl, shift, alt) || GLOBALS.NavSignal == 3)
                        {
                            logger.Debug($"Rendering level \"{GLOBALS.Level.ProjectName}\"");

                            _renderWindow = new DrizzleRenderWindow();
                            GLOBALS.NavSignal = 0;
                        }
                    }
                    #endregion
                    
                    // page switch

                    switch (GLOBALS.Page)
                    {

                        case 0: startPage.Draw(); break;
                        case 1: mainPage.Draw(); break;
                        case 2: experimentalGeometryPage.Draw(); break;
                        case 3: tilePage.Draw(); break;
                        case 4: camerasPage.Draw(); break;
                        case 5: lightPage.Draw(); break;
                        case 6: dimensionsPage.Draw(); break;
                        case 7: effectsPage.Draw(); break;
                        case 8: propsPage.Draw(); break;
                        case 9: settingsPage.Draw(); break;
                        case 11: newLevelPage.Draw(); break;
                        case 12: savePage.Draw(); break;
                        case 13: failedTileCheckOnLoadPage.Draw(); break;
                        case 14: assetsNukedPage.Draw(); break;
                        case 15: missingAssetsPage.Draw(); break;
                        case 16: missingTexturesPage.Draw(); break;
                        case 17: missingInitFilePage.Draw(); break;
                        case 18: geoPage.Draw(); break;
                        case 19: missingPropTexturesPage.Draw(); break;
                        case 99: deathScreen.Draw(); break;
                        
                        default:
                            GLOBALS.Page = GLOBALS.PreviousPage;
                            break;
                    }
                }
            }
            catch (Data.Tiles.Exceptions.DuplicateTileCategoryException dtce) {
                logger.Fatal($"Found duplicate tile category \"{(dtce.Category)}\"");
                CloseWindow();
                break;
            }
            catch (Data.Tiles.Exceptions.DuplicateTileDefinitionException dtde) {
                logger.Fatal($"Found duplicate tile definition \"{(dtde.Name)}\"");
                CloseWindow();
                break;
            }
            catch (Data.Tiles.Exceptions.TileCategoryNotFoundException tcnfe) {
                logger.Fatal($"Tile {(string.IsNullOrEmpty(tcnfe.Tile) ? "" : $"\"{(tcnfe.Tile)}\"")} claimed to belong to an non-existent category \"{(tcnfe.Category)}\"");
                CloseWindow();
                break;
            }
            catch (Exception e)
            {
                logger.Fatal($"Bruh Moment detected: loop try-catch block has caught an unexpected error: {e}");

                if (GLOBALS.Settings.Misc.FunnyDeathScreen)
                {
                    var screenshot = LoadImageFromScreen();
                    screenshotTexture = LoadTextureFromImage(screenshot);
                    UnloadImage(screenshot);
                }

                deathScreen = new()
                {
                    Screenshot = screenshotTexture,
                    Exception = e,
                    Logger = logger
                };
                
                

                GLOBALS.Page = 99; // game over
            }
        }

        logger.Debug("Exiting main loop");
        logger.Information("unloading textures");
        
        UnloadImage(icon);
        
        UnloadTexture(GLOBALS.Textures.MissingTile);

        foreach (var texture in GLOBALS.Textures.GeoMenu) UnloadTexture(texture);
        foreach (var texture in GLOBALS.Textures.GeoBlocks) UnloadTexture(texture);
        foreach (var texture in GLOBALS.Textures.GeoStackables) UnloadTexture(texture);
        foreach (var texture in GLOBALS.Textures.LightBrushes) UnloadTexture(texture);
        foreach (var texture in GLOBALS.Textures.PropMenuCategories) UnloadTexture(texture);
        foreach (var category in GLOBALS.Textures.Props) { foreach (var texture in category) UnloadTexture(texture); }
        foreach (var texture in GLOBALS.Textures.LongProps) UnloadTexture(texture);
        foreach (var texture in GLOBALS.Textures.RopeProps) UnloadTexture(texture);
        foreach (var texture in GLOBALS.Textures.PropEditModes) UnloadTexture(texture);
        foreach (var texture in GLOBALS.Textures.PropGenerals) UnloadTexture(texture);
        foreach (var texture in GLOBALS.Textures.GeoInterface) UnloadTexture(texture);
        
        logger.Debug("Unloading light map");

        UnloadRenderTexture(GLOBALS.Textures.LightMap);
        
        UnloadRenderTexture(GLOBALS.Textures.TileSpecs);
        UnloadRenderTexture(GLOBALS.Textures.PropDepth);
        UnloadRenderTexture(GLOBALS.Textures.DimensionsVisual);
        
        logger.Debug("Unloading shaders");

        UnloadShader(GLOBALS.Shaders.TilePreview);
        UnloadShader(GLOBALS.Shaders.ShadowBrush);
        UnloadShader(GLOBALS.Shaders.LightBrush);
        UnloadShader(GLOBALS.Shaders.ApplyLightBrush);
        UnloadShader(GLOBALS.Shaders.ApplyShadowBrush);
        UnloadShader(GLOBALS.Shaders.Prop);
        UnloadShader(GLOBALS.Shaders.StandardProp);
        UnloadShader(GLOBALS.Shaders.VariedStandardProp);
        UnloadShader(GLOBALS.Shaders.SoftProp);
        UnloadShader(GLOBALS.Shaders.VariedSoftProp);
        UnloadShader(GLOBALS.Shaders.SimpleDecalProp);
        UnloadShader(GLOBALS.Shaders.VariedDecalProp);
        UnloadShader(GLOBALS.Shaders.ColoredTileProp);
        UnloadShader(GLOBALS.Shaders.ColoredBoxTileProp);
        UnloadShader(GLOBALS.Shaders.LongProp);
        UnloadShader(GLOBALS.Shaders.DefaultProp);
        UnloadShader(GLOBALS.Shaders.PreviewColoredTileProp);
        UnloadShader(GLOBALS.Shaders.LightMapStretch);
        
        UnloadShader(GLOBALS.Shaders.VFlip);
        
        UnloadRenderTexture(GLOBALS.Textures.GeneralLevel);
        
        // Unloading Pages
        
        logger.Debug("Unloading Pages");
        
        // GLOBALS.Pager.Dispose();
        
        geoPage.Dispose();
        tilePage.Dispose();
        camerasPage.Dispose();
        lightPage.Dispose();
        dimensionsPage.Dispose();
        deathScreen.Dispose();
        effectsPage.Dispose();
        propsPage.Dispose();
        mainPage.Dispose();
        startPage.Dispose();
        savePage.Dispose();
        failedTileCheckOnLoadPage.Dispose();
        assetsNukedPage.Dispose();
        missingAssetsPage.Dispose();
        missingTexturesPage.Dispose();
        missingPropTexturesPage.Dispose();
        missingInitFilePage.Dispose();
        experimentalGeometryPage.Dispose();
        settingsPage.Dispose();

        //
        rlImGui.Shutdown();
        //

        {
            StringBuilder builder = new();

            foreach (var (path, _) in GLOBALS.RecentProjects)
                builder.AppendLine(path);
            
            File.WriteAllText(GLOBALS.Paths.RecentProjectsPath, builder.ToString());
        }

        CloseWindow();

        logger.Information("program has terminated");
    }
}
