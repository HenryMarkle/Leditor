using ImGuiNET;
using rlImGui_cs;

using Raylib_cs;
using static Raylib_cs.Raylib;

using System.Numerics;

using Leditor.Data;
using Leditor.Data.Tiles;
using Leditor.Data.Geometry;

using Color = Raylib_cs.Color;
using Leditor.Data.Materials;
using Microsoft.Win32;
using Leditor.Renderer.RL;

namespace Leditor.Renderer;

/// <summary>
/// The main class responsible for rendering the whole level.
/// </summary>
public class Engine
{
    protected class RenderState
    {
        private bool Initialized { get; set; }
        public bool Disposed { get; private set; }

        public RenderTexture2D vertImg;
        public RenderTexture2D horiImg;

        public void Initialize(Registry registry)
        {
            if (Initialized) return;

            var lib = registry.CastLibraries.Single(l => l.Name == "levelEditor");

            var vTexture = lib.Members["vertImg"].Texture;
            var hTexture = lib.Members["horiImg"].Texture;

            vertImg = LoadRenderTexture(vTexture.Width, vTexture.Height);
            horiImg = LoadRenderTexture(hTexture.Width, hTexture.Height);

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

            Initialized = true;
        }

        public void Reset(Registry registry)
        {
            var lib = registry.CastLibraries.Single(l => l.Name == "levelEditor");

            var vTexture = lib.Members["vertImg"].Texture;
            var hTexture = lib.Members["horiImg"].Texture;

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
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;

            UnloadRenderTexture(vertImg);
            UnloadRenderTexture(horiImg);
        }
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

    public RenderTexture2D Canvas
    {
        get
        {
            Compose();
            return _canvas;
        }
    }

    /// <summary>
    /// The level's state is not managed by <see cref="Engine"/>
    /// </summary>
    public LevelState? Level { get; protected set; }

    public Serilog.ILogger? Logger { get; init; }

    public bool Disposed { get; protected set; }
    public bool Initialized { get; protected set; }

    protected RenderState State { get; set; }


    public Engine()
    {
        _layers = new RenderTexture2D[30];
        _layersDC = new RenderTexture2D[30];
        _gradientA = new RenderTexture2D[30];
        _gradientB = new RenderTexture2D[30];

        State = new();
    }

    ~Engine()
    {
        if (!Disposed) throw new InvalidOperationException("Engine was not disposed by consumer");
    }

    public void Initialize()
    {
        if (Initialized) return;

        for (var l = 0; l < 30; l++)
        {
            _layers[l] = LoadRenderTexture(Width, Height);
            _layersDC[l] = LoadRenderTexture(Width, Height);
            _gradientA[l] = LoadRenderTexture(Width, Height);
            _gradientB[l] = LoadRenderTexture(Width, Height);
        }

        _canvas = LoadRenderTexture(Width, Height);

        BeginTextureMode(_canvas);
        ClearBackground(Color.White);
        EndTextureMode();
    }

    protected void Compose()
    {
        var shader = Shaders.WhiteRemover;

        BeginTextureMode(_canvas);

        ClearBackground(Color.White);
        foreach (var layer in _layers)
        {
            BeginShaderMode(shader);
            SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), layer.Texture);

            DrawTexture(layer.Texture, 0, 0, Color.White);

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
    /// <param name="x">X coordinates relative to the canvas</param>
    /// <param name="y">Y coordinates relative to the canvas</param>
    /// <param name="layer">The current layer (0, 1, 2)</param>
    /// <param name="camera">The current render camera</param>
    /// <param name="rt">the temprary canvas to draw on</param>
    protected virtual void DrawTile_MTX(
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
                    for (var repeat = 0; repeat < tile.Repeat[l]; l++)
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

        }
    }

    /// <summary>
    /// This gets called each frame
    /// </summary>
    public virtual void Render()
    {
        if (Level is null || !Initialized) return;

        var layer = 2;

        var frontRT = LoadRenderTexture(Width, Height);
        var middleRT = LoadRenderTexture(Width, Height);
        var backRT = LoadRenderTexture(Width, Height);

        var poleColor = Color.Red;

        var camera = Level.Cameras[0];

        List<(int rnd, int x, int y)> drawLaterTiles = [];
        List<(int rnd, int x, int y)> drawLastTiles = [];
        List<(int rnd, int x, int y)> entrances = [];
        List<(int x, int y)> shortcuts = [];

        Dictionary<string, List<(int rnd, int x, int y)>> drawMaterials = [];

        BeginTextureMode(middleRT);

        // Setting drawing order
        for (var x = 0; x < Columns; x++)
        {
            for (var y = 0; y < Rows; y++)
            {
                // Acquire absolute position
                var mx = x + (int)camera.Coords.X;
                var my = y + (int)camera.Coords.Y;
            
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
                        drawLastTiles.Add((Random.Generate(999), mx, my));
                    }
                }
                else if (tileCell.Type is not TileCellType.Body)
                {
                    drawLastTiles.Add((Random.Generate(999), mx, my));
                }
            }
        }

        drawLastTiles.Sort();

        EndTextureMode();


        // Draw
        foreach (var tile in drawLastTiles)
        {
            // var tileSeed = GenSeedForTile(tile.x, tile.y, Level.Width, Level.Seed + layer); // unuse?
            ref var queuedCell = ref Level.TileMatrix[tile.y, tile.x, layer];

            switch (queuedCell.Type)
            {
                case TileCellType.Material:
                {
                    if (queuedCell.MaterialDefinition is not null)
                    {
                        if (drawMaterials.TryGetValue(queuedCell.MaterialDefinition.Name, out var list))
                        {
                            list.Add(tile);
                        }
                        else
                        {
                            drawMaterials[queuedCell.MaterialDefinition.Name] = [ tile ];
                        }
                    }
                    break;
                }

                case TileCellType.Default:
                {
                    if (drawMaterials.TryGetValue(Level.DefaultMaterial.Name, out var list))
                    {
                        list.Add(tile);
                    }
                    else
                    {
                        drawMaterials[Level.DefaultMaterial.Name] = [ tile ];
                    }
                }
                    break;
                
                case TileCellType.Head:
                {
                    if (queuedCell.TileDefinition is not null)
                    {
                        DrawTile_MTX(queuedCell.TileDefinition, tile.x, tile.y, layer, camera, frontRT);
                    }
                }
                break;
            }
        }


        UnloadRenderTexture(frontRT);
        UnloadRenderTexture(middleRT);
        UnloadRenderTexture(backRT);
    }
}