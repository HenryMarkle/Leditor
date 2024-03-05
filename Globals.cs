using static Raylib_CsLo.Raylib;

using System.Numerics;
using System.Text.Json;

namespace Leditor;


/// <summary>
/// Global state - accessible for pages and the Main function
/// </summary>
internal static class GLOBALS
{
    
    /// <summary>
    /// Do not access before InitWindow() is called in the Main function.
    /// </summary>
    internal class TextureService
    {
        public Texture SplashScreen { get; set; }
        
        public Texture MissingTile { get; set; }

        public Texture[] GeoMenu { get; set; } = [];
        public Texture[] GeoInterface { get; set; } = [];
        public Texture[] GeoBlocks { get; set; } = [];
        public Texture[] GeoStackables { get; set; } = [];
        public Texture[] LightBrushes { get; set; } = [];
        public Texture[][] Tiles { get; set; } = [];
        public Texture[][] Props { get; set; } = [];
        public Texture[] LongProps { get; set; } = [];
        public Texture[] RopeProps { get; set; } = [];
        public Texture[] PropMenuCategories { get; set; } = [];
        public Texture[] PropModes { get; set; } = [];
        public Texture[] PropEditModes { get; set; } = [];
        public Texture[] EffectsUI { get; set; } = [];
        public Texture[] PropGenerals { get; set; } = [];
        
        public RenderTexture LightMap { get; set; }
        
        // Might be a really bad idea
        
        public Raylib_cs.RenderTexture2D TileSpecs { get; set; }
        public Raylib_cs.RenderTexture2D PropDepth { get; set; }
    }

    /// <summary>
    /// Do not access before InitWindow() is called in the Main function.
    /// </summary>
    internal class ShaderService
    {
        internal Shader TilePreview { get; set; }
        internal Shader ColoredTileProp { get; set; }
        internal Shader ColoredBoxTileProp { get; set; }
        internal Shader ShadowBrush { get; set; }
        internal Shader LightBrush { get; set; }
        internal Shader ApplyShadowBrush { get; set; }
        internal Shader ApplyLightBrush { get; set; }
        internal Shader Prop { get; set; }
        internal Shader StandardProp { get; set; }
        internal Shader VariedStandardProp { get; set; }
        internal Shader SoftProp { get; set; }
        internal Shader VariedSoftProp { get; set; }
        internal Shader SimpleDecalProp { get; set; }
        internal Shader VariedDecalProp { get; set; }
        internal Shader LongProp { get; set; }
    }

    /// <summary>
    /// A namespace for paths used by the program to access resources.
    /// </summary>
    internal static class Paths
    {
        internal static string ExecutableDirectory => Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? throw new Exception("unable to retreive current executable's path");

        internal static string ProjectsDirectory => Path.Combine(ExecutableDirectory, "projects");
        internal static string IndexDirectory => Path.Combine(ExecutableDirectory, "index");
        internal static string AssetsDirectory => Path.Combine(ExecutableDirectory, "assets");
        internal static string TilesAssetsDirectory => Path.Combine(AssetsDirectory, "tiles");
        internal static string GeoAssetsDirectory => Path.Combine(AssetsDirectory, "geo");
        internal static string LightAssetsDirectory => Path.Combine(AssetsDirectory, "light");
        internal static string PropsAssetsDirectory => Path.Combine(AssetsDirectory, "props");
        internal static string ShadersAssetsDirectory => Path.Combine(AssetsDirectory, "shaders");
        internal static string CacheDirectory => Path.Combine(ExecutableDirectory, "cache");
        internal static string UiAssetsDirectory => Path.Combine(AssetsDirectory, "interface");
        internal static string PackagesDirectory => Path.Combine(ExecutableDirectory, "packs");
        internal static string TilePackagesDirectory => Path.Combine(PackagesDirectory, "tiles");
        internal static string FontsDirectory => Path.Combine(AssetsDirectory, "fonts");
        internal static string RendererDirectory => Path.Combine(ExecutableDirectory, "renderer");
        
        
        internal static string TilesInitPath => Path.Combine(IndexDirectory, "tiles.txt");
        internal static string MaterialsInitPath => Path.Combine(IndexDirectory, "materials.txt");
        internal static string EffectsInitPath => Path.Combine(IndexDirectory, "effects.txt");
        internal static string PropsInitPath => Path.Combine(IndexDirectory, "props.txt");
        
        internal static string SettingsPath => Path.Combine(ExecutableDirectory, "settings.json");



        private static IEnumerable<string> EssentialDirectories =>
        [
            IndexDirectory, 
            AssetsDirectory, 
            TilesAssetsDirectory, 
            GeoAssetsDirectory, 
            LightAssetsDirectory,
            PropsAssetsDirectory, 
            ShadersAssetsDirectory,
            UiAssetsDirectory
        ];

        private static IEnumerable<string> EssentialFiles =>
        [
            TilesInitPath,
            MaterialsInitPath,
            EffectsInitPath,
            PropsInitPath
        ];
        
        internal static readonly IEnumerable<(string, bool)> DirectoryIntegrity =
            from directory 
                in GLOBALS.Paths.EssentialDirectories 
            select (directory, Directory.Exists(directory));

        internal static readonly IEnumerable<(string, bool)> FileIntegrity =
            from file 
                in GLOBALS.Paths.EssentialFiles 
            select (file, File.Exists(file));
    }

    /// <summary>
    /// Stores common data for the current loaded level.
    /// </summary>
    internal class LevelState
    {
        private (int left, int top, int right, int bottom) _padding;
        
        internal int Width { get; private set; }
        internal int Height { get; private set; }

        internal (int left, int top, int right, int bottom) Padding
        {
            get => _padding;
            set
            {
                _padding = value;
                Border = new Rectangle(
                    _padding.left * Scale,
                    _padding.top * Scale,
                    (Width - (_padding.right + _padding.left)) * Scale,
                    (Height - (_padding.bottom + _padding.top)) * Scale
                );
            }
        }
        internal Rectangle Border { get; private set; }
        internal int WaterLevel { get; set; } = -1;
        internal bool WaterAtFront { get; set; } = false;
        internal int LightAngle { get; set; } = 180; // 0 - 360
        internal int LightFlatness { get; set; } = 1; // 1 - 10
        
        internal bool LightMode { get; set; } = true;
        internal bool DefaultTerrain { get; set; } = true;

        internal List<RenderCamera> Cameras { get; set; } = [];

        internal RunCell[,,] GeoMatrix { get; private set; } = new RunCell[0, 0, 0];
        internal TileCell[,,] TileMatrix { get; private set; } = new TileCell[0, 0, 0];
        internal Color[,,] MaterialColors { get; private set; } = new Color[0, 0, 0];
        internal (string, EffectOptions[], double[,])[] Effects { get; set; } = [];
        internal (InitPropType type, (int category, int index) position, Prop prop)[] Props { get; set; } = [];

        internal string DefaultMaterial { get; set; } = "Concrete";
        
        internal string ProjectName { get; set; } = "New Project";
        
        internal int Seed { get; set; } = new Random().Next(1000);
        
        internal LevelState(int width, int height, (int left, int top, int right, int bottom) padding)
        {
            New(width, height, padding, [1, 1, 0]);
        }

        internal void Import(
            int width,
            int height,
            (int left, int top, int right, int bottom) padding,
            RunCell[,,] geoMatrix,
            TileCell[,,] tileMatrix,
            Color[,,] materialColorMatrix,
            (string, EffectOptions[], double[,])[] effects,
            List<RenderCamera> cameras,
            (InitPropType type, (int category, int index) position, Prop prop)[] props,
            (int angle, int flatness) lightSettings,
            bool lightMode,
            bool terrainMedium,
            int seed,
            int waterLevel,
            bool waterInFront,
            string defaultMaterial = "Concrete",
            string projectName = "New Project"
        )
        {
            Width = width;
            Height = height;
            Seed = seed;
            WaterLevel = waterLevel;
            WaterAtFront = waterInFront;
            Padding = padding;
            GeoMatrix = geoMatrix;
            TileMatrix = tileMatrix;
            MaterialColors = materialColorMatrix;
            Effects = effects;
            DefaultMaterial = defaultMaterial;
            ProjectName = projectName;

            LightAngle = lightSettings.angle;
            LightFlatness = lightSettings.flatness;

            Cameras = cameras;
            Props = props;

            LightMode = lightMode;
            DefaultTerrain = terrainMedium;
        }

        internal void New(
            int width,
            int height,
            (int left, int top, int right, int bottom) padding,
            Span<int> geoIdFill
        )
        {
            Width = width;
            Height = height;
            Padding = padding;

            LightFlatness = 1;
            LightAngle = 180;

            /*Border = new(
                Padding.left * Scale,
                Padding.top * Scale,
                (Width - (Padding.right + Padding.left)) * Scale,
                (Height - (Padding.bottom + Padding.top)) * Scale
            );*/

            Cameras = [new RenderCamera { Coords = new Vector2(20f, 30f), Quad = new(new(), new(), new(), new()) }];

            Effects = [];
            
            // Geo Matrix
            {
                var matrix = new RunCell[height, width, 3];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        matrix[y, x, 0] = new() { Geo = geoIdFill[0], Stackables = new bool[22] };
                        matrix[y, x, 1] = new() { Geo = geoIdFill[1], Stackables = new bool[22] };
                        matrix[y, x, 2] = new() { Geo = geoIdFill[2], Stackables = new bool[22] };
                    }
                }

                GeoMatrix = matrix;
            }

            // Tile Matrix
            {
                var matrix = new TileCell[height, width, 3];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        matrix[y, x, 0] = new()
                        {
                            Type = TileType.Default,
                            Data = new TileDefault()
                        };

                        matrix[y, x, 1] = new()
                        {
                            Type = TileType.Default,
                            Data = new TileDefault()
                        };

                        matrix[y, x, 2] = new()
                        {
                            Type = TileType.Default,
                            Data = new TileDefault()
                        };
                    }
                }

                TileMatrix = matrix;
            }

            // Material Color Matrix
            {
                var matrix = new Color[height, width, 3];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        matrix[y, x, 0] = BLACK;
                        matrix[y, x, 1] = BLACK;
                        matrix[y, x, 2] = BLACK;
                    }
                }

                MaterialColors = matrix;
            }
        }

        internal void Resize(
            int width,
            int height,
            (int left, int top, int right, int bottom) padding,
            ReadOnlySpan<int> geoIdFill
        )
        {
            // Geo Matrix
            GeoMatrix = Utils.Resize(
                GeoMatrix,
                Width,
                Height,
                width,
                height,
                [
                    new RunCell { Geo = geoIdFill[0], Stackables = new bool[22] },
                    new RunCell { Geo = geoIdFill[1], Stackables = new bool[22] },
                    new RunCell { Geo = geoIdFill[2], Stackables = new bool[22] }
                ]
            );

            // Tile Matrix
            TileMatrix = Utils.Resize(
                TileMatrix,
                width,
                height,
                [
                    new TileCell(),
                    new TileCell(),
                    new TileCell()
                ]
            );

            // Material Color Matrix
            MaterialColors = Utils.Resize(
                MaterialColors,
                width,
                height,
                [BLACK, BLACK, BLACK]
            );

            // Update Dimensions

            Width = width;
            Height = height;
            Padding = padding;

            /*Border = new(
                Padding.left * Scale,
                Padding.top * Scale,
                (Width - (Padding.right + Padding.left)) * Scale,
                (Height - (Padding.bottom + Padding.top)) * Scale
            );*/
        }
    }

    internal static string ProjectPath { get; set; } = "";

    internal static int MinScreenWidth => 1280;
    internal static int MinScreenHeight => 800;

    // These are for the camera sprite in the camera editor.
    internal static int EditorCameraWidth => 1400;
    internal static int EditorCameraHeight => 800;

    /// This is the scale of a single geo cell.
    internal static int Scale => 20;

    /// This is the scale of a single square in the tile editor.
    internal static int PreviewScale => 16;

    /// This is for a single slot from the geo menu in the geo editor.
    internal static int UiScale => 40;

    internal static float ZoomIncrement => 0.125f;

    internal static int[] CamQuadLocks { get; set; } = [0, 0];
    internal static int CamLock { get; set; }

    internal static int InitialMatrixWidth => 72;
    internal static int InitialMatrixHeight => 43;

    internal static int Page { get; set; } = 0;
    internal static int PreviousPage { get; set; } = 0;

    /// ResizeFlag and NewFlag are used when moving to page 6 (dimensions page)
    /// to indicate whether you want to resize the levels or over override it.
    internal static bool ResizeFlag { get; set; } = false;
    internal static bool NewFlag { get; set; } = false;
    //

    /// Current working layer
    internal static int Layer { get; set; } = 0;
    

    /// The current loaded level
    internal static LevelState Level { get; set; } = new(InitialMatrixWidth, InitialMatrixHeight, (6, 3, 6, 5));

    /// Global textures; Do not access before window is initialized.
    internal static TextureService Textures { get; set; } = new();
    
    /// Global shaders; Do not access before window is initialized.
    internal static ShaderService Shaders { get; set; } = new();
    
    internal static (string, Color)[] TileCategories { get; set; } = [];
    
    internal static (string, Color)[] PropCategories { get; set; } = [ ("Ropes", new(255, 0, 0, 255)), ("Long Props", new(0, 255, 0, 255)) ];
    
    /// Tile definitions
    internal static InitTile[][] Tiles { get; set; } = [];

    internal static InitTile MissingTile { get; } = new()
    {
        Name = "Undefined",
        Specs = [],
        Specs2 = [],
        Repeat = [],
        Tags = []
    };

    /// Embedded rope prop definitions
    internal static InitRopeProp[] RopeProps { get; } =
    [
        new(name: "Wire", type: InitPropType.Rope, depth: 0, segmentLength: 3, collisionDepth: 0, segmentRadius: 1f, gravity: 0.5f, friction: 0.5f, airFriction: 0.9f, stiff: false, previewColor: new(255, 0, 0, 255), previewEvery: 4, edgeDirection: 0, rigid: 0, selfPush: 0, sourcePush: 0),
        new("Tube", InitPropType.Rope, 4, 10, 2, 4.5f, 0.5f, 0.5f, 0.9f, true, new(0, 0, 255, 255), 2, 5, 1.6f, 0, 0),
        new("ThickWire", InitPropType.Rope, 3, 4, 1, 2f, 0.5f, 0.8f, 0.9f, true, new(255, 255, 0, 255), 2, 0, 0.2f, 0, 0),
        new("RidgedTube", InitPropType.Rope, 4, 5, 2, 5, 0.5f, 0.3f, 0.7f, true, new Color(255, 0, 255, 255), 2, 0, 0.1f, 0, 0),
        new("Fuel Hose", InitPropType.Rope, 5, 16, 1, 7, 0.5f, 0.8f, 0.9f, true, new(255, 150, 0, 255), 1, 1.4f, 0.2f, 0, 0),
        new("Broken Fuel Hose", InitPropType.Rope, 6, 16, 1, 7, 0.5f, 0.8f, 0.9f, true, new(255, 150, 0, 255), 1, 1.4f, 0.2f, 0, 0),
        new("Large Chain", InitPropType.Rope, 9, 28, 3, 9.5f, 0.9f, 0.8f, 0.95f, true, new(0, 255, 0, 255), 1, 0, 0, 6.5f, 0),
        new("Large Chain 2", InitPropType.Rope, 9, 28, 3, 9.5f, 0.9f, 0.8f, 0.95f, true, new(20, 205, 0, 255), 1, 0, 0, 6.5f, 0),
        new("Bike Chain", InitPropType.Rope, 9, 38, 3, 6.5f, 0.9f, 0.8f, 0.95f, true, new(100, 100, 100, 255), 1, 0, 0, 16.5f, 0),
        new("Zero-G Tube", InitPropType.Rope, 4, 10, 2, 4.5f, 0, 0.5f, 0.9f, true, new(0, 255, 0, 255), 2, 0, 0.6f, 2, 0.5f),
        new("Zero-G Wire", InitPropType.Rope, 0, 8, 0, 1, 0, 0.5f, 0.9f, true, new(255, 0, 0, 255), 2, 0.3f, 0.5f, 1.2f, 0.5f),
        new("Fat Hose", InitPropType.Rope, 6, 40, 3, 20, 0.9f, 0.6f, 0.95f, true, new(0, 100, 255, 255), 1, 0.1f, 0.2f, 10, 0.1f),
        new("Wire Bunch", InitPropType.Rope, 9, 50, 3, 20, 0.9f, 0.6f, 0.95f, true, new(255, 100, 150, 255), 1, 0.1f, 0.2f, 10, 0.1f),
        new("Wire Bunch 2", InitPropType.Rope, 9, 50, 3, 20, 0.9f, 0.6f, 0.95f, true, new(255, 100, 150, 255), 1, 0.1f, 0.2f, 10, 0.1f),
        new("Big Big Pipe", InitPropType.Rope, 6, 40, 3, 20, 0.9f, 0.6f, 0.95f, true, new(50, 150, 210, 255), 1, 0.1f, 0.2f, 10, 0.1f),
        new("Ring Chain", InitPropType.Rope, 6, 40, 3, 20, 0.9f, 0.6f, 0.95f, true, new(100, 200, 0, 255), 1, 0.1f, 0.2f, 10, 0.1f),
        new("Christmas Wire", InitPropType.Rope, 0, 17, 0, 8.5f, 0.5f, 0.5f, 0.9f, false, new(200, 0, 200, 255), 1, 0, 0, 0, 0),
        new("Ornate Wire", InitPropType.Rope, 0, 17, 0, 8.5f, 0.5f, 0.5f, 0.9f, false, new(0, 200, 200, 255), 1, 0, 0, 0, 0),
    ];

    /// Embedded long prop definitions
    internal static InitLongProp[] LongProps { get; } = 
    [
        new("Cabinet Clamp", InitPropType.Long, 0),
        new("Drill Suspender", InitPropType.Long, 5),
        new("Thick Chain", InitPropType.Long, 0),
        new("Drill", InitPropType.Long, 10),
        new("Piston", InitPropType.Long, 4),
        
        new("Stretched Pipe", InitPropType.Long, 0),
        new("Twisted Thread", InitPropType.Long, 0),
        new("Stretched Wire", InitPropType.Long, 0),
    ];
    
    /// Prop definitions
    internal static InitPropBase[][] Props { get; set; } = [  ];
    
    internal static string[] MaterialCategories { get; set; } = [
        "Materials",
        "Drought Materials",
        "Community Materials"
    ];
    
    /// Embedded material definitions
    internal static (string, Color)[][] Materials { get; set; } = [
        [
            ("Standard", new(150, 150, 150, 255)),
            ("Concrete", new(150, 255, 255, 255)),
            ("RainStone", new(0, 0, 255, 255)),
            ("Bricks", new(200, 150, 100, 255)),
            ("BigMetal", new(255, 0, 0, 255)),
            ("Tiny Signs", new(255, 255, 255, 255)),
            ("Scaffolding", new(60, 60, 40, 255)),
            ("Dense Pipes", new(10, 10, 255, 255)),
            ("SuperStructure", new(160, 180, 255, 255)),
            ("SuperStructure2", new(190, 160, 0, 255)),
            ("Tiled Stone", new(100, 0, 255, 255)),
            ("Chaotic Stone", new(255, 0, 255, 255)),
            ("Small Pipes", new(255, 255, 0, 255)),
            ("Trash", new(90, 255, 0, 255)),
            ("Invisible", new(200, 200, 200, 255)),
            ("LargeTrash", new(150, 30, 255, 255)),
            ("3DBricks", new(255, 150, 0, 255)),
            ("Random Machines", new(72, 116, 80, 255)),
            ("Dirt", new(124, 72, 52, 255)),
            ("Ceramic Tile", new(60, 60, 100, 255)),
            ("Temple Stone", new(0, 120, 180, 255)),
            ("Circuits", new(15, 200, 15, 255)),
            ("Ridge", new(200, 15, 60, 255)),
        ],
        [
            ("Steel", new(220, 170, 195, 255)),
            ("4Mosaic", new(227, 76, 13, 255)),
            ("Color A Ceramic", new(120, 0, 90, 255)),
            ("Color B Ceramic", new(0, 175, 175, 255)),
            ("Rocks", new(185, 200, 0, 255)),
            ("Rough Rock", new(155, 170, 0, 255)),
            ("Random Metal", new(180, 10, 10, 255)),
            ("Non-Slip Metal", new(180, 80, 80, 255)),
            ("Stained Glass", new(180, 80, 180, 255)),
            ("Sandy Dirt", new(180, 180, 80, 255)),
            ("MegaTrash", new(135, 10, 255, 255)),
            ("Shallow Dense Pipes", new(13, 23, 110, 255)),
            ("Sheet Metal", new(145, 135, 125, 255)),
            ("Chaotic Stone 2", new(90, 90, 90, 255)),
            ("Asphalt", new(115, 115, 115, 255))
        ],
        [
            ("Shallow Circuits", new(15, 200, 155, 255)),
            ("Random Machines 2", new(116, 116, 80, 255)),
            ("Small Machines", new(80, 116, 116, 255)),
            ("Random Metals", new(255, 0, 80, 255)),
            ("ElectricMetal", new(255, 0, 100, 255)),
            ("Grate", new(190, 50, 190, 255)),
            ("CageGrate", new(50, 190, 190, 255)),
            ("BulkMetal", new(50, 19, 190, 255)),
            ("MassiveBulkMetal", new(255, 19, 19, 255)),
            ("Dune Sand", new(255, 255, 100, 255))
        ]
    ];
    
    /// A map of each material and its associated color; used when loading a level.
    internal static Dictionary<string, Color> MaterialColors => new()
    {
        ["Standard"] = new(150, 150, 150, 255),
        ["Concrete"] = new(150, 255, 255, 255),
        ["RainStone"] = new(0, 0, 255, 255),
        ["Bricks"] = new(200, 150, 100, 255),
        ["BigMetal"] = new(255, 0, 0, 255),
        ["Tiny Signs"] = new(255, 255, 255, 255),
        ["Scaffolding"] = new(60, 60, 40, 255),
        ["Dense Pipes"] = new(10, 10, 255, 255),
        ["SuperStructure"] = new(160, 180, 255, 255),
        ["SuperStructure2"] = new(190, 160, 0, 255),
        ["Tiled Stone"] = new(100, 0, 255, 255),
        ["Chaotic Stone"] = new(255, 0, 255, 255),
        ["Small Pipes"] = new(255, 255, 0, 255),
        ["Trash"] = new(90, 255, 0, 255),
        ["Invisible"] = new(200, 200, 200, 255),
        ["LargeTrash"] = new(150, 30, 255, 255),
        ["3DBricks"] = new(255, 150, 0, 255),
        ["Random Machines"] = new(72, 116, 80, 255),
        ["Dirt"] = new(124, 72, 52, 255),
        ["Ceramic Tile"] = new(60, 60, 100, 255),
        ["Temple Stone"] = new(0, 120, 180, 255),
        ["Circuits"] = new(15, 200, 15, 255),
        ["Ridge"] = new(200, 15, 60, 255),

        ["Steel"] = new(220, 170, 195, 255),
        ["4Mosaic"] = new(227, 76, 13, 255),
        ["Color A Ceramic"] = new(120, 0, 90, 255),
        ["Color B Ceramic"] = new(0, 175, 175, 255),
        ["Rocks"] = new(185, 200, 0, 255),
        ["Rough Rock"] = new(155, 170, 0, 255),
        ["Random Metal"] = new(180, 10, 10, 255),
        ["Non-Slip Metal"] = new(180, 80, 80, 255),
        ["Stained Glass"] = new(180, 80, 180, 255),
        ["Sandy Dirt"] = new(180, 180, 80, 255),
        ["MegaTrash"] = new(135, 10, 255, 255),
        ["Shallow Dense Pipes"] = new(13, 23, 110, 255),
        ["Sheet Metal"] = new(145, 135, 125, 255),
        ["Chaotic Stone 2"] = new(90, 90, 90, 255),
        ["Asphalt"] = new(115, 115, 115, 255),

        ["Shallow Circuits"] = new(15, 200, 155, 255),
        ["Random Machines 2"] = new(116, 116, 80, 255),
        ["Small Machines"] = new(80, 116, 116, 255),
        ["Random Metals"] = new(255, 0, 80, 255),
        ["ElectricMetal"] = new(255, 0, 100, 255),
        ["Grate"] = new(190, 50, 190, 255),
        ["CageGrate"] = new(50, 190, 190, 255),
        ["BulkMetal"] = new(50, 19, 190, 255),
        ["MassiveBulkMetal"] = new(255, 19, 19, 255),
        ["Dune Sand"] = new(255, 255, 100, 255)
    };
    
    public static string[][] Effects { get; } = [
        ["Slime", "Melt", "Rust", "Barnacles", "Rubble", "DecalsOnlySlime"], // 6
        ["Roughen", "SlimeX3", "Super Melt", "Destructive Melt", "Erode", "Super Erode", "DaddyCorruption"], // 7
        ["Wires", "Chains"], // 2
        ["Root Grass", "Seed Pods", "Growers", "Cacti", "Rain Moss", "Hang Roots", "Grass"], // 7
        ["Arm Growers", "Horse Tails", "Circuit Plants", "Feather Plants", "Thorn Growers", "Rollers", "Garbage Spirals"], // 7
        ["Thick Roots", "Shadow Plants"], // 2
        ["Fungi Flowers", "Lighthouse Flowers", "Fern", "Giant Mushroom", "Sprawlbush", "featherFern", "Fungus Tree"], // 7
        ["BlackGoo", "DarkSlime"], // 2
        ["Restore As Scaffolding", "Ceramic Chaos"], // 2
        ["Colored Hang Roots", "Colored Thick Roots", "Colored Shadow Plants", "Colored Lighthouse Flowers", "Colored Fungi Flowers", "Root Plants"], // 6
        ["Foliage", "Mistletoe", "High Fern", "High Grass", "Little Flowers", "Wastewater Mold"], // 6
        ["Spinets", "Small Springs", "Mini Growers", "Clovers", "Reeds", "Lavenders", "Dense Mold"], // 7
        ["Ultra Super Erode", "Impacts"], // 2
        ["Super BlackGoo", "Stained Glass Properties"], // 2
        ["Colored Barnacles", "Colored Rubble", "Fat Slime"], // 3
        ["Assorted Trash", "Colored Wires", "Colored Chains", "Ring Chains"], // 4
        ["Left Facing Kelp", "Right Facing Kelp", "Mixed Facing Kelp", "Bubble Grower", "Moss Wall", "Club Moss"], // 6
        ["Ivy"], // 1
        ["Fuzzy Growers"] // 1
    ];

    public static string EffectType(string name) => name switch
    {
        "Slime" or "LSlime" or "Fat Slime" or "Scales" or "SlimeX3" or 
            "DecalsOnlySlime" or "Melt" or "Rust" or "Barnacles" or "Colored Barnacles" or 
            "Clovers" or "Erode" or "Sand" or "Super Erode" or "Ultra Super Erode" or 
            "Roughen" or "Impacts" or "Super Melt" or "Destructive Melt" => "standardErosion",
        
        _ => "nn"
    };

    public static Dictionary<string, int> EffectRepeats => new()
    {
        ["Slime"] = 130,
        ["DecalsOnlySlime"] = 130,
        ["Fat Slime"] = 200,
        ["Scales"] = 200,
        ["SlimeX3"] = 390,
        ["Melt"] = 60,
        ["Super Erode"] = 60,
        ["Ultra Super Erode"] = 60,
        ["Rust"] = 60,
        ["Barnacles"] = 60,
        ["Colored Barnacles"] = 60,
        ["Clovers"] = 20,
        ["Erode"] = 80,
        ["Sand"] = 80,
        ["Roughen"] = 30,
        ["Impacts"] = 75,
        ["Super Melt"] = 50,
        ["Destructive Melt"] = 50,
    };

    public static Dictionary<string, float> EffectOpenAreas => new()
    {
        ["Slime"] = 0.5f,
        ["DecalsOnlySlime"] = 0.5f,
        ["Fat Slime"] = 0.5f,
        ["Scales"] = 0.05f,
        ["SlimeX3"] = 0.5f,
        ["Melt"] = 0.5f,
        ["Super Erode"] = 0.5f,
        ["Ultra Super Erode"] = 0.5f,
        ["Rust"] = 0.2f,
        ["Barnacles"] = 0.3f,
        ["Colored Barnacles"] = 0.3f,
        ["Clovers"] = 0.2f,
        ["Erode"] = 0.5f,
        ["Sand"] = 0.5f,
        ["Roughen"] = 0.05f,
        ["Impacts"] = 0.05f,
        ["Super Melt"] = 0.5f,
        ["Destructive Melt"] = 0.5f,
    };

    public static bool IsEffectCrossScreen(string name) => name switch
    {
        "Ivy" or "Rollers" or "Thorn Growers" or "Garbage Spirals" or "Spinets" or 
            "Small Springs" or "Fuzzy Growers" or "Wires" or "Chains" or "Colored Wires" or 
            "Colored Chains" or "Hang Roots" or "Thick Roots" or "Shadow Plants" or 
            "Colored Hang Roots" or "Colored Thick Roots" or "Colored Shadow Plants" or
            "Root Plants" or "Arm Growers" or "Growers" or "Mini Growers" or
            "Left Facing Kelp" or "Right Facing Kelp" or "Mixed Facing Kelp" or 
            "Bubble Grower" => true,
        
        _ => false
    };

    public static string[] EffectCategories { get; } = [
        "Natural",                  // 0
        "Erosion",                  // 1
        "Artificial",               // 2
        "Plants",                   // 3
        "Plants2",                  // 4
        "Plants3",                  // 5
        "Plants (Individual)",      // 6
        "Paint Effects",            // 7
        "Restoration",              // 8
        "Drought Plants",           // 9
        "Drought Plants 2",         // 10
        "Drought Plants 3",         // 11
        "Drought Erosion",          // 12
        "Drought Paint Effects",    // 13
        "Drought Natural",          // 14
        "Drought Artificial",       // 15
        "Dakras Plants",            // 16
        "Leo Plants",               // 17
        "Nautillo Plants"           // 18
    ];
    
    // Layers 2 and 3 do not show geo features like shortcuts and entrances 
    internal static readonly bool[] LayerStackableFilter =
    [
        false, 
        true, 
        true, 
        true, 
        false, // 5
        false, // 6
        false, // 7
        true, 
        false, // 9
        false, // 10
        true, 
        false, // 12
        false, // 13
        true, 
        true, 
        true, 
        true, 
        false, // 18
        false, // 19
        false, // 20
        false, // 21
        true
    ];

    internal static readonly bool[] GeoPathsFilter =
    [
        false, // 0
        false, // 1
        false, // 2
        false, // 3 
        true, // 4 entrance
        true, // 5 path
        true, // 6 passage
        true, // 7 den
        false, // 8
        false, // 9
        false, // 10
        false, // 11
        false, // 12
        false, // 13
        false, // 14
        false, // 15
        false, // 16
        false, // 17
        false, // 18
        true, // 19 wack
        false, // 20
        true  // 21 scavenger
    ];
    
    internal static Settings Settings { get; set; } = new(
            new GeneralSettings(),
            new Shortcuts(
                new GlobalShortcuts(),
                new GeoShortcuts(),
                new ExperimentalGeoShortcuts(),
                new TileShortcuts(),
                new CameraShortcuts(),
                new LightShortcuts(),
                new EffectsShortcuts(),
                new PropsShortcuts()
            ),
            new Misc(splashScreen:false, tileImageScansPerFrame: 100),
            new GeoEditor(
                new LayerColors(
                    layer1: new ConColor(0, 0, 0, 255),
                    layer2: new ConColor(0, 255, 0, 50),
                    layer3: new ConColor(255, 0, 0, 50)
                ),
                new ConColor(0, 0, 255, 70)
            ),
            new TileEditor(),
            new LightEditor(background: new ConColor(66, 108, 245, 255)),
            new EffectsSettings(
                effectColorLight:GREEN,
                effectColorDark:new(214, 187, 9, 255)),
            new PropEditor(),
            new Experimental()
    );
    
    #nullable enable
    
    internal static Font? Font { get; set; }
    
    /// Used when loading a level
    internal static Task<TileCheckResult>? TileCheck { get; set; } = null;
    
    /// Used when loading a level
    internal static Task<PropCheckResult>? PropCheck { get; set; } = null;
    
    public static Serilog.Core.Logger? Logger { get; set; }

    public static JsonSerializerOptions JsonSerializerOptions { get; set; } =
        new JsonSerializerOptions { WriteIndented = true };
    
    public static bool RendererExists { get; set; }
    
    public static Camera2D Camera { get; set; }

    internal static void OnLevelLoaded(object? sender, EventArgs e)
    {
        NewFlag = false;
        ResizeFlag = false;
    }
    
    internal static IntPtr WindowHandle { get; set; }

    internal static Color DarkThemeWaterColor { get; set; } = new(80, 80, 255, 110);
    internal static Color LightThemeWaterColor { get; set; } = new(0, 0, 255, 110);
    
    internal static Gram Gram { get; set; } = new(100);
#nullable disable
}
