global using Raylib_cs;
using System.Globalization;
using static Raylib_cs.Raylib;
using Leditor.Types;

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
using System.Diagnostics;

#nullable enable

namespace Leditor;

class Program
{
    // Used to load geo blocks menu item textures.
    // Do not alter the indices, and do NOT call before InitWindow()
    private static Texture2D[] LoadUiTextures() => [
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ui/solid.png")),             // 0
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ui/air.png")),               // 1
        // LoadTexture("assets/geo/ui/slopebr.png"),     
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ui/slopebl.png")),           // 2
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ui/multisolid.png")),        // 3
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ui/multiair.png")),          // 4
        // LoadTexture("assets/geo/ui/slopetr.png"),        
        // LoadTexture("assets/geo/ui/slopetl.png"),        
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ui/platform.png")),          // 5
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ui/move.png")),              // 6
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ui/rock.png")),              // 7
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ui/spear.png")),             // 8
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ui/crack.png")),             // 9
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ui/ph.png")),                // 10
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ui/pv.png")),                // 11
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ui/glass.png")),             // 12
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ui/backcopy.png")),          // 13
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ui/entry.png")),             // 14
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ui/shortcut.png")),          // 15
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ui/den.png")),               // 16
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ui/passage.png")),           // 17
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ui/bathive.png")),           // 18
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ui/waterfall.png")),         // 19
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ui/scav.png")),              // 20
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ui/wack.png")),              // 21
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ui/garbageworm.png")),       // 22
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ui/worm.png")),              // 23
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ui/forbidflychains.png")),   // 24
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ui/clearall.png")),          // 25
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ui/save-to-memory.png")),    // 26
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ui/load-from-memory.png")),  // 27
    ];

    // Used to load geo block textures.
    // Do not alter the indices, and do NOT call before InitWindow()
    private static Texture2D[] LoadGeoTextures() => [
        // 0: air
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/solid.png")),
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/cbl.png")),
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/cbr.png")),
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ctl.png")),
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ctr.png")),
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/platform.png")),
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/entryblock.png")),
        // 7: NONE
        // 8: NONE
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/thickglass.png")),
    ];


    // Used to load geo stackables textures.
    // And you guessed it: do not alter the indices, and do NOT call before InitWindow()
    static Texture2D[] LoadStackableTextures() => [
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/ph.png")),             // 0
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/pv.png")),             // 1
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/bathive.png")),        // 2
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/dot.png")),            // 3
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/crackbl.png")),        // 4
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/crackbr.png")),        // 5
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/crackc.png")),         // 6
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/crackh.png")),         // 7
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/cracklbr.png")),       // 8
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/cracktbr.png")),       // 9
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/cracktl.png")),        // 10
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/cracktlb.png")),       // 11
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/cracktlr.png")),       // 12
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/cracktr.png")),        // 13
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/cracku.png")),         // 14
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/crackv.png")),         // 15
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/garbageworm.png")),    // 16
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/scav.png")),           // 17
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/rock.png")),           // 18
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/waterfall.png")),      // 19
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/wack.png")),           // 20
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/worm.png")),           // 21
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/entryb.png")),         // 22
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/entryl.png")),         // 23
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/entryr.png")),         // 24
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/entryt.png")),         // 25
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/looseentry.png")),     // 26
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/passage.png")),        // 27
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/den.png")),            // 28
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/spear.png")),          // 29
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/forbidflychains.png")),// 30
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/crackb.png")),         // 31
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/crackr.png")),         // 32
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/crackt.png")),         // 33
        LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "geo/crackl.png")),         // 34
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

    public static Texture2D[] LoadInternalMaterialTextures() => Directory.Exists(Path.Combine(GLOBALS.Paths.AssetsDirectory, "materials")) 
        ? [
          LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "materials", "Concrete.png")),        // 0
          LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "materials", "RainStone.png")),       // 1
          LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "materials", "Bricks.png")),          // 2
          LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "materials", "Non-Slip Metal.png")),  // 3
          LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "materials", "Asphalt.png")),         // 4
          LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "materials", "Small Pipes.png")),     // 5
          LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "materials", "Chaotic Stone.png")),   // 6
          LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "materials", "Random Machines.png")), // 7
          LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "materials", "Trash.png")),           // 8
        ]
        
        : [];

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


    private static bool _downloadingAssets;
    private static string _downloadAssetsRepoUrl = "";
    private static Task<string>? _downloadAssetsTask = null;
    private static Task? _movingAssetsTask = null;
    private static bool _downloadExceptionLogged;
    private static bool _moveExceptionLogged;
    private static float _downloadProgress;
    private static bool _deletingRepo;

    private static long _downloadSpinFrames;
    private static int _downloadSpinFrame;



    public static bool HandleAssetsDownloadProgress(LibGit2Sharp.TransferProgress progress) {
        _downloadProgress = progress.ReceivedObjects / progress.TotalObjects * 100;
        return true;
    }


    private static bool _askForPath;

    private static bool _isGuiLocked;

    // ImGui ini file name allocation
    private static nint _iniFilenameAlloc = 0;

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
    


    private static ((string category, Color color)[] categories, InitPropBase[][] init) LoadPropInitFromRenderer()
    {
        var text = File.ReadAllText(Path.Combine(GLOBALS.Paths.RendererDirectory, "Props", "Init.txt")).ReplaceLineEndings();
        return Importers.GetPropsInit(text);
    }
    
    //

    private static TileLoader? _tileLoader;
    private static PropLoader? _propLoader;

    // MAIN FUNCTION
    private static void Main()
    {
        float initialFrames = 0;
        Texture2D? screenshotTexture = null;
        Task<List<Action>> loadPropImagesTask = default!;
        Task<List<Action>> loadLightImagesTask = default!;
        var isLingoRuntimeInit = false;
        string[] ropePropImagePaths;
        string[] longPropImagePaths;
        string[][] otherPropImagePaths;
        string[] lightImagePaths;
        var propTextures = new PropTexturesLoader();
        var lightTextures = new LightTexturesLoader();

        List<Action> loadPropTexturesList = [];
        List<Action> loadLightTexturesList = [];

        var tileTexturesLoadProgress = 0;
        var propTexturesLoadProgress = 0;
        var lightTexturesLoadProgress = 0;

        Image icon = new();

        var fatalException = false;
        
        var paletteLoadProgress = 0;

        var totalPropTexturesLoadProgress = 0;
        var totalLightTexturesLoadProgress = 0;

        List<Action>.Enumerator loadPropTexturesEnumerator = default!;
        List<Action>.Enumerator loadLightTexturesEnumerator = default!;

        var isLoadingTileTexturesDone = false;
        var isLoadingPropTexturesDone = false;
        var isLoadingLightTexturesDone = false;

        LegacyGeoEditorPage? geoPage = null;
        TileEditorPage? tilePage = null;
        CamerasEditorPage? camerasPage = null;
        LightEditorPage? lightPage = null;
        DimensionsEditorPage? dimensionsPage = null;
        NewLevelPage? newLevelPage = null;
        DeathScreen? deathScreen = null;
        EffectsEditorPage? effectsPage = null;
        PropsEditorPage? propsPage = null;
        L4MakerPage? l4MakerPage = null;
        MainPage? mainPage = null;
        StartPage? startPage = null;
        SaveProjectPage? savePage = null;
        FailedTileCheckOnLoadPage? failedTileCheckOnLoadPage = null;
        AssetsNukedPage? assetsNukedPage = null;
        MissingAssetsPage? missingAssetsPage = null;
        MissingTexturesPage? missingTexturesPage = null;
        MissingPropTexturesPage? missingPropTexturesPage = null;
        MissingInitFilePage? missingInitFilePage = null;
        ExperimentalGeometryPage? experimentalGeometryPage = null;
        SettingsPage? settingsPage = null;
        TileViewerPage? tileViewerPage = null;
        TileCreatorPage? tileCreatorPage = null;

        Task allCacheTasks = default!;

        var _tileDexTaskSet = false;


        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

        #if DEBUG
        GLOBALS.BuildConfiguration = "Build Configuration: Debug";
        #elif RELEASE
        GLOBALS.BuildConfiguration = "Build Configuration: Release";
        #elif EXPERIMENTAL
        GLOBALS.BuildConfiguration = "Build Configuration: Experimental";
        #endif

        GLOBALS.OperatingSystem = System.Environment.OSVersion.ToString();

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

        // Set the custom renderer path

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

        var gShortcuts = GLOBALS.Settings.Shortcuts.GlobalShortcuts;
        var loadRate = GLOBALS.Settings.Misc.TileImageScansPerFrame;


        if (!string.IsNullOrEmpty(GLOBALS.Settings.GeneralSettings.RenderingAssetsPath)) {
            GLOBALS.Paths.RendererDirectory = GLOBALS.Settings.GeneralSettings.RenderingAssetsPath;
        }

        var palettesDirExists = Directory.Exists(GLOBALS.Paths.PalettesDirectory);

        PaletteLoader? paletteLoader = palettesDirExists 
            ? new(GLOBALS.Paths.PalettesDirectory) 
            : null;

        // Check for the assets folder and subfolders
        
        // Check /assets. A common mistake when building is to forget to copy the 
        // assets folder

        var missingAssetsDirectory = !Directory.Exists(GLOBALS.Paths.AssetsDirectory);
        var missingRenderingAssetsDirecotry = !Directory.Exists(GLOBALS.Paths.RendererDirectory);

        var missingRenderingAssetsDirecotrySavable = 
            Directory.Exists(GLOBALS.Paths.RendererDirectory) &&
            Directory.Exists(Path.Combine(GLOBALS.Paths.RendererDirectory, "Cast")) &&
            !Directory.Exists(Path.Combine(GLOBALS.Paths.RendererDirectory, "Graphics")) &&
            !Directory.Exists(Path.Combine(GLOBALS.Paths.RendererDirectory, "Props"));
        
        var failedIntegrity = missingAssetsDirectory;

        var failedTileInitLoad = false;
        var failedPropInitLoad = false;
        
        if (failedIntegrity) goto right_before_gl_context;
        
        foreach (var ed in GLOBALS.Paths.EssentialDirectories)
        {
            if (!Directory.Exists(ed))
            {
                logger.Fatal($"Critical directory not found: \"{ed}\"");
                failedIntegrity = true;
            }
            
            if (failedIntegrity) goto right_before_gl_context;
        }

        foreach (var ef in GLOBALS.Paths.EssentialFiles)
        {
            if (!File.Exists(ef))
            {
                logger.Fatal($"Critical file not found: \"{ef}]\"");
                failedIntegrity = true;
            }

            if (failedIntegrity) goto right_before_gl_context;
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
                
                var lines = File.ReadAllLines(GLOBALS.Paths.RecentProjectsPath).ToHashSet();

                foreach (var path in lines)
                {
                    if (!File.Exists(path)) continue;

                    GLOBALS.RecentProjects.AddFirst((path, Path.GetFileNameWithoutExtension(path)));
                    
                    if (GLOBALS.RecentProjects.Count > GLOBALS.RecentProjectsLimit) 
                        GLOBALS.RecentProjects.RemoveLast();
                }
            });

        allCacheTasks = Task.WhenAll([loadRecentProjectsTask]);

        var tilePackagesTask = Task.FromResult<TileInitLoadInfo[]>([]);

        (string name, Color color)[] loadedPackageTileCategories = [];
        InitTile[][] loadedPackageTiles = [];
        
        if (!failedIntegrity)
        {
            // Load tiles and props
            
            logger.Information("Indexing tiles and props");

            try
            {
                // (GLOBALS.TileCategories, GLOBALS.Tiles) = LoadTileInitFromRenderer();
                _tileLoader = new([
                    GLOBALS.Paths.TilesAssetsDirectory, 
                    ..Directory.GetDirectories(GLOBALS.Paths.TilePackagesDirectory)
                ])
                {
                    // Doesn't seem to work..
                    Logger = logger
                };

                _tileLoader.Start();
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
                        if (!GLOBALS.MaterialColors.TryAdd(material, color)) {
                            throw new Exception($"Duplicate material definition \"{material}\"");
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
                // throw new Exception(innerException: e, message: $"Failed to load props init: {e}");s
                failedPropInitLoad = true;
                goto right_before_gl_context;
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

        
        
        // 1. Get all texture paths
        
        ropePropImagePaths = GLOBALS.RopeProps
            .Select(prop => {
                if (Environment.OSVersion.Platform == PlatformID.Unix) {
                    var entries = Directory.GetFiles(Path.Combine(GLOBALS.Paths.RendererDirectory, "Props"));

                        if (entries.Length > 0) {
                            try {
                                var found = entries
                                    .Select(Path.GetFileNameWithoutExtension)
                                    .SingleOrDefault(f => string.Equals(f, prop.Name, StringComparison.OrdinalIgnoreCase));

                                return Path.Combine(GLOBALS.Paths.RendererDirectory, "Props", (found ?? prop.Name) + ".png");
                            } catch {
                                return Path.Combine(GLOBALS.Paths.RendererDirectory, "Props", prop.Name + ".png");
                            }
                        }

                        return Path.Combine(GLOBALS.Paths.RendererDirectory, "Props", prop.Name + ".png");
                }

                return Path.Combine(GLOBALS.Paths.RendererDirectory, "Props", prop.Name + ".png");
            })
            .ToArray();
        
        longPropImagePaths = GLOBALS.LongProps
            .Select(prop => {
                if (Environment.OSVersion.Platform == PlatformID.Unix) {
                    var entries = Directory.GetFiles(Path.Combine(GLOBALS.Paths.RendererDirectory, "Props"));

                        if (entries.Length > 0) {
                            try {
                                var found = entries
                                    .Select(Path.GetFileNameWithoutExtension)
                                    .SingleOrDefault(f => string.Equals(f, prop.Name, StringComparison.OrdinalIgnoreCase));

                                return Path.Combine(GLOBALS.Paths.RendererDirectory, "Props", (found ?? prop.Name) + ".png");
                            } catch {
                                return Path.Combine(GLOBALS.Paths.RendererDirectory, "Props", prop.Name + ".png");
                            }
                        }

                        return Path.Combine(GLOBALS.Paths.RendererDirectory, "Props", prop.Name + ".png");
                }

                return Path.Combine(GLOBALS.Paths.RendererDirectory, "Props", prop.Name + ".png");
            })
            .ToArray();
        
        otherPropImagePaths = GLOBALS.Props.Select(category =>
            category.Select(prop => {
                if (Environment.OSVersion.Platform == PlatformID.Unix) {
                    var entries = Directory.GetFiles(Path.Combine(GLOBALS.Paths.RendererDirectory, "Props"));

                        if (entries.Length > 0) {
                            try {
                                var found = entries
                                    .Select(Path.GetFileNameWithoutExtension)
                                    .SingleOrDefault(f => string.Equals(f, prop.Name, StringComparison.OrdinalIgnoreCase));

                                return Path.Combine(GLOBALS.Paths.RendererDirectory, "Props", (found ?? prop.Name) + ".png");
                            } catch {
                                return Path.Combine(GLOBALS.Paths.RendererDirectory, "Props", prop.Name + ".png");
                            }
                        }

                        return Path.Combine(GLOBALS.Paths.RendererDirectory, "Props", prop.Name + ".png");
                }

                return Path.Combine(GLOBALS.Paths.RendererDirectory, "Props", prop.Name + ".png");
            }
            ).ToArray()
        ).ToArray();
        
        lightImagePaths = failedIntegrity ? [] : Directory
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
        
        if (!failedIntegrity) {
            loadPropImagesTask = propTextures.PrepareFromPathsAsync(ropePropImagePaths, longPropImagePaths, otherPropImagePaths);
            loadLightImagesTask = lightTextures.PrepareFromPathsAsync(lightImagePaths);
        }

        // 4. Await loading in later stages

        right_before_gl_context:
        
        //

        logger.Information("Initializing window");

        icon = LoadImage(GLOBALS.Paths.IconPath);

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

        
        if (failedIntegrity || failedTileInitLoad || failedPropInitLoad) goto skip_loading;

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

            GLOBALS.Textures.InternalMaterials = LoadInternalMaterialTextures();
        }
        catch (Exception e)
        {
            logger.Fatal($"{e}");
        }

        GLOBALS.Textures.MissingTile =
            LoadTexture(Path.Combine(GLOBALS.Paths.AssetsDirectory, "other", "missing tile.png"));

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

        GLOBALS.Textures.PropDepth = LoadRenderTexture(290, 20);
        GLOBALS.Textures.DimensionsVisual = LoadRenderTexture(1400, 800);

        //

        logger.Information("loading shaders");

        // GLOBALS.Shaders.OppositeBrightness = LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "opposite_brightness.frag"));
        GLOBALS.Shaders.Palette = LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "palette.frag"));
        GLOBALS.Shaders.GeoPalette = LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "geo_palette2.frag"));
        GLOBALS.Shaders.TilePreview = LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "tile_preview2.frag"));

        // These two are used to display the light/shadow brush beneath the cursor
        GLOBALS.Shaders.ShadowBrush = LoadShader(null, Path.Combine(GLOBALS.Paths.AssetsDirectory, "shaders", "shadow_brush.fs"));
        GLOBALS.Shaders.LightBrush = LoadShader(null, Path.Combine(GLOBALS.Paths.AssetsDirectory, "shaders", "light_brush.fs"));

        // These two are used to actually draw/erase the shadow on the light map
        GLOBALS.Shaders.ApplyLightBrush = LoadShader(null, Path.Combine(GLOBALS.Paths.AssetsDirectory, "shaders", "apply_light_brush.fs"));
        GLOBALS.Shaders.ApplyShadowBrush = LoadShader(null, Path.Combine(GLOBALS.Paths.AssetsDirectory, "shaders", "apply_shadow_brush.fs"));

        GLOBALS.Shaders.Prop = LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop.frag"));
        GLOBALS.Shaders.BoxTile = LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_box.frag"));

        GLOBALS.Shaders.StandardProp =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_standard.frag"));
        
        GLOBALS.Shaders.StandardPropColored =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_standard_colored.frag"));
        
        GLOBALS.Shaders.StandardPropPalette =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_standard_palette.frag"));
        
        GLOBALS.Shaders.VariedStandardProp =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_varied_standard.frag"));

        GLOBALS.Shaders.VariedStandardPropColored =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_varied_standard_colored.frag"));
        
        GLOBALS.Shaders.VariedStandardPropPalette =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_varied_standard_palette.frag"));
        
        GLOBALS.Shaders.SoftProp =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_soft.frag"));
        
        GLOBALS.Shaders.SoftPropColored =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_soft_colored.frag"));
        
        GLOBALS.Shaders.SoftPropPalette =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_soft_palette.frag"));
        
        GLOBALS.Shaders.VariedSoftProp =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_varied_soft.frag"));
        
        GLOBALS.Shaders.VariedSoftPropColored =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_varied_soft_colored.frag"));
        
        GLOBALS.Shaders.VariedSoftPropPalette =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_varied_soft_palette.frag"));
        
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

        GLOBALS.Shaders.TilePalette = LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "tile_palette.frag"));
        GLOBALS.Shaders.BoxTilePalette = LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "tile_box_palette.frag"));

        GLOBALS.Shaders.VFlip = LoadShaderFromMemory(null, @"#version 330
in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D inputTexture;

out vec4 FragColor;

void main() {
    vec2 op = vec2(fragTexCoord.x, 1 - fragTexCoord.y);
    FragColor = texture(inputTexture, op);
}");

        GLOBALS.Shaders.TilePreviewFragment = LoadShaderFromMemory(null, @"#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 FragColor;

uniform sampler2D inputTexture;

void main()
{
    vec4 color = texture(inputTexture, fragTexCoord);

    if (color.r == 1.0 && color.g == 1.0 && color.b == 1.0) {
        discard;
    }

    FragColor = fragColor;
}
");

        GLOBALS.Shaders.LightMapMask = LoadShaderFromMemory(null, @"#version 330
        
in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 FragColor;

uniform sampler2D inputTexture;
uniform int vflip = 0;

void main() {
    vec4 color = vec4(0, 0, 0, 0);

    if (vflip == 0) {
        color = texture(inputTexture, fragTexCoord);
    } else {
        color = texture(inputTexture, vec2(fragTexCoord.x, 1.0 - fragTexCoord.y));
    }

    if (color.r == 1.0 && color.g == 1.0 && color.b == 1.0) { discard; }

    FragColor = fragColor;
}");

        //

        SetTargetFPS(GLOBALS.Settings.Misc.FPS);

        GLOBALS.Camera = new Camera2D { Zoom = GLOBALS.Settings.GeneralSettings.DefaultZoom };
        
        logger.Information("Initializing pages");
        
        // Initialize pages

        geoPage = new() { Logger = logger };
        tilePage = new() { Logger = logger };
        camerasPage = new() { Logger = logger };
        lightPage = new() { Logger = logger };
        dimensionsPage = new() { Logger = logger };
        newLevelPage = new() { Logger = logger };
        deathScreen = new() { Logger = logger };
        effectsPage = new() { Logger = logger };
        propsPage = new() { Logger = logger };
        l4MakerPage = new() { Logger = logger };
        mainPage = new() { Logger = logger };
        startPage = new() { Logger = logger };
        savePage = new() { Logger = logger };
        failedTileCheckOnLoadPage = new() { Logger = logger };
        assetsNukedPage = new() { Logger = logger };
        missingAssetsPage = new() { Logger = logger };
        missingTexturesPage = new() { Logger = logger };
        missingPropTexturesPage = new() { Logger = logger };
        missingInitFilePage = new() { Logger = logger };
        experimentalGeometryPage = new() { Logger = logger };
        settingsPage = new() { Logger = logger };
        tileViewerPage = new() { Logger = logger };
        tileCreatorPage = new() { Logger = logger };

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
        
        
        
        logger.Information("Initializing events");
        
        // Page event handlers
        startPage.ProjectLoaded += propsPage.OnProjectLoaded;
        startPage.ProjectLoaded += mainPage.OnProjectLoaded;
        startPage.ProjectLoaded += dimensionsPage.OnProjectLoaded;
        startPage.ProjectLoaded += l4MakerPage.OnProjectLoaded;
        startPage.ProjectLoaded += camerasPage.OnProjectLoaded;
        startPage.ProjectLoaded += lightPage.OnProjectLoaded;
        
        mainPage.ProjectLoaded += propsPage.OnProjectLoaded;
        mainPage.ProjectLoaded += dimensionsPage.OnProjectLoaded;
        mainPage.ProjectLoaded += l4MakerPage.OnProjectLoaded;
        mainPage.ProjectLoaded += camerasPage.OnProjectLoaded;
        mainPage.ProjectLoaded += lightPage.OnProjectLoaded;
        
        newLevelPage.ProjectCreated += propsPage.OnProjectCreated;
        newLevelPage.ProjectCreated += dimensionsPage.OnProjectCreated;
        newLevelPage.ProjectCreated += l4MakerPage.OnProjectCreated;
        newLevelPage.ProjectCreated += camerasPage.OnProjectCreated;
        newLevelPage.ProjectCreated += lightPage.OnProjectCreated;

        GLOBALS.PageUpdated += mainPage.OnPageUpdated;
        GLOBALS.PageUpdated += tilePage.OnPageUpdated;
        GLOBALS.PageUpdated += lightPage.OnPageUpdated;
        GLOBALS.PageUpdated += effectsPage.OnPageUpdated;
        GLOBALS.PageUpdated += propsPage.OnPageUpdated;
        GLOBALS.PageUpdated += camerasPage.OnPageUpdated;
        GLOBALS.PageUpdated += savePage.OnPageUpdated;
        GLOBALS.PageUpdated += experimentalGeometryPage.OnPageUpdated;
        GLOBALS.PageUpdated += dimensionsPage.OnPageUpdated;
        GLOBALS.PageUpdated += l4MakerPage.OnPageUpdated;

        //

        unsafe
        {
            GLOBALS.WindowHandle = new IntPtr(GetWindowHandle());
        }
        
        //

        rlImGui.Setup(GLOBALS.Settings.GeneralSettings.DarkTheme, true);
        
        var iniPath = Encoding.ASCII.GetBytes(Path.Combine(GLOBALS.Paths.ExecutableDirectory, "imgui.ini") + "\0");
        _iniFilenameAlloc = System.Runtime.InteropServices.Marshal.AllocHGlobal(iniPath.Length);
        System.Runtime.InteropServices.Marshal.Copy(iniPath, 0, _iniFilenameAlloc, iniPath.Length);

        // unsafe {
        //     ImGui.GetIO().NativePtr->IniFilename = (byte*) _iniFilenameAlloc;
        // }

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

        if (!failedIntegrity) Task.WaitAll([loadPropImagesTask, loadLightImagesTask]);

        

        if (!failedIntegrity) {
            loadPropTexturesList = loadPropImagesTask.Result;
            loadLightTexturesList = loadLightImagesTask.Result;
            totalPropTexturesLoadProgress = loadPropTexturesList.Count;
            totalLightTexturesLoadProgress = loadLightTexturesList.Count;
        }
        
        loadPropTexturesEnumerator = loadPropTexturesList.GetEnumerator();
        loadLightTexturesEnumerator = loadLightTexturesList.GetEnumerator();
        
        logger.Information("Begin main loop");


        // Timer

        GLOBALS.AutoSaveTimer = new System.Timers.Timer(GLOBALS.Settings.GeneralSettings.AutoSaveSeconds * 1000);
        
        GLOBALS.AutoSaveTimer.Elapsed += (sender, args) => {
            // Only save if the level was saved before
            if (!File.Exists(Path.Combine(GLOBALS.ProjectPath, GLOBALS.Level.ProjectName+".txt")) || !(GLOBALS.Page is 1 or 2 or 3 or 4 or 5 or 6 or 7 or 8)) return;

            _isGuiLocked = _globalSave = true;

            Console.WriteLine("Auto saving..");

            logger.Debug("Auto saving..");
        };

        GLOBALS.AutoSaveTimer.AutoReset = true;
        GLOBALS.AutoSaveTimer.Enabled = GLOBALS.Settings.GeneralSettings.AutoSave;

        if (failedIntegrity) goto skip_loading;

        // _propLoader!.Start();
        // _propLoader.IncludeDefined(("Ropes", new Data.Color(0, 0, 0)), GLOBALS.Ropes, GLOBALS.Paths.PropsAssetsDirectory);
        // _propLoader.IncludeDefined(("Longs", new Data.Color(0, 0, 0)), GLOBALS.Longs, GLOBALS.Paths.PropsAssetsDirectory);

        skip_loading:

        while (!WindowShouldClose())
        {
            try
            {
                // The funny
                #if !DEBUG
                if (GLOBALS.Settings.GeneralSettings.PressingEscCrashes && 
                    IsKeyPressed(KeyboardKey.Escape)) throw new Exception("The program didn't know what to do.");
                #endif
                //

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
                    continue;
                } else if (missingRenderingAssetsDirecotry) {
                    BeginDrawing();
                    ClearBackground(Color.Black);
                    
                    DrawText("Could Not Find The Rendering Assets Folder", 50, 50, 35, Color.White);
                    
                    DrawText("This folder is required by the editor, in order to render tiles and props.", 
                        50, 
                        200, 
                        20, 
                        Color.White
                    );

                    DrawText("If you've set the 'RenderingAssetsPath' setting, you might have set the wrong folder.\n", 
                        50, 
                        230, 
                        20, 
                        Color.White
                    );

                    DrawText("If you've built this program yourself, you might have forgot to include the folder from the repository.", 
                        50, 
                        260, 
                        20, 
                        Color.White
                    );
                    
                    DrawText(GLOBALS.Version, 10, GetScreenHeight() - 25, 20, Color.White);
                    EndDrawing();
                    continue;
                }
                // Allow downloading assets from GitHub
                else if (missingRenderingAssetsDirecotrySavable) {
                    BeginDrawing();
                    ClearBackground(Color.Black);
                    _downloadSpinFrames++;


                    if (_downloadSpinFrames % 1000 == 0) {
                        _downloadSpinFrame++;

                        if (_downloadSpinFrame % 3 == 0) _downloadSpinFrame = 0;
                    }

                    if (_downloadingAssets)
                    {
                        if (_deletingRepo) DrawText("Download Complete", (GetScreenWidth() - MeasureText("Download Complete", 35))/2, 50, 35, Color.White); 
                        else DrawText("Downloading Assets", (GetScreenWidth() - MeasureText("Downloading Assets", 35))/2, 50, 35, Color.White);
                    
                        var progressRect = new Rectangle(50, 300, GetScreenWidth() - 100, 50);
                        DrawRectangleLinesEx(progressRect, 3, Color.White);

                        if (_downloadAssetsTask is null) {
                            logger.Information("Download started");

                            if (Directory.Exists(Path.Combine(GLOBALS.Paths.CacheDirectory, "repos"))) {
                                Directory.Delete(Path.Combine(GLOBALS.Paths.CacheDirectory, "repos"), true);
                            }

                            var options = new LibGit2Sharp.CloneOptions();

                            options.FetchOptions.OnTransferProgress += HandleAssetsDownloadProgress;

                            _downloadAssetsTask = Task.Factory.StartNew(
                                () => LibGit2Sharp.Repository.Clone(
                                    _downloadAssetsRepoUrl, 
                                    Path.Combine(GLOBALS.Paths.CacheDirectory, "repos"),
                                    options
                                )
                            );
                        }

                        if (!_downloadAssetsTask.IsCompleted) {
                            DrawRectangleRec(progressRect with { Width = _downloadProgress }, Color.White);

                            DrawText("Downloading assets from the repository" + _downloadSpinFrame switch { 0 => ".", 1 => "..", 2 => "...", _ => "" },
                                (GetScreenWidth() - MeasureText("Downloading assets from the repository", 20))/2,
                                400,
                                20,
                                Color.White
                            );

                            EndDrawing();
                            continue;
                        }

                        if (_downloadAssetsTask.IsFaulted) {
                            DrawText("Failed to download assets; Please check the logs for more information",
                                (GetScreenWidth() - MeasureText("Failed to download assets; Please check the logs for more information", 20))/2,
                                400,
                                20,
                                Color.White
                            );

                            if (!_downloadExceptionLogged) {
                                logger.Error("Failed to download assets: " + _downloadAssetsTask.Exception?.ToString() ?? "NULL");
                                _downloadExceptionLogged = true;
                            }

                            EndDrawing();
                            continue;
                        }

                        if (_movingAssetsTask is null) {
                            logger.Information("Moving downloaded assets");


                            _movingAssetsTask = Task.Factory.StartNew(() => {
                                var path = Path.Combine(_downloadAssetsTask.Result, "..");

                                if (Directory.Exists(Path.Combine(GLOBALS.Paths.RendererDirectory, "Props"))) {
                                    Directory.Delete(Path.Combine(GLOBALS.Paths.RendererDirectory, "Props"));
                                }

                                if (Directory.Exists(Path.Combine(GLOBALS.Paths.RendererDirectory, "Graphics"))) {
                                    Directory.Delete(Path.Combine(GLOBALS.Paths.RendererDirectory, "Graphics"));
                                }


                                Directory.Move(Path.Combine(path, "Props"), Path.Combine(GLOBALS.Paths.RendererDirectory, "Props"));
                                Directory.Move(Path.Combine(path, "Graphics"), Path.Combine(GLOBALS.Paths.RendererDirectory, "Graphics"));
                            });

                        }

                        if (!_movingAssetsTask.IsCompleted) {
                            DrawRectangleRec(progressRect with { Width = progressRect.Width/2 }, Color.White);
                            
                            DrawText("Moving assets" + _downloadSpinFrame switch { 0 => ".", 1 => "..", 2 => "...", _ => "" },
                                (GetScreenWidth() - MeasureText("Moving assets", 20))/2,
                                400,
                                20,
                                Color.White
                            );

                            EndDrawing();
                            continue;
                        }

                        if (_movingAssetsTask.IsFaulted) {
                            DrawText("Failed to download assets; please check the logs for more information",
                                (GetScreenWidth() - MeasureText("Failed to download assets; please check the logs for more information", 20))/2,
                                400,
                                20,
                                Color.White
                            );

                            if (!_moveExceptionLogged) {
                                logger.Error("Failed to move downloaded assets into their appropriate location: " + _movingAssetsTask.Exception?.ToString() ?? "NULL");
                                _moveExceptionLogged = true;
                            }

                            EndDrawing();
                            continue;
                        }

                        if (!_deletingRepo) {
                            logger.Information("Deleting the remaining files");

                            DrawRectangleRec(progressRect with { Width = progressRect.Width * 2/3 }, Color.White);
                            Directory.Delete(Path.Combine(GLOBALS.Paths.CacheDirectory, "repos"), true);
                            _deletingRepo = true;
                        }
                        else
                        {
                            logger.Information("Downloading done");

                            DrawRectangleRec(progressRect, Color.White);

                            DrawText("Download succeeded; Please restart the editor now",
                                (GetScreenWidth() - MeasureText("Download succeeded; Please restart the editor now", 20))/2,
                                400,
                                20,
                                Color.White
                            );
                        }
                        
                    }
                    else
                    {
                        DrawText("No Assets Found", (GetScreenWidth() - MeasureText("No Assets Found", 35))/2, 50, 35, Color.White);
                        
                        DrawText("Select a repository to download assets from", 
                            (GetScreenWidth() - MeasureText("Select a repository to download assets from", 20))/2, 
                            250, 
                            20, 
                            Color.White
                        );

                        var solarBtn = new Rectangle((GetScreenWidth() - 450)/2, 300, 200, 50);
                        var vanillaBtn = new Rectangle((GetScreenWidth() - 450)/2 + 250, 300, 200, 50);

                        var solarHovered = CheckCollisionPointRec(GetMousePosition(), solarBtn);
                        var vanillaHovered = CheckCollisionPointRec(GetMousePosition(), vanillaBtn);

                        DrawRectangleLinesEx(solarBtn, 3, Color.White);
                        DrawRectangleLinesEx(vanillaBtn, 3, Color.White);

                        DrawText("Solar", (int)solarBtn.X + 20, (int)solarBtn.Y + 15, 20, Color.White);
                        DrawText("Vanilla", (int)vanillaBtn.X + 20, (int)vanillaBtn.Y + 15, 20, Color.White);

                        if (solarHovered || vanillaHovered) {
                            SetMouseCursor(MouseCursor.PointingHand);

                            if (solarHovered) {
                                DrawRectangleRec(solarBtn, Color.White);
                                DrawText("Solar", (int)solarBtn.X + 20, (int)solarBtn.Y + 15, 20, Color.Black);

                                // Description

                                DrawText(
                                    "A rich, all-in-one collection of high-quality tiles and props", 
                                    (GetScreenWidth() - MeasureText("A rich, all-in-one collection of high-quality tiles and props", 20))/2,
                                    400,
                                    20,
                                    Color.White
                                );

                                // Click

                                if (IsMouseButtonPressed(MouseButton.Left)) {
                                    SetMouseCursor(MouseCursor.Default);

                                    _downloadAssetsRepoUrl = "https://github.com/solaristheworstcatever/Modded-Regions-Starter-Pack.git";

                                    _downloadingAssets = true;
                                    _downloadExceptionLogged = false;
                                    _downloadAssetsTask = null;

                                    logger.Information("Download solar repo selected");
                                }

                            } else if (vanillaHovered) {
                                DrawRectangleRec(vanillaBtn, Color.White);
                                DrawText("Vanilla", (int)vanillaBtn.X + 20, (int)vanillaBtn.Y + 15, 20, Color.Black);

                                // Description

                                DrawText(
                                    "The default assets included in the community editor", 
                                    (GetScreenWidth() - MeasureText("The default assets included in the community editor", 20))/2,
                                    400,
                                    20,
                                    Color.White
                                );

                                // Click

                                if (IsMouseButtonPressed(MouseButton.Left)) {
                                    SetMouseCursor(MouseCursor.Default);

                                    _downloadAssetsRepoUrl = "https://github.com/SlimeCubed/Drizzle.Data.git";

                                    _downloadingAssets = true;
                                    _downloadExceptionLogged = false;
                                    _downloadAssetsTask = null;

                                    logger.Information("Download vanilla repo selected");
                                }
                            }
                        } else {
                            SetMouseCursor(MouseCursor.Default);
                        }
                        
                        DrawText(GLOBALS.Version, 10, GetScreenHeight() - 25, 20, Color.White);
                    }
                    
                    EndDrawing();
                    continue;
                }
                #endregion
                skip_failed_integrity:

                if (!failedPropInitLoad) goto skip_failed_prop_init_load;
                #region FailedPropInitLoad 
                BeginDrawing();
                ClearBackground(Color.Black);
                
                DrawText("Corrupted Props Assets Index", 50, 50, 50, Color.White);
                DrawText("The /assets/renderer/Props/Init.txt folder contains invalid data.", 
                    50, 
                    200, 
                    20, 
                    Color.White
                );
                
                DrawText("Check the logs for more information.", 
                    50, 
                    230, 
                    20, 
                    Color.White
                );
                
                
                DrawText(GLOBALS.Version, 10, GetScreenHeight() - 25, 20, Color.White);
                EndDrawing();
                continue;
                #endregion
                skip_failed_prop_init_load:
                
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


                    if (initialFrames > 60) {
                        if (GLOBALS.Font is null) DrawText(GLOBALS.Version, 700, 50, 15, Color.White);
                        else DrawTextPro(GLOBALS.Font.Value, GLOBALS.Version, new(700, 50), new(0, 0), 0, 30, 0, Color.White);
                    }

                    if (initialFrames > 70)
                    {
                        if (GLOBALS.Font is null) DrawText(GLOBALS.RaylibVersion, 700, 70, 15, Color.White);
                        else DrawTextPro(GLOBALS.Font.Value, GLOBALS.RaylibVersion, new(700, 80), new(0, 0), 0, 30, 0, Color.White);
                    }

                    if (initialFrames > 80)
                    {
                        if (GLOBALS.Font is null) DrawText(GLOBALS.BuildConfiguration, 700, 90, 15, Color.White);
                        else DrawTextPro(GLOBALS.Font.Value, GLOBALS.BuildConfiguration, new(700, 110), new(0, 0), 0, 30, 0, Color.White);
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
                    deathScreen?.Draw();
                    continue;
                }

                if (!_tileDexTaskSet) {
                    if (_tileLoader!.DexTask?.IsCompleted is false) {
                        var width = GetScreenWidth();
                        var height = GetScreenHeight();
                        
                        BeginDrawing();
                        Printers.DrawSplashScreen(true);

                        {
                            if (GLOBALS.Font is null)
                                DrawText("Building Tile Dex", 100, height - 120, 20, Color.White);
                            else
                                DrawTextEx(GLOBALS.Font.Value, "Building Tile Dex", new Vector2(100, height - 120), 20, 1, Color.White);
                        }

                        EndDrawing();

                        continue;
                    } else if (_tileLoader.DexTask?.IsCompleted is true) {
                        GLOBALS.TileDex = _tileLoader.DexTask.Result;
                        _tileDexTaskSet = true;

                        // _propLoader!.IncludeTiles(GLOBALS.TileDex!);

                        GLOBALS.TileDex.TextureUpdated += tilePage!.OnGlobalResourcesUpdated;
                        GLOBALS.TileDex.TextureUpdated += mainPage!.OnGlobalResourcesUpdated;
                        GLOBALS.TileDex.TextureUpdated += propsPage!.OnGlobalResourcesUpdated;
                        GLOBALS.TileDex.TextureUpdated += lightPage!.OnGlobalResourcesUpdated;
                        GLOBALS.TileDex.TextureUpdated += camerasPage!.OnGlobalResourcesUpdated;
                    }
                }

                if (palettesDirExists && !paletteLoader!.Done)
                {
                    paletteLoadProgress++;

                    while (paletteLoadProgress % loadRate != 0) {
                        if (paletteLoader!.Proceed()) {
                            var (textures, names) = paletteLoader!.GetPalettes();

                            GLOBALS.Textures.Palettes = textures;
                            GLOBALS.Textures.PaletteNames = names;

                            if (textures.Length > 0) GLOBALS.SelectedPalette = textures[0];
                            
                            break;
                        }

                        paletteLoadProgress++;
                    }

                    var width = GetScreenWidth();
                    var height = GetScreenHeight();
                    
                    BeginDrawing();
                    Printers.DrawSplashScreen(true);

                    if (GLOBALS.Font is null)
                            DrawText("Loading Palettes", 100, height - 120, 20, Color.White);
                        else
                            DrawTextEx(GLOBALS.Font.Value, "Loading Palettes", new Vector2(100, height - 120), 20, 1, Color.White);

                    Printers.DrawProgressBar(new Rectangle(100, height - 100, width - 200, 30), paletteLoadProgress, paletteLoader.TotalProgress, false, Color.White);
                    EndDrawing();
                    continue;
                }

                // TODO: To be replaced
                if (!isLoadingLightTexturesDone)
                {
                    // Loading screen

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

                        if (GLOBALS.Settings.GeneralSettings.DebugScreen) DrawText("Developer mode active", 50, 300, 16, Color.Yellow);

                        if (GLOBALS.Font is null) DrawText(GLOBALS.Version, 700, 50, 15, Color.White);
                        else DrawTextPro(GLOBALS.Font.Value, GLOBALS.Version, new(700, 50), new(0, 0), 0, 30, 0, Color.White);

                        if (GLOBALS.Font is null) DrawText(GLOBALS.RaylibVersion, 700, 70, 15, Color.White);
                        else DrawTextPro(GLOBALS.Font.Value, GLOBALS.RaylibVersion, new(700, 80), new(0, 0), 0, 30, 0, Color.White);

                        if (GLOBALS.Font is null) DrawText(GLOBALS.BuildConfiguration, 700, 90, 15, Color.White);
                        else DrawTextPro(GLOBALS.Font.Value, GLOBALS.BuildConfiguration, new(700, 110), new(0, 0), 0, 30, 0, Color.White);
                        
                        if (GLOBALS.Font is null)
                            DrawText("Loading light brushed", 100, height - 120, 20, Color.White);
                        else
                            DrawTextEx(GLOBALS.Font.Value, "Loading light brushes", new Vector2(100, height - 120), 20, 1, Color.White);

                        //Raylib_CsLo.RayGui.GuiProgressBar(new(100, height - 100, width - 200, 30), "", "", lightTexturesLoadProgress, 0, totalLightTexturesLoadProgress);
                        Printers.DrawProgressBar(new Rectangle(100, height - 100, width - 200, 30), lightTexturesLoadProgress, totalLightTexturesLoadProgress, false, Color.White);
                        
                        EndDrawing();
                        
                        continue;
                    }

                    // GLOBALS.Textures.Props = propTextures.Others;
                    // GLOBALS.Textures.RopeProps = propTextures.Ropes;
                    // GLOBALS.Textures.LongProps = propTextures.Longs;
                    GLOBALS.Textures.LightBrushes = lightTextures.Textures;
                    
                    // loadPropTexturesList.Clear();
                    loadLightTexturesList.Clear();

                    isLoadingLightTexturesDone = true;
                    // Enabled = true;
                }
                else if (allCacheTasks?.IsCompleted is not true)
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

                    if (GLOBALS.Font is null) DrawText(GLOBALS.Version, 700, 50, 15, Color.White);
                    else DrawTextPro(GLOBALS.Font.Value, GLOBALS.Version, new(700, 50), new(0, 0), 0, 30, 0, Color.White);

                    if (GLOBALS.Font is null) DrawText(GLOBALS.RaylibVersion, 700, 70, 15, Color.White);
                    else DrawTextPro(GLOBALS.Font.Value, GLOBALS.RaylibVersion, new(700, 80), new(0, 0), 0, 30, 0, Color.White);

                    if (GLOBALS.Font is null) DrawText(GLOBALS.BuildConfiguration, 700, 90, 15, Color.White);
                    else DrawTextPro(GLOBALS.Font.Value, GLOBALS.BuildConfiguration, new(700, 110), new(0, 0), 0, 30, 0, Color.White);
                        
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
                    
                        if (GLOBALS.Font is null) DrawText(GLOBALS.Version, 700, 50, 15, Color.White);
                        else DrawTextPro(GLOBALS.Font.Value, GLOBALS.Version, new(700, 50), new(0, 0), 0, 30, 0, Color.White);

                        if (GLOBALS.Font is null) DrawText(GLOBALS.RaylibVersion, 700, 70, 15, Color.White);
                        else DrawTextPro(GLOBALS.Font.Value, GLOBALS.RaylibVersion, new(700, 80), new(0, 0), 0, 30, 0, Color.White);

                        if (GLOBALS.Font is null) DrawText(GLOBALS.BuildConfiguration, 700, 90, 15, Color.White);
                        else DrawTextPro(GLOBALS.Font.Value, GLOBALS.BuildConfiguration, new(700, 110), new(0, 0), 0, 30, 0, Color.White);
                            
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
                                BeginTextureMode(GLOBALS.Textures.LightMap);
                                DrawRectangle(0, 0, 1, 1, Color.Black);
                                EndTextureMode();
                                
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
                            BeginTextureMode(GLOBALS.Textures.LightMap);
                            DrawRectangle(0, 0, 1, 1, Color.Black);
                            EndTextureMode();
                            
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
                    if (_renderWindow.DrawWindow(logger))
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

                        if (gShortcuts.CycleTileRenderModes.Check(ctrl, shift, alt)) {
                            GLOBALS.Settings.GeneralSettings.DrawTileMode = (TileDrawMode)((((int) GLOBALS.Settings.GeneralSettings.DrawTileMode) + 1) % 3);
                        }
                        if (gShortcuts.CyclePropRenderModes.Check(ctrl, shift, alt)) {
                            GLOBALS.Settings.GeneralSettings.DrawPropMode = (PropDrawMode)((((int) GLOBALS.Settings.GeneralSettings.DrawPropMode) + 1) % 3);
                        }

                        if (gShortcuts.Open.Check(ctrl, shift, alt)) GLOBALS.Page = 0;
                        else if (gShortcuts.TakeScreenshot.Check(ctrl, shift, alt)) {
                            if (!Directory.Exists(GLOBALS.Paths.ScreenshotsDirectory))
                                Directory.CreateDirectory(GLOBALS.Paths.ScreenshotsDirectory);
                        
                            var img = LoadImageFromTexture(GLOBALS.Textures.GeneralLevel.Texture);

                            ImageFlipVertical(ref img);

                            ExportImage(img, Path.Combine(GLOBALS.Paths.ScreenshotsDirectory, $"screenshot-{(DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss"))}.png"));

                            UnloadImage(img);
                        }
                        else if (gShortcuts.QuickSave.Check(ctrl, shift, alt) || GLOBALS.NavSignal == 1)
                        {
                            if (string.IsNullOrEmpty(GLOBALS.ProjectPath) || !File.Exists(Path.Combine(GLOBALS.ProjectPath, GLOBALS.Level.ProjectName+".txt")))
                            {
                                GLOBALS.Page = 12;
                            }
                            else
                            {
                                _askForPath = false;
                                _globalSave = true;
                                _isGuiLocked = true;
                                GLOBALS.NavSignal = 0;
                            }
                        }
                        else if (gShortcuts.QuickSaveAs.Check(ctrl, shift, alt) || GLOBALS.NavSignal == 2)
                        {
                            // _askForPath = true;
                            // _saveFileDialog = Utils.SetFilePathAsync();
                            // _isGuiLocked = true;
                            // _globalSave = true;
                            // GLOBALS.NavSignal = 0;

                            GLOBALS.Page = 12;
                        }
                        else if (gShortcuts.Render.Check(ctrl, shift, alt) || GLOBALS.NavSignal == 3)
                        {
                            logger.Debug($"Rendering level \"{GLOBALS.Level.ProjectName}\"");

                            _renderWindow = new DrizzleRenderWindow();
                            GLOBALS.NavSignal = 0;
                        }
                    
                        if (gShortcuts.ToggleDebugScreen.Check(ctrl, shift, alt)) {
                            GLOBALS.Settings.GeneralSettings.DebugScreen = !GLOBALS.Settings.GeneralSettings.DebugScreen;
                        }
                    }
                    #endregion
                    
                    // Load Tile Textures

                    if (!isLoadingTileTexturesDone) {
                        if (!_tileLoader!.Proceed())
                        {
                            while (++tileTexturesLoadProgress % GLOBALS.Settings.Misc.TileImageScansPerFrame != 0) {
                                if (_tileLoader.Proceed()) {
                                    _tileLoader.ResetTask();
                                    isLoadingTileTexturesDone = true;

                                    goto break_tile_texture_loading;
                                }
                            }
                        }
                        else
                        {
                            isLoadingTileTexturesDone = true;
                        }
                    }
                    break_tile_texture_loading: {}

                    // Load Prop Textures

                    if (!isLoadingPropTexturesDone) {
                        if (loadPropTexturesEnumerator.MoveNext() is true)
                        {
                            loadPropTexturesEnumerator.Current?.Invoke();
                            propTexturesLoadProgress++;

                            while (propTexturesLoadProgress % loadRate != 0)
                            {
                                if (loadPropTexturesEnumerator.MoveNext() is true) {
                                    loadPropTexturesEnumerator.Current?.Invoke();
                                    propTexturesLoadProgress++;
                                }
                                else
                                {
                                    isLoadingPropTexturesDone = true;
                                    GLOBALS.Textures.Props = propTextures.Others;
                                    GLOBALS.Textures.RopeProps = propTextures.Ropes;
                                    GLOBALS.Textures.LongProps = propTextures.Longs;
                                    
                                    loadPropTexturesList.Clear();

                                    goto break_prop_texture_loading;
                                }
                            }
                        }

                        break_prop_texture_loading: {}
                    }

                    BeginDrawing();

                    // page switch

                    switch (GLOBALS.Page)
                    {

                        case 0: startPage?.Draw(); break;
                        case 1: mainPage?.Draw(); break;
                        case 2: experimentalGeometryPage?.Draw(); break;
                        case 3: tilePage?.Draw(); break;
                        case 4: camerasPage?.Draw(); break;
                        case 5: lightPage?.Draw(); break;
                        case 6: dimensionsPage?.Draw(); break;
                        case 7: effectsPage?.Draw(); break;
                        case 8: propsPage?.Draw(); break;
                        case 9: settingsPage?.Draw(); break;
                        case 10: l4MakerPage?.Draw(); break;
                        case 11: newLevelPage?.Draw(); break;
                        case 12: savePage?.Draw(); break;
                        case 13: failedTileCheckOnLoadPage?.Draw(); break;
                        case 14: assetsNukedPage?.Draw(); break;
                        case 15: missingAssetsPage?.Draw(); break;
                        case 16: missingTexturesPage?.Draw(); break;
                        case 17: missingInitFilePage?.Draw(); break;
                        case 18: geoPage?.Draw(); break;
                        case 19: missingPropTexturesPage?.Draw(); break;
                        case 20: tileViewerPage?.Draw(); break;
                        case 21: tileCreatorPage!.Draw(); break;
                        case 99: deathScreen?.Draw(); break;
                        
                        default:
                            GLOBALS.Page = GLOBALS.PreviousPage;
                            break;
                    }

                    if (GLOBALS.Settings.GeneralSettings.DebugScreen) Printers.Debug.F3Screen();

                    EndDrawing();
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
        
        if (failedIntegrity) {
            CloseWindow();
            return;
        }
        
        logger.Information("unloading textures");
        
        UnloadImage(icon);
        
        UnloadTexture(GLOBALS.Textures.MissingTile);

        foreach (var texture in GLOBALS.Textures.GeoMenu) UnloadTexture(texture);
        foreach (var texture in GLOBALS.Textures.GeoBlocks) UnloadTexture(texture);
        foreach (var texture in GLOBALS.Textures.GeoStackables) UnloadTexture(texture);
        foreach (var texture in GLOBALS.Textures.LightBrushes) UnloadTexture(texture);
        foreach (var category in GLOBALS.Textures.Props) { foreach (var texture in category) UnloadTexture(texture); }
        foreach (var texture in GLOBALS.Textures.LongProps) UnloadTexture(texture);
        foreach (var texture in GLOBALS.Textures.RopeProps) UnloadTexture(texture);
        foreach (var texture in GLOBALS.Textures.PropEditModes) UnloadTexture(texture);
        foreach (var texture in GLOBALS.Textures.PropGenerals) UnloadTexture(texture);
        foreach (var texture in GLOBALS.Textures.GeoInterface) UnloadTexture(texture);

        foreach (var texture in GLOBALS.Textures.InternalMaterials) UnloadTexture(texture);
        
        logger.Debug("Unloading light map");

        UnloadRenderTexture(GLOBALS.Textures.LightMap);
        
        UnloadRenderTexture(GLOBALS.Textures.PropDepth);
        UnloadRenderTexture(GLOBALS.Textures.DimensionsVisual);
        
        logger.Debug("Unloading shaders");

        // UnloadShader(GLOBALS.Shaders.OppositeBrightness);
        UnloadShader(GLOBALS.Shaders.Palette);
        UnloadShader(GLOBALS.Shaders.GeoPalette);
        UnloadShader(GLOBALS.Shaders.TilePreview);
        UnloadShader(GLOBALS.Shaders.ShadowBrush);
        UnloadShader(GLOBALS.Shaders.LightBrush);
        UnloadShader(GLOBALS.Shaders.ApplyLightBrush);
        UnloadShader(GLOBALS.Shaders.ApplyShadowBrush);
        UnloadShader(GLOBALS.Shaders.Prop);
        UnloadShader(GLOBALS.Shaders.BoxTile);
        UnloadShader(GLOBALS.Shaders.StandardProp);
        UnloadShader(GLOBALS.Shaders.StandardPropColored);
        UnloadShader(GLOBALS.Shaders.StandardPropPalette);
        UnloadShader(GLOBALS.Shaders.VariedStandardProp);
        UnloadShader(GLOBALS.Shaders.VariedStandardPropPalette);
        UnloadShader(GLOBALS.Shaders.VariedStandardPropColored);
        UnloadShader(GLOBALS.Shaders.SoftProp);
        UnloadShader(GLOBALS.Shaders.SoftPropColored);
        UnloadShader(GLOBALS.Shaders.SoftPropPalette);
        UnloadShader(GLOBALS.Shaders.VariedSoftPropPalette);
        UnloadShader(GLOBALS.Shaders.VariedSoftPropColored);
        UnloadShader(GLOBALS.Shaders.VariedSoftProp);
        UnloadShader(GLOBALS.Shaders.SimpleDecalProp);
        UnloadShader(GLOBALS.Shaders.VariedDecalProp);
        UnloadShader(GLOBALS.Shaders.ColoredTileProp);
        UnloadShader(GLOBALS.Shaders.ColoredBoxTileProp);
        UnloadShader(GLOBALS.Shaders.LongProp);
        UnloadShader(GLOBALS.Shaders.DefaultProp);
        UnloadShader(GLOBALS.Shaders.PreviewColoredTileProp);
        UnloadShader(GLOBALS.Shaders.TilePalette);
        UnloadShader(GLOBALS.Shaders.BoxTilePalette);
        UnloadShader(GLOBALS.Shaders.LightMapStretch);
        UnloadShader(GLOBALS.Shaders.TilePreviewFragment);
        
        UnloadShader(GLOBALS.Shaders.LightMapMask);
        // UnloadShader(GLOBALS.Shaders.LightMapCroppedMask);
        UnloadShader(GLOBALS.Shaders.VFlip);
        
        UnloadRenderTexture(GLOBALS.Textures.GeneralLevel);

        //

        _tileLoader?.Dispose();
        
        // Unlistening

        GLOBALS.TileDex!.TextureUpdated -= tilePage!.OnGlobalResourcesUpdated;
        GLOBALS.TileDex!.TextureUpdated -= mainPage!.OnGlobalResourcesUpdated;
        GLOBALS.TileDex!.TextureUpdated -= propsPage!.OnGlobalResourcesUpdated;
        GLOBALS.TileDex!.TextureUpdated -= lightPage!.OnGlobalResourcesUpdated;
        GLOBALS.TileDex!.TextureUpdated -= camerasPage!.OnGlobalResourcesUpdated;

        startPage!.ProjectLoaded -= propsPage.OnProjectLoaded;
        startPage.ProjectLoaded -= mainPage.OnProjectLoaded;
        startPage.ProjectLoaded -= dimensionsPage!.OnProjectLoaded;
        startPage.ProjectLoaded -= l4MakerPage!.OnProjectLoaded;
        startPage.ProjectLoaded -= camerasPage.OnProjectLoaded;
        startPage.ProjectLoaded -= lightPage.OnProjectLoaded;
        
        mainPage.ProjectLoaded -= propsPage.OnProjectLoaded;
        mainPage.ProjectLoaded -= dimensionsPage.OnProjectLoaded;
        mainPage.ProjectLoaded -= l4MakerPage.OnProjectLoaded;
        mainPage.ProjectLoaded -= camerasPage.OnProjectLoaded;
        mainPage.ProjectLoaded -= lightPage.OnProjectLoaded;
        
        newLevelPage!.ProjectCreated -= propsPage.OnProjectCreated;
        newLevelPage.ProjectCreated -= dimensionsPage.OnProjectCreated;
        newLevelPage.ProjectCreated -= l4MakerPage.OnProjectCreated;
        newLevelPage.ProjectCreated -= camerasPage.OnProjectCreated;
        newLevelPage.ProjectCreated -= lightPage.OnProjectCreated;

        GLOBALS.PageUpdated -= mainPage.OnPageUpdated;
        GLOBALS.PageUpdated -= tilePage.OnPageUpdated;
        GLOBALS.PageUpdated -= lightPage.OnPageUpdated;
        GLOBALS.PageUpdated -= effectsPage!.OnPageUpdated;
        GLOBALS.PageUpdated -= propsPage.OnPageUpdated;
        GLOBALS.PageUpdated -= camerasPage.OnPageUpdated;
        GLOBALS.PageUpdated -= savePage!.OnPageUpdated;
        GLOBALS.PageUpdated -= experimentalGeometryPage!.OnPageUpdated;
        GLOBALS.PageUpdated -= dimensionsPage.OnPageUpdated;
        GLOBALS.PageUpdated -= l4MakerPage.OnPageUpdated;

        // Unloading Pages
        
        logger.Debug("Unloading Pages");
        
        // GLOBALS.Pager.Dispose();

        loadPropTexturesEnumerator.Dispose();
        loadLightTexturesEnumerator.Dispose();
        
        geoPage?.Dispose();
        tilePage?.Dispose();
        camerasPage?.Dispose();
        lightPage?.Dispose();
        dimensionsPage?.Dispose();
        deathScreen?.Dispose();
        effectsPage?.Dispose();
        propsPage?.Dispose();
        l4MakerPage?.Dispose();
        mainPage?.Dispose();
        startPage?.Dispose();
        savePage?.Dispose();
        failedTileCheckOnLoadPage?.Dispose();
        assetsNukedPage?.Dispose();
        missingAssetsPage?.Dispose();
        missingTexturesPage?.Dispose();
        missingPropTexturesPage?.Dispose();
        missingInitFilePage?.Dispose();
        experimentalGeometryPage?.Dispose();
        settingsPage?.Dispose();
        tileViewerPage?.Dispose();
        tileCreatorPage?.Dispose();

        paletteLoader?.Dispose();

        GLOBALS.AutoSaveTimer.Dispose();

        //
        System.Runtime.InteropServices.Marshal.FreeHGlobal(_iniFilenameAlloc);
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
