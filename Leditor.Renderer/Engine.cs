using ImGuiNET;
using rlImGui_cs;

using Raylib_cs;
using static Raylib_cs.Raylib;

using System.Numerics;
using System.Linq;

using Leditor.Data;
using Leditor.Data.Tiles;
using Leditor.Data.Geometry;

using Color = Raylib_cs.Color;
using Leditor.Data.Materials;
using Leditor.Renderer.RL;

namespace Leditor.Renderer;

/// <summary>
/// The main class responsible for rendering the whole level.
/// Is the rendering code is too big to be contained in a single file, 
/// therefor the class had to be partial.
/// </summary>
public partial class Engine
{
    /// <summary>
    /// Stores textures that are acquired from cast members. 
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

        public Dictionary<string, Texture2D> tileSets = [];

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

        public virtual void Initialize(Registry registry)
        {
            if (Initialized) return;

            var levelEditorLib = registry.CastLibraries["levelEditor"];
            var internalLib = registry.CastLibraries["Internal"];
            var droughtLib = registry.CastLibraries["Drought"];
            var dryLib = registry.CastLibraries["Dry Editor"];

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
            
            memberTask.Wait();
            tileSetsTask.Wait();
            randomMachinesTilePoolTask.Wait();
            randomMetalPoolTask.Wait();
            chaoticStone2PoolTask.Wait();
            randomMetalsPoolTask.Wait();
            sandPoolTask.Wait();

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

    protected RNG Random { get; init; } = new();

    protected const int Columns = 100;
    protected const int Rows = 60;

    protected const int Width = 2000;
    protected const int Height = 1200;

    protected bool _tinySignsDrawn;

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

        State.Initialize(Registry);

        Initialized = true;
    }

    public void Configure(in Config c)
    {
        Configuration = c;
    }

    public void Compose(int offsetX, int offsetY)
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

            DrawTexture(layer.Texture, 29 - l * offsetX, 29 - l * offsetY, Color.White);

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

        State.Dispose();
    }

    public virtual void Load(in LevelState level)
    {
        if (!Initialized) return;
        
        Logger?.Information("[Engine::Load] Loading a new level \"{name}\"", level.ProjectName);

        Clear();

        Level = level;
        _lightmap = LoadTextureFromImage(Level.LightMap);

        Random.Seed = (uint)Level.Seed;

        State.Reset(Registry);
    }

    protected virtual int GenSeedForTile(int x, int y, int columns, int seed)
    {
        return seed + x + y * columns;
    }

    /// <summary>
    /// Draws a tile into the canvas
    /// </summary>
    /// <param name="tile">Tile definition</param>
    /// <param name="x">Matrix X coordinates</param>
    /// <param name="y">Matrix Y coordinates</param>
    /// <param name="layer">The current layer (0, 1, 2)</param>
    /// <param name="camera">The current render camera</param>
    /// <param name="rt">the temprary canvas to draw on</param>
    protected virtual void DrawTile_MTX(
        Tile cell,
        TileDefinition tile,
        int x, 
        int y, 
        int layer,
        in RenderCamera camera, 
        RenderTexture2D rt
    )
    {
        var (hx, hy) = Data.Utils.GetTileHeadPositionI(tile);

        int startX = x - hx;
        int startY = y - hy;

        var colored = tile.Tags.Contains("colored");
        var effectColorA = tile.Tags.Contains("effectColorA");
        var effectColorB = tile.Tags.Contains("effectColorB");

        var (width, height) = tile.Size;
        var bf = tile.BufferTiles;

        SetTextureWrap(tile.Texture, TextureWrap.Clamp);

        switch (tile.Type)
        {
            case TileType.Box:
            {
                var num = tile.Size.Width * tile.Size.Height;
                var n = 0;

                for (var g = startX; g < startX + tile.Size.Width; g++)
                {
                    for (var h = startY; h < startY + tile.Size.Height; h++)
                    {
                        var rect = new Rectangle(g * 20, h * 20, 20, 20);
                    
                        BeginTextureMode(State.vertImg);
                        DrawTexturePro(tile.Texture, new(20, n * 20, 20, 20), new(g * 20, h * 20, 20, 20), Vector2.Zero, 0, Color.White);
                        EndTextureMode();

                        BeginTextureMode(State.horiImg);
                        DrawTexturePro(tile.Texture, new( 0, n * 20, 20, 20), new(g * 20, h * 20, 20, 20), Vector2.Zero, 0, Color.White);
                        EndTextureMode();

                        BeginTextureMode(rt);
                        {
                            Rectangle dest = new(
                                startX * 20 - 20 * bf,
                                startY * 20 - 20 * bf,

                                width  * 20 + 2 * 20 * bf,
                                height * 20 + 2 * 20 * bf
                            );

                            Rectangle src = new(
                                0,
                                0 + num * 20,
                                width  * 20 + 2 * 20 * bf,
                                height * 20 + 2 * 20 * bf
                            );

                            var rnd = Random.Generate(tile.Rnd);

                            src.X += src.Width * (rnd - 1);

                            var shader = Shaders.WhiteRemover;
                            BeginShaderMode(shader);
                            SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), tile.Texture);

                            DrawTexturePro(
                                tile.Texture,
                                src,
                                dest,
                                Vector2.Zero,
                                0,
                                Color.White
                            );

                            EndShaderMode();
                        }
                        EndTextureMode();

                        n++;
                    }
                }
            }
            break;
        
            case TileType.VoxelStruct:
            {
                var sublayer = layer * 10;

                Rectangle dest = new(
                    startX * 20 - 20 * bf,
                    startY * 20 - 20 * bf,
                    
                    width  * 20 + 2 * 20 * bf,
                    height * 20 + 2 * 20 * bf
                );

                Rectangle src = new(
                    0,
                    0,
                    (width  + 2 * bf) * 20,
                    (height + 2 * bf) * 20
                );

                int rnd;

                if (tile.Rnd == -1)
                {
                    rnd = 1;

                    foreach (var dir in new (int x, int y)[4] { (-1, 0), (0, -1), (1, 0), (0, 1) })
                    {
                        var cx = x + dir.x + (int)camera.Coords.X;
                        var cy = y + dir.y + (int)camera.Coords.Y;
                    
                        GeoType cellType = GeoType.Solid;

                        if (Data.Utils.InBounds(Level!.GeoMatrix, cx, cy))
                        {
                            cellType = Level!.GeoMatrix[cy, cx, 0].Type;
                        }

                        if (cellType is GeoType.Air or GeoType.Platform) break;
                        
                        rnd++;
                    }
                }
                else
                {
                    rnd = Random.Generate(tile.Rnd);
                }

                if (tile.Tags.Contains("ramp"))
                {
                    rnd = 2;

                    GeoType cellType = GeoType.Solid;

                    var cx = x + (int)camera.Coords.X;
                    var cy = y + (int)camera.Coords.Y;

                    if (Data.Utils.InBounds(Level!.GeoMatrix, cx, y + cy))
                    {
                        cellType = Level!.GeoMatrix[cy, cx, 0].Type;
                    }

                    if (cellType is GeoType.SlopeNW)
                    {
                        rnd = 1;
                    }
                }
            
                src.X += src.Width * (rnd - 1);
                src.Y += 1;

                BeginTextureMode(rt);
                {
                    var shader = Shaders.WhiteRemover;
                    BeginShaderMode(shader);
                    SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), tile.Texture);
                    DrawTexturePro(
                        tile.Texture,
                        src,
                        dest,
                        Vector2.Zero,
                        0,
                        Color.White
                    );
                    EndShaderMode();
                }
                EndTextureMode();

                var d = -1;

                for (var l = 0; l < tile.Repeat.Length; l++)
                {
                    for (var repeat = 0; repeat < tile.Repeat[l]; repeat++)
                    {
                        d++;

                        if (d + sublayer > 29) goto out_of_repeat;

                        BeginTextureMode(_layers[d + sublayer]);
                        {
                            var shader = Shaders.WhiteRemover;

                            var currentSrc = src with { 
                                X = src.X + src.Width * (rnd - 1), 
                                Y = src.Y + src.Height * l 
                            };
                            
                            BeginShaderMode(shader);
                            SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), tile.Texture);
                            DrawTexturePro(
                                tile.Texture,
                                currentSrc,
                                dest,
                                Vector2.Zero,
                                0,
                                Color.White
                            );
                            EndShaderMode();

                            if (colored && !effectColorA && !effectColorB)
                            {
                                BeginTextureMode(_layersDC[d + sublayer]);
                                Draw.DrawTextureDarkest(
                                    tile.Texture, 
                                    currentSrc,
                                    dest
                                );
                                EndTextureMode();
                            }

                            if (effectColorA)
                            {
                                BeginTextureMode(_gradientA[d + sublayer]);
                                Draw.DrawTextureDarkest(
                                    tile.Texture,
                                    currentSrc,
                                    dest
                                );
                                EndTextureMode();
                            }

                            if (effectColorB)
                            {
                                BeginTextureMode(_gradientB[d + sublayer]);
                                Draw.DrawTextureDarkest(
                                    tile.Texture,
                                    currentSrc,
                                    dest
                                );
                                EndTextureMode();
                            }
                        }
                        EndTextureMode();
                    }
                }

                out_of_repeat:
                {}
            }
            break;
        
            case TileType.VoxelStructRandomDisplaceHorizontal:
            case TileType.VoxelStructRandomDisplaceVertical:
            {
                var sublayer = layer * 10;

                Rectangle dest = new(
                    (startX - bf) * 20,
                    (startY - bf) * 20,

                    (width  + 2 * bf) * 20,
                    (height + 2 * bf) * 20
                );

                Rectangle src = new(
                    0,
                    0,
                    (width  + 2 * bf) * 20,
                    (height + 2 * bf) * 20
                );

                Rectangle src1, src2, dest1, dest2;

                if (tile.Type is TileType.VoxelStructRandomDisplaceVertical)
                {
                    var rndSeed = Level!.Seed + x;

                    var dsplcPoint = Random.Generate((int)src.Height);

                    src1 = src with { Height = dsplcPoint };
                    src2 = src with { Y = src.Y + dsplcPoint, Height = src.Height - dsplcPoint };

                    dest1 = dest with { Y = dest.Y + dest.Height - dsplcPoint };
                    dest2 = dest with { Height = dest.Height - dsplcPoint };
                }
                else
                {
                    var rndSeed = Level!.Seed + y;

                    var dsplcPoint = Random.Generate((int)src.Width);

                    src1 = src with { Width = dsplcPoint };
                    src2 = src with { X = src.X + dsplcPoint };
                
                    dest1 = dest with { X = dest.X + dest.Width + dsplcPoint };
                    dest2 = dest with { Width = dest.Width - dsplcPoint };
                }

                src1.Y += 1;
                src2.Y += 1;

                BeginTextureMode(rt);
                {
                    var shader = Shaders.WhiteRemover;
                    BeginShaderMode(shader);
                    SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), tile.Texture);
                    DrawTexturePro(
                        tile.Texture,
                        src1,
                        dest1,
                        Vector2.Zero,
                        0,
                        Color.White
                    );
                    DrawTexturePro(
                        tile.Texture,
                        src2,
                        dest2,
                        Vector2.Zero,
                        0,
                        Color.White
                    );
                    EndShaderMode();
                }
                EndTextureMode();

                var d = -1;

                for (var l = 0; l < tile.Repeat.Length; l++)
                {
                    for (var repeat = 0; repeat < tile.Repeat[l]; repeat++)
                    {
                        d++;

                        if (d + sublayer > 29) goto out_of_repeat;

                        BeginTextureMode(_layers[d + sublayer]);
                        {
                            var shader = Shaders.WhiteRemover;
                            BeginShaderMode(shader);
                            SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), tile.Texture);
                            DrawTexturePro(
                                tile.Texture,
                                src1 with { Y = src1.Y + src1.Height * l },
                                dest1,
                                Vector2.Zero,
                                0,
                                Color.White
                            );

                            DrawTexturePro(
                                tile.Texture,
                                src2 with { Y = src2.Y + src2.Height * l },
                                dest2,
                                Vector2.Zero,
                                0,
                                Color.White
                            );
                            EndShaderMode();
                        }
                        EndTextureMode();

                        var newSrc1 = src1 with {
                            X = src1.X + (width + 2 * bf) * 20,
                            Y = src1.Y + src1.Height * l + (height + 2 * bf) * 20 
                        };

                        var newSrc2 = src1 with {
                            X = src2.X + (width + 2 * bf) * 20,
                            Y = src2.Y + src2.Height * l + (height + 2 * bf) * 20 
                        };

                        if (colored && !effectColorA && !effectColorB)
                        {
                            BeginTextureMode(_layersDC[d + sublayer]);
                            {
                                var shader = Shaders.WhiteRemover;
                                BeginShaderMode(shader);
                                SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), tile.Texture);
                                DrawTexturePro(
                                    tile.Texture,
                                    newSrc1,
                                    dest1,
                                    Vector2.Zero,
                                    0,
                                    Color.White
                                );

                                DrawTexturePro(
                                    tile.Texture,
                                    newSrc2,
                                    dest2,
                                    Vector2.Zero,
                                    0,
                                    Color.White
                                );
                                EndShaderMode();
                            }
                            EndTextureMode();
                        }

                        if (effectColorA)
                        {
                            BeginTextureMode(_gradientA[d + sublayer]);
                            {
                                Draw.DrawTextureDarkest(
                                    tile.Texture,
                                    newSrc1,
                                    dest1
                                );

                                Draw.DrawTextureDarkest(
                                    tile.Texture,
                                    newSrc2,
                                    dest2
                                );
                            }
                            EndTextureMode();
                        }
                        
                        if (effectColorB)
                        {
                            BeginTextureMode(_gradientB[d + sublayer]);
                            {
                                Draw.DrawTextureDarkest(
                                    tile.Texture,
                                    newSrc1,
                                    dest1
                                );

                                Draw.DrawTextureDarkest(
                                    tile.Texture,
                                    newSrc2,
                                    dest2
                                );
                            }
                            EndTextureMode();
                        }
                    }
                }

                out_of_repeat:
                {}
            }
            break;
        
            case TileType.VoxelStructRockType:
            {
                var sublayer = layer * 10;

                Rectangle dest = new(
                    (startX - bf) * 20,
                    (startY - bf) * 20,

                    (width  + 2 * bf) * 20,
                    (height + 2 * bf) * 20
                );

                Rectangle src = new(
                    0,
                    1,
                    (width  + 2 * bf) * 20,
                    (height + 2 * bf) * 20
                );

                var rnd = Random.Generate(tile.Rnd);

                for (var d = sublayer; d < Utils.Restrict(sublayer + 9 + (10 * Utils.BoolInt(tile.HasSpecsLayer(1))), 0, 29); d++)
                {
                    if (d is 12 or 8 or 4)
                    {
                        rnd = Random.Generate(tile.Rnd);
                    }

                    BeginTextureMode(_layers[d]);
                    {
                        var shader = Shaders.WhiteRemover;
                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), tile.Texture);
                        DrawTexturePro(
                            tile.Texture,
                            src with { X = src.X + src.Width * (rnd - 1) },
                            dest,
                            Vector2.Zero,
                            0,
                            Color.White
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    if (colored && !effectColorA && !effectColorB)
                    {
                        BeginTextureMode(_layersDC[d]);
                        var shader = Shaders.WhiteRemover;
                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), tile.Texture);
                        DrawTexturePro(
                            tile.Texture, 
                            src with { X = src.X + src.Width * (rnd - 1) + (width + 2 * bf) * 20 * tile.Rnd }, 
                            dest,
                            Vector2.Zero,
                            0,
                            Color.White
                        );
                        EndShaderMode();
                        EndTextureMode();
                    }

                    if (effectColorA)
                    {
                        BeginTextureMode(_gradientA[d]);
                        Draw.DrawTextureDarkest(
                            tile.Texture, 
                            src with { X = src.X + src.Width * (rnd - 1) + (width + 2 * bf) * 20 * tile.Rnd }, 
                            dest
                        );
                        EndShaderMode();
                    }

                    if (effectColorB)
                    {
                        BeginTextureMode(_gradientB[d]);
                        Draw.DrawTextureDarkest(
                            tile.Texture, 
                            src with { X = src.X + src.Width * (rnd - 1) + (width + 2 * bf) * 20 * tile.Rnd }, 
                            dest
                        );
                        EndShaderMode();
                    }
                }
            }
            break;
        
            case TileType.VoxelStructSandType:
            {
                var sublayer = layer * 10 + 1;

                Rectangle dest = new(
                    (startX - bf) * 20,
                    (startY - bf) * 20,

                    (width  + 2 * bf) * 20,
                    (height + 2 * bf) * 20
                );

                Rectangle src = new(
                    0,
                    1,
                    (width  + 2 * bf) * 20,
                    (height + 2 * bf) * 20
                );

                for (var d = sublayer; d < Utils.Restrict(sublayer + 9 + (10 * Utils.BoolInt(tile.HasSpecsLayer(1))), 0, 29); d++)
                {
                    var rnd = Random.Generate(tile.Rnd);

                    var newSrc = src with {
                        X = src.X + src.Width * (rnd - 1)
                    };

                    BeginTextureMode(_layers[d]);
                    {
                        var shader = Shaders.WhiteRemover;
                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), tile.Texture);
                        DrawTexturePro(
                            tile.Texture,
                            newSrc,
                            dest,
                            Vector2.Zero,
                            0,
                            Color.White
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    newSrc.X += (width + 2 * bf) * 20 * tile.Rnd;

                    if (colored && !effectColorA && !effectColorB)
                    {
                        BeginTextureMode(_layersDC[d]);
                        var shader = Shaders.WhiteRemover;
                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), tile.Texture);
                        DrawTexturePro(
                            tile.Texture,
                            newSrc,
                            dest,
                            Vector2.Zero,
                            0,
                            Color.White
                        );
                        EndShaderMode();
                        EndTextureMode();
                    }

                    if (effectColorA)
                    {
                        BeginTextureMode(_gradientA[d]);
                        Draw.DrawTextureDarkest(
                            tile.Texture,
                            newSrc,
                            dest
                        );
                        EndTextureMode();
                    }

                    if (effectColorB)
                    {
                        BeginTextureMode(_gradientB[d]);
                        Draw.DrawTextureDarkest(
                            tile.Texture,
                            newSrc,
                            dest
                        );
                        EndTextureMode();
                    }
                }
            }
            break;
        }

        // Special behavior tags

        foreach (var tag in tile.Tags)
        {
            switch (tag)
            {
                case "Chain Holder":
                {
                    var (cx, cy) = cell.SecondChainHolderPosition;

                    if (cx is -1 || cy is -1) continue;

                    Vector2 p1 = new Vector2(x, y) * 20 / 2 + new Vector2(10.1f, 10.1f);
                    Vector2 p2 = (new Vector2(cx, cy) - camera.Coords) * 20 / 2 + new Vector2(10.1f, 10.1f);

                    var sublayer = layer * 10 + 2;

                    var steps = (int)(Utils.Diag(p1, p2) / 12.0f + 0.4999f);
                    var dr = Utils.MoveToPoint(p1, p2, 1.0f);
                    var ornt = Random.Generate(2) - 1;
                    var degDir = Utils.LookAtPoint(p1, p2);
                    var stp = Random.Generate(100) * 0.01f;

                    for (var q = 1; q <= steps; q++)
                    {
                        var pos = p1 + (dr * 12 * (q - stp));

                        Rectangle dest, src;

                        if (ornt != 0)
                        {
                            dest = new(
                                pos.X - 6,
                                pos.Y - 10,
                                12,
                                20
                            );

                            src = new(
                                0,
                                0,
                                12,
                                20
                            );

                            ornt = 0;
                        }
                        else
                        {
                            dest = new(
                                pos.X - 2,
                                pos.Y - 10,
                                4,
                                10
                            );

                            src = new(
                                13,
                                0,
                                29,
                                20
                            );

                            ornt = 1;
                        }

                        BeginTextureMode(_layers[sublayer]);
                        {
                            var shader = Shaders.WhiteRemoverApplyColor;
                            BeginShaderMode(shader);
                            SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.bigChainHolder);
                            Draw.DrawQuad(
                                State.bigChainHolder,
                                src,
                                Utils.RotateRect(dest, degDir),
                                Color.Red
                            );
                            EndShaderMode();
                        }
                        EndTextureMode();
                    }
                }
                break;
                case "fanBlade":
                {
                    var sublayer = (layer + 1) * 10;

                    if (sublayer > 20) sublayer -= 5;

                    var middle = new Vector2(x * 20 - 10, y * 20 - 10);

                    Quad q = new(
                        middle,                             // Top left
                        new(middle.X + 46, middle.Y),       // Top right
                        new(middle.X + 46, middle.Y + 46),  // Bottom right
                        new(middle.X, middle.Y + 46)        // bottom left
                    );

                    BeginTextureMode(_layers[sublayer - 2]);
                    {
                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.fanBlade);

                        Draw.DrawQuad(
                            State.fanBlade,
                            q.Rotated(Random.Generate(360)),
                            Color.Green
                        );

                        EndShaderMode();
                    }
                    EndTextureMode();

                    BeginTextureMode(_layers[sublayer]);
                    {
                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.fanBlade);

                        Draw.DrawQuad(
                            State.fanBlade,
                            q.Rotated(Random.Generate(360)),
                            Color.Green
                        );

                        EndShaderMode();
                    }
                    EndTextureMode();
                }
                break;
                case "Big Wheel":
                {
                    int[] dpsL = layer switch {
                        0 => [  0,  7 ],
                        1 => [  9, 17 ],
                        _ => [ 19, 27 ]
                    };

                    Vector2 offset = new Vector2(x, y) * 20 + Vector2.One * 10; // Needs tweaking

                    Quad q = new(
                        new(-90 + offset.X      , -90 + offset.Y      ),    // Top left
                        new(-90 + offset.X + 180, -90 + offset.Y      ),    // Top right
                        new(-90 + offset.X + 180, -90 + offset.Y + 180),    // Bottom right
                        new(-90 + offset.X      , -90 + offset.Y + 180)     // Bottom left
                    );

                    foreach (var l in dpsL)
                    {
                        var rnd = Random.Generate(360);

                        foreach (var dp in new int[3] { l, l + 1, l + 2 })
                        {
                            BeginTextureMode(_layers[dp]);
                            
                            var shader = Shaders.WhiteRemoverApplyColor;
                            BeginShaderMode(shader);
                            SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.BigWheelGraf);
                            Draw.DrawQuad(
                                State.BigWheelGraf,
                                q.Rotated(rnd + 0.001f),
                                Color.Green
                            );
                            EndShaderMode();

                            EndTextureMode();
                        }
                    }
                }
                break;
                case "Sawblades":
                {
                    int[] dpsL = layer switch {
                        0 => [  0,  7 ],
                        1 => [  9, 17 ],
                        _ => [ 19, 27 ]
                    };

                    Vector2 offset = Utils.GetMiddleCellPos(x, y) + new Vector2(10, 10);

                    Rectangle rect = new(
                        -90 + offset.X,
                        -90 + offset.Y,
                        180,
                        180
                    );

                    foreach (var l in dpsL)
                    {
                        var rnd = Random.Generate(360);

                        foreach (var dp in new int[3] { l, l + 1, l + 2 })
                        {
                            BeginTextureMode(_layers[dp]);
                            
                            var shader = Shaders.WhiteRemoverApplyColor;
                            BeginShaderMode(shader);
                            SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.sawbladeGraf);
                            Draw.DrawQuad(
                                State.sawbladeGraf,
                                Utils.RotateRect(rect, rnd + 0.001f),
                                Color.Green
                            );
                            EndShaderMode();

                            EndTextureMode();
                        }
                    }
                }
                break;
            
                case "randomCords":
                {
                    var sublayer = layer * 10 + Random.Generate(9);
                
                    var pnt = Utils.GetMiddleCellPos(new Vector2(x, y + tile.Size.Height/2f));

                    Rectangle rect = new(
                        -50 + pnt.X,
                        -50 + pnt.Y,
                        100,
                        100
                    );

                    var rnd = Random.Generate(7);

                    BeginTextureMode(_layers[sublayer]);
                    {
                        var shader = Shaders.WhiteRemover;

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.randomCords);
                        Draw.DrawQuad(
                            State.randomCords,
                            new Rectangle((rnd - 1)*100 + 1, 1, 100, 100),
                            Utils.RotateRect(rect, -30+Random.Generate(60))
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();
                }
                break;
                case "Big Sign":
                case "Big SignB":
                {
                    var sublayer = layer * 10;

                    var texture = LoadRenderTexture(60, 60);

                    var rnd = Random.Generate(20);

                    Rectangle dest = new(3, 3, 26, 30);

                    BeginTextureMode(texture);
                    {
                        ClearBackground(Color.White);

                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.bigSigns1);
                        DrawTexturePro(
                            State.bigSigns1,
                            new((rnd - 1)*26, 0, 26, 30),
                            dest,
                            Vector2.Zero,
                            0,
                            Color.Black
                        );
                        EndShaderMode();

                        rnd = Random.Generate(20);
                        dest = new(31, 3, 57, 30);

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.bigSigns1);
                        DrawTexturePro(
                            State.bigSigns1,
                            new((rnd - 1)*26, 0, 26, 30),
                            dest,
                            Vector2.Zero,
                            0,
                            Color.Black
                        );
                        EndShaderMode();

                        rnd = Random.Generate(14);
                        dest = new(3, 35, 55, 24);

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.bigSigns2);
                        DrawTexturePro(
                            State.bigSigns2,
                            new((rnd - 1)*55, 0, 55, 24),
                            dest,
                            Vector2.Zero,
                            0,
                            Color.Black
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    BeginTextureMode(rt);
                    {
                        var middle = Utils.GetMiddleCellPos(x, y);

                        var shader = Shaders.WhiteRemoverApplyColor;

                        foreach (
                            var (pos, color) 
                            in 
                            new ReadOnlySpan<(Vector2 pos, Color color)>([ 
                                (new(-4, -4), Color.Blue ), 
                                (new(-3, -3), Color.Blue ), 
                                (new( 3,  3), Color.Red  ), 
                                (new( 4,  4), Color.Red  ), 
                                (new(-2, -2), Color.Green), 
                                (new(-1, -1), Color.Green), 
                                (new( 0,  0), Color.Green), 
                                (new( 1,  1), Color.Green), 
                                (new( 2,  2), Color.Green), 
                                (new( 2,  2), Color.Green), 
                            ])
                        )
                        {

                            BeginShaderMode(shader);
                            SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                            DrawTexturePro(
                                texture.Texture,
                                new(0, 0, 60, 60),
                                new(
                                    -30 + middle.X + pos.X,
                                    -30 + middle.Y + pos.Y,
                                    60,
                                    60
                                ),
                                Vector2.Zero,
                                0,
                                color
                            );
                            EndShaderMode();
                        }
                    
                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                        DrawTexturePro(
                            texture.Texture,
                            new(0, 0, 60, 60),
                            new(
                                -30 + middle.X,
                                -30 + middle.Y,
                                60,
                                60
                            ),
                            Vector2.Zero,
                            0,
                            new Color(255, 0, 255, 255)
                        );
                        EndShaderMode();                     
                    }
                    EndTextureMode();


                    Draw.DrawToEffectColor(
                        State.bigSignGradient, 
                        new(0, 0, 60, 60), 
                        new(-30, -30, 60, 60), 
                        tag == "Big Sign" ? _gradientA : _gradientB, 
                        sublayer, 
                        1, 
                        1
                    );

                    UnloadRenderTexture(texture);
                }
                break;
                
                // Highly repetative code incoming.

                case "Big Western Sign":
                case "Big Western Sign Titled":
                {
                    var texture = LoadRenderTexture(36, 48);
                    var rnd = Random.Generate(20);
                    var middle = Utils.GetMiddleCellPos(x, y);
                    middle.X += 10;

                    BeginTextureMode(texture);
                    {
                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        DrawTexturePro(
                            State.bigWesternSigns,
                            new((rnd - 1)*36, 0, 36, 48),
                            new(0, 0, texture.Texture.Width, texture.Texture.Height),
                            Vector2.Zero,
                            0,
                            Color.Black
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    ReadOnlySpan<(Vector2, Color)> list = [
                        (new(-4, -4), Color.Blue),
                        (new(-3, -3), Color.Blue),
                        (new( 3,  3), Color.Red),
                        (new( 4,  4), Color.Red),
                        (new(-2, -2), Color.Green),
                        (new(-1, -1), Color.Green),
                        (new( 0,  0), Color.Green),
                        (new( 1,  1), Color.Green),
                        (new( 2,  2), Color.Green),
                        (new( 0,  0), new(255, 0, 255, 255)),
                    ];

                    BeginTextureMode(rt);
                    {
                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                        if (tag == "Big Western Sign Titled")
                        {
                            var tilt = -45.1f + Random.Generate(90);


                            foreach (var (point, color) in list)
                            {
                                Draw.DrawQuad(
                                    texture.Texture,
                                    Utils.RotateRect(new Rectangle(
                                            middle.X - 18 + point.X,
                                            middle.Y - 24 + point.Y,
                                            36,
                                            48
                                        ),
                                        tilt
                                    ),
                                    color
                                );
                            }
                        }
                        else
                        {
                            foreach (var (point, color) in list)
                            {
                                DrawTexturePro(
                                    texture.Texture,
                                    new Rectangle(0, 0, 36, 48),
                                    new Rectangle(
                                        middle.X - 18 + point.X,
                                        middle.Y - 24 + point.Y,
                                        36,
                                        48
                                    ),
                                    Vector2.Zero,
                                    0,
                                    color
                                );
                            }
                        }

                        EndShaderMode();
                    }
                    EndTextureMode();

                    var sublayer = layer * 10;

                    Draw.DrawToEffectColor(
                        State.bigSignGradient,
                        new Rectangle(0, 0, 60, 60),
                        new Rectangle(
                            middle.X - 25,
                            middle.Y - 30,
                            50,
                            60
                        ),
                        _gradientA,
                        sublayer,
                        1,
                        1
                    );

                    UnloadRenderTexture(texture);
                }
                break;
            
                case "Big Western Sign B":
                case "Big Western Sign Titled B":
                {
                    var texture = LoadRenderTexture(36, 48);
                    var rnd = Random.Generate(20);
                    var middle = Utils.GetMiddleCellPos(x, y);
                    middle.X += 10;

                    BeginTextureMode(texture);
                    {
                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        DrawTexturePro(
                            State.bigWesternSigns,
                            new((rnd - 1)*36, 0, 36, 48),
                            new(0, 0, texture.Texture.Width, texture.Texture.Height),
                            Vector2.Zero,
                            0,
                            Color.Black
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    ReadOnlySpan<(Vector2, Color)> list = [
                        (new(-4, -4), Color.Blue),
                        (new(-3, -3), Color.Blue),
                        (new( 3,  3), Color.Red),
                        (new( 4,  4), Color.Red),
                        (new(-2, -2), Color.Green),
                        (new(-1, -1), Color.Green),
                        (new( 0,  0), Color.Green),
                        (new( 1,  1), Color.Green),
                        (new( 2,  2), Color.Green),
                        (new( 0,  0), new(255, 0, 255, 255)),
                    ];

                    BeginTextureMode(rt);
                    {
                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                        if (tag == "Big Western Sign Titled B")
                        {
                            var tilt = -45.1f + Random.Generate(90);


                            foreach (var (point, color) in list)
                            {
                                Draw.DrawQuad(
                                    texture.Texture,
                                    Utils.RotateRect(new Rectangle(
                                            middle.X - 18 + point.X,
                                            middle.Y - 24 + point.Y,
                                            36,
                                            48
                                        ),
                                        tilt
                                    ),
                                    color
                                );
                            }
                        }
                        else
                        {
                            foreach (var (point, color) in list)
                            {
                                DrawTexturePro(
                                    texture.Texture,
                                    new Rectangle(0, 0, 36, 48),
                                    new Rectangle(
                                        middle.X - 18 + point.X,
                                        middle.Y - 24 + point.Y,
                                        36,
                                        48
                                    ),
                                    Vector2.Zero,
                                    0,
                                    color
                                );
                            }
                        }

                        EndShaderMode();
                    }
                    EndTextureMode();

                    var sublayer = layer * 10;

                    Draw.DrawToEffectColor(
                        State.bigSignGradient,
                        new Rectangle(0, 0, 60, 60),
                        new Rectangle(
                            middle.X - 25,
                            middle.Y - 30,
                            50,
                            60
                        ),
                        _gradientB,
                        sublayer,
                        1,
                        1
                    );

                    UnloadRenderTexture(texture);
                }
                break;
            
                case "Small Asian Sign": 
                case "small asian sign on wall":
                {
                    var texture = LoadRenderTexture(20, 20);
                    var rnd = Random.Generate(14);

                    Rectangle dest = new(
                        0,
                        1,
                        20,
                        17
                    );

                    BeginTextureMode(texture);
                    {
                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.smallAsianSigns);
                        DrawTexturePro(
                            State.smallAsianSigns,
                            dest,
                            new Rectangle(
                                (rnd - 1) * 20,
                                0,
                                20,
                                17
                            ),
                            Vector2.Zero,
                            0,
                            Color.Black
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    ReadOnlySpan<(Vector2, Color)> list = [
                        (new(-4, -4), Color.Blue),
                        (new(-3, -3), Color.Blue),
                        (new( 3,  3), Color.Red),
                        (new( 4,  4), Color.Red),
                        (new(-2, -2), Color.Green),
                        (new(-1, -1), Color.Green),
                        (new( 0,  0), Color.Green),
                        (new( 1,  1), Color.Green),
                        (new( 2,  2), Color.Green),
                        (new( 0,  0), new Color(255, 0, 255, 255)),
                    ];

                    if (tag == "Small Asian Sign")
                    {
                        var middle = Utils.GetMiddleCellPos(x, y);

                        BeginTextureMode(rt);
                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);

                        foreach (var (point, color) in list)
                        {
                            DrawTexturePro(
                                texture.Texture,
                                new Rectangle(0, 0, 20, 20),
                                new Rectangle(
                                    -10 + middle.X + point.X,
                                    -10 + middle.Y + point.Y,
                                    20,
                                    20
                                ),
                                Vector2.Zero,
                                0,
                                color
                            );
                        }

                        EndShaderMode();

                        EndTextureMode();

                        Draw.DrawToEffectColor(
                            State.bigSignGradient,
                            new Rectangle(0, 0, 60, 60),
                            new Rectangle(-13, -13, 26, 26),
                            _gradientA,
                            layer * 10,
                            1
                        );
                    }
                    else
                    {
                        var sublayer = layer * 10 + 8;
                        var middle = Utils.GetMiddleCellPos(x, y);

                        var shader = Shaders.WhiteRemoverApplyColor;

                        foreach (var (point, color) in list)
                        {
                            BeginTextureMode(_layers[sublayer]);
                            {
                                BeginShaderMode(shader);
                                SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                                DrawTexturePro(
                                    texture.Texture,
                                    new Rectangle(0, 0, 20, 20),
                                    new Rectangle(
                                        -10 + middle.X + point.X,
                                        -10 + middle.Y + point.Y,
                                        20,
                                        20
                                    ),
                                    Vector2.Zero,
                                    0,
                                    color
                                );
                                EndShaderMode();
                            }
                            EndTextureMode();

                            BeginTextureMode(_layers[sublayer + 1]);
                            {
                                BeginShaderMode(shader);
                                SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                                DrawTexturePro(
                                    texture.Texture,
                                    new Rectangle(0, 0, 20, 20),
                                    new Rectangle(
                                        -10 + middle.X + point.X,
                                        -10 + middle.Y + point.Y,
                                        20,
                                        20
                                    ),
                                    Vector2.Zero,
                                    0,
                                    color
                                );
                                EndShaderMode();
                            }
                            EndTextureMode();
                        }
                    
                        Draw.DrawToEffectColor(
                            State.bigSignGradient,
                            new Rectangle(0, 0, 60, 60),
                            new Rectangle(middle.X -13, middle.Y - 13, 26, 26),
                            _gradientA,
                            sublayer,
                            1,
                            1
                        );
                    }

                    UnloadRenderTexture(texture);
                }
                break;

                case "Small Asian Sign B": 
                case "small asian sign on wall B":
                {
                    var texture = LoadRenderTexture(20, 20);
                    var rnd = Random.Generate(14);

                    Rectangle dest = new(
                        0,
                        1,
                        20,
                        17
                    );

                    BeginTextureMode(texture);
                    {
                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.smallAsianSigns);
                        DrawTexturePro(
                            State.smallAsianSigns,
                            dest,
                            new Rectangle(
                                (rnd - 1) * 20,
                                0,
                                20,
                                17
                            ),
                            Vector2.Zero,
                            0,
                            Color.Black
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    ReadOnlySpan<(Vector2, Color)> list = [
                        (new(-4, -4), Color.Blue),
                        (new(-3, -3), Color.Blue),
                        (new( 3,  3), Color.Red),
                        (new( 4,  4), Color.Red),
                        (new(-2, -2), Color.Green),
                        (new(-1, -1), Color.Green),
                        (new( 0,  0), Color.Green),
                        (new( 1,  1), Color.Green),
                        (new( 2,  2), Color.Green),
                        (new( 0,  0), new Color(255, 0, 255, 255)),
                    ];

                    if (tag == "Small Asian Sign B")
                    {
                        var middle = Utils.GetMiddleCellPos(x, y);

                        BeginTextureMode(rt);
                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);

                        foreach (var (point, color) in list)
                        {
                            DrawTexturePro(
                                texture.Texture,
                                new Rectangle(0, 0, 20, 20),
                                new Rectangle(
                                    -10 + middle.X + point.X,
                                    -10 + middle.Y + point.Y,
                                    20,
                                    20
                                ),
                                Vector2.Zero,
                                0,
                                color
                            );
                        }

                        EndShaderMode();

                        EndTextureMode();

                        Draw.DrawToEffectColor(
                            State.bigSignGradient,
                            new Rectangle(0, 0, 60, 60),
                            new Rectangle(-13, -13, 26, 26),
                            _gradientB,
                            layer * 10,
                            1
                        );
                    }
                    else
                    {
                        var sublayer = layer * 10 + 8;
                        var middle = Utils.GetMiddleCellPos(x, y);

                        var shader = Shaders.WhiteRemoverApplyColor;

                        foreach (var (point, color) in list)
                        {
                            BeginTextureMode(_layers[sublayer]);
                            {
                                BeginShaderMode(shader);
                                SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                                DrawTexturePro(
                                    texture.Texture,
                                    new Rectangle(0, 0, 20, 20),
                                    new Rectangle(
                                        -10 + middle.X + point.X,
                                        -10 + middle.Y + point.Y,
                                        20,
                                        20
                                    ),
                                    Vector2.Zero,
                                    0,
                                    color
                                );
                                EndShaderMode();
                            }
                            EndTextureMode();

                            BeginTextureMode(_layers[sublayer + 1]);
                            {
                                BeginShaderMode(shader);
                                SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                                DrawTexturePro(
                                    texture.Texture,
                                    new Rectangle(0, 0, 20, 20),
                                    new Rectangle(
                                        -10 + middle.X + point.X,
                                        -10 + middle.Y + point.Y,
                                        20,
                                        20
                                    ),
                                    Vector2.Zero,
                                    0,
                                    color
                                );
                                EndShaderMode();
                            }
                            EndTextureMode();
                        }
                    
                        Draw.DrawToEffectColor(
                            State.bigSignGradient,
                            new Rectangle(0, 0, 60, 60),
                            new Rectangle(middle.X -13, middle.Y - 13, 26, 26),
                            _gradientB,
                            sublayer,
                            1,
                            1
                        );
                    }

                    UnloadRenderTexture(texture);
                }
                break;

                case "Small Asian Sign Station": 
                case "Small Asian Sign On Wall Station":
                {
                    var texture = LoadRenderTexture(20, 20);
                    var rnd = Random.Generate(14);

                    Rectangle dest = new(
                        0,
                        1,
                        20,
                        17
                    );

                    BeginTextureMode(texture);
                    {
                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.smallAsianSignsStation);
                        DrawTexturePro(
                            State.smallAsianSignsStation,
                            dest,
                            new Rectangle(
                                (rnd - 1) * 20,
                                0,
                                20,
                                17
                            ),
                            Vector2.Zero,
                            0,
                            Color.Black
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    ReadOnlySpan<(Vector2, Color)> list = [
                        (new(-4, -4), Color.Blue),
                        (new(-3, -3), Color.Blue),
                        (new( 3,  3), Color.Red),
                        (new( 4,  4), Color.Red),
                        (new(-2, -2), Color.Green),
                        (new(-1, -1), Color.Green),
                        (new( 0,  0), Color.Green),
                        (new( 1,  1), Color.Green),
                        (new( 2,  2), Color.Green),
                        (new( 0,  0), new Color(255, 0, 255, 255)),
                    ];

                    if (tag == "Small Asian Sign Station")
                    {
                        var middle = Utils.GetMiddleCellPos(x, y);

                        BeginTextureMode(rt);
                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);

                        foreach (var (point, color) in list)
                        {
                            DrawTexturePro(
                                texture.Texture,
                                new Rectangle(0, 0, 20, 20),
                                new Rectangle(
                                    -10 + middle.X + point.X,
                                    -10 + middle.Y + point.Y,
                                    20,
                                    20
                                ),
                                Vector2.Zero,
                                0,
                                color
                            );
                        }

                        EndShaderMode();

                        EndTextureMode();

                        Draw.DrawToEffectColor(
                            State.bigSignGradient,
                            new Rectangle(0, 0, 60, 60),
                            new Rectangle(-13, -13, 26, 26),
                            _gradientA,
                            layer * 10,
                            1
                        );
                    }
                    else
                    {
                        var sublayer = layer * 10 + 8;
                        var middle = Utils.GetMiddleCellPos(x, y);

                        var shader = Shaders.WhiteRemoverApplyColor;

                        foreach (var (point, color) in list)
                        {
                            BeginTextureMode(_layers[sublayer]);
                            {
                                BeginShaderMode(shader);
                                SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                                DrawTexturePro(
                                    texture.Texture,
                                    new Rectangle(0, 0, 20, 20),
                                    new Rectangle(
                                        -10 + middle.X + point.X,
                                        -10 + middle.Y + point.Y,
                                        20,
                                        20
                                    ),
                                    Vector2.Zero,
                                    0,
                                    color
                                );
                                EndShaderMode();
                            }
                            EndTextureMode();

                            BeginTextureMode(_layers[sublayer + 1]);
                            {
                                BeginShaderMode(shader);
                                SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                                DrawTexturePro(
                                    texture.Texture,
                                    new Rectangle(0, 0, 20, 20),
                                    new Rectangle(
                                        -10 + middle.X + point.X,
                                        -10 + middle.Y + point.Y,
                                        20,
                                        20
                                    ),
                                    Vector2.Zero,
                                    0,
                                    color
                                );
                                EndShaderMode();
                            }
                            EndTextureMode();
                        }
                    
                        Draw.DrawToEffectColor(
                            State.bigSignGradient,
                            new Rectangle(0, 0, 60, 60),
                            new Rectangle(middle.X -13, middle.Y - 13, 26, 26),
                            _gradientA,
                            sublayer,
                            1,
                            1
                        );
                    }

                    UnloadRenderTexture(texture);
                }
                break;
            
                case "Small Asian Sign Station B": 
                case "Small Asian Sign On Wall Station B":
                {
                    var texture = LoadRenderTexture(20, 20);
                    var rnd = Random.Generate(14);

                    Rectangle dest = new(
                        0,
                        1,
                        20,
                        17
                    );

                    BeginTextureMode(texture);
                    {
                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.smallAsianSignsStation);
                        DrawTexturePro(
                            State.smallAsianSignsStation,
                            dest,
                            new Rectangle(
                                (rnd - 1) * 20,
                                0,
                                20,
                                17
                            ),
                            Vector2.Zero,
                            0,
                            Color.Black
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    ReadOnlySpan<(Vector2, Color)> list = [
                        (new(-4, -4), Color.Blue),
                        (new(-3, -3), Color.Blue),
                        (new( 3,  3), Color.Red),
                        (new( 4,  4), Color.Red),
                        (new(-2, -2), Color.Green),
                        (new(-1, -1), Color.Green),
                        (new( 0,  0), Color.Green),
                        (new( 1,  1), Color.Green),
                        (new( 2,  2), Color.Green),
                        (new( 0,  0), new Color(255, 0, 255, 255)),
                    ];

                    if (tag == "Small Asian Sign Station")
                    {
                        var middle = Utils.GetMiddleCellPos(x, y);

                        BeginTextureMode(rt);
                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);

                        foreach (var (point, color) in list)
                        {
                            DrawTexturePro(
                                texture.Texture,
                                new Rectangle(0, 0, 20, 20),
                                new Rectangle(
                                    -10 + middle.X + point.X,
                                    -10 + middle.Y + point.Y,
                                    20,
                                    20
                                ),
                                Vector2.Zero,
                                0,
                                color
                            );
                        }

                        EndShaderMode();

                        EndTextureMode();

                        Draw.DrawToEffectColor(
                            State.bigSignGradient,
                            new Rectangle(0, 0, 60, 60),
                            new Rectangle(-13, -13, 26, 26),
                            _gradientB,
                            layer * 10,
                            1
                        );
                    }
                    else
                    {
                        var sublayer = layer * 10 + 8;
                        var middle = Utils.GetMiddleCellPos(x, y);

                        var shader = Shaders.WhiteRemoverApplyColor;

                        foreach (var (point, color) in list)
                        {
                            BeginTextureMode(_layers[sublayer]);
                            {
                                BeginShaderMode(shader);
                                SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                                DrawTexturePro(
                                    texture.Texture,
                                    new Rectangle(0, 0, 20, 20),
                                    new Rectangle(
                                        -10 + middle.X + point.X,
                                        -10 + middle.Y + point.Y,
                                        20,
                                        20
                                    ),
                                    Vector2.Zero,
                                    0,
                                    color
                                );
                                EndShaderMode();
                            }
                            EndTextureMode();

                            BeginTextureMode(_layers[sublayer + 1]);
                            {
                                BeginShaderMode(shader);
                                SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                                DrawTexturePro(
                                    texture.Texture,
                                    new Rectangle(0, 0, 20, 20),
                                    new Rectangle(
                                        -10 + middle.X + point.X,
                                        -10 + middle.Y + point.Y,
                                        20,
                                        20
                                    ),
                                    Vector2.Zero,
                                    0,
                                    color
                                );
                                EndShaderMode();
                            }
                            EndTextureMode();
                        }
                    
                        Draw.DrawToEffectColor(
                            State.bigSignGradient,
                            new Rectangle(0, 0, 60, 60),
                            new Rectangle(middle.X -13, middle.Y - 13, 26, 26),
                            _gradientB,
                            sublayer,
                            1,
                            1
                        );
                    }

                    UnloadRenderTexture(texture);
                }
                break;
            
                // deprecated?
                case "glass":
                if (layer == 0) {
                    var middle = Utils.GetMiddleCellPos(x, y);

                    Rectangle dest = new(
                        width * -10 + middle.X,
                        height * -10 + middle.Y,
                        20,
                        20
                    );

                    // Unknown behaviour..
                    // (An image gets modified and then never used again)
                }
                break;
            
                case "harvester":
                {
                    var middle = Utils.GetMiddleCellPos(x, y);

                    var big = tile.Name == "Harvester B";

                    char letter;
                    Vector2 eye, arm, lowerPart = Vector2.Zero;

                    if (big)
                    {
                        letter = 'B';
                        middle.X += 10;
                        eye = new(75, -126);
                        arm = new(105, 108);
                    }
                    else
                    {
                        letter = 'A';
                        middle.X += 10;
                        eye = new(37, -85);
                        arm = new(58, 60);
                    }

                    var absX = x + (int)(camera.Coords.X / 20);
                    var absY = y + (int)(camera.Coords.Y / 20);
                                        
                    for (var h = absY; h < Level!.Height; h++)
                    {
                        if ((letter == 'A' && Level!.TileMatrix[h, absX, layer] is { Type: TileCellType.Head, TileDefinition.Name: "Harvester Arm A" }) ||
                            (letter == 'B' && Level!.TileMatrix[h, absX, layer] is { Type: TileCellType.Head, TileDefinition.Name: "Harvester Arm B" }))
                        {
                            if (Level!.TileMatrix[h, absX, layer] is { Type: TileCellType.Head, TileDefinition.Name: "Harvester Arm A" })
                            {
                                lowerPart = new Vector2(x, h - camera.Coords.Y/20f);
                            }
                        }
                    }

                    var lowerMiddle = Vector2.Zero;
                    if (lowerPart != Vector2.Zero)
                    {
                        lowerMiddle = Utils.GetMiddleCellPos(lowerPart);
                        if (big) lowerMiddle.X += 10;
                    }

                    for (var side = 1; side <= 2; side++)
                    {
                        var dr = side == 1 ? -1 : 1;

                        var eyePastePos = middle + new Vector2(eye.X * dr, eye.Y);
                        
                        var eyeMember = letter == 'A' ? State.HarvesterAEye : State.HarvesterBEye;
                        
                        var quad = Utils.RotateRect(new Rectangle(
                                eyePastePos.X - eyeMember.Width / 2,
                                eyePastePos.Y = eyeMember.Height / 2,
                                eyeMember.Width,
                                eyeMember.Height
                            ),
                            Random.Generate(360)
                        );


                        var shader = Shaders.WhiteRemoverApplyColor;

                        for (var depth = layer * 10 +3; depth <= layer * 10 + 6; depth++)
                        {
                            BeginTextureMode(_layers[depth]);
                            BeginShaderMode(shader);
                            SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), eyeMember);
                            Draw.DrawQuad(
                                eyeMember,
                                quad,
                                Color.Green
                            );
                            EndShaderMode();
                            EndTextureMode();
                        }
                    }
                }
                break;
            
                // incomplete
                case "Temple Floor":
                if (State.TempleStoneWedge is not null &&
                    State.TempleStoneSlopeSW is not null &&
                    State.TempleStoneSlopeSE is not null) {
                    var absX = x + (int)(camera.Coords.X / 20);
                    var absY = y + (int)(camera.Coords.Y / 20);
                
                    var nextIsFloor = false;
                    
                    if (absY + 8 < Level!.Height && 
                        Level!.TileMatrix[absY + 8, absX, layer] is { Type: TileCellType.Head, TileDefinition.Name: "Temple Floor" })
                    {
                        nextIsFloor = true;
                    }

                    var prevIsFloor = false;


                    if (absY - 8 >= 0 && 
                        Level!.TileMatrix[absY + 8, absX, layer] is { Type: TileCellType.Head, TileDefinition.Name: "Temple Floor" })
                    {
                        prevIsFloor = true;
                    }

                    BeginTextureMode(rt);
                    if (prevIsFloor)
                    {
                        DrawTile_MTX(new(), State.TempleStoneWedge, x + (int)camera.Coords.X - 4, y + (int)camera.Coords.Y - 1, layer, camera, rt);
                    }
                    else
                    {
                        DrawTile_MTX(new(), State.TempleStoneSlopeSE, x + (int)camera.Coords.X - 3, y + (int)camera.Coords.Y - 1, layer, camera, rt);
                    }

                    if (!nextIsFloor)
                    {
                        DrawTile_MTX(new(), State.TempleStoneSlopeSW, x + (int)camera.Coords.X + 4, y + (int)camera.Coords.Y - 1, layer, camera, rt);
                    }
                    EndTextureMode();
                }
                break;
            
                case "Larger Sign":
                case "Larger Sign B":
                {
                    var texture = LoadRenderTexture(86, 106);
                    var rnd = Random.Generate(14);
                    var dest = new Rectangle(3, 3, 80, 100);
                    var shader = Shaders.WhiteRemoverApplyColor;

                    BeginTextureMode(texture);
                    {
                        ClearBackground(Color.White);

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.largerSigns);

                        DrawTexturePro(
                            State.largerSigns,
                            new Rectangle(
                                (rnd - 1) * 80,
                                0,
                                80,
                                100
                            ),
                            dest,
                            Vector2.Zero,
                            0,
                            Color.Black
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    var sublayer = layer * 10;

                    var middle = Utils.GetMiddleCellPos(x, y);

                    middle.X += 10;

                    ReadOnlySpan<(Vector2, Color)> list = [
                        (new(-4, -4), Color.Blue),
                        (new(-3, -3), Color.Blue),
                        (new( 3,  3), Color.Red),
                        (new( 4,  4), Color.Red),
                        (new(-2, -2), Color.Green),
                        (new(-1, -1), Color.Green),
                        (new( 0,  0), Color.Green),
                        (new( 1,  1), Color.Green),
                        (new( 2,  2), Color.Green),
                        (new( 2,  2), Color.Green),
                    ];

                    foreach (var (point, color) in list)
                    {
                        BeginTextureMode(_layers[sublayer]);
                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                        DrawTextureV(
                            texture.Texture,
                            middle + point - new Vector2(43, 53),
                            color
                        );
                        EndShaderMode();
                        EndTextureMode();

                        BeginTextureMode(_layers[sublayer + 1]);
                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                        DrawTextureV(
                            texture.Texture,
                            middle + point - new Vector2(43, 53),
                            color
                        );
                        EndShaderMode();
                        EndTextureMode();
                    }

                    BeginTextureMode(_layers[sublayer]);
                    BeginShaderMode(shader);
                    SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                    DrawTextureV(
                        texture.Texture,
                        middle - new Vector2(43, 53),
                        Color.White
                    );
                    EndShaderMode();
                    EndTextureMode();

                    BeginTextureMode(_layers[sublayer + 1]);
                    BeginShaderMode(shader);
                    SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                    DrawTextureV(
                        texture.Texture,
                        middle - new Vector2(43, 53),
                        new Color(255, 0, 255, 255)
                    );
                    EndShaderMode();
                    EndTextureMode();

                    BeginTextureMode(State.largeSignGrad2);
                    {
                        DrawTexture(State.largeSignGrad, 0, 0, Color.White);
                    }
                    EndTextureMode();

                    BeginTextureMode(State.largeSignGrad2);
                    for (var a = 0; a <= 6; a++)
                    {
                        for (var b = 0; b <= 13; b++)
                        {
                            Rectangle rect = new(
                                a * 16 - 6,
                                b * 8 - 1,
                                16,
                                8
                            );

                            if (Random.Generate(7) == 1)
                            {
                                var blend = Random.Generate(Random.Generate(100));

                                DrawRectangleRec(
                                    rect with {
                                        Width = rect.Width + 1,
                                        Height = rect.Height + 1
                                    },
                                    Color.White with { A = (byte)(blend/2) }
                                );

                                DrawRectangleRec(
                                    rect with {
                                        X = rect.X + 1,
                                        Y = rect.Y + 1,
                                        Width = rect.Width - 1,
                                        Height = rect.Height - 1
                                    },
                                    Color.White with { A = (byte)(blend/2) }
                                );
                            }
                            else if (Random.Generate(7) == 1)
                            {
                                DrawRectangleRec(
                                    rect with {
                                        X = rect.X + 1,
                                        Y = rect.Y + 1,
                                        Width = rect.Width - 1,
                                        Height = rect.Height - 1
                                    },
                                    Color.Black with { A = (byte)Random.Generate(Random.Generate(60)) }
                                );
                            }

                            DrawRectangleRec(
                                rect with { Height = 1 },
                                Color.White with { A = 20 }
                            );

                            DrawRectangleRec(
                                rect with {
                                    Y = rect.Y + 1, 
                                    Width = 1 
                                },
                                Color.White with { A = 20 }
                            );

                        }
                    }
                    EndTextureMode();
                
                    Draw.DrawToEffectColor(
                        State.largeSignGrad2.Texture, 
                        new Rectangle(0, 0, 86, 106),
                        new Rectangle(
                            -43 + middle.X,
                            -53 + middle.Y,
                            86,
                            106
                        ),
                        tag == "Larger Sign B" ? _gradientB : _gradientA,
                        sublayer + 1,
                        1,
                        1
                    );

                    UnloadRenderTexture(texture);
                }
                break;
            
                case "Station Larger Sign":
                case "Station Larger Sign B":
                {
                    var texture = LoadRenderTexture(86, 106);
                    var rnd = Random.Generate(14);
                    var dest = new Rectangle(3, 3, 80, 100);
                    var shader = Shaders.WhiteRemoverApplyColor;

                    BeginTextureMode(texture);
                    {
                        ClearBackground(Color.White);

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.largerSignsStation);

                        DrawTexturePro(
                            State.largerSignsStation,
                            new Rectangle(
                                (rnd - 1) * 80,
                                0,
                                80,
                                100
                            ),
                            dest,
                            Vector2.Zero,
                            0,
                            Color.Black
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    var sublayer = layer * 10;

                    var middle = Utils.GetMiddleCellPos(x, y);

                    middle.X += 10;

                    ReadOnlySpan<(Vector2, Color)> list = [
                        (new(-4, -4), Color.Blue),
                        (new(-3, -3), Color.Blue),
                        (new( 3,  3), Color.Red),
                        (new( 4,  4), Color.Red),
                        (new(-2, -2), Color.Green),
                        (new(-1, -1), Color.Green),
                        (new( 0,  0), Color.Green),
                        (new( 1,  1), Color.Green),
                        (new( 2,  2), Color.Green),
                        (new( 2,  2), Color.Green),
                    ];

                    foreach (var (point, color) in list)
                    {
                        BeginTextureMode(_layers[sublayer]);
                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                        DrawTextureV(
                            texture.Texture,
                            middle + point - new Vector2(43, 53),
                            color
                        );
                        EndShaderMode();
                        EndTextureMode();

                        BeginTextureMode(_layers[sublayer + 1]);
                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                        DrawTextureV(
                            texture.Texture,
                            middle + point - new Vector2(43, 53),
                            color
                        );
                        EndShaderMode();
                        EndTextureMode();
                    }

                    BeginTextureMode(_layers[sublayer]);
                    BeginShaderMode(shader);
                    SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                    DrawTextureV(
                        texture.Texture,
                        middle - new Vector2(43, 53),
                        Color.White
                    );
                    EndShaderMode();
                    EndTextureMode();

                    BeginTextureMode(_layers[sublayer + 1]);
                    BeginShaderMode(shader);
                    SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                    DrawTextureV(
                        texture.Texture,
                        middle - new Vector2(43, 53),
                        new Color(255, 0, 255, 255)
                    );
                    EndShaderMode();
                    EndTextureMode();

                    BeginTextureMode(State.largeSignGrad2);
                    {
                        DrawTexture(State.largeSignGrad, 0, 0, Color.White);
                    }
                    EndTextureMode();

                    BeginTextureMode(State.largeSignGrad2);
                    for (var a = 0; a <= 6; a++)
                    {
                        for (var b = 0; b <= 13; b++)
                        {
                            Rectangle rect = new(
                                a * 16 - 6,
                                b * 8 - 1,
                                16,
                                8
                            );

                            if (Random.Generate(7) == 1)
                            {
                                var blend = Random.Generate(Random.Generate(100));

                                DrawRectangleRec(
                                    rect with {
                                        Width = rect.Width + 1,
                                        Height = rect.Height + 1
                                    },
                                    Color.White with { A = (byte)(blend/2) }
                                );

                                DrawRectangleRec(
                                    rect with {
                                        X = rect.X + 1,
                                        Y = rect.Y + 1,
                                        Width = rect.Width - 1,
                                        Height = rect.Height - 1
                                    },
                                    Color.White with { A = (byte)(blend/2) }
                                );
                            }
                            else if (Random.Generate(7) == 1)
                            {
                                DrawRectangleRec(
                                    rect with {
                                        X = rect.X + 1,
                                        Y = rect.Y + 1,
                                        Width = rect.Width - 1,
                                        Height = rect.Height - 1
                                    },
                                    Color.Black with { A = (byte)Random.Generate(Random.Generate(60)) }
                                );
                            }

                            DrawRectangleRec(
                                rect with { Height = 1 },
                                Color.White with { A = 20 }
                            );

                            DrawRectangleRec(
                                rect with {
                                    Y = rect.Y + 1, 
                                    Width = 1 
                                },
                                Color.White with { A = 20 }
                            );

                        }
                    }
                    EndTextureMode();
                
                    Draw.DrawToEffectColor(
                        State.largeSignGrad2.Texture, 
                        new Rectangle(0, 0, 86, 106),
                        new Rectangle(
                            -43 + middle.X,
                            -53 + middle.Y,
                            86,
                            106
                        ),
                        tag == "Station Larger Sign B" ? _gradientB : _gradientA,
                        sublayer + 1,
                        1,
                        1
                    );

                    UnloadRenderTexture(texture);
                }
                break;
            
                case "Station Lamp":
                {
                    var texture = LoadRenderTexture(40, 20);
                    var rnd = Random.Generate(1);
                    var rect = new Rectangle(1, 1, 38, 18);
                    var middle = Utils.GetMiddleCellPos(x, y);

                    middle.X += 11;
                    middle.Y += 1;
                    
                    var shader = Shaders.WhiteRemoverApplyColor;

                    BeginTextureMode(texture);
                    {
                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.StationLamp);
                        DrawTexturePro(
                            State.StationLamp,
                            new Rectangle(
                                (rnd - 1) * 40, 
                                0, 
                                40, 
                                20
                            ),
                            new Rectangle(0, 0, texture.Texture.Width, texture.Texture.Height),
                            Vector2.Zero,
                            0,
                            Color.Black
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    BeginTextureMode(rt);
                    {
                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                        DrawTexturePro(
                            texture.Texture,
                            new Rectangle(),
                            new Rectangle(
                                -20 + middle.X,
                                -10 + middle.Y,
                                40,
                                20
                            ),
                            Vector2.Zero,
                            0,
                            new Color(255, 0, 255, 255)
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    var sublayer = layer * 10 + 1;

                    Draw.DrawToEffectColor(
                        State.StationLampGradient,
                        new Rectangle(0, 0, 40, 20),
                        new Rectangle(
                            middle.X - 20, 
                            middle.Y - 10, 
                            40, 
                            20
                        ),
                        _gradientA,
                        sublayer,
                        1
                    );

                    UnloadRenderTexture(texture);
                }
                break;
            
                case "LumiaireH":
                {
                    var sublayer = layer * 10 + 7;
                    var middle = Utils.GetMiddleCellPos(x, y);
                    var shader = Shaders.WhiteRemoverApplyColor;

                    var dest = new Rectangle(
                        -29 + middle.X + 10,
                        -11 + middle.Y + 10,
                        58,
                        22
                    );

                    BeginTextureMode(_layers[sublayer]);
                    {
                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.LumiaireH);
                        DrawTexturePro(
                            State.LumiaireH,
                            new Rectangle(0, 0, State.LumiaireH.Width, State.LumiaireH.Height),
                            dest,
                            Vector2.Zero,
                            0,
                            new Color(255, 0, 255, 255)
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    BeginTextureMode(_gradientA[sublayer]);
                    {
                        Draw.DrawTextureDarkest(
                            State.LumHGrad,
                            new Rectangle(0, 0, State.LumHGrad.Width, State.LumHGrad.Height),
                            dest
                        );
                    }
                    EndTextureMode();
                }
                break;
            
                case "LumiaireV":
                {
                    var sublayer = layer * 10 + 7;
                    var middle = Utils.GetMiddleCellPos(x, y);
                    var shader = Shaders.WhiteRemoverApplyColor;

                    var dest = new Rectangle(
                        -11 + middle.X + 10,
                        -29 + middle.Y + 10,
                        22,
                        58
                    );

                    BeginTextureMode(_layers[sublayer]);
                    {
                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.LumiaireV);
                        DrawTexturePro(
                            State.LumiaireV,
                            new Rectangle(0, 0, State.LumiaireV.Width, State.LumiaireV.Height),
                            dest,
                            Vector2.Zero,
                            0,
                            new Color(255, 0, 255, 255)
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    BeginTextureMode(_gradientA[sublayer]);
                    {
                        Draw.DrawTextureDarkest(
                            State.LumVGrad,
                            new Rectangle(0, 0, State.LumVGrad.Width, State.LumVGrad.Height),
                            dest
                        );
                    }
                    EndTextureMode();
                }
                break;
            }
        }
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
    /// Draws materials
    /// </summary>
    /// <param name="x">Matrix X coordinates</param>
    /// <param name="y">Matrix Y coordinates</param>
    /// <param name="layer">Current layer (0, 1, 2)</param>
    /// <param name="camera">The current rendering camera</param>
    /// <param name="mat">The material definition</param>
    /// <param name="rt">The render texture (canvas)</param>
    protected virtual void DrawMaterial_MTX(
        int x,
        int y,
        int layer,
        in RenderCamera camera,
        in MaterialDefinition mat,
        RenderTexture2D rt
    )
    {
        var sublayer = layer * 10;
        var cellRect = new Rectangle(x * 20, y * 20, 20, 20);

        var tileSetName = mat.Name;

        if (mat.Name == "Scaffolding" && Configuration.MaterialFixes)
        {
            tileSetName += "DR";
        }
        else if (mat.Name == "Invisible")
        {
            tileSetName = "SuperStructure";
        }

        var tileSet = State.tileSets.TryGetValue(tileSetName, out var foundTileSet) ? foundTileSet : mat.Texture;

        ref var cell = ref Level!.GeoMatrix[y, x, layer];

        switch (cell.Type)
        {
            case GeoType.Solid:
            {
                for (var f = 1; f <= 4; f++)
                {
                    (Vector2, Vector2) profL;
                    int gtAtV, gtAtH;
                    Rectangle pstRect;

                    switch (f)
                    {
                        case 1:
                            profL = (new(-1, 0), new(0, -1));
                            gtAtV = 2;
                            pstRect = cellRect with {
                                Width = cellRect.Width - 10,
                                Height = cellRect.Height -10
                            };
                            break;

                        case 2:
                            profL = (new(1, 0), new(0, -1));
                            gtAtV = 4;
                            pstRect = cellRect with {
                                X = cellRect.X + 10,
                                Width = cellRect.Width - 10,
                                Height = cellRect.Height -10
                            };
                            break;

                        case 3:
                            profL = (new(1, 0), new(0, 1));
                            gtAtV = 6;
                            pstRect = cellRect with {
                                X = cellRect.X + 10,
                                Y = cellRect.Y + 10,
                                Width = cellRect.Width - 10,
                                Height = cellRect.Height -10
                            };
                            break;

                        default:
                            profL = (new(-1, 0), new(0, 1));
                            gtAtV = 6;
                            pstRect = cellRect with {
                                X = cellRect.X,
                                Y = cellRect.Y + 10,
                                Width = cellRect.Width - 10,
                                Height = cellRect.Height -10
                            };
                            break;
                    }
                
                    (bool, bool) id = (
                        IsMyTileOpenToThisTile(mat, x + (int)profL.Item1.X, y + (int)profL.Item1.Y, layer), 
                        IsMyTileOpenToThisTile(mat, x + (int)profL.Item2.X, y + (int)profL.Item2.Y, layer) 
                    );

                    if (id is (true, true))
                    {
                        if (IsMyTileOpenToThisTile(mat, x + (int)(profL.Item1.X + profL.Item2.X), y + (int)(profL.Item1.Y + profL.Item2.Y), layer))
                        {
                            gtAtH = 10;
                            gtAtV = 2;
                        }
                        else
                        {
                            gtAtH = 8;
                        }
                    }
                    else
                    {
                        gtAtH = id switch {
                            (false, false) => 2,
                            (false, true) => 4,
                            (true, false) => 6,
                            _ => 0
                        };
                    }
                
                    // Don't even ask me what the fuck is this
                    if (gtAtH == 4)
                    {
                        if (gtAtV == 6)
                        {
                            gtAtV = 4;
                        }
                        else if (gtAtV == 8)
                        {
                            gtAtV = 2;
                        }
                    }
                    else if (gtAtH == 6)
                    {
                        if (gtAtV is 4 or 8)
                        {
                            gtAtV -= 2;
                        }
                    }

                    Rectangle gtRect = new(
                        (gtAtH - 1) * 10 - 5,
                        (gtAtV - 1) * 10 - 5,
                        20,
                        20
                    );

                    // pstRect = pstRect with {
                    //     X = pstRect.X - camera.Coords.X,
                    //     Y = pstRect.Y - camera.Coords.Y
                    // };

                    if (mat.Name != "Sand Block")
                    {
                        var shader = Shaders.WhiteRemover;

                        BeginTextureMode(rt);
                        {
                            BeginShaderMode(shader);
                            SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), tileSet);
                            DrawTexturePro(
                                tileSet,
                                gtRect,
                                pstRect with {
                                    X = pstRect.X - 5,
                                    Y = pstRect.Y - 5,
                                    Width = pstRect.Width + 10,
                                    Height = pstRect.Height + 10
                                },
                                Vector2.Zero,
                                0,
                                Color.White
                            );
                            EndShaderMode();
                        }
                        EndTextureMode();

                        for (var d = sublayer + 1; d <= sublayer + 9; d++)
                        {
                            BeginTextureMode(_layers[d]);
                            {
                                BeginShaderMode(shader);
                                SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), tileSet);
                                DrawTexturePro(
                                    tileSet,
                                    gtRect with {
                                        X = gtRect.X + 120
                                    },
                                    pstRect with {
                                        X = pstRect.X - 5,
                                        Y = pstRect.Y - 5,
                                        Width = pstRect.Width + 10,
                                        Height = pstRect.Height + 10
                                    },
                                    Vector2.Zero,
                                    0,
                                    Color.White
                                );
                                EndShaderMode();
                            }
                            EndTextureMode();
                        }
                    }
                }
            }
            break;
            
            case GeoType.SlopeNW:
            case GeoType.SlopeNE:
            case GeoType.SlopeSW:
            case GeoType.SlopeES:
            {
                var slope = cell.Type;

                (Vector2, Vector2) dir;

                if (Configuration.MaterialFixes)
                {
                    dir = slope switch {
                        GeoType.SlopeNE => (new(-1,  0), new(0,  1)),
                        GeoType.SlopeNW => (new( 0,  1), new(1,  0)),
                        GeoType.SlopeES => (new(-1,  0), new(0, -1)),
                        GeoType.SlopeSW => (new( 0, -1), new(1,  0)),
                        _ => (Vector2.Zero, Vector2.Zero)
                    };
                }
                else
                {
                    dir = slope switch {
                        GeoType.SlopeNE => (new(-1,  0), new(0,  1)),
                        GeoType.SlopeNW => (new( 1,  0), new(0,  1)),
                        GeoType.SlopeES => (new(-1,  0), new(0, -1)),
                        GeoType.SlopeSW => (new( 1,  0), new(0, -1)),
                        _ => (Vector2.Zero, Vector2.Zero)
                    };
                }

                Rectangle pstRect = new(
                    x * 20 + camera.Coords.X, 
                    y * 20 + camera.Coords.Y, 
                    20, 
                    20
                );

                // Expanded loop

                Rectangle gtRect;
                var shader = Shaders.WhiteRemover;

                // First iteration (i = dir.Item1)

                {
                    gtRect.X = 10;
                    gtRect.Y = 90 + 30 * ((int)cell.Type - 2);
                    gtRect.Width = 20;
                    gtRect.Height = 20;

                    if (IsMyTileOpenToThisTile(mat, x + (int)dir.Item1.X, y + (int)dir.Item1.Y, layer))
                    {
                        gtRect.X += 30;
                    }

                    gtRect.X -= 5;
                    gtRect.Y -= 5;
                    gtRect.Width += 10;
                    gtRect.Height += 10;

                    pstRect.X -= 5;
                    pstRect.Y -= 5;
                    pstRect.Width += 10;
                    pstRect.Height += 10;

                    if (mat.Name == "Scaffolding" && !Configuration.MaterialFixes)
                    {
                        gtRect.X += 120;
                        
                        SDraw.Draw_NoWhite_NoColor(_layers[sublayer + 5], tileSet, shader, gtRect, pstRect);
                        SDraw.Draw_NoWhite_NoColor(_layers[sublayer + 6], tileSet, shader, gtRect, pstRect);
                        SDraw.Draw_NoWhite_NoColor(_layers[sublayer + 8], tileSet, shader, gtRect, pstRect);
                        SDraw.Draw_NoWhite_NoColor(_layers[sublayer + 9], tileSet, shader, gtRect, pstRect);
                    }
                    else if (mat.Name != "Sand Block")
                    {
                        SDraw.Draw_NoWhite_NoColor(rt, tileSet, shader, gtRect, pstRect);

                        gtRect.X += 120;

                        for (var d = sublayer + 1; d <= sublayer + 9; d++)
                        {
                            SDraw.Draw_NoWhite_NoColor(_layers[d], tileSet, shader, gtRect, pstRect);
                        }
                    }
                }

                // Second iteration (i = dir.Item2)

                {
                    gtRect.X = 10;
                    gtRect.Y = 90 + 30 * ((int)cell.Type - 2);
                    gtRect.Width = 20;
                    gtRect.Height = 20;

                    if (IsMyTileOpenToThisTile(mat, x + (int)dir.Item2.X, y + (int)dir.Item2.Y, layer))
                    {
                        gtRect.X += 30;
                    }

                    gtRect.X -= 5;
                    gtRect.Y -= 5;
                    gtRect.Width += 10;
                    gtRect.Height += 10;

                    pstRect.X -= 5;
                    pstRect.Y -= 5;
                    pstRect.Width += 10;
                    pstRect.Height += 10;

                    if (mat.Name == "Scaffolding" && !Configuration.MaterialFixes)
                    {
                        gtRect.X += 120;
                        
                        SDraw.Draw_NoWhite_NoColor(_layers[sublayer + 5], tileSet, shader, gtRect, pstRect);
                        SDraw.Draw_NoWhite_NoColor(_layers[sublayer + 6], tileSet, shader, gtRect, pstRect);
                        SDraw.Draw_NoWhite_NoColor(_layers[sublayer + 8], tileSet, shader, gtRect, pstRect);
                        SDraw.Draw_NoWhite_NoColor(_layers[sublayer + 9], tileSet, shader, gtRect, pstRect);
                    }
                    else if (mat.Name != "Sand Block")
                    {
                        SDraw.Draw_NoWhite_NoColor(rt, tileSet, shader, gtRect, pstRect);

                        gtRect.X += 120;

                        for (var d = sublayer + 1; d <= sublayer + 9; d++)
                        {
                            SDraw.Draw_NoWhite_NoColor(_layers[d], tileSet, shader, gtRect, pstRect);
                        }
                    }
                }
            }
            break;

            case GeoType.Platform:
            if (mat.Name != "Invisible") {
                
                Rectangle pstRect = new(
                    x * 20 - camera.Coords.X,
                    y * 20 - camera.Coords.Y,
                    20,
                    20
                );

                if (mat.Name == "Stained Glass")
                {
                    DrawTile_MTX(Level!.TileMatrix[y, x, layer], State.SGFL!, x, y, layer, camera, rt);
                }
                else if (
                    Configuration.MaterialFixes ||
                    (mat.Name != "Sand Block" && mat.Name != "Scaffolding" && mat.Name != "Tiny Signs")
                )
                {
                    // Fine. You win.
                    DrawTile_MTX(
                        Level!.TileMatrix[y, x, layer], 
                        new TileDefinition(
                            $"tileSet{tileSetName}Floor", 
                            (1, 1), 
                            TileType.VoxelStruct, 
                            1, 
                            new int[0,0,0], 
                            [6, 1, 1, 1, 1], 
                            [], 1
                        ) { Texture = State.tileSets[$"tileSet{tileSetName}Floor"] },
                        x,
                        y,
                        layer,
                        camera,
                        rt
                    );
                }
                else
                {
                    DrawTile_MTX(
                        Level!.TileMatrix[y, x, layer],
                        State.tileSetBigMetalFloor!,
                        x,
                        y
                        ,
                        layer,
                        camera,
                        rt
                    );
                }
            }
            break;
        }
    
        var modder = mat.Name switch {
            "Concrete"          => 45,
            "RainStone"         => 6,
            "Bricks"            => 1,
            "Tiny Signs"        => 10,
            "Cliff"             => 45,
            "Non-Slip Metal"    => 5,
            "BulkMetal"         => 5,
            "MassiveBulkMetal"  => 10,
            "Asphalt"           => 45,
            
            _ => 0
        };

        if (modder is not 0)
        {
            Rectangle gtRect = new (
                (x % modder) * 20,
                (y % modder) * 20,
                20,
                20
            );

            if (mat.Name is "Bricks")
            {
                gtRect = new Rectangle(0, 0, 20, 20);
            }

            if (mat.Name is "Tiny Signs")
            {
                DrawTinySigns();
                _tinySignsDrawn = true;
            }

            if (cell[GeoType.Solid])
            {
                Rectangle pstRect = new (
                    x * 20,
                    y * 20,
                    20,
                    20
                );

                SDraw.Draw_NoWhite_NoColor(
                    _layers[sublayer],
                    mat.Texture,
                    Shaders.WhiteRemover,
                    gtRect,
                    pstRect
                );
            }
            else if (cell.IsSlope)
            {
                // This rectangle was not initialized within the scope
                // of this code block.
                // In fact, I do not know where the initialization is supposed to be.
                Rectangle pstRect = new(
                    x * 20 + camera.Coords.X, 
                    y * 20 + camera.Coords.Y, 
                    20, 
                    20
                );

                SDraw.Draw_NoWhite_NoColor(
                    _layers[sublayer],
                    mat.Texture,
                    Shaders.WhiteRemover,
                    gtRect,
                    pstRect
                );

                var pos = new Vector2(x, y) * 20;
                var offPos = new Vector2(x + 1, y + 1) * 20;

                var topLeft = pos;
                var bottomLeft = new Vector2(pos.X, offPos.Y);
                var topRight = new Vector2(offPos.X, pos.Y);
                var bottomRight = offPos;

                (Vector2, Vector2, Vector2) triangle = cell.Type switch {
                    // The triangles are in the reverse order because these are
                    // for cropping a square into a triangle.
                    GeoType.SlopeSW => (topLeft    , bottomRight, bottomLeft),
                    GeoType.SlopeES => (topRight   , bottomLeft , offPos    ),
                    GeoType.SlopeNW => (bottomLeft , topRight   , topLeft   ),
                    GeoType.SlopeNE => (bottomRight, topLeft    , topRight  ),
                    
                    _ => (Vector2.Zero, Vector2.Zero, Vector2.Zero)
                };

                // Translate to absolute position
                triangle.Item1 -= camera.Coords;
                triangle.Item2 -= camera.Coords;
                triangle.Item3 -= camera.Coords;
            
                BeginTextureMode(_layers[sublayer]);
                {
                    DrawTriangle(
                        triangle.Item1, 
                        triangle.Item2,
                        triangle.Item3,
                        Color.White
                    );
                }
                EndTextureMode();
            }
        }
    
        if (mat.Name is "Stained Glass")
        {
            // The original code was very confusing.

            modder = 1;

            var imgLoad = "SG";
            Rectangle gtRect = new (0, 0, 20, 20);

            var v = "1";
            var clr1 = "A";
            var clr2 = "B";

            var x2 = x + camera.Coords.X;
            var y2 = y + camera.Coords.Y;
        
            foreach (var effect in Level!.Effects)
            {
                if (effect.Name is not "Stained Glass Properties") continue;
                if (effect.Matrix[y, x] < 0) continue;

                var varOpt = effect.Options.FirstOrDefault(o => o.Name is "Variation");
                var clr1Opt = effect.Options.FirstOrDefault(o => o.Name is "Color 1");
                var clr2Opt = effect.Options.FirstOrDefault(o => o.Name is "Color 2");
            
                if (varOpt is { Choice: string })
                {
                    v = varOpt.Choice is "1" or "2" or "3" 
                        ? varOpt.Choice 
                        : "1";
                }

                if (clr1Opt is { Choice: string })
                {
                    clr1 = clr1Opt.Choice switch 
                    {
                        "EffectColor1" => "A",
                        "EffectColor2" => "B",
                        "None" => "C",
                        _ => "A"
                    };
                }

                if (clr2Opt is { Choice: string })
                {
                    clr2 = clr2Opt.Choice switch 
                    {
                        "EffectColor1" => "A",
                        "EffectColor2" => "B",
                        "None" => "C",
                        _ => "B"
                    };
                }

                break;
            }
        
            // Really bad, but temporary.

            var lib = Registry.CastLibraries["Drought"];

            var textureSocket = lib[$"{imgLoad}{v}Socket"].Texture;
            var textureGrad = lib[$"{imgLoad}{v}Grad"].Texture;
            var textureClr = lib[$"{imgLoad}{v}{clr1}{clr2}"].Texture;

            // The original code duplicated this section three times and then cropped
            // the cell to match the shape.

            Rectangle pstRect = new(x * 20, y * 20, 20, 20);

            pstRect.X -= camera.Coords.X;
            pstRect.Y -= camera.Coords.Y;

            SDraw.Draw_NoWhite_Color(
                _layers[sublayer],
                textureSocket,
                Shaders.WhiteRemoverApplyColor,
                gtRect,
                pstRect,
                Color.Green
            );

            SDraw.Draw_NoWhite_Color(
                _layers[sublayer + 1],
                textureSocket,
                Shaders.WhiteRemoverApplyColor,
                gtRect,
                pstRect,
                Color.Green
            );

            SDraw.Draw_NoWhite_Color(
                _layers[sublayer + 1],
                textureClr,
                Shaders.WhiteRemoverApplyColor,
                gtRect,
                pstRect,
                Color.Green
            );

            BeginTextureMode(_gradientA[sublayer + 1]);
            Draw.DrawTextureDarkest(
                textureGrad,
                gtRect,
                pstRect
            );
            EndTextureMode();

            BeginTextureMode(_gradientB[sublayer + 1]);
            Draw.DrawTextureDarkest(
                textureGrad,
                gtRect,
                pstRect
            );
            EndTextureMode();
            
            if (cell.IsSlope)
            {
                // Code copied from earlier

                var pos = new Vector2(x, y) * 20;
                var offPos = new Vector2(x + 1, y + 1) * 20;

                var topLeft = pos;
                var bottomLeft = new Vector2(pos.X, offPos.Y);
                var topRight = new Vector2(offPos.X, pos.Y);
                var bottomRight = offPos;

                (Vector2, Vector2, Vector2) triangle = cell.Type switch {
                    // The triangles are in the reverse order because these are
                    // for cropping a square into a triangle.
                    GeoType.SlopeSW => (topLeft    , bottomRight, bottomLeft),
                    GeoType.SlopeES => (topRight   , bottomLeft , offPos    ),
                    GeoType.SlopeNW => (bottomLeft , topRight   , topLeft   ),
                    GeoType.SlopeNE => (bottomRight, topLeft    , topRight  ),
                    
                    _ => (Vector2.Zero, Vector2.Zero, Vector2.Zero)
                };

                // Translate to absolute position
                triangle.Item1 -= camera.Coords;
                triangle.Item2 -= camera.Coords;
                triangle.Item3 -= camera.Coords;

                // Expanded loop

                BeginTextureMode(_layers[sublayer]);
                {
                    DrawTriangle(
                        triangle.Item1,
                        triangle.Item2,
                        triangle.Item3,
                        Color.White
                    );
                }
                EndTextureMode();

                BeginTextureMode(_layers[sublayer + 1]);
                {
                    DrawTriangle(
                        triangle.Item1,
                        triangle.Item2,
                        triangle.Item3,
                        Color.White
                    );
                }
                EndTextureMode();
            }
            else if (cell.Type is GeoType.Platform)
            {
                Rectangle halfACell = new(x * 20, y * 20 + 10, 20, 20);

                halfACell.X -= camera.Coords.X;
                halfACell.Y -= camera.Coords.Y;

                // Expanded loop

                BeginTextureMode(_layers[sublayer]);
                {
                    DrawRectangleRec(halfACell, Color.White);
                }
                EndTextureMode();

                BeginTextureMode(_layers[sublayer + 1]);
                {
                    DrawRectangleRec(halfACell, Color.White);
                }
                EndTextureMode();
            }
        }
    
        // At last. The final battle
        else if (mat.Name is "Sand Block")
        {
            modder = 28;

            Rectangle gtRect = new (
                (x % modder) * 20 - camera.Coords.X,
                (y % modder) * 20 - camera.Coords.Y,
                20,
                20
            );

            switch (cell.Type)
            {
                case GeoType.Solid:
                {
                    var rnd = Random.Generate(4);
                    Rectangle pstRect = new (
                        x * 20 - camera.Coords.X,
                        y * 20 - camera.Coords.Y,
                        20,
                        20
                    );

                    for (var d = 0; d <= 9; d++)
                    {
                        var texture = Registry.CastLibraries["Drought"][$"Sand BlockTexture{Random.Generate(4)}"].Texture;

                        SDraw.Draw_NoWhite_NoColor(
                            _layers[sublayer + d],
                            texture,
                            Shaders.WhiteRemover,
                            gtRect,
                            pstRect
                        );
                    }
                }
                break;

                case GeoType.SlopeNE:
                case GeoType.SlopeNW:
                case GeoType.SlopeES:
                case GeoType.SlopeSW:
                {
                    var rnd = Random.Generate(4);
                    Rectangle pstRect = new (
                        x * 20 - camera.Coords.X,
                        y * 20 - camera.Coords.Y,
                        20,
                        20
                    );

                    for (var d = 0; d <= 9; d++)
                    {
                        SDraw.Draw_NoWhite_NoColor(
                            _layers[d + sublayer],
                            Registry.CastLibraries["Drought"][$"Sand BlockTexture{Random.Generate(4)}"].Texture,
                            Shaders.WhiteRemover,
                            gtRect,
                            pstRect
                        );
                    }

                    var pos = new Vector2(x, y) * 20;
                    var offPos = new Vector2(x + 1, y + 1) * 20;

                    var topLeft = pos;
                    var bottomLeft = new Vector2(pos.X, offPos.Y);
                    var topRight = new Vector2(offPos.X, pos.Y);
                    var bottomRight = offPos;

                    (Vector2, Vector2, Vector2) triangle = cell.Type switch {
                        // The triangles are in the reverse order because these are
                        // for cropping a square into a triangle.
                        GeoType.SlopeSW => (topLeft    , bottomRight, bottomLeft),
                        GeoType.SlopeES => (topRight   , bottomLeft , offPos    ),
                        GeoType.SlopeNW => (bottomLeft , topRight   , topLeft   ),
                        GeoType.SlopeNE => (bottomRight, topLeft    , topRight  ),
                        
                        _ => (Vector2.Zero, Vector2.Zero, Vector2.Zero)
                    };

                    // Translate to absolute position
                    triangle.Item1 -= camera.Coords;
                    triangle.Item2 -= camera.Coords;
                    triangle.Item3 -= camera.Coords;

                    for (var d = 0; d <= 9; d++)
                    {
                        DrawTriangle(
                            triangle.Item1,
                            triangle.Item2,
                            triangle.Item3,
                            Color.White
                        );
                    }
                }
                break;
            
                case GeoType.Platform:
                {
                    Rectangle halfACell = new (
                        x * 20      - camera.Coords.X, 
                        y * 20 + 10 - camera.Coords.Y,
                        20, 
                        20
                    );

                    for (var d = 0; d <= 9; d++)
                    {
                        SDraw.Draw_NoWhite_NoColor(
                            _layers[sublayer + d],
                            Registry.CastLibraries["Drought"][$"Sand BlockTexture{Random.Generate(4)}"].Texture,
                            Shaders.WhiteRemover,
                            gtRect,
                            halfACell
                        );
                    }
                }
                break;
            }
        }
    }

    protected virtual bool CheckIfAMaterialIsSolidAndSameMaterial(
        int x, int y, int layer,
        MaterialDefinition? mat
    ) {
        x = Utils.Restrict(x, 0, Level!.Width  - 1);
        y = Utils.Restrict(y, 0, Level!.Height - 1);

        if (Level!.GeoMatrix[y, x, layer] is not { Type: GeoType.Solid }) return false;

        ref var cell = ref Level!.TileMatrix[y, x, layer];

        return cell.Type is TileCellType.Material && cell.MaterialDefinition == mat 
            || cell.Type is TileCellType.Default && Level!.DefaultMaterial == mat; 
    }

    protected virtual void AttemptDrawTempleStone_MTX(
        int x,
        int y,
        int layer,
        RenderCamera camera,
        List<(int x, int y)> list,
        List<(int x, int y)>[] corners,
        TileDefinition tile,
        RenderTexture2D rt
    )
    {
        List<(int x, int y)> occupy = [];

        switch (tile.Name)
        {
            case "Big Temple Stone No Slopes":
            occupy = [ (-1, 0), (0, -1), (0, 0), (0, 1), (1, -1), (1, 0), (1, 1), (2, 0) ];
            break;

            case "Wide Temple Stone":
            occupy = [ (0, 0), (1, 0) ];
            break;
        }

        foreach (var o in occupy)
        {
            if (!CheckIfAMaterialIsSolidAndSameMaterial(x + o.x, y + o.y, layer, State.templeStone)) return;
        }

        DrawTile_MTX(
            Level!.TileMatrix[y, x, layer],
            tile,
            x,
            y,
            layer,
            camera,
            rt
        );

        if (tile.Name is "Big Temple Stone No Slopes")
        {
            corners[0].Add((x - 1, y - 1));
            corners[1].Add((x + 2, y - 1));
            corners[2].Add((x + 2, y + 1));
            corners[3].Add((x - 1, y + 1));
        }

        foreach (var o in occupy) list.Remove((x + o.x, y + o.y));
    }

    /// <summary>
    /// Draws a material of "tile" render type.
    /// </summary>
    /// <param name="layer">The current layer</param>
    /// <param name="camera">The current rendering camera</param>
    /// <param name="mat">The material definition</param>
    /// <param name="rt">The render texture</param>
    protected virtual void RenderTileMaterial_MTX(
        int layer,
        in RenderCamera camera,
        in MaterialDefinition mat,
        RenderTexture2D rt
    ) {
        List<(int rnd, int x, int y)> orderedTiles = [];

        for (var mx = 0; mx < Level!.Width; mx++)
        {
            for (var my = 0; my < Level!.Height; my++)
            {
                ref var geoCell = ref Level!.GeoMatrix[my, mx, layer];

                if (geoCell[GeoType.Air]) continue;

                ref var tileCell = ref Level!.TileMatrix[my, mx, layer];

                if (
                    !(tileCell.Type is TileCellType.Material && 
                    tileCell.MaterialDefinition == mat) 
                    &&
                    !(tileCell.Type is TileCellType.Default &&
                    Level!.DefaultMaterial == mat)
                ) continue;

                if (
                    geoCell[GeoType.Solid] ||
                    Configuration.MaterialFixes 
                        && mat.Name is 
                        not "Tiled Stone" 
                        or "Chaotic Stone" 
                        or "Random Machines" 
                        or "3DBricks"
                )
                {
                    orderedTiles.Add((Random.Generate(Level!.Width + Level!.Height), mx, my));
                }
                else if (CheckCollisionPointRec(new Vector2(mx, my), new Rectangle(camera.Coords/20, new Vector2(100, 60))))
                {
                    DrawMaterial_MTX(mx, my, layer, camera, Registry.Materials!.Get("Standard"), rt);
                }
            }
        }
    
        orderedTiles.Sort((l1, l2) => {
            if (l1.rnd > l2.rnd) return 1;
            if (l2.rnd > l1.rnd) return -1;
            if (l1.rnd == l2.rnd) return 0;
            return 0;
        });

        switch (mat.Name)
        {
            case "Chaotic Stone":
            {
                bool[,] waiting = new bool[Level!.Height, Level!.Width];
                bool[,] deleted = new bool[Level!.Height, Level!.Width];

                // Draw Chaotic Stone corners and platoform
                if (Configuration.MaterialFixes)
                {
                    var SmallStoneSlopeNE = Registry.Tiles!.Get("Small Stone Slope NE");
                    var SmallStoneSlopeNW = Registry.Tiles!.Get("Small Stone Slope NW");
                    var SmallStoneSlopeSW = Registry.Tiles!.Get("Small Stone Slope SW");
                    var SmallStoneSlopeSE = Registry.Tiles!.Get("Small Stone Slope SE");
                    var SmallStoneFloor = Registry.Tiles!.Get("Small Stone Floor");

                    for (var q = 0; q <= orderedTiles.Count; q++)
                    {
                        var queued = orderedTiles[^q];

                        switch (Level!.GeoMatrix[queued.y, queued.x, layer].Type)
                        {
                            case GeoType.SlopeNE:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneSlopeNE, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeNW:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneSlopeNW, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeES:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneSlopeSE, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeSW:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneSlopeSW, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.Platform:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneFloor, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.Solid:
                            waiting[queued.y, queued.x] = true;
                            continue;
                        }

                        orderedTiles.Remove(queued);
                        deleted[queued.y, queued.x] = true;
                    }
                }
            
                var SquareStone = Registry.Tiles!.Get("Square Stone");
                var SmallStone = Registry.Tiles!.Get("Small Stone");

                foreach (var (_, mx, my) in orderedTiles)
                {
                    // At this point, all deleted tiles should not appear in the list.
                    // if (deleted[my, mx]) continue;

                    var hts = 0;
                    
                    // Expanded loop

                    { // First iteration (dir = point(1, 0))

                        hts += Utils.BoolInt(orderedTiles.Any(v => v == (v.rnd, v.x + 1, v.y))) * Utils.BoolInt(deleted[my, mx+1]);
                    }
                    { // Second iteration (dir = point(0, 1))

                        hts += Utils.BoolInt(orderedTiles.Any(v => v == (v.rnd, v.x, v.y + 1))) * Utils.BoolInt(deleted[my+1, mx]);
                    }
                    { // Third iteration (dir = point(1, 1))

                        hts += Utils.BoolInt(orderedTiles.Any(v => v == (v.rnd, v.x + 1, v.y + 1))) * Utils.BoolInt(deleted[my+1, mx+1]);
                    }

                    // Big boy (2 x 2)
                    if (hts is not 3) continue;

                    // Check if the tile is in the camera bounds
                    if (CheckCollisionPointRec(new Vector2(mx, my), new Rectangle(camera.Coords/20, new Vector2(100, 60))))
                    {
                        DrawTile_MTX(
                            Level!.TileMatrix[my, mx, layer],
                            SquareStone,
                            mx,
                            my,
                            layer,
                            camera,
                            rt
                        );
                    }

                    // Now mark the cells as used or unavailable

                    // Expanded loop

                    { // First iteration (dir = point(1, 0))
                        deleted[mx+1, my] = true;
                        waiting[mx+1, my] = false;
                    }
                    { // Second iteration (dir = point(0, 1))
                        deleted[mx, my+1] = true;
                        waiting[mx, my+1] = false;
                    }
                    { // Third iteration (dir = point(1, 1))
                        deleted[mx+1, my+1] = true;
                        waiting[mx+1, my+1] = false;
                    }

                    deleted[my, mx] = true;
                    waiting[mx, my] = false;
                }
            
                orderedTiles = orderedTiles.Where(t => !deleted[t.y, t.x]).ToList();

                while (orderedTiles.Count > 0)
                {
                    var index = Random.Generate(orderedTiles.Count - 1);
                    var (_, tx, ty) = orderedTiles[index];
                    
                    // Check if it's in camera bounds
                    if (CheckCollisionPointRec(new Vector2(tx, ty), new Rectangle(camera.Coords/20, new Vector2(100, 60))))
                    {
                        DrawTile_MTX(
                            Level!.TileMatrix[ty, tx, layer],
                            SmallStone,
                            tx,
                            ty,
                            layer,
                            camera,
                            rt
                        );
                    }

                    orderedTiles.RemoveAt(index);
                }
            }
            break;
        
            // Same as "Chaotic Stone", but without placing Square Stone.
            case "Tiled Stone":
            {
                bool[,] waiting = new bool[Level!.Height, Level!.Width];
                bool[,] deleted = new bool[Level!.Height, Level!.Width];

                // Draw Chaotic Stone corners and platoform
                if (Configuration.MaterialFixes)
                {
                    var SmallStoneSlopeNE = Registry.Tiles!.Get("Small Stone Slope NE");
                    var SmallStoneSlopeNW = Registry.Tiles!.Get("Small Stone Slope NW");
                    var SmallStoneSlopeSW = Registry.Tiles!.Get("Small Stone Slope SW");
                    var SmallStoneSlopeSE = Registry.Tiles!.Get("Small Stone Slope SE");
                    var SmallStoneFloor = Registry.Tiles!.Get("Small Stone Floor");

                    for (var q = 0; q <= orderedTiles.Count; q++)
                    {
                        var queued = orderedTiles[^q];

                        switch (Level!.GeoMatrix[queued.y, queued.x, layer].Type)
                        {
                            case GeoType.SlopeNE:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneSlopeNE, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeNW:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneSlopeNW, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeES:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneSlopeSE, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeSW:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneSlopeSW, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.Platform:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneFloor, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.Solid:
                            waiting[queued.y, queued.x] = true;
                            continue;
                        }

                        orderedTiles.Remove(queued);
                        deleted[queued.y, queued.x] = true;
                    }
                }
            
                var SmallStone = Registry.Tiles!.Get("Small Stone");

                while (orderedTiles.Count > 0)
                {
                    var index = Random.Generate(orderedTiles.Count);
                    var (_, tx, ty) = orderedTiles[index];
                    
                    // Check if it's in camera bounds
                    if (CheckCollisionPointRec(new Vector2(tx, ty), new Rectangle(camera.Coords/20, new Vector2(100, 60))))
                    {
                        DrawTile_MTX(
                            Level!.TileMatrix[ty, tx, layer],
                            SmallStone,
                            tx,
                            ty,
                            layer,
                            camera,
                            rt
                        );
                    }

                    orderedTiles.RemoveAt(index);
                }
            }
            break;
            
            // The reason I grouped all three together
            // is because all of them are pulling from the 
            // exact same tile pool.
            case "Random Machines":
            case "Random Machines 2":
            case "Small Machines":
            {
                bool[,] waiting = new bool[Level!.Height, Level!.Width];
                bool[,] deleted = new bool[Level!.Height, Level!.Width];

                // Draw Chaotic Stone corners and platoform
                if (Configuration.MaterialFixes)
                {
                    var SmallMachineSlopeNE = Registry.Tiles!.Get("Small Machine Slope NE");
                    var SmallMachineSlopeNW = Registry.Tiles!.Get("Small Machine Slope NW");
                    var SmallMachineSlopeSW = Registry.Tiles!.Get("Small Machine Slope SW");
                    var SmallMachineSlopeSE = Registry.Tiles!.Get("Small Machine Slope SE");
                    var SmallMachineFloor = Registry.Tiles!.Get("Small Machine Floor");

                    for (var q = 0; q <= orderedTiles.Count; q++)
                    {
                        var queued = orderedTiles[^q];

                        switch (Level!.GeoMatrix[queued.y, queued.x, layer].Type)
                        {
                            case GeoType.SlopeNE:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMachineSlopeNE, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeNW:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMachineSlopeNW, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeES:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMachineSlopeSE, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeSW:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMachineSlopeSW, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.Platform:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMachineFloor, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.Solid:
                            waiting[queued.y, queued.x] = true;
                            continue;
                        }

                        orderedTiles.RemoveAt(orderedTiles.Count - q);
                        deleted[queued.y, queued.x] = true;
                    }
                }

                // You could probably turn them into hash sets.
                Dictionary<(int x, int y), bool> delL = [];
                Dictionary<(int x, int y), bool> blocks = [];

                foreach (var (_, mx, my) in orderedTiles) blocks[(mx, my)] = true;

                foreach (var (_, mx, my) in orderedTiles)
                {
                    if (delL.ContainsKey((mx, my))) continue;

                    List<(int rnd, TileDefinition tile)> randomMachines = State.randomMachinesPool
                        .Select(t => (Random.Generate(1000), t))
                        .ToList();

                    randomMachines.Sort((r1, r2) => {
                        if (r1.rnd >  r2.rnd) return  1;
                        if (r1.rnd <  r2.rnd) return -1;
                        if (r1.rnd == r2.rnd) return  0;
                        return 0;
                    });

                    foreach (var (_, tile) in randomMachines)
                    {
                        // Testing if we can place the tile.

                        var legal = true;

                        for (var sx = 0; sx < tile.Size.Width; sx++)
                        {
                            for (var sy = 0; sy < tile.Size.Height; sy++)
                            {
                                var tx = mx + sx;
                                var ty = my + sy;

                                var spec = tile.Specs[sy, sx, 0];

                                if (!blocks.ContainsKey((tx, ty)))
                                {
                                    legal = false;
                                    goto stop_testing;
                                }

                                if (spec is -1) continue;

                                if (delL.ContainsKey((tx, ty)))
                                {
                                    legal = false;
                                    goto stop_testing;
                                }

                                if (
                                    !Data.Utils.InBounds(Level!.GeoMatrix, tx, ty) ||
                                    spec != (int)Level!.GeoMatrix[ty, tx, layer].Type
                                ) {
                                    legal = false;
                                    goto stop_testing;
                                }
                            }
                        }

                        stop_testing:

                        if (!legal) continue;

                        // Determining the tile head position, I suppose.
                        var rootPosX = mx + (int)(tile.Size.Width /2f + 0.4999f) - 1;
                        var rootPosY = my + (int)(tile.Size.Height/2f + 0.4999f) - 1;

                        // Drawing the tile if it's in camera bounds.
                        if (CheckCollisionPointRec(new Vector2(rootPosX, rootPosY), new Rectangle(camera.Coords/20f, new Vector2(100, 60))))
                        {
                            DrawTile_MTX(
                                Level!.TileMatrix[rootPosY, rootPosX, layer],
                                tile,
                                rootPosX,
                                rootPosY,
                                layer,
                                camera,
                                rt
                            );
                        }

                        for (var w = 0; w < tile.Size.Width; w++)
                        {
                            for (var h = 0; h < tile.Size.Height; h++)
                            {
                                var spec = tile.Specs[h, w, 0];

                                if (spec is not -1) delL[(mx + w, my + h)] = true;
                            }
                        }

                        break;
                    }

                }
            }
            break;

            // The code was basically copied from previous cases.
            case "Random Metal":
            {
                bool[,] waiting = new bool[Level!.Height, Level!.Width];
                bool[,] deleted = new bool[Level!.Height, Level!.Width];

                // Draw Chaotic Stone corners and platoform
                if (Configuration.MaterialFixes)
                {
                    var SmallMetalSlopeNE = Registry.Tiles!.Get("Small Metal Slope NE");
                    var SmallMetalSlopeNW = Registry.Tiles!.Get("Small Metal Slope NW");
                    var SmallMetalSlopeSW = Registry.Tiles!.Get("Small Metal Slope SW");
                    var SmallMetalSlopeSE = Registry.Tiles!.Get("Small Metal Slope SE");
                    var SmallMetalFloor = Registry.Tiles!.Get("Small Metal Floor");

                    for (var q = 0; q <= orderedTiles.Count; q++)
                    {
                        var queued = orderedTiles[^q];

                        switch (Level!.GeoMatrix[queued.y, queued.x, layer].Type)
                        {
                            case GeoType.SlopeNE:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMetalSlopeNE, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeNW:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMetalSlopeNW, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeES:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMetalSlopeSE, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeSW:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMetalSlopeSW, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.Platform:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMetalFloor, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.Solid:
                            waiting[queued.y, queued.x] = true;
                            continue;
                        }

                        orderedTiles.RemoveAt(orderedTiles.Count - q);
                        deleted[queued.y, queued.x] = true;
                    }
                }



                // You could probably turn them into hash sets.
                Dictionary<(int x, int y), bool> delL = [];
                Dictionary<(int x, int y), bool> blocks = [];

                foreach (var (_, mx, my) in orderedTiles) blocks[(mx, my)] = true;

                foreach (var (_, mx, my) in orderedTiles)
                {
                    if (delL.ContainsKey((mx, my))) continue;

                    List<(int rnd, TileDefinition tile)> randomMetals = State.randomMetalPool
                        .Select(t => (Random.Generate(1000), t))
                        .ToList();

                    randomMetals.Sort((r1, r2) => {
                        if (r1.rnd >  r2.rnd) return  1;
                        if (r1.rnd <  r2.rnd) return -1;
                        if (r1.rnd == r2.rnd) return  0;
                        return 0;
                    });

                    foreach (var (_, tile) in randomMetals)
                    {
                        // Testing if we can place the tile.

                        var legal = true;

                        for (var sx = 0; sx < tile.Size.Width; sx++)
                        {
                            for (var sy = 0; sy < tile.Size.Height; sy++)
                            {
                                var tx = mx + sx;
                                var ty = my + sy;

                                var spec = tile.Specs[sy, sx, 0];

                                if (!blocks.ContainsKey((tx, ty)))
                                {
                                    legal = false;
                                    goto stop_testing;
                                }

                                if (spec is -1) continue;

                                if (delL.ContainsKey((tx, ty)))
                                {
                                    legal = false;
                                    goto stop_testing;
                                }

                                if (
                                    !Data.Utils.InBounds(Level!.GeoMatrix, tx, ty) ||
                                    spec != (int)Level!.GeoMatrix[ty, tx, layer].Type
                                ) {
                                    legal = false;
                                    goto stop_testing;
                                }
                            }
                        }

                        stop_testing:

                        if (!legal) continue;

                        // Determining the tile head position, I suppose.
                        var rootPosX = mx + (int)(tile.Size.Width /2f + 0.4999f) - 1;
                        var rootPosY = my + (int)(tile.Size.Height/2f + 0.4999f) - 1;

                        // Drawing the tile if it's in camera bounds.
                        if (CheckCollisionPointRec(new Vector2(rootPosX, rootPosY), new Rectangle(camera.Coords/20f, new Vector2(100, 60))))
                        {
                            DrawTile_MTX(
                                Level!.TileMatrix[rootPosY, rootPosX, layer],
                                tile,
                                rootPosX,
                                rootPosY,
                                layer,
                                camera,
                                rt
                            );
                        }

                        for (var w = 0; w < tile.Size.Width; w++)
                        {
                            for (var h = 0; h < tile.Size.Height; h++)
                            {
                                var spec = tile.Specs[h, w, 0];

                                if (spec is not -1) delL[(mx + w, my + h)] = true;
                            }
                        }

                        break;
                    }

                }
            }
            break;
        
            // The code was basically copied from previous cases.
            case "Chaotic Stone 2":
            {
                bool[,] waiting = new bool[Level!.Height, Level!.Width];
                bool[,] deleted = new bool[Level!.Height, Level!.Width];

                
                // Draw Chaotic Stone corners and platoform
                if (Configuration.MaterialFixes)
                {
                    var SmallStoneSlopeNE = Registry.Tiles!.Get("Small Stone Slope NE");
                    var SmallStoneSlopeNW = Registry.Tiles!.Get("Small Stone Slope NW");
                    var SmallStoneSlopeSW = Registry.Tiles!.Get("Small Stone Slope SW");
                    var SmallStoneSlopeSE = Registry.Tiles!.Get("Small Stone Slope SE");
                    var SmallStoneFloor = Registry.Tiles!.Get("Small Stone Floor");

                    for (var q = 0; q <= orderedTiles.Count; q++)
                    {
                        var queued = orderedTiles[^q];

                        switch (Level!.GeoMatrix[queued.y, queued.x, layer].Type)
                        {
                            case GeoType.SlopeNE:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneSlopeNE, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeNW:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneSlopeNW, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeES:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneSlopeSE, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeSW:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneSlopeSW, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.Platform:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneFloor, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.Solid:
                            waiting[queued.y, queued.x] = true;
                            continue;
                        }

                        orderedTiles.RemoveAt(orderedTiles.Count - q);
                        deleted[queued.y, queued.x] = true;
                    }
                }

                // You could probably turn them into hash sets.
                Dictionary<(int x, int y), bool> delL = [];
                Dictionary<(int x, int y), bool> blocks = [];

                foreach (var (_, mx, my) in orderedTiles) blocks[(mx, my)] = true;

                foreach (var (_, mx, my) in orderedTiles)
                {
                    if (delL.ContainsKey((mx, my))) continue;

                    List<(int rnd, TileDefinition tile)> chaoticStone2 = State.chaoticStone2Pool
                        .Select(t => (Random.Generate(1000), t))
                        .ToList();

                    chaoticStone2.Sort((r1, r2) => {
                        if (r1.rnd >  r2.rnd) return  1;
                        if (r1.rnd <  r2.rnd) return -1;
                        if (r1.rnd == r2.rnd) return  0;
                        return 0;
                    });

                    foreach (var (_, tile) in chaoticStone2)
                    {
                        // Testing if we can place the tile.

                        var legal = true;

                        for (var sx = 0; sx < tile.Size.Width; sx++)
                        {
                            for (var sy = 0; sy < tile.Size.Height; sy++)
                            {
                                var tx = mx + sx;
                                var ty = my + sy;

                                var spec = tile.Specs[sy, sx, 0];

                                if (!blocks.ContainsKey((tx, ty)))
                                {
                                    legal = false;
                                    goto stop_testing;
                                }

                                if (spec is -1) continue;

                                if (delL.ContainsKey((tx, ty)))
                                {
                                    legal = false;
                                    goto stop_testing;
                                }

                                if (
                                    !Data.Utils.InBounds(Level!.GeoMatrix, tx, ty) ||
                                    spec != (int)Level!.GeoMatrix[ty, tx, layer].Type
                                ) {
                                    legal = false;
                                    goto stop_testing;
                                }
                            }
                        }

                        stop_testing:

                        if (!legal) continue;

                        // Determining the tile head position, I suppose.
                        var rootPosX = mx + (int)(tile.Size.Width /2f + 0.4999f) - 1;
                        var rootPosY = my + (int)(tile.Size.Height/2f + 0.4999f) - 1;

                        // Drawing the tile if it's in camera bounds.
                        if (CheckCollisionPointRec(new Vector2(rootPosX, rootPosY), new Rectangle(camera.Coords/20f, new Vector2(100, 60))))
                        {
                            DrawTile_MTX(
                                Level!.TileMatrix[rootPosY, rootPosX, layer],
                                tile,
                                rootPosX,
                                rootPosY,
                                layer,
                                camera,
                                rt
                            );
                        }

                        for (var w = 0; w < tile.Size.Width; w++)
                        {
                            for (var h = 0; h < tile.Size.Height; h++)
                            {
                                var spec = tile.Specs[h, w, 0];

                                if (spec is not -1) delL[(mx + w, my + h)] = true;
                            }
                        }

                        break;
                    }

                }
            }
            break;
        
            case "Random Metals":
            {
                bool[,] waiting = new bool[Level!.Height, Level!.Width];
                bool[,] deleted = new bool[Level!.Height, Level!.Width];

                // Draw Chaotic Stone corners and platoform
                if (Configuration.MaterialFixes)
                {
                    var SmallMetalSlopeNE = Registry.Tiles!.Get("Small Metal Slope NE");
                    var SmallMetalSlopeNW = Registry.Tiles!.Get("Small Metal Slope NW");
                    var SmallMetalSlopeSW = Registry.Tiles!.Get("Small Metal Slope SW");
                    var SmallMetalSlopeSE = Registry.Tiles!.Get("Small Metal Slope SE");
                    var SmallMetalFloor = Registry.Tiles!.Get("Small Metal Floor");

                    for (var q = 0; q <= orderedTiles.Count; q++)
                    {
                        var queued = orderedTiles[^q];

                        switch (Level!.GeoMatrix[queued.y, queued.x, layer].Type)
                        {
                            case GeoType.SlopeNE:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMetalSlopeNE, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeNW:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMetalSlopeNW, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeES:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMetalSlopeSE, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeSW:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMetalSlopeSW, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.Platform:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMetalFloor, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.Solid:
                            waiting[queued.y, queued.x] = true;
                            continue;
                        }

                        orderedTiles.RemoveAt(orderedTiles.Count - q);
                        deleted[queued.y, queued.x] = true;
                    }
                }


                // You could probably turn them into hash sets.
                Dictionary<(int x, int y), bool> delL = [];
                Dictionary<(int x, int y), bool> blocks = [];

                foreach (var (_, mx, my) in orderedTiles) blocks[(mx, my)] = true;

                foreach (var (_, mx, my) in orderedTiles)
                {
                    if (delL.ContainsKey((mx, my))) continue;

                    List<(int rnd, TileDefinition tile)> randomMetals = State.randomMetalsPool
                        .Select(t => (Random.Generate(1000), t))
                        .ToList();

                    randomMetals.Sort((r1, r2) => {
                        if (r1.rnd >  r2.rnd) return  1;
                        if (r1.rnd <  r2.rnd) return -1;
                        if (r1.rnd == r2.rnd) return  0;
                        return 0;
                    });

                    foreach (var (_, tile) in randomMetals)
                    {
                        // Testing if we can place the tile.

                        var legal = true;

                        for (var sx = 0; sx < tile.Size.Width; sx++)
                        {
                            for (var sy = 0; sy < tile.Size.Height; sy++)
                            {
                                var tx = mx + sx;
                                var ty = my + sy;

                                var spec = tile.Specs[sy, sx, 0];

                                if (!blocks.ContainsKey((tx, ty)))
                                {
                                    legal = false;
                                    goto stop_testing;
                                }

                                if (spec is -1) continue;

                                if (delL.ContainsKey((tx, ty)))
                                {
                                    legal = false;
                                    goto stop_testing;
                                }

                                if (
                                    !Data.Utils.InBounds(Level!.GeoMatrix, tx, ty) ||
                                    spec != (int)Level!.GeoMatrix[ty, tx, layer].Type
                                ) {
                                    legal = false;
                                    goto stop_testing;
                                }
                            }
                        }

                        stop_testing:

                        if (!legal) continue;

                        // Determining the tile head position, I suppose.
                        var rootPosX = mx + (int)(tile.Size.Width /2f + 0.4999f) - 1;
                        var rootPosY = my + (int)(tile.Size.Height/2f + 0.4999f) - 1;

                        // Drawing the tile if it's in camera bounds.
                        if (CheckCollisionPointRec(new Vector2(rootPosX, rootPosY), new Rectangle(camera.Coords/20f, new Vector2(100, 60))))
                        {
                            DrawTile_MTX(
                                Level!.TileMatrix[rootPosY, rootPosX, layer],
                                tile,
                                rootPosX,
                                rootPosY,
                                layer,
                                camera,
                                rt
                            );
                        }

                        for (var w = 0; w < tile.Size.Width; w++)
                        {
                            for (var h = 0; h < tile.Size.Height; h++)
                            {
                                var spec = tile.Specs[h, w, 0];

                                if (spec is not -1) delL[(mx + w, my + h)] = true;
                            }
                        }

                        break;
                    }

                }
            }
            break;
        
            case "Dune Sand":
            {
                for (var index = 1; index < orderedTiles.Count; index++)
                {
                    var (_, tx, ty) = orderedTiles[^index];
                
                    if (
                        !(!Data.Utils.InBounds(Level!.GeoMatrix, tx, ty) ||
                        Level!.GeoMatrix[ty, tx, layer].Type is not GeoType.Solid)
                    )
                    {
                        DrawTile_MTX(
                            Level!.TileMatrix[ty, tx, layer],
                            State.sandPool[Random.Generate(State.sandPool.Length - 1)],
                            tx,
                            ty,
                            layer,
                            camera,
                            rt
                        );
                    }
                    
                    orderedTiles.RemoveAt(orderedTiles.Count - index);
                }
            }
            break;

            case "Temple Stone":
            {
                bool[,] waiting = new bool[Level!.Height, Level!.Width];
                bool[,] deleted = new bool[Level!.Height, Level!.Width];

                var TempleStoneSlopeNE = Registry.Tiles!.Get("Temple Stone Slope NE");
                var TempleStoneSlopeNW = Registry.Tiles!.Get("Temple Stone Slope NW");
                var TempleStoneSlopeSW = Registry.Tiles!.Get("Temple Stone Slope SW");
                var TempleStoneSlopeSE = Registry.Tiles!.Get("Temple Stone Slope SE");
                var TempleStoneFloor = Registry.Tiles!.Get("Temple Stone Floor");

                var bigNoSlopes = Registry.Tiles!.Get("Big Temple Stone No Slopes");
                var wideTempleStone = Registry.Tiles!.Get("Wide Temple Stone");

                for (var q = 0; q <= orderedTiles.Count; q++)
                {
                    var queued = orderedTiles[^q];

                    switch (Level!.GeoMatrix[queued.y, queued.x, layer].Type)
                    {
                        case GeoType.SlopeNE:
                        DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], TempleStoneSlopeNE, queued.x, queued.y, layer, camera, rt);
                        orderedTiles.Remove(queued);
                        deleted[queued.y, queued.x] = true;
                        break;

                        case GeoType.SlopeNW:
                        DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], TempleStoneSlopeNW, queued.x, queued.y, layer, camera, rt);
                        orderedTiles.Remove(queued);
                        deleted[queued.y, queued.x] = true;
                        break;

                        case GeoType.SlopeES:
                        DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], TempleStoneSlopeSE, queued.x, queued.y, layer, camera, rt);
                        orderedTiles.Remove(queued);
                        deleted[queued.y, queued.x] = true;
                        break;

                        case GeoType.SlopeSW:
                        DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], TempleStoneSlopeSW, queued.x, queued.y, layer, camera, rt);
                        orderedTiles.Remove(queued);
                        deleted[queued.y, queued.x] = true;
                        break;

                        case GeoType.Solid:
                        waiting[queued.y, queued.x] = true;
                        continue;

                        case GeoType.Glass:
                        if (Configuration.MaterialFixes) {
                            orderedTiles.Remove(queued);
                            deleted[queued.y, queued.x] = true;
                        }
                        break;
                    }
                }

                List<(int x, int y)> orderedTilesWithoutRND = orderedTiles.Select(o => (o.x, o.y)).ToList();
                List<(int x, int y)> duplicated = [..orderedTilesWithoutRND];
                List<(int x, int y)>[] corners = [ [], [], [], [] ];
            
                foreach (var t in duplicated)
                {
                    var (tx, ty) = t;

                    if ((tx % 6 == 0 && ty % 4 == 0) || (tx % 6 == 3 && ty % 4 == 2))
                    {
                        AttemptDrawTempleStone_MTX(
                            tx,
                            ty,
                            layer,
                            camera,
                            orderedTilesWithoutRND,
                            corners,
                            bigNoSlopes,
                            rt
                        );
                    }
                }

                for (var c = 1; c <= corners[0].Count; c++)
                {
                    var corner = corners[0][^c];

                    if (corners[2].Contains(corners[0][^c]))
                    {
                        orderedTilesWithoutRND.Remove(corners[0][^c]);
                    }
                }

                for (var c = 1; c <= corners[1].Count; c++)
                {
                    var corner = corners[1][^c];

                    if (corners[3].Contains(corners[1][^c]))
                    {
                        orderedTilesWithoutRND.Remove(corners[1][^c]);
                    }
                }

                while (orderedTilesWithoutRND.Count > 0)
                {
                    // Get a random item from the list
                    var t = orderedTilesWithoutRND[Random.Generate(orderedTilesWithoutRND.Count - 1)];

                    var drawn = false;

                    if (corners[0].Contains(t))
                    {
                        DrawTile_MTX(
                            Level!.TileMatrix[t.y, t.x, layer],
                            TempleStoneSlopeSE,
                            t.x,
                            t.y,
                            layer,
                            camera,
                            rt
                        );

                        drawn = true;
                    }
                    else if (corners[1].Contains(t))
                    {
                        DrawTile_MTX(
                            Level!.TileMatrix[t.y, t.x, layer],
                            TempleStoneSlopeSW,
                            t.x,
                            t.y,
                            layer,
                            camera,
                            rt
                        );

                        drawn = true;
                    }
                    else if (corners[2].Contains(t))
                    {
                        DrawTile_MTX(
                            Level!.TileMatrix[t.y, t.x, layer],
                            TempleStoneSlopeNW,
                            t.x,
                            t.y,
                            layer,
                            camera,
                            rt
                        );

                        drawn = true;
                    }
                    else if (corners[3].Contains(t))
                    {
                        DrawTile_MTX(
                            Level!.TileMatrix[t.y, t.x, layer],
                            TempleStoneSlopeNE,
                            t.x,
                            t.y,
                            layer,
                            camera,
                            rt
                        );

                        drawn = true;
                    }

                    if (!drawn)
                    {
                        
                    }
                }
            }
            break;
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

        var frontRT = LoadRenderTexture(Width, Height);
        var middleRT = LoadRenderTexture(Width, Height);
        var backRT = LoadRenderTexture(Width, Height);

        var poleColor = Color.Red;

        var camera = Level.Cameras[0];

        List<(int rnd, int x, int y)> drawLaterTiles = [];
        List<(int rnd, int x, int y)> drawLastTiles = [];
        List<(int rnd, int x, int y)> entrances = [];
        List<(int x, int y)> shortcuts = [];

        Dictionary<MaterialDefinition, List<(int rnd, int x, int y)>> drawMaterials = [];

        BeginTextureMode(middleRT);

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
                        DrawTile_MTX(queuedCell, queuedCell.TileDefinition, tile.x, tile.y, layer, camera, frontRT);
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
                    foreach (var q in queued) DrawMaterial_MTX(q.x, q.y, layer, camera, material, frontRT);
                }
                break;

                case MaterialRenderType.Unified:
                foreach (var q in queued) DrawMaterial_MTX(q.x, q.y, layer, camera, material, frontRT);
                break;

                case MaterialRenderType.CustomUnified:
                {
                    // To be implemented
                }
                break;

                case MaterialRenderType.Tiles:
                RenderTileMaterial_MTX(layer, camera, material, frontRT);
                break;
            }
        }

        UnloadRenderTexture(frontRT);
        UnloadRenderTexture(middleRT);
        UnloadRenderTexture(backRT);
    }
}