/*
    I know partial classes are not favorable; lord forgive me.
*/

using Raylib_cs;
using static Raylib_cs.Raylib;

using System.Numerics;

using Leditor.Data;
using Leditor.Data.Tiles;
using Leditor.Data.Geometry;

using Color = Raylib_cs.Color;
using Leditor.Data.Materials;
using Leditor.Data.Props.Legacy;

namespace Leditor.Renderer.Partial;

/// <summary>
/// The main class responsible for rendering the whole level.
/// Is the rendering code is too big to be contained in a single file, 
/// therefor the class had to be partial.
/// </summary>
public partial class Engine
{
    /// <summary>
    /// Caches textures that are acquired from cast members. 
    /// This class does not manage any <see cref="Texture2D"/> field or property.
    /// </summary>
    protected class RenderState
    {
        private bool Initialized { get; set; }
        public bool Disposed { get; private set; }

        public RenderTexture2D vertImg;
        public RenderTexture2D horiImg;
        public RenderTexture2D largeSignGrad2;
        public RenderTexture2D TinySignsTexture;

        public TileDefinition? mediumTempleStone;
        public TileDefinition? TempleStoneWedge;
        public TileDefinition? TempleStoneSlopeSE;
        public TileDefinition? TempleStoneSlopeSW;

        public TileDefinition? SGFL;
        public TileDefinition? tileSetBigMetalFloor;

        public MaterialDefinition? templeStone;

        /// <summary>
        /// An array of tiles organized by size
        /// </summary>
        // public List<TileDefinition>[,] randomMachinesPool = new List<TileDefinition>[9, 9];

        /// A list of tiles `Random Machines` material pulls from
        public TileDefinition[] randomMachinesPool = [];

        /// A list of tiles `Random Metal` material pulls from
        public TileDefinition[] randomMetalPool = [];

        /// A list of tiles `Random Metals` material pulls from.
        /// Don't confuse it with `Random Metal`; There's a `s` in there.
        public TileDefinition[] randomMetalsPool = [];

        /// A list of tiles `Chaotic Stone 2` material pulls from
        public TileDefinition[] chaoticStone2Pool = [];

        /// <summary>
        /// A list of tiles used to render 'Dune Sand' material.
        /// </summary>
        public TileDefinition[] sandPool = [];

        /// <summary>
        /// Used to cache tiles used for large trash material.
        /// </summary>
        public TileDefinition[] largeTrashPool = [];
        
        /// <summary>
        /// Used to cache tiles used for mega trash material.
        /// </summary>
        public TileDefinition[] megaTrashPool = [];

        public List<Prop_Legacy> largeTrashRenderQueue = [];

        public Dictionary<string, Texture2D> tileSets = [];
        public Dictionary<string, Texture2D> wvTiles = [];
        public Dictionary<string, Texture2D> densePipesImages = [];
        public Dictionary<string, Texture2D> densePipesImages2 = [];

        /// <summary>
        /// Used for rendering `Random Metal` material
        /// </summary>
        public virtual string[] DR_RandomMetalsNeeded { get; } = [
            "Small Metal", "Metal Floor", "Square Metal", 
            "Big Metal", "Big Metal Marked", "Four Holes", 
            "Cross Beam Intersection"
        ];

        /// Used for (you guessed it) rendering `Chaotic Stone 2` material
        public virtual string[] ChaoticStone2Needed { get; } = ["Small Stone", "Square Stone", "Tall Stone", "Wide Stone", "Big Stone", "Big Stone Marked"];
        
        public virtual string[] RandomMetalsAllowed { get; } = ["Small Metal", "Metal Floor", "Square Metal", "Big Metal", "Big Metal Marked", "C Beam Horizontal AA", "C Beam Horizontal AB", "C Beam Vertical AA", "C Beam Vertical BA", "Plate 2"];

        public Texture2D bigChainHolder;
        public Texture2D fanBlade;
        public Texture2D BigWheelGraf;
        public Texture2D sawbladeGraf;
        public Texture2D randomCords;
        public Texture2D bigSigns1;
        public Texture2D bigSigns2;
        public Texture2D bigSignGradient;
        public Texture2D bigWesternSigns;
        public Texture2D smallAsianSigns;
        public Texture2D smallAsianSignsStation;
        public Texture2D glassImage;
        public Texture2D HarvesterAEye;
        public Texture2D HarvesterBEye;
        public Texture2D largerSigns;
        public Texture2D largeSignGrad;
        public Texture2D largerSignsStation;
        public Texture2D StationLamp;
        public Texture2D StationLampGradient;
        public Texture2D LumiaireH;
        public Texture2D LumHGrad;
        public Texture2D LumiaireV;
        public Texture2D LumVGrad;
        public Texture2D tinySigns;
        public Texture2D framework;

        public Texture2D pipeTiles;
        public Texture2D trashTiles;
        public Texture2D largeTrashTiles;
        public Texture2D dirtTiles;
        public Texture2D sandyDirtTiles;

        public Texture2D assortedTrash;

        public Texture2D rockTiles;

        public Texture2D bigJunk;
        public Texture2D roughRock;
        public Texture2D sandRR;

        public Texture2D rubbleGraf1;
        public Texture2D rubbleGraf2;
        public Texture2D rubbleGraf3;
        public Texture2D rubbleGraf4;

        public Texture2D ridgeBase;
        public Texture2D ridgeRocks;

        public Texture2D circuitsImage;
        public Texture2D densePipesImage;

        public Texture2D ceramicTileSocket;
        public Texture2D ceramicTileSocketNE;
        public Texture2D ceramicTileSocketNW;
        public Texture2D ceramicTileSocketSW;
        public Texture2D ceramicTileSocketSE;
        public Texture2D ceramicTileSocketFL;

        public Texture2D ceramicTileSilhCPSW;
        public Texture2D ceramicTileSilhCPNW;
        public Texture2D ceramicTileSilhCPSE;
        public Texture2D ceramicTileSilhCPNE;
        public Texture2D ceramicTileSilhCPFL;


        public virtual void Initialize(Registry registry)
        {
            if (Initialized) return;

            var levelEditorLib = registry.CastLibraries["levelEditor"];
            var internalLib = registry.CastLibraries["Internal"];
            var droughtLib = registry.CastLibraries["Drought"];
            var dryLib = registry.CastLibraries["Dry Editor"];
            var mscLib = registry.CastLibraries["MSC"];

            var tileSetsTask = Task.Run(() => {
                foreach (var lib in registry.CastLibraries.Values)
                {
                    foreach (var (key, member) in lib.Members)
                    {
                        if (key.StartsWith("tileSet")) tileSets[key] = member.Texture;
                    }
                }
            });

            var randomMachinesTilePoolTask = Task.Run(() => {
                // I had to hardcode them for now. Forgive me.
                string[] poolNames = [
                    // Machinery
                    "Metal Holes", "Dyson Fan", "Big Fan", "machine box A",
                    "machine box B", "machine box C_E", "machine box C_W",
                    "machine box C_Sym", "Tank Holder", "Machine Box D",
                    "Machine Box E L", "Machine Box E R", "Pillar Machine",
                    "Mud Elevator", "Elevator Track", "Huge Fan", "Sky Box",
                    "Pole Holder", "valve", "Hub Machine", "Monster Fan", 
                    "Compressor L", "Compressor R",
                    "Compressor Segment", "Giant Screw", "Pipe Box R", "Pipe Box L",
                    "Door Holder R", "Door Holder L",

                    // Machinery2
                    "Piston Top", "Piston Segment Empty", "Piston Head",
                    "Piston Segment Filled", "Piston Bottom",
                    "Piston Segment Horizontal A", "Piston Segment Horizontal B",
                    "Vertical Conveyor Belt B",
                    "Ventilation Box Empty", "Drill Head", "Drill A",
                    "Drill B", "Conveyor Belt Segment", "Conveyor Belt Wheel",
                    "Conveyor Belt Covered", "Conveyor Belt L", "Conveyor Belt R",
                    "Drill Shell A", "Drill Shell B", "Drill Shell Top",
                    "Drill Shell Bottom", "Big Drill", "Drill Rim",

                    // Small machines
                    "Small Machine A", "Small Machine B", "Small Machine C",
                    "Small Machine D", "Small Machine E", "Small Machine F",
                    "Small Machine G"
                ];

                randomMachinesPool = poolNames
                    .AsParallel()
                    .Where(n => registry.Tiles!.Names.ContainsKey(n))
                    .Select(registry.Tiles!.Get)
                    .Where(t => t.Size.Width <= 8 && t.Size.Height <= 8)
                    .ToArray();
                
                // for (int x = 1; x < 9; x++)
                // {
                //     for (int y = 1; y < 9; y++) randomMachinesPool[x, y] = [];
                // }
                    
                // foreach (var t in randomMachines) randomMachinesPool[t.Size.Width, t.Size.Height].Add(t);
            });

            var randomMetalPoolTask = Task.Run(() => {
                string[] names = [
                    "Small Metal", "Metal Floor", "Square Metal", 
                    "Big Metal", "Big Metal Marked", "C Beam Horizontal AA", 
                    "C Beam Horizontal AB", "C Beam Vertical AA", "C Beam Vertical BA", 
                    "Plate 2"
                ];

                randomMetalPool = names
                    .AsParallel()
                    .Select(registry.Tiles!.Get)
                    .ToArray();
            });

            var randomMetalsPoolTask = Task.Run(() => {
                randomMetalsPool = registry.Tiles!.Names.Values
                    .AsParallel()
                    .Where(t => t.Size.Width <= 8 && t.Size.Height <= 8 && !t.HasSpecsLayer(1) && RandomMetalsAllowed.Contains(t.Name))
                    .ToArray();
            });

            var chaoticStone2PoolTask = Task.Run(() => {
                chaoticStone2Pool = registry.Tiles!.Names.Values
                    .AsParallel()
                    .Where(t => 
                        t.Tags.Contains("chaoticStone2") || 
                        t.Tags.Contains("chaoticStone2 : rare") || 
                        t.Tags.Contains("chaoticStone2 : very rare") ||
                        ChaoticStone2Needed.Contains(t.Name)
                    ).ToArray();
            });

            var sandPoolTask = Task.Run(() => {
                sandPool = registry.Tiles!.Names.Values
                    .AsParallel()
                    .Where(t => t is { Size: (1, 1), Type: TileType.VoxelStructSandType } )
                    .ToArray();
            });

            var largeTrashPoolTask = Task.Run(() => {
                largeTrashPool = registry.Tiles!.Names.Values
                    .AsParallel()
                    .Where(t => t.Size.Width < 5 && t.Size.Height < 5 && !t.Tags.Contains("INTERNAL") && !t.Tags.Contains("notTrashProp") && !t.Tags.Contains("notTrashProp fix"))
                    .ToArray();
            });

            var megaTrashPoolTask = Task.Run(() => {
                megaTrashPool = registry.Tiles!.Names.Values
                    .AsParallel()
                    .Where(t => 
                        t.Size.Width >= 4 && 
                        t.Size.Width >= 4 && 
                        t.Size.Width <= 2 && 
                        t.Size.Height <= 20 &&
                        !t.Tags.Contains("notMegaTrashProp") &&
                        !t.Tags.Contains("colored") &&
                        !t.Tags.Contains("colored") &&
                        !t.Tags.Contains("effectColorA") &&
                        !t.Tags.Contains("effectColorB")
                    )
                    .ToArray();
            });

            var wvTilesTask = Task.Run(() => {
                wvTiles = registry.Materials!.Names.Values
                    .Where(m => m.RenderType == MaterialRenderType.WV)
                    .Select(m => {
                        var texture = registry.CastLibraries.Values.Where(c => c.Members.ContainsKey($"{m.Name}WVTiles")).Select(c => c[$"{m.Name}WVTiles"]).First().Texture;
                    
                        return (m.Name, texture);
                    })
                    .ToDictionary(m => m.Item1, m => m.Item2, StringComparer.OrdinalIgnoreCase);
            });

            var desnsePipesImagesTask = Task.Run(() => {
                densePipesImages = registry.Materials!.Names.Values
                    .Where(m => m.RenderType == MaterialRenderType.DensePipe)
                    .Select(m => {
                        var texture = registry.CastLibraries.Values.Where(c => c.Members.ContainsKey($"{m.Name}Image")).Select(c => c[$"{m.Name}WVTiles"]).First().Texture;
                    
                        return (m.Name, texture);
                    })
                    .ToDictionary(m => m.Item1, m => m.Item2, StringComparer.OrdinalIgnoreCase);
            });
            
            var desnsePipesImages2Task = Task.Run(() => {
                densePipesImages2 = registry.Materials!.Names.Values
                    .Where(m => m.RenderType == MaterialRenderType.DensePipe)
                    .Select(m => {
                        var texture = registry.CastLibraries.Values.Where(c => c.Members.ContainsKey($"{m.Name}Image2")).Select(c => c[$"{m.Name}WVTiles"]).First().Texture;
                    
                        return (m.Name, texture);
                    })
                    .ToDictionary(m => m.Item1, m => m.Item2, StringComparer.OrdinalIgnoreCase);
            });

            var ceramicTilesTask = Task.Run(() => {
                ceramicTileSocket = internalLib["ceramicTileSocket"].Texture;
                ceramicTileSocketNE = droughtLib["ceramicTileSocketNE"].Texture;
                ceramicTileSocketNW = droughtLib["ceramicTileSocketNW"].Texture;
                ceramicTileSocketSW = droughtLib["ceramicTileSocketSW"].Texture;
                ceramicTileSocketSE = droughtLib["ceramicTileSocketSE"].Texture;
                ceramicTileSocketFL = droughtLib["ceramicTileSocketFL"].Texture;

                ceramicTileSilhCPSW = droughtLib["ceramicTileSilhCPSW"].Texture;
                ceramicTileSilhCPNW = droughtLib["ceramicTileSilhCPNW"].Texture;
                ceramicTileSilhCPSE = droughtLib["ceramicTileSilhCPSE"].Texture;
                ceramicTileSilhCPNE = droughtLib["ceramicTileSilhCPNE"].Texture;
                ceramicTileSilhCPFL = droughtLib["ceramicTileSilhCPFL"].Texture;
            });

            var memberTask = Task.Run(() => {
                registry.Tiles?.Names.TryGetValue("Temple Stone Wedge", out TempleStoneWedge);
                registry.Tiles?.Names.TryGetValue("Temple Stone Slope SE", out TempleStoneSlopeSE);
                registry.Tiles?.Names.TryGetValue("Temple Stone Slope SW", out TempleStoneSlopeSW);

                // Creating tiles out of thin air because of course we should.
                SGFL = new TileDefinition("SGFL", (1, 1), TileType.VoxelStruct, 0, new int[0,0,0], [10], [], 1)
                {
                    Texture = droughtLib["SGFL"].Texture
                };

                tileSetBigMetalFloor = new TileDefinition(
                    "tileSetBigMetalFloor", 
                    (1, 1),
                    TileType.VoxelStruct,
                    1,
                    new int[0,0,0],
                    [6, 1, 1, 1, 1],
                    [],
                    1
                )
                {
                    Texture = droughtLib["tileSetBigMetalFloor"].Texture
                };

                bigChainHolder = internalLib["bigChainSegment"].Texture;
                fanBlade = internalLib["fanBlade"].Texture;
                BigWheelGraf = internalLib["Big Wheel Graf"].Texture;
                sawbladeGraf = droughtLib["sawbladeGraf"].Texture;
                randomCords = levelEditorLib["randomCords"].Texture;
                bigSigns1 = internalLib["bigSigns1"].Texture;
                bigSigns2 = internalLib["bigSigns2"].Texture;
                bigSignGradient = internalLib["bigSignGradient"].Texture;
                bigWesternSigns = internalLib["bigWesternSigns"].Texture;
                smallAsianSigns = internalLib["smallAsianSigns"].Texture;
                smallAsianSignsStation = droughtLib["smallAsianSignsStation"].Texture;
                glassImage = levelEditorLib["glassImage"].Texture;
                HarvesterAEye = levelEditorLib["HarvesterAEye"].Texture;
                HarvesterBEye = levelEditorLib["HarvesterBEye"].Texture;
                largerSigns = internalLib["largerSigns"].Texture;
                largeSignGrad = internalLib["largeSignGrad"].Texture;
                largerSignsStation = droughtLib["largerSignsStation"].Texture;
                StationLamp = dryLib["StationLamp"].Texture;
                StationLampGradient = dryLib["StationLampGradient"].Texture;
                LumiaireH = dryLib["LumiaireH"].Texture;
                LumHGrad = dryLib["LumHGrad"].Texture;
                LumiaireV = dryLib["LumiaireV"].Texture;
                LumVGrad = dryLib["LumVGrad"].Texture;
                tinySigns = internalLib["tinySigns"].Texture;
                framework = internalLib["frameWork"].Texture;

                pipeTiles = droughtLib["pipeTiles2"].Texture;
                trashTiles = droughtLib["trashTiles3"].Texture;
                largeTrashTiles = droughtLib["largeTrashTiles"].Texture;
                dirtTiles = droughtLib["dirtTiles"].Texture;
                sandyDirtTiles = droughtLib["sandyDirtTiles"].Texture;

                assortedTrash = levelEditorLib["assortedTrash"].Texture;

                rockTiles = droughtLib["rockTiles"].Texture;

                bigJunk = levelEditorLib["bigJunk"].Texture;
                roughRock = droughtLib["roughRock"].Texture;
                sandRR = droughtLib["sandRR"].Texture;

                rubbleGraf1 = levelEditorLib["rubbleGraf1"].Texture;
                rubbleGraf2 = levelEditorLib["rubbleGraf2"].Texture;
                rubbleGraf3 = levelEditorLib["rubbleGraf3"].Texture;
                rubbleGraf4 = levelEditorLib["rubbleGraf4"].Texture;

                ridgeBase = mscLib["ridgeBase"].Texture;
                ridgeRocks = mscLib["ridgeRocks"].Texture;

                circuitsImage = droughtLib["circuitsImage"].Texture;
                densePipesImage = droughtLib["dense PipesImage"].Texture;
            });

            var vTexture = levelEditorLib["vertImg"].Texture;
            var hTexture = levelEditorLib["horiImg"].Texture;
            var largeSignGrad2Texture = internalLib["largeSignGrad2"].Texture;

            vertImg = LoadRenderTexture(vTexture.Width, vTexture.Height);
            horiImg = LoadRenderTexture(hTexture.Width, hTexture.Height);
            largeSignGrad2 = LoadRenderTexture(largeSignGrad2Texture.Width, largeSignGrad2Texture.Height);
            
            var texture = internalLib["Tiny SignsTexture"].Texture;
            TinySignsTexture = LoadRenderTexture(texture.Width, texture.Height);

            BeginTextureMode(vertImg);
            {
                ClearBackground(Color.White);
                DrawTexture(vTexture, 0, 0, Color.White);
            }
            EndTextureMode();

            BeginTextureMode(horiImg);
            {
                ClearBackground(Color.White);
                DrawTexture(hTexture, 0, 0, Color.White);
            }
            EndTextureMode();

            BeginTextureMode(largeSignGrad2);
            {
                ClearBackground(Color.White);
                DrawTexture(largeSignGrad2Texture, 0, 0, Color.White);
            }
            EndTextureMode();

            BeginTextureMode(TinySignsTexture);
            {
                ClearBackground(Color.White);
                DrawTexture(internalLib["Tiny SignsTexture"].Texture, 0, 0, Color.White);
            }
            EndTextureMode();

            registry.Materials?.Names.TryGetValue("Temple Stone", out templeStone);
            registry.Tiles?.Names.TryGetValue("Medium Temple Stone", out mediumTempleStone);
            
            memberTask.Wait();
            tileSetsTask.Wait();
            randomMachinesTilePoolTask.Wait();
            randomMetalPoolTask.Wait();
            chaoticStone2PoolTask.Wait();
            randomMetalsPoolTask.Wait();
            sandPoolTask.Wait();
            largeTrashPoolTask.Wait();
            megaTrashPoolTask.Wait();
            wvTilesTask.Wait();
            desnsePipesImagesTask.Wait();
            desnsePipesImages2Task.Wait();
            ceramicTilesTask.Wait();

            Initialized = true;
        }

        public void Reset(Registry registry)
        {
            var lib = registry.CastLibraries["levelEditor"];
            var internalLib = registry.CastLibraries["Internal"];

            var vTexture = lib["vertImg"].Texture;
            var hTexture = lib["horiImg"].Texture;

            BeginTextureMode(vertImg);
            {
                ClearBackground(Color.White);
                DrawTexture(vTexture, 0, 0, Color.White);
            }
            EndTextureMode();

            BeginTextureMode(horiImg);
            {
                ClearBackground(Color.White);
                DrawTexture(hTexture, 0, 0, Color.White);
            }
            EndTextureMode();

            BeginTextureMode(largeSignGrad2);
            {
                ClearBackground(Color.White);
                DrawTexture(internalLib["largeSignGrad2"].Texture, 0, 0, Color.White);
            }
            EndTextureMode();

            BeginTextureMode(TinySignsTexture);
            {
                ClearBackground(Color.White);
                DrawTexture(internalLib["Tiny SignsTexture"].Texture, 0, 0, Color.White);
            }
            EndTextureMode();

            largeTrashRenderQueue.Clear();
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;

            UnloadRenderTexture(vertImg);
            UnloadRenderTexture(horiImg);
            UnloadRenderTexture(largeSignGrad2);
            UnloadRenderTexture(TinySignsTexture);
        }
    }

    public class Config
    {
        public bool InvisibleMarterialFix { get; set; }
        public bool MaterialFixes { get; set; }
        public bool RoughRockSpreadsMore { get; set; }
    }

    /// <summary>
    /// Not managed by this class
    /// </summary>
    public required Shaders Shaders { protected get; init; }
    public required Registry Registry { protected get; init; }

    protected readonly RenderTexture2D[] _layers;
    protected readonly RenderTexture2D[] _layersDC;
    protected readonly RenderTexture2D[] _gradientA;
    protected readonly RenderTexture2D[] _gradientB;
    protected RenderTexture2D _canvas;
    protected Texture2D _lightmap;

    protected RenderTexture2D _frontImage;
    protected RenderTexture2D _middleImage;
    protected RenderTexture2D _backImage;

    public RenderTexture2D FrontImage => _frontImage;
    public RenderTexture2D MiddleImage => _middleImage;
    public RenderTexture2D BackImage => _backImage;

    protected RNG Random { get; init; } = new();

    protected const int Columns = 100;
    protected const int Rows = 60;

    protected const int Width = 2000;
    protected const int Height = 1200;

    protected bool _tinySignsDrawn;
    protected bool _anyDecals;

    protected int _currentLayer;

    public RenderTexture2D Canvas
    {
        get
        {
            return _canvas;
        }
    }

    #if DEBUG
    public RenderTexture2D[] Layers => _layers;
    #endif

    /// <summary>
    /// The level's state is not managed by <see cref="Engine"/>
    /// </summary>
    public LevelState? Level { get; protected set; }

    public Serilog.ILogger? Logger { get; init; }

    public bool Disposed { get; protected set; }
    public bool Initialized { get; protected set; }

    protected RenderState State { get; set; }

    protected Config Configuration { get; set; }


    public Engine()
    {
        _layers = new RenderTexture2D[30];
        _layersDC = new RenderTexture2D[30];
        _gradientA = new RenderTexture2D[30];
        _gradientB = new RenderTexture2D[30];

        State = new();
        Configuration = new();
    }

    ~Engine()
    {
        if (!Disposed) throw new InvalidOperationException("Engine was not disposed by consumer");
    }

    /// <summary>
    /// Must be called within a GL context.
    /// </summary>
    public void Initialize()
    {
        if (Initialized) return;

        Logger?.Information("[Engine::Initialize] Begin initializing renderer");

        for (var l = 0; l < 30; l++)
        {
            _layers[l] = LoadRenderTexture(Width, Height);
            _layersDC[l] = LoadRenderTexture(Width, Height);
            _gradientA[l] = LoadRenderTexture(Width, Height);
            _gradientB[l] = LoadRenderTexture(Width, Height);

            BeginTextureMode(_layers[l]);
            ClearBackground(Color.White);
            EndTextureMode();

            BeginTextureMode(_layersDC[l]);
            ClearBackground(Color.White);
            EndTextureMode();

            BeginTextureMode(_gradientA[l]);
            ClearBackground(Color.White);
            EndTextureMode();

            BeginTextureMode(_gradientB[l]);
            ClearBackground(Color.White);
            EndTextureMode();
        }

        _canvas = LoadRenderTexture(Width, Height);

        BeginTextureMode(_canvas);
        ClearBackground(Color.White);
        EndTextureMode();

        _frontImage = LoadRenderTexture(Width, Height);
        _middleImage = LoadRenderTexture(Width, Height);
        _backImage = LoadRenderTexture(Width, Height);

        State.Initialize(Registry);

        Initialized = true;
    }

    public void Configure(in Config c)
    {
        Configuration = c;
    }

    public void Compose(float offsetX, float offsetY)
    {
        var shader = Shaders.WhiteRemoverVFlipDepthAccumulator;

        BeginTextureMode(_canvas);

        ClearBackground(Color.White);

        for (var l = 29; l >= 0; l--)
        {
            var layer = _layers[l];

            BeginShaderMode(shader);
            SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), layer.Texture);
            SetShaderValue(shader, GetShaderLocation(shader, "d"), l / 30f, ShaderUniformDataType.Float);

            DrawTextureRec(layer.Texture, new(0, 0, layer.Texture.Width, layer.Texture.Height), new(29 - l * offsetX, 29 - l * offsetY), Color.White);

            EndShaderMode();
        }

        EndTextureMode();
    }

    protected virtual void Clear()
    {
        if (!Initialized) return;

        Logger?.Information("[Engine::Clear] Clearing engine's data");

        for (var l = 0; l < 30; l++)
        {
            BeginTextureMode(_layers[l]);
            ClearBackground(Color.White);
            EndTextureMode();

            BeginTextureMode(_layersDC[l]);
            ClearBackground(Color.White);
            EndTextureMode();

            BeginTextureMode(_gradientA[l]);
            ClearBackground(Color.White);
            EndTextureMode();

            BeginTextureMode(_gradientB[l]);
            ClearBackground(Color.White);
            EndTextureMode();
        }

        BeginTextureMode(_canvas);
        ClearBackground(Color.White);
        EndTextureMode();
        
        if (_lightmap.Id != 0)
        {
            UnloadTexture(_lightmap);
            _lightmap.Id = 0;
        }

        Level = null;
        _tinySignsDrawn = false;
        _anyDecals = false;
        _currentLayer = 0;
    }

    public void Dispose()
    {
        Logger?.Information("[Engine::Dispose] Disposing engine");

        if (Disposed || !Initialized) return;
        Disposed = true;

        UnloadRenderTexture(_canvas);
        
        for (var l = 0; l < 30; l++)
        {
            UnloadRenderTexture(_layers[l]);
            UnloadRenderTexture(_layersDC[l]);
            UnloadRenderTexture(_gradientA[l]);
            UnloadRenderTexture(_gradientB[l]);
        }

        if (_lightmap.Id != 0) UnloadTexture(_lightmap);
        Level = null;

        if (_frontImage.Id != 0) UnloadRenderTexture(_frontImage);
        if (_middleImage.Id != 0) UnloadRenderTexture(_middleImage);
        if (_backImage.Id != 0) UnloadRenderTexture(_backImage);

        _frontImage.Id = 0;
        _middleImage.Id = 0;
        _backImage.Id = 0;

        State.Dispose();
    }

    public virtual void Load(in LevelState level)
    {
        if (!Initialized) return;
        
        Logger?.Information("[Engine::Load] Loading a new level \"{name}\"", level.ProjectName);

        Clear();

        Level = level;

        if (_lightmap.Id != 0) UnloadTexture(_lightmap);
        _lightmap = LoadTextureFromImage(Level.LightMap);

        Random.Seed = (uint)Level.Seed;

        State.Reset(Registry);
    }

    protected virtual int GenSeedForTile(int x, int y, int columns, int seed)
    {
        return seed + x + y * columns;
    }

    
    protected virtual bool IsMyTileOpenToThisTile(
        MaterialDefinition material,
        int x,
        int y,
        int layer
    )
    {
        if (Data.Utils.InBounds(Level!.GeoMatrix, x, y))
        {
            ref var geoCell = ref Level!.GeoMatrix[y, x, layer];

            if (geoCell.IsSlope || geoCell.Type is GeoType.Solid)
            {
                ref var tileCell = ref Level!.TileMatrix[y, x, layer];
                
                if (tileCell.Type is TileCellType.Material && tileCell.MaterialDefinition == material)
                {
                    return true;
                }

                if (tileCell.Type is TileCellType.Default && Level!.DefaultMaterial == material)
                {
                    return true;
                }
            }
        }
        else return Level?.DefaultMaterial == material;

        return false;
    }

    protected virtual void DrawTinySigns()
    {
        BeginTextureMode(State.TinySignsTexture);
        {
            ClearBackground(Color.Green);
        }
        EndTextureMode();

        var lang = 1;

        Vector2[] blueList = [new( 1,  1), new( 1,  0), new( 0,  1)]; 
        Vector2[] redList =  [new(-1, -1), new(-1,  0), new( 0, -1)];

        var shader = Shaders.WhiteRemoverApplyColor;

        var size = 8;

        for (var c = 0; c <= 100; c++)
        {
            for (var q = 0; q <= 135; q++)
            {
                var middle = new Vector2((c + 0.5f) * size, (q + 0.5f) * size);
            
                var gtPos = new Vector2(Random.Generate(new ReadOnlySpan<int>([20, 14, 1])[lang]), lang + 1);

                if (Random.Generate(50) == 1)
                {
                    lang = 2;
                }
                else if (Random.Generate(80) == 1)
                {
                    lang = 1;
                }

                if (Random.Generate(7) == 1)
                {
                    if (Random.Generate(3) == 1)
                    {
                        gtPos = new Vector2(1, 3);
                    }
                    else
                    {
                        gtPos = new Vector2(Random.Generate(Random.Generate(7)), 3);
                    
                        if (Random.Generate(5) == 1)
                        {
                            lang = 2;
                        }
                        else if (Random.Generate(10) == 1)
                        {
                            lang = 1;
                        }
                    }
                }

                BeginTextureMode(State.TinySignsTexture);
                {
                    foreach (var p in redList)
                    {
                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.tinySigns);
                        DrawTexturePro(
                            State.tinySigns,
                            new Rectangle(
                                (gtPos.X - 1)*6,
                                (gtPos.Y - 1)*6,
                                6,
                                6
                            ),
                            new Rectangle(
                                middle.X - 3 + p.X,
                                middle.Y - 3 + p.Y,
                                6,
                                6
                            ),
                            Vector2.Zero,
                            0,
                            Color.Red
                        );
                        EndShaderMode();
                    }

                    foreach (var p in blueList)
                    {
                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.tinySigns);
                        DrawTexturePro(
                            State.tinySigns,
                            new Rectangle(
                                (gtPos.X - 1)*6,
                                (gtPos.Y - 1)*6,
                                6,
                                6
                            ),
                            new Rectangle(
                                middle.X - 3 + p.X,
                                middle.Y - 3 + p.Y,
                                6,
                                6
                            ),
                            Vector2.Zero,
                            0,
                            Color.Blue
                        );
                        EndShaderMode();
                    }

                    BeginShaderMode(shader);
                    SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.tinySigns);
                    DrawTexturePro(
                        State.tinySigns,
                        new Rectangle(
                            (gtPos.X - 1)*6,
                            (gtPos.Y - 1)*6,
                            6,
                            6
                        ),
                        new Rectangle(
                            middle.X - 3,
                            middle.Y - 3,
                            6,
                            6
                        ),
                        Vector2.Zero,
                        0,
                        Color.Green
                    );
                    EndShaderMode();
                }
                EndTextureMode();
            }
        }
    }

    /// <summary>
    /// This gets called each frame
    /// </summary>
    public virtual void Render()
    {
        if (Level is null || !Initialized) return;
        if (_currentLayer > 2) return;

        var layer = _currentLayer++;

        BeginTextureMode(_frontImage);
        ClearBackground(Color.White);
        EndTextureMode();

        BeginTextureMode(_middleImage);
        ClearBackground(Color.White);
        EndTextureMode();
        
        BeginTextureMode(_backImage);
        ClearBackground(Color.White);
        EndTextureMode();

        var poleColor = Color.Red;

        var camera = Level.Cameras[0];

        List<(int rnd, int x, int y)> drawLaterTiles = [];
        List<(int rnd, int x, int y)> drawLastTiles = [];
        List<(int rnd, int x, int y)> entrances = [];
        List<(int x, int y)> shortcuts = [];

        Dictionary<MaterialDefinition, List<(int rnd, int x, int y)>> drawMaterials = [];

        BeginTextureMode(_middleImage);

        // Setting drawing order
        for (var x = 0; x < Columns; x++)
        {
            for (var y = 0; y < Rows; y++)
            {
                // Acquire absolute position
                var mx = x + (int)camera.Coords.X / 20;
                var my = y + (int)camera.Coords.Y / 20;

                // Must be in matrix bounds
                if (mx < 0 || mx >= Level.Width || my < 0 || my >= Level.Height) continue;

                ref var geoCell = ref Level.GeoMatrix[my, mx, layer];
                ref var topCell = ref Level.GeoMatrix[my, mx, 0];

                var isEntrance = geoCell[GeoFeature.ShortcutEntrance];
            
                if (geoCell[GeoFeature.VerticalPole])
                {
                    DrawRectangle(mx * 20 + 8, my * 20, 20 - 16, 20, poleColor);
                }

                if (geoCell[GeoFeature.HorizontalPole])
                {
                    DrawRectangle(mx * 20, my * 20 + 8, 20, 20 - 16, poleColor);
                }

                if (topCell[GeoType.ShortcutEntrance] && layer == 0)
                {
                    entrances.Add((Random.Generate(1000), mx, my));
                    continue;
                }

                if (topCell[GeoFeature.ShortcutPath])
                {
                    if (layer == 0 && 
                        topCell[GeoType.Solid] && 
                        Level.TileMatrix[my, mx, 0].Type is TileCellType.Default or TileCellType.Material)
                    {
                        shortcuts.Add((mx, my));
                    }
                    else if (layer == 1 && 
                        Level.GeoMatrix[my, mx, 1][GeoType.Solid] &&
                        !topCell[GeoType.Solid] &&
                        Level.TileMatrix[my, mx, 1].Type is TileCellType.Default or TileCellType.Material)
                    {
                        shortcuts.Add((mx, my));
                    }
                }

                ref var tileCell = ref Level.TileMatrix[my, mx, layer];


                if (tileCell.Type is TileCellType.Head)
                {
                    if (tileCell.TileDefinition?.Tags.Contains("drawLast") == true)
                    {
                        drawLastTiles.Add((Random.Generate(999), mx, my));
                    }
                    else
                    {
                        drawLaterTiles.Add((Random.Generate(999), mx, my));
                    }
                }
                else if (tileCell.Type is not TileCellType.Body)
                {
                    drawLaterTiles.Add((Random.Generate(999), mx, my));
                }
            }
        }

        drawLastTiles.Sort((t1, t2) => {
            if (t1.rnd > t2.rnd) return 1;
            if (t1.rnd < t2.rnd) return -1;
            if (t1.rnd == t2.rnd) return 0;
            return 0;
        });

        EndTextureMode();

        // Draw
        foreach (var tile in drawLaterTiles)
        {
            // var tileSeed = GenSeedForTile(tile.x, tile.y, Level.Width, Level.Seed + layer); // unused?
            ref var queuedCell = ref Level.TileMatrix[tile.y, tile.x, layer];

            switch (queuedCell.Type)
            {
                case TileCellType.Material:
                {
                    if (queuedCell.MaterialDefinition is not null)
                    {
                        if (drawMaterials.TryGetValue(queuedCell.MaterialDefinition, out var list))
                        {
                            list.Add(tile);
                        }
                        else
                        {
                            drawMaterials[queuedCell.MaterialDefinition] = [ tile ];
                        }
                    }
                    break;
                }

                case TileCellType.Default:
                {
                    if (drawMaterials.TryGetValue(Level.DefaultMaterial, out var list))
                    {
                        list.Add(tile);
                    }
                    else
                    {
                        drawMaterials[Level.DefaultMaterial] = [ tile ];
                    }
                }
                    break;
                
                case TileCellType.Head:
                {
                    if (queuedCell.TileDefinition is not null)
                    {
                        DrawTile_MTX(queuedCell, queuedCell.TileDefinition, tile.x, tile.y, layer, camera, _frontImage);
                    }
                }
                break;
            }
        }

        foreach (var (material, queued) in drawMaterials)
        {
            // May be redundant
            if (queued.Count == 0) continue;

            Console.WriteLine($"{material.Name}: {queued.Count}");

            switch (material.RenderType)
            {
                case MaterialRenderType.Invisible:
                if (!Configuration.InvisibleMarterialFix) {
                    foreach (var q in queued) DrawMaterial_MTX(q.x, q.y, layer, camera, material, _frontImage);
                }
                break;

                case MaterialRenderType.Unified:
                foreach (var q in queued) DrawMaterial_MTX(q.x, q.y, layer, camera, material, _frontImage);
                break;

                case MaterialRenderType.CustomUnified:
                {
                    // To be implemented
                }
                break;

                case MaterialRenderType.Tiles:
                RenderTileMaterial_MTX(layer, camera, material, _frontImage);
                break;

                case MaterialRenderType.Pipe:
                foreach (var q in queued)
                {
                    if (Level.GeoMatrix[q.y, q.x, layer][GeoType.Air]) continue;

                    DrawPipeMaterial_MTX(material, q.x, q.y, layer, camera);
                }
                break;

                case MaterialRenderType.Rock:
                {
                    foreach (var q in queued)
                    {
                        if (Level.GeoMatrix[q.y, q.x, layer][GeoType.Air]) continue;

                        DrawRockMaterial_MTX(material, q.x, q.y, layer, camera, false);
                    }
                }
                break;

                case MaterialRenderType.LargeTrash:
                {
                    foreach (var q in queued)
                    {
                        ref var cell = ref Level.GeoMatrix[q.y, q.x, layer]; 
                        
                        if (Configuration.MaterialFixes && cell.Type is GeoType.SlopeNE or GeoType.SlopeNW or GeoType.SlopeES or GeoType.SlopeSW or GeoType.Platform)
                        {
                            DrawPipeMaterial_MTX(material, q.x, q.y, layer, camera);
                        }
                        
                        if (cell.Type is GeoType.Solid || cell.IsSlope)
                        {
                            DrawLargeTrashMaterial_MTX(material, q.x, q.y, layer, camera, _frontImage);
                        }
                    }
                }
                break;
            
                case MaterialRenderType.RoughRock:
                foreach (var q in queued) {
                    ref var cell = ref Level.GeoMatrix[q.y, q.x, layer];

                    if (cell.IsSlope || cell[GeoType.Platform]) {
                        if (material.Name == "Rough Rock") {
                            DrawRockMaterial_MTX(material, q.x, q.y, layer, camera, true);
                        } else if (material.Name == "Sandy Dirt") {
                            DrawPipeMaterial_MTX(material, q.x, q.y, layer, camera);
                        }
                    }

                    if (cell[GeoType.Solid]) {
                        DrawRoughRockMaterial_MTX(material, q.x, q.y, layer, camera, _frontImage);
                    }
                }
                break;
            
                case MaterialRenderType.MegaTrash:
                foreach (var q in queued) {
                    ref var cell = ref Level.GeoMatrix[q.y, q.x, layer];

                    if (cell.IsSlope || cell[GeoType.Solid]) {
                        DrawPipeMaterial_MTX(material, q.x, q.y, layer, camera);
                    }

                    if (!cell[GeoType.Air]) {
                        DrawMegaTrashMaterial_MTX(material, q.x, q.y, layer, camera, _frontImage);
                    }
                }
                break;
            
                case MaterialRenderType.Dirt:
                foreach (var q in queued) {
                    ref var cell = ref Level.GeoMatrix[q.y, q.x, layer];

                    if (
                        Configuration.MaterialFixes && (cell.IsSlope || cell[GeoType.Platform])
                    ) {
                        DrawPipeMaterial_MTX(material, q.x, q.y, layer, camera);
                    }

                    if (cell[GeoType.Solid]) {
                        DrawDirtMaterial_MTX(material, q.x, q.y, layer, camera, _frontImage);
                    }
                }
                break;
            
                case MaterialRenderType.Sandy:
                {
                    // Render sandy materials
                }
                break;

                case MaterialRenderType.WV:
                foreach (var q in queued) {
                    ref var cell = ref Level.GeoMatrix[q.y, q.x, layer];
                    
                    if (cell.IsSlope || cell[GeoType.Solid]) {
                        DrawWVMaterial_MTX(material, q.x, q.y, layer, camera);
                    }
                }
                break;

                case MaterialRenderType.Ridge:
                foreach (var q in queued) {
                    if (!Level.GeoMatrix[q.y, q.x, layer][GeoType.Solid]) continue;

                    DrawRidgeMaterial_MTX(material, q.x, q.y, layer, camera, _frontImage);
                }
                break;

                case MaterialRenderType.DensePipe:
                foreach (var q in queued) {
                    if (Level!.GeoMatrix[q.y, q.x, layer][GeoType.Air]) continue;

                    DrawDensePipeMaterial_MTX(material, q.x, q.y, layer, camera, _frontImage);
                }
                break;
            
                case MaterialRenderType.RandomPipes:
                foreach (var q in queued) {
                    if (Level.GeoMatrix[q.y, q.x, layer][GeoType.Air]) continue;

                    DrawRandomPipesMaterial_MTX(material, q.x, q.y, layer, camera, _frontImage);
                }
                break;
            }
        }
    }
}