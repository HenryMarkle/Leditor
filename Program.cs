global using Raylib_cs;
using System.Globalization;
using static Raylib_cs.Raylib;

using rlImGui_cs;

using System.Numerics;
using Leditor.Lingo;
using System.Text.Json;
using Serilog;
using System.Security.Cryptography;
using System.Threading;
using ImGuiNET;
using Leditor.Renderer;

#nullable enable

namespace Leditor;

public interface IPage { void Draw(); }

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

    private static Texture2D[] LoadSettingsPreviewTextures() =>
    [
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "Bigger Head.png")),
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "Crossbox B.png")),
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "mega chimney A.png")),
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "Big Ball.png")),
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "Big Stone Marked.png")),
        LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "Big Fan.png"))
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
            var strTask = Leditor.Lingo.Exporters.ExportAsync(GLOBALS.Level);

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

    // Unused
    private static ((string, Color)[], InitTile[][]) LoadTileInitFromRenderer()
    {
        var path = Path.Combine(GLOBALS.Paths.RendererDirectory, "Graphics", "Init.txt");

        var text = File.ReadAllText(path).ReplaceLineEndings();

        return Importers.GetTileInit(text);
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

    // This is used to check whether the Init.txt file has been altered
    private static string InitChecksum => "77-D1-5E-5F-D7-EF-80-6B-0B-12-30-C1-7E-39-A6-CD-C1-9A-8A-7B-E6-E4-F8-EA-15-3B-85-89-73-BE-9B-0B-AD-35-8C-9E-89-AE-34-42-57-1B-A6-A8-BE-8A-9B-CB-97-3E-AE-33-98-E1-51-92-74-24-2F-DF-81-E6-58-A2";

    // Unused
    private static bool CheckInit()
    {
        using var stream = File.OpenRead(GLOBALS.Paths.TilesInitPath);
        using SHA512 sha = SHA512.Create();

        var hash = sha.ComputeHash(stream);

        return BitConverter.ToString(hash) == InitChecksum;
    }
    
    // MAIN FUNCTION
    private static void Main()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

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

        // check for the assets folder and subfolders
        var integrityFailed = false;
        foreach (var (directory, exists) in GLOBALS.Paths.DirectoryIntegrity)
        {
            if (!exists)
            {
                logger.Fatal($"critical directory not found: \"{directory}\"");
                integrityFailed = true;
            }
            
            if (integrityFailed) goto skip_file_check;
        }

        foreach (var (file, exists) in GLOBALS.Paths.FileIntegrity)
        {
            if (!exists)
            {
                logger.Fatal($"critical file not found: \"{file}]\"");
                integrityFailed = true;
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

        // check for projects folder

        if (!Directory.Exists(GLOBALS.Paths.ProjectsDirectory))
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
        
        // Check for renderer

        if (File.Exists(Path.Combine(GLOBALS.Paths.RendererDirectory, "Drizzle.ConsoleApp.exe")))
        {
            GLOBALS.RendererExists = true;
        }

        logger.Information("Initializing data");

        const string version = "Henry's Leditor v0.9.49";
        const string raylibVersion = "Raylib v5.0.0";
        
        logger.Information(version);
        logger.Information(raylibVersion);
        
        // Load tiles and props

        logger.Information("Indexing tiles and props");

        try
        {
            (GLOBALS.TileCategories, GLOBALS.Tiles) = LoadTileInitFromRenderer();
        }
        catch (Exception e)
        {
            logger.Fatal($"Failed to load tiles init: {e}");
            throw new Exception(innerException: e, message: $"Failed to load tiles init: {e}");
        }
        
        try
        {
            var materialsInit = LoadMaterialInit();

            GLOBALS.MaterialCategories = [..GLOBALS.MaterialCategories, ..materialsInit.Item1];
            GLOBALS.Materials = [..GLOBALS.Materials, ..materialsInit.Item2];
        }
        catch (Exception e)
        {
            logger.Fatal($"Failed to load materials init: {e}");
            throw new Exception(innerException: e, message: $"Failed to load materials init: {e}");
        }

        try
        {
            (GLOBALS.PropCategories, GLOBALS.Props) = LoadPropInitFromRenderer();
        }
        catch (Exception e)
        {
            logger.Fatal($"Failed to load props init: {e}");
            throw new Exception(innerException: e, message: $"Failed to load props init: {e}");
        }

        //

        // Merge tile packages
        
        logger.Debug("Loading custom tiles");

        // List<string> tileInitLoadDirs = [GLOBALS.Paths.TilesAssetsDirectory];
        var tilePackagesTask = Task.FromResult<TileInitLoadInfo[]>([]);
        (string name, Color color)[] loadedPackageTileCategories = [];
        InitTile[][] loadedPackageTiles = [];
        
        if (Directory.Exists(GLOBALS.Paths.TilePackagesDirectory))
        {
            try
            {
                tilePackagesTask = LoadTileInitPackages();
                
                tilePackagesTask.Wait();

                var packages = tilePackagesTask.Result;

                if (packages.Length > 0)
                {
                    loadedPackageTileCategories = packages.SelectMany(p => p.Categories).ToArray();
                    
                    loadedPackageTiles = packages.SelectMany(p => p.Tiles).ToArray();

                    // tileInitLoadDirs = [..tileInitLoadDirs, ..packages.Select(p => p.LoadDirectory)];
                }
            }
            catch (Exception e)
            {
                logger.Error($"Failed to load tile packages: {e}"); 
            }
        }
        
        // MERGE TILES
        
        logger.Debug("Merging custom tiles");
        
        GLOBALS.TileCategories = [..GLOBALS.TileCategories, ..loadedPackageTileCategories];
        GLOBALS.Tiles = [..GLOBALS.Tiles, ..loadedPackageTiles];
        
        // Load Tile Textures

        var tileTextures = new TileTexturesLoader();
        var propTextures = new PropTexturesLoader();
        var lightTextures = new LightTexturesLoader();
        
        // 1. Get all texture paths
        
        var tileImagePaths = GLOBALS.Tiles
            .Select((category, index) =>
                category.Select(tile => 
                        Path.Combine(GLOBALS.Paths.RendererDirectory, "Graphics", $"{tile.Name}.png"))
                    .ToArray()
            )
            .ToArray();
        
        var packTileImagePaths = tilePackagesTask.Result
            .SelectMany(result => result.Tiles
                .Select(category => category
                    .Select(tile =>
                    {
                        var path = Path.Combine(result.LoadDirectory, $"{tile.Name}.png");
                        if (!File.Exists(path)) logger.Error($"Missing tile texture: \"{path}\"");

                        return path;
                    })
                    .ToArray())
                .ToArray()
            ).ToArray();
        
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
        
        var lightImagePaths = Directory
            .GetFileSystemEntries(GLOBALS.Paths.LightAssetsDirectory)
            .Where(e => e.EndsWith(".png"))
            .Select(e =>
            {
                logger.Debug($"Loading light texture \"{e}\""); 
                return e; 
            })
            .ToArray();
        
        // 2. Load the images

        var loadTileImagesTask = tileTextures.PrepareFromPathsAsync([..tileImagePaths, ..packTileImagePaths]);
        var loadPropImagesTask = propTextures.PrepareFromPathsAsync(ropePropImagePaths, longPropImagePaths, otherPropImagePaths);
        var loadLightImagesTask = lightTextures.PrepareFromPathsAsync(lightImagePaths);
        
        // 3. Await loading in later stages
        
        //

        logger.Information("Initializing window");

        var icon = LoadImage("icon.png");

        SetConfigFlags(ConfigFlags.ResizableWindow);
        SetConfigFlags(ConfigFlags.Msaa4xHint);
        
        #if DEBUG
        // TODO: Change this
        SetTraceLogLevel(TraceLogLevel.Error);
        #else
        SetTraceLogLevel(TraceLogLevel.Error);
        #endif
        
        //----------------------------------------------------------------------------
        // No texture loading prior to this point
        //----------------------------------------------------------------------------
        InitWindow(GLOBALS.MinScreenWidth, GLOBALS.MinScreenHeight, "Henry's Leditor");
        //
        
        if (!GLOBALS.Settings.GeneralSettings.DefaultFont)
        {
            GLOBALS.Font = LoadFont(Path.Combine(GLOBALS.Paths.FontsDirectory, "oswald", "Oswald-Regular.ttf"));
        }
        
        SetWindowIcon(icon);
        SetWindowMinSize(GLOBALS.MinScreenWidth, GLOBALS.MinScreenHeight);
        SetExitKey(KeyboardKey.Null);

        // The splashscreen
        GLOBALS.Textures.SplashScreen = LoadTexture(Path.Combine(GLOBALS.Paths.ExecutableDirectory, "splashscreen.png"));

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

        // These two are going to be populated with textures later
        GLOBALS.Textures.Tiles = [];

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

        Texture2D[] settingsPreviewTextures = LoadSettingsPreviewTextures();

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
        //

        SetTargetFPS(GLOBALS.Settings.Misc.FPS);

        GLOBALS.Camera = new Camera2D { Zoom = 1f };

        float initialFrames = 0;

        Texture2D? screenshotTexture = null;

        logger.Information("Initializing pages");
        
        // Initialize pages

        GeoEditorPage geoPage = new(logger);
        TileEditorPage tilePage = new(logger);
        CamerasEditorPage camerasPage = new(logger);
        LightEditorPage lightPage = new(logger);
        DimensionsEditorPage dimensionsPage = new(logger);
        DeathScreen deathScreen = new(logger, null, null);
        EffectsEditorPage effectsPage = new(logger);
        PropsEditorPage propsPage = new(logger);
        MainPage mainPage = new(logger);
        StartPage startPage = new(logger);
        FailedTileCheckOnLoadPage failedTileCheckOnLoadPage = new(logger);
        AssetsNukedPage assetsNukedPage = new(logger);
        MissingAssetsPage missingAssetsPage = new(logger);
        MissingTexturesPage missingTexturesPage = new(logger);
        MissingPropTexturesPage missingPropTexturesPage = new(logger);
        MissingInitFilePage missingInitFilePage = new(logger);
        ExperimentalGeometryPage experimentalGeometryPage = new(logger);
        SettingsPage settingsPage = new(logger);
        
        logger.Information("Initializing events");
        
        // Page event handlers
        startPage.ProjectLoaded += propsPage.OnProjectLoaded;
        // startPage.ProjectLoaded += savePage.OnProjectLoaded;
        startPage.ProjectLoaded += mainPage.OnLevelLoadedFromStart;
        startPage.ProjectLoaded += dimensionsPage.OnProjectLoaded;
        
        mainPage.ProjectLoaded += propsPage.OnProjectLoaded;
        // mainPage.ProjectLoaded += savePage.OnProjectLoaded;
        mainPage.ProjectLoaded += dimensionsPage.OnProjectLoaded;
        
        dimensionsPage.ProjectCreated += propsPage.OnProjectCreated;
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
        
        // Tile & Prop Textures

        Task.WaitAll([loadTileImagesTask, loadPropImagesTask, loadLightImagesTask]);

        var loadTileTexturesList = loadTileImagesTask.Result;
        var loadPropTexturesList = loadPropImagesTask.Result;
        var loadLightTexturesList = loadLightImagesTask.Result;
        
        using var loadTileTexturesEnumerator = loadTileTexturesList.GetEnumerator();
        using var loadPropTexturesEnumerator = loadPropTexturesList.GetEnumerator();
        using var loadLightTexturesEnumerator = loadLightTexturesList.GetEnumerator();

        var tileTexturesLoadProgress = 0;
        var propTexturesLoadProgress = 0;
        var lightTexturesLoadProgress = 0;

        var totalTileTexturesLoadProgress = loadTileTexturesList.Count;
        var totalPropTexturesLoadProgress = loadPropTexturesList.Count;
        var totalLightTexturesLoadProgress = loadLightTexturesList.Count;

        var loadRate = GLOBALS.Settings.Misc.TileImageScansPerFrame;

        var isLoadingTexturesDone = false;
        
        logger.Information("Begin main loop");
        
        while (!WindowShouldClose())
        {
            try
            {
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


                    if (initialFrames > 60) DrawText(version, 700, 50, 15, Color.White);

                    if (initialFrames > 70)
                    {
                        DrawText(raylibVersion, 700, 70, 15, Color.White);
                        if (GLOBALS.Settings.GeneralSettings.DeveloperMode) DrawText("Developer mode active", 50, 300, 16, Color.Yellow);
                    }

                    if (initialFrames > 90)
                    {
                        if (integrityFailed) 
                            DrawText("missing resources", 700, 320, 16, new(252, 38, 38, 255));
                    }

                    EndDrawing();

                    continue;
                }
                #endregion

                // First, check if the folders exist at all
                if (integrityFailed) GLOBALS.Page = 15;
                
                // Then check tile textures individually
                else if (!isLoadingTexturesDone)
                {
                    // Loading screen

                    loadLoop:
                    
                    if (loadTileTexturesEnumerator.MoveNext())
                    {
                        loadTileTexturesEnumerator.Current?.Invoke();
                        tileTexturesLoadProgress++;
                        
                        while (tileTexturesLoadProgress % loadRate != 0 && loadTileTexturesEnumerator.MoveNext())
                        {
                            loadTileTexturesEnumerator.Current?.Invoke();
                            tileTexturesLoadProgress++;
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

                        DrawText(version, 700, 50, 15, Color.White);
                        DrawText(raylibVersion, 700, 70, 15, Color.White);
                        
                        if (GLOBALS.Font is null)
                            DrawText("Loading tile textures", 100, height - 120, 20, Color.White);
                        else
                            DrawTextEx(GLOBALS.Font.Value, "Loading tile textures", new Vector2(100, height - 120), 20, 1, Color.White);


                        //Raylib_CsLo.RayGui.GuiProgressBar(new(100, height - 100, width - 200, 30), "", "", tileTexturesLoadProgress, 0, totalTileTexturesLoadProgress);
                        EndDrawing();

                        if (tileTexturesLoadProgress % loadRate != 0) goto loadLoop;
                        
                        continue;
                    }
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

                        DrawText(version, 700, 50, 15, Color.White);
                        DrawText(raylibVersion, 700, 70, 15, Color.White);
                        
                        
                        if (GLOBALS.Font is null)
                            DrawText("Loading prop textures", 100, height - 120, 20, Color.White);
                        else
                            DrawTextEx(GLOBALS.Font.Value, "Loading prop textures", new Vector2(100, height - 120), 20, 1, Color.White);


                        //Raylib_CsLo.RayGui.GuiProgressBar(new(100, height - 100, width - 200, 30), "", "", propTexturesLoadProgress, 0, totalPropTexturesLoadProgress);
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

                        DrawText(version, 700, 50, 15, Color.White);
                        DrawText(raylibVersion, 700, 70, 15, Color.White);
                        
                        if (GLOBALS.Font is null)
                            DrawText("Loading light brushed", 100, height - 120, 20, Color.White);
                        else
                            DrawTextEx(GLOBALS.Font.Value, "Loading light brushes", new Vector2(100, height - 120), 20, 1, Color.White);

                        //Raylib_CsLo.RayGui.GuiProgressBar(new(100, height - 100, width - 200, 30), "", "", lightTexturesLoadProgress, 0, totalLightTexturesLoadProgress);
                        EndDrawing();
                        
                        continue;
                    }

                    GLOBALS.Textures.Tiles = tileTextures.Textures;
                    GLOBALS.Textures.Props = propTextures.Others;
                    GLOBALS.Textures.RopeProps = propTextures.Ropes;
                    GLOBALS.Textures.LongProps = propTextures.Longs;
                    GLOBALS.Textures.LightBrushes = lightTextures.Textures;
                    
                    loadTileTexturesList.Clear();
                    loadPropTexturesList.Clear();
                    loadLightTexturesList.Clear();

                    isLoadingTexturesDone = true;
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
                else
                {
                    {
                        var ctrl = IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.RightControl);
                        var shift = IsKeyDown(KeyboardKey.LeftShift) || IsKeyDown(KeyboardKey.RightShift);
                        var alt = IsKeyDown(KeyboardKey.LeftAlt) || IsKeyDown(KeyboardKey.RightAlt);

                        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.QuickSave.Check(ctrl, shift, alt))
                        {
                            if (string.IsNullOrEmpty(GLOBALS.ProjectPath))
                            {
                                _askForPath = true;
                                _saveFileDialog = Utils.SetFilePathAsync();
                            }
                            else
                            {
                                _askForPath = false;
                            }

                            _globalSave = true;
                            _isGuiLocked = true;
                        }
                        else if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.QuickSaveAs.Check(ctrl, shift, alt))
                        {
                            _askForPath = true;
                            _saveFileDialog = Utils.SetFilePathAsync();
                            _isGuiLocked = true;
                            _globalSave = true;
                        }
                        else if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.Render.Check(ctrl, shift, alt))
                        {
                            logger.Debug($"Rendering level \"{GLOBALS.Level.ProjectName}\"");

                            _renderWindow = new DrizzleRenderWindow();
                        }
                    }
                    
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
                        // case 11: loadPage.Draw(); break;
                        // case 12: savePage.Draw(); break;
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
            catch (Exception e)
            {
                logger.Fatal($"Bruh Moment detected: loop try-catch block has caught an unexpected error: {e}");

                if (GLOBALS.Settings.Misc.FunnyDeathScreen)
                {
                    var screenshot = LoadImageFromScreen();
                    screenshotTexture = LoadTextureFromImage(screenshot);
                    UnloadImage(screenshot);
                }

                deathScreen = new(logger, screenshotTexture, e);

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
        foreach (var category in GLOBALS.Textures.Tiles) { foreach (var texture in category) UnloadTexture(texture); }
        foreach (var texture in GLOBALS.Textures.PropMenuCategories) UnloadTexture(texture);
        foreach (var category in GLOBALS.Textures.Props) { foreach (var texture in category) UnloadTexture(texture); }
        foreach (var texture in GLOBALS.Textures.LongProps) UnloadTexture(texture);
        foreach (var texture in GLOBALS.Textures.RopeProps) UnloadTexture(texture);
        foreach (var texture in GLOBALS.Textures.PropEditModes) UnloadTexture(texture);
        foreach (var texture in GLOBALS.Textures.PropGenerals) UnloadTexture(texture);
        foreach (var texture in settingsPreviewTextures) UnloadTexture(texture);
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

        // unloadTileImages?.Wait();
        
        //
        rlImGui.Shutdown();
        //

        CloseWindow();

        logger.Information("program has terminated");
    }
}
