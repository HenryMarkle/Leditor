using static Raylib_CsLo.Raylib;

using System.Numerics;

using Quads = (
    System.Numerics.Vector2 topLeft, 
    System.Numerics.Vector2 topRight, 
    System.Numerics.Vector2 bottomRight, 
    System.Numerics.Vector2 bottomLeft
    );

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

        public Texture[] GeoMenu { get; set; } = [];
        public Texture[] GeoBlocks { get; set; } = [];
        public Texture[] GeoStackables { get; set; } = [];
        public Texture[] LightBrushes { get; set; } = [];
        public Texture[][] Tiles { get; set; } = [];
        public Texture[][] Props { get; set; } = [];
        public Texture[] PropMenuCategories { get; set; } = [];
        public Texture[] PropModes { get; set; } = [];

        public RenderTexture LightMap { get; set; }
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
        internal static string TilesInitPath => Path.Combine(IndexDirectory, "tiles.txt");
        internal static string MaterialsInitPath => Path.Combine(IndexDirectory, "materials.txt");
        internal static string EffectsInitPath => Path.Combine(IndexDirectory, "effects.txt");
        internal static string PropsInitPath => Path.Combine(IndexDirectory, "props.txt");

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
        internal int Width { get; private set; }
        internal int Height { get; private set; }

        internal (int left, int top, int right, int bottom) Padding { get; private set; }
        internal Rectangle Border { get; private set; }
        internal int WaterLevel { get; set; } = -1;
        internal bool WaterAtFront { get; set; } = false;

        internal List<RenderCamera> Cameras { get; set; } = [];

        internal RunCell[,,] GeoMatrix { get; private set; } = new RunCell[0, 0, 0];
        internal TileCell[,,] TileMatrix { get; private set; } = new TileCell[0, 0, 0];
        internal Color[,,] MaterialColors { get; private set; } = new Color[0, 0, 0];
        internal (string, EffectOptions, double[,])[] Effects { get; set; } = [];
        internal (InitPropType type, (int category, int index) position, Prop prop)[] Props { get; set; } = [];

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
            (string, EffectOptions, double[,])[] effects,
            List<RenderCamera> cameras,
            (InitPropType type, (int category, int index) position, Prop prop)[] props
        )
        {
            Width = width;
            Height = height;
            Padding = padding;
            GeoMatrix = geoMatrix;
            TileMatrix = tileMatrix;
            MaterialColors = materialColorMatrix;
            Effects = effects;

            Cameras = cameras;
            Props = props;

            Border = new(
                Padding.left * Scale,
                Padding.top * Scale,
                (Width - (Padding.right + Padding.left)) * Scale,
                (Height - (Padding.bottom + Padding.top)) * Scale
            );
        }

        internal void New(
            int width,
            int height,
            (int left, int top, int right, int bottom) padding,
            Span<int> geoIdFill)
        {
            Width = width;
            Height = height;
            Padding = padding;

            Border = new(
                Padding.left * Scale,
                Padding.top * Scale,
                (Width - (Padding.right + Padding.left)) * Scale,
                (Height - (Padding.bottom + Padding.top)) * Scale
            );

            Cameras = [new RenderCamera() { Coords = (20f, 30f), Quads = new(new(), new(), new(), new()) }];

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
                    new RunCell() { Geo = geoIdFill[0], Stackables = [] },
                    new RunCell() { Geo = geoIdFill[1], Stackables = [] },
                    new RunCell() { Geo = geoIdFill[2], Stackables = [] }
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

            Border = new(
                Padding.left * Scale,
                Padding.top * Scale,
                (Width - (Padding.right + Padding.left)) * Scale,
                (Height - (Padding.bottom + Padding.top)) * Scale
            );
        }

        internal (int category, int index)? PickupTile(int x, int y, int z)
        {
            var cell = TileMatrix[y, x, z];

            if (cell.Type == TileType.TileHead)
            {
                var (category, index, _) = ((TileHead)cell.Data).CategoryPostition;
                return (category - 5, index - 1);
            }
            else if (cell.Type == TileType.TileBody)
            {
                // find where the head is


                var (headX, headY, headZ) = ((TileBody)cell.Data).HeadPosition;
                // This is done because Lingo is 1-based index
                var supposedHead = TileMatrix[headY - 1, headX - 1, headZ - 1];

                if (supposedHead.Type != TileType.TileHead) return null;

                var headTile = (TileHead)supposedHead.Data;
                return (headTile.CategoryPostition.Item1 - 5, headTile.CategoryPostition.Item2 - 1);
            }


            return null;
        }
        internal (int category, int index)? PickupMaterial(int x, int y, int z)
        {
            var cell = TileMatrix[y, x, z];

            if (cell.Type == TileType.Material)
            {
                for (int c = 0; c < Materials.Length; c++)
                {
                    for (int i = 0; i < Materials[c].Length; i++)
                    {
                        if (Materials[c][i].Item1 == ((TileMaterial)cell.Data).Name) return (c, i);
                    }
                }

                return null;
            }

            return null;
        }
        internal bool IsTileLegal(ref InitTile init, System.Numerics.Vector2 point)
        {
            var (width, height) = init.Size;
            var specs = init.Specs;
            var specs2 = init.Specs2;

            // get the "middle" point of the tile
            var head = Utils.GetTileHeadOrigin(ref init);

            // the top-left of the tile
            var start = RayMath.Vector2Subtract(point, head);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int matrixX = x + (int)start.X;
                    int matrixY = y + (int)start.Y;

                    // This function depends on the rest of the program to guarantee that all level matrices have the same x and y dimensions
                    if (
                        matrixX >= 0 &&
                        matrixX < GeoMatrix.GetLength(1) &&
                        matrixY >= 0 &&
                        matrixY < GeoMatrix.GetLength(0)
                    )
                    {
                        var tileCell = TileMatrix[matrixY, matrixX, Layer];
                        var geoCell = GeoMatrix[matrixY, matrixX, Layer];
                        var specsIndex = (x * height) + y;


                        var spec = specs[specsIndex];

                        bool isLegal = false;

                        if (specs2.Length > 0 && Layer != 2)
                        {
                            var tileCellNextLayer = TileMatrix[matrixY, matrixX, Layer + 1];
                            var geoCellNextLayer = GeoMatrix[matrixY, matrixX, Layer + 1];

                            var spec2 = specs2[specsIndex];

                            isLegal =
                                (tileCell.Type == TileType.Default || tileCell.Type == TileType.Material)
                                &&
                                (tileCellNextLayer.Type == TileType.Default || tileCellNextLayer.Type == TileType.Material)
                                &&
                                (spec == -1 || geoCell.Geo == spec)
                                &&
                                (spec2 == -1 || geoCellNextLayer.Geo == spec2);
                        }
                        else
                        {
                            isLegal = (tileCell.Type == TileType.Default || tileCell.Type == TileType.Material) && (spec == -1 || geoCell.Geo == spec);
                        }

                        if (!isLegal) return false;
                    }
                    else return false;

                }
            }

            return true;
        }
        internal void ForcePlaceTileWithGeo(
            ref InitTile init,
            int tileCategoryIndex,
            int tileIndex,
            (int x, int y, int z) matrixPosition
        )
        {
            var (mx, my, mz) = matrixPosition;
            var (width, height) = init.Size;
            var specs = init.Specs;
            var specs2 = init.Specs2;

            // get the "middle" point of the tile
            var head = Utils.GetTileHeadOrigin(ref init);

            // the top-left of the tile
            var start = RayMath.Vector2Subtract(new(mx, my), head);

            // first: place the head of the tile at matrixPosition
            TileMatrix[my, mx, mz] = new TileCell()
            {
                Type = TileType.TileHead,
                Data = new TileHead(tileCategoryIndex, tileIndex, init.Name)
            };

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // leave the newly placed tile head
                    if (x == (int)head.X && y == (int)head.Y) continue;

                    int matrixX = x + (int)start.X;
                    int matrixY = y + (int)start.Y;

                    // This function depends on the rest of the program to guarantee that all level matrices have the same x and y dimensions
                    if (
                        matrixX >= 0 &&
                        matrixX < GeoMatrix.GetLength(1) &&
                        matrixY >= 0 &&
                        matrixY < GeoMatrix.GetLength(0)
                    )
                    {
                        var specsIndex = (x * height) + y;

                        var spec = specs[specsIndex];
                        var spec2 = specs2.Length > 0 ? specs2[specsIndex] : -1;

                        if (spec != -1) GeoMatrix[matrixY, matrixX, mz].Geo = spec;
                        if (spec2 != -1 && mz != 2) GeoMatrix[matrixY, matrixX, mz + 1].Geo = spec2;

                        TileMatrix[matrixY, matrixX, mz] = new TileCell
                        {
                            Type = TileType.TileBody,
                            Data = new TileBody(mx + 1, my + 1, mz + 1) // <- Indices are incremented by 1 because Lingo is 1-based indexed
                        };

                        if (specs2.Length > 0 && mz != 2)
                        {
                            TileMatrix[matrixY, matrixX, mz + 1] = new TileCell
                            {
                                Type = TileType.TileBody,
                                Data = new TileBody(mx + 1, my + 1, mz + 1) // <- Indices are incremented by 1 because Lingo is 1-based indexed
                            };
                        }
                    }
                }
            }
        }

        internal void RemoveTile(int mx, int my, int mz)
        {
            var cell = TileMatrix[my, mx, mz];

            if (cell.Type == TileType.TileHead)
            {
                //Console.WriteLine($"Deleting tile head at ({mx},{my},{mz})");
                var data = (TileHead)cell.Data;
                var tileInit = Tiles[data.CategoryPostition.Item1 - 5][data.CategoryPostition.Item2 - 1];
                var (width, height) = tileInit.Size;

                bool isThick = tileInit.Specs2.Length > 0;

                // get the "middle" point of the tile
                var head = Utils.GetTileHeadOrigin(ref tileInit);

                // the top-left of the tile
                var start = RayMath.Vector2Subtract(new(mx, my), head);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int matrixX = x + (int)start.X;
                        int matrixY = y + (int)start.Y;

                        TileMatrix[matrixY, matrixX, mz] = new TileCell { Type = TileType.Default, Data = new TileDefault() };
                        if (isThick && mz != 2) TileMatrix[matrixY, matrixX, mz + 1] = new TileCell { Type = TileType.Default, Data = new TileDefault() };
                    }
                }
            }
            else if (cell.Type == TileType.TileBody)
            {
                var (headX, headY, headZ) = ((TileBody)cell.Data).HeadPosition;

                // This is done because Lingo is 1-based index
                var supposedHead = TileMatrix[headY - 1, headX - 1, headZ - 1];

                // if the head was not found, only delete the given tile body
                if (supposedHead.Type != TileType.TileHead)
                {
                    //Console.WriteLine($"({mx}, {my}, {mz}) reported that ({headX}, {headY}, {headZ}) is supposed to be a tile head, but was found to be a body");
                    TileMatrix[my, mx, mz] = new TileCell() { Type = TileType.Default, Data = new TileDefault() };
                    return;
                }

                var headTile = (TileHead)supposedHead.Data;
                var tileInit = Tiles[headTile.CategoryPostition.Item1 - 5][headTile.CategoryPostition.Item2 - 1];
                var (width, height) = tileInit.Size;

                bool isThick = tileInit.Specs2.Length > 0;

                // get the "middle" point of the tile
                var head = Utils.GetTileHeadOrigin(ref tileInit);

                // the top-left of the tile
                var start = RayMath.Vector2Subtract(new(headX, headY), RayMath.Vector2AddValue(head, 1));

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int matrixX = x + (int)start.X;
                        int matrixY = y + (int)start.Y;

                        TileMatrix[matrixY, matrixX, mz] = new TileCell { Type = TileType.Default, Data = new TileDefault() };
                        if (isThick && mz != 2) TileMatrix[matrixY, matrixX, mz + 1] = new TileCell { Type = TileType.Default, Data = new TileDefault() };
                    }
                }
            }
        }

        internal void PlaceMaterial((string name, Color color) material, (int x, int y, int z) position)
        {
            var (x, y, z) = position;
            var cell = TileMatrix[y, x, z];

            if (cell.Type != TileType.Default && cell.Type != TileType.Material) return;

            cell.Type = TileType.Material;
            cell.Data = new TileMaterial(material.name);

            TileMatrix[y, x, z] = cell;
            MaterialColors[y, x, z] = material.color;
        }
        internal void RemoveMaterial(int x, int y, int z)
        {
            var cell = TileMatrix[y, x, z];

            if (cell.Type != TileType.Material) return;

            cell.Type = TileType.Default;
            cell.Data = new TileDefault();

            TileMatrix[y, x, z] = cell;
        }
    }

    internal static string ProjectName { get; set; } = "New Project";
    
    // TODO: decide whether to move to LevelState
    internal static int Seed { get; set; } = new Random().Next(1000);
    internal static bool LightMode { get; set; } = true;
    internal static bool DefaultTerrian { get; set; } = true;

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

    internal static bool CamScaleMode { get; set; } = false;

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

    /// Embedded rope prop definitions
    private static InitRopeProp[] RopeProps { get; } =
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
    private static InitLongProp[] LongProps { get; } = 
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
    internal static InitPropBase[][] Props { get; set; } = [ [..RopeProps], [..LongProps] ];
    
    internal static string[] MaterialCategories => [
        "Materials",
        "Drought Materials",
        "Community Materials"
    ];
    
    /// Embedded material definitions
    internal static (string, Color)[][] Materials => [
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
    
    internal static Settings Settings { get; set; } = new(
            false,
            new(
                new(),
                new(),
                new(),
                new()
                ),
            new(),
            new(
                new(
                    layer1: new(0, 0, 0, 255),
                    layer2: new(0, 255, 0, 50),
                    layer3: new(255, 0, 0, 50)
                    )
                ),
            new(true),
            new(background: new(66, 108, 245, 255)),
            new()
    );
    
    #nullable enable
    
    /// Used when loading a level
    internal static Task<TileCheckResult>? TileCheck { get; set; } = null;
    
    /// Used when loading a level
    internal static Task<PropCheckResult>? PropCheck { get; set; } = null;
#nullable disable
}

/// A collection of helper functions used across pages
internal static class Utils
{
    private static int GetMiddle(int number)
    {
        if (number < 3) return 0;
        if (number % 2 == 0) return number / 2 - 1;
        return number / 2;
    }

    /// <summary>
    /// Determines the "middle" of a tile, which where the tile head is positioned.
    /// </summary>
    /// <param name="init">a reference to the tile definition</param>
    internal static System.Numerics.Vector2 GetTileHeadOrigin(ref InitTile init)
    {
        var (width, height) = init.Size;
        return new System.Numerics.Vector2(GetMiddle(width), GetMiddle(height));
    }

    /// Maps a geo block id to a block texture index
    public static int GetBlockIndex(int id) => id switch
    {
        1 => 0,
        2 => 1,
        3 => 2,
        4 => 3,
        5 => 4,
        6 => 5,
        7 => 6,
        9 => 7,

        _ => -1,
    };

    /// Maps a UI texture index to block ID
    public static int GetBlockID(uint index) => index switch
    {
        0 => 1,
        1 => 0,
        7 => 4,
        3 => 2,
        2 => 3,
        6 => 6,
        30 => 9,
        _ => -1
    };


    /// Maps a UI texture index to a stackable ID
    public static int GetStackableID(uint index) => index switch
    {
        28 => 9,
        29 => 10,
        9 => 11,
        10 => 1,
        11 => 2,
        14 => 4,
        15 => 5,
        16 => 7,
        17 => 6,
        18 => 3,
        19 => 18,
        20 => 21,
        21 => 19,
        22 => 13,
        23 => 20,
        24 => 12,

        _ => -1
    };

    public static int GetStackableTextureIndex(int id) => id switch
    {
        1 => 0,
        2 => 1,
        3 => 2,
        5 => 3,
        6 => 27,
        7 => 28,
        9 => 18,
        10 => 29,
        12 => 30,
        13 => 16,
        18 => 19,
        19 => 20,
        20 => 21,
        21 => 17,
        _ => -1
    };

    /// <summary>
    /// This is used to determine the index of the stackable texture, including the directional ones.
    /// </summary>
    /// <param name="id">the ID of the geo-tile feature</param>
    /// <param name="context">a 3x3 slice of the geo-matrix where the geo-tile feature is in the middle</param>
    /// <returns>the index of the texture in the textures array (GLOBALS.Textures.GeoStackables)</returns>
    public static int GetStackableTextureIndex(int id, RunCell[][] context)
    {
        var i = id switch
        {
            1 => 0,
            2 => 1,
            3 => 2,
            4 => -4,
            5 => 3,
            6 => 27,
            7 => 28,
            9 => 18,
            10 => 29,
            11 => -11,
            12 => 30,
            13 => 16,
            18 => 19,
            19 => 20,
            20 => 21,
            21 => 17,
            _ => -1
        };


        if (i == -4)
        {
            if (
                context[0][0].Stackables[4] || context[0][1].Stackables[4] || context[0][2].Stackables[4] ||
                context[1][0].Stackables[4] || context[1][2].Stackables[4] ||
                context[2][0].Stackables[4] || context[2][1].Stackables[4] || context[2][2].Stackables[4]
            ) return 26;

            var pattern = (
                false, context[0][1].Stackables[5] ^ context[0][1].Stackables[6] ^ context[0][1].Stackables[7] ^ context[0][1].Stackables[19] ^ context[0][1].Stackables[21], false,
                context[1][0].Stackables[5] ^ context[1][0].Stackables[6] ^ context[1][0].Stackables[7] ^ context[1][0].Stackables[19] ^ context[1][0].Stackables[21], false, context[1][2].Stackables[5] ^ context[1][2].Stackables[6] ^ context[1][2].Stackables[7] ^ context[1][2].Stackables[19] ^ context[1][2].Stackables[21],
                false, context[2][1].Stackables[5] ^ context[2][1].Stackables[6] ^ context[2][1].Stackables[7] ^ context[2][1].Stackables[19] ^ context[2][1].Stackables[21], false
            );

            var directionIndex = pattern switch
            {

                (
                    _, true, _,
                    false, _, false,
                    _, false, _
                ) => 25,

                (
                    _, false, _,
                    false, _, true,
                    _, false, _
                ) => 24,

                (
                    _, false, _,
                    false, _, false,
                    _, true, _
                ) => 22,

                (
                    _, false, _,
                    true, _, false,
                    _, false, _
                ) => 23,

                _ => 26
            };

            if (directionIndex == 26) return 26;

            var geoPattern = (
                context[0][0].Geo, context[0][1].Geo, context[0][2].Geo,
                context[1][0].Geo, 0, context[1][2].Geo,
                context[2][0].Geo, context[2][1].Geo, context[2][2].Geo
            );

            directionIndex = geoPattern switch
            {

                (
                    1, _, 1,
                    1, _, 1,
                    1, 1, 1
                ) => context[0][1].Geo is 0 or 6 ? directionIndex : 26,

                (
                    1, 1, 1,
                    1, _, _,
                    1, 1, 1
                ) => context[1][2].Geo is 0 or 6 ? directionIndex : 26,

                (
                    1, 1, 1,
                    1, _, 1,
                    1, _, 1
                ) => context[2][1].Geo is 0 or 6 ? directionIndex : 26,

                (
                    1, 1, 1,
                    _, _, 1,
                    1, 1, 1
                ) => context[1][0].Geo is 0 or 6 ? directionIndex : 26,

                _ => 26
            };

            return directionIndex;
        }
        else if (i == -11)
        {
            i = (
                false, context[0][1].Stackables[11], false,
                context[1][0].Stackables[11], false, context[1][2].Stackables[11],
                false, context[2][1].Stackables[11], false
            ) switch
            {

                (
                    _, true, _,
                    false, _, false,
                    _, false, _
                ) => 33,

                (
                    _, false, _,
                    false, _, true,
                    _, false, _
                ) => 32,

                (
                    _, false, _,
                    false, _, false,
                    _, true, _
                ) => 31,

                (
                    _, false, _,
                    true, _, false,
                    _, false, _
                ) => 34,

                //

                (
                    _, true, _,
                    false, _, true,
                    _, false, _
                ) => 13,

                (
                    _, false, _,
                    false, _, true,
                    _, true, _
                ) => 5,

                (
                    _, false, _,
                    true, _, false,
                    _, true, _
                ) => 4,

                (
                    _, true, _,
                    true, _, false,
                    _, false, _
                ) => 10,

                //

                (
                    _, true, _,
                    true, _, true,
                    _, false, _
                ) => 12,

                (
                    _, true, _,
                    false, _, true,
                    _, true, _
                ) => 9,

                (
                    _, false, _,
                    true, _, true,
                    _, true, _
                ) => 8,

                (
                    _, true, _,
                    true, _, false,
                    _, true, _
                ) => 11,

                //

                (
                    _, false, _,
                    true, _, true,
                    _, false, _
                ) => 7,

                (
                    _, true, _,
                    false, _, false,
                    _, true, _
                ) => 15,

                //

                (
                    _, true, _,
                    true, _, true,
                    _, true, _
                ) => 6,

                (
                    _, false, _,
                    false, _, false,
                    _, false, _
                ) => (
                false, context[0][1].Geo == 1, false,
                context[1][0].Geo == 1, context[1][1].Geo == 1, context[1][2].Geo == 1,
                false, context[2][0].Geo == 1, false
                ) switch
                {
                    (
                    _, false, _,
                    true, true, true,
                    _, false, _
                    ) => 15,
                    (
                    _, true, _,
                    false, true, false,
                    _, true, _
                    ) => 7,
                    _ => 14
                }
            };
        }

        return i;
    }


    /// <summary>
    /// Determines the id (direction) of the slope depending on the surrounding geos.
    /// </summary>
    /// <param name="context">a 3x3 slice of the geo-matrix where the slope is in the middle</param>
    /// <returns>the ID of the slope representing the proper direction</returns>
    public static int GetCorrectSlopeID(RunCell[][] context)
    {
        return (
            false, context[0][1].Geo == 1, false,
            context[1][0].Geo == 1, false, context[1][2].Geo == 1,
            false, context[2][1].Geo == 1, false
        ) switch
        {
            (
                _, false, _,
                true, _, false,
                _, true, _
            ) => 2,
            (
                _, false, _,
                false, _, true,
                _, true, _
            ) => 3,
            (
                _, true, _,
                true, _, false,
                _, false, _
            ) => 4,
            (
                _, true, _,
                false, _, true,
                _, false, _
            ) => 5,

            _ => -1

        };
    }

    internal static RunCell[,,] Resize(RunCell[,,] array, int width, int height, int newWidth, int newHeight, RunCell[] layersFill)
    {

        RunCell[,,] newArray = new RunCell[newHeight, newWidth, 3];

        if (height > newHeight)
        {
            if (width > newWidth)
            {
                for (int y = 0; y < newHeight; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        newArray[y, x, 0] = array[y, x, 0];
                        newArray[y, x, 1] = array[y, x, 1];
                        newArray[y, x, 2] = array[y, x, 2];
                    }
                }
            }
            else
            {
                for (int y = 0; y < newHeight; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        newArray[y, x, 0] = array[y, x, 0];
                        newArray[y, x, 1] = array[y, x, 1];
                        newArray[y, x, 2] = array[y, x, 2];
                    }

                    for (int x = width; x < newWidth; x++)
                    {
                        newArray[y, x, 0] = layersFill[0];
                        newArray[y, x, 1] = layersFill[1];
                        newArray[y, x, 2] = layersFill[2];
                    }
                }
            }

        }
        else
        {
            if (width > newWidth)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        newArray[y, x, 0] = array[y, x, 0];
                        newArray[y, x, 1] = array[y, x, 1];
                        newArray[y, x, 2] = array[y, x, 2];
                    }
                }

                for (int y = height; y < newHeight; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        newArray[y, x, 0] = layersFill[0];
                        newArray[y, x, 1] = layersFill[1];
                        newArray[y, x, 2] = layersFill[2];
                    }
                }
            }
            else
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        newArray[y, x, 0] = array[y, x, 0];
                        newArray[y, x, 1] = array[y, x, 1];
                        newArray[y, x, 2] = array[y, x, 2];
                    }

                    for (int x = width; x < newWidth; x++)
                    {
                        newArray[y, x, 0] = layersFill[0];
                        newArray[y, x, 1] = layersFill[1];
                        newArray[y, x, 2] = layersFill[2];
                    }
                }

                for (int y = height; y < newHeight; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        newArray[y, x, 0] = layersFill[0];
                        newArray[y, x, 1] = layersFill[1];
                        newArray[y, x, 2] = layersFill[2];
                    }
                }
            }
        }

        return newArray;
    }

    internal static void Resize((string, EffectOptions, double[,])[] list, int width, int height, int newWidth, int newHeight)
    {
        for (int i = 0; i < list.Length; i++)
        {
            var array = list[i].Item3;
            var newArray = new double[newHeight, newWidth];

            if (height > newHeight)
            {
                if (width > newWidth)
                {
                    for (int y = 0; y < newHeight; y++)
                    {
                        for (int x = 0; x < newWidth; x++)
                        {
                            newArray[y, x] = array[y, x];
                            newArray[y, x] = array[y, x];
                            newArray[y, x] = array[y, x];
                        }
                    }
                }
                else
                {
                    for (int y = 0; y < newHeight; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            newArray[y, x] = array[y, x];
                            newArray[y, x] = array[y, x];
                            newArray[y, x] = array[y, x];
                        }

                        for (int x = width; x < newWidth; x++)
                        {
                            newArray[y, x] = 0;
                            newArray[y, x] = 0;
                            newArray[y, x] = 0;
                        }
                    }
                }

            }
            else
            {
                if (width > newWidth)
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < newWidth; x++)
                        {
                            newArray[y, x] = array[y, x];
                            newArray[y, x] = array[y, x];
                            newArray[y, x] = array[y, x];
                        }
                    }

                    for (int y = height; y < newHeight; y++)
                    {
                        for (int x = 0; x < newWidth; x++)
                        {
                            newArray[y, x] = 0;
                            newArray[y, x] = 0;
                            newArray[y, x] = 0;
                        }
                    }
                }
                else
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            newArray[y, x] = array[y, x];
                            newArray[y, x] = array[y, x];
                            newArray[y, x] = array[y, x];
                        }

                        for (int x = width; x < newWidth; x++)
                        {
                            newArray[y, x] = 0;
                            newArray[y, x] = 0;
                            newArray[y, x] = 0;
                        }
                    }

                    for (int y = height; y < newHeight; y++)
                    {
                        for (int x = 0; x < newWidth; x++)
                        {
                            newArray[y, x] = 0;
                            newArray[y, x] = 0;
                            newArray[y, x] = 0;
                        }
                    }
                }
            }
            list[i].Item3 = newArray;
        }
    }

    internal static RunCell[,,] NewGeoMatrix(int width, int height, int geoFill = 0)
    {
        RunCell[,,] matrix = new RunCell[height, width, 3];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                matrix[y, x, 0] = new() { Geo = geoFill, Stackables = new bool[22] };
                matrix[y, x, 1] = new() { Geo = geoFill, Stackables = new bool[22] };
                matrix[y, x, 2] = new() { Geo = 0, Stackables = new bool[22] };
            }
        }

        return matrix;
    }

    internal static Color[,,] NewMaterialColorMatrix(int width, int height, Color @default)
    {
        Color[,,] matrix = new Color[height, width, 3];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                matrix[y, x, 0] = @default;
                matrix[y, x, 1] = @default;
                matrix[y, x, 2] = @default;
            }
        }

        return matrix;
    }

    /// <summary>
    /// Retrieves a 3x3 slide from the geo-matrix.
    /// </summary>
    /// <param name="matrix">the geo-matrix (GLOBALS.Level.GeoMatrix)</param>
    /// <param name="width">the width of the matrix</param>
    /// <param name="height">the height of the matrix</param>
    /// <param name="x">x-position of the middle of the slice</param>
    /// <param name="y">y-position of the middle of the slice</param>
    /// <param name="z">z-position of the middle of the slice</param>
    internal static RunCell[][] GetContext(RunCell[,,] matrix, int width, int height, int x, int y, int z) =>
        [
            [
                x > 0 && y > 0 ? matrix[y - 1, x - 1, z] : new(),
                y > 0 ? matrix[y - 1, x, z] : new(),
                x < width - 1 && y > 0 ? matrix[y - 1, x + 1, z] : new()
            ],
            [
                x > 0 ? matrix[y, x - 1, z] : new(),
                matrix[y, x, z],
                x < width - 1 ? matrix[y, x + 1, z] : new(),
            ],
            [
                x > 0 && y < height - 1 ? matrix[y + 1, x - 1, z] : new(),
                y < height - 1 ? matrix[y + 1, x, z] : new(),
                x < width - 1 && y < height - 1 ? matrix[y + 1, x + 1, z] : new()
            ]
        ];
    
    /// <summary>
    /// Retrieves a 3x3 slide from a temporary 0-depth geo-matrix slice (for copy-paste feature).
    /// </summary>
    /// <param name="matrix">the geo-matrix (GLOBALS.Level.GeoMatrix)</param>
    /// <param name="x">x-position of the middle of the slice</param>
    /// <param name="y">y-position of the middle of the slice</param>
    internal static RunCell[][] GetContext(RunCell[,] matrix, int x, int y) =>
        [
            [
                x > 0 && y > 0 ? matrix[y - 1, x - 1] : new(),
                y > 0 ? matrix[y - 1, x] : new(),
                x < matrix.GetLength(1) - 1 && y > 0 ? matrix[y - 1, x + 1] : new()
            ],
            [
                x > 0 ? matrix[y, x - 1] : new(),
                matrix[y, x],
                x < matrix.GetLength(1) - 1 ? matrix[y, x + 1] : new(),
            ],
            [
                x > 0 && y < matrix.GetLength(0) - 1 ? matrix[y + 1, x - 1] : new(),
                y < matrix.GetLength(0) - 1 ? matrix[y + 1, x] : new(),
                x < matrix.GetLength(1) - 1 && y < matrix.GetLength(0) - 1 ? matrix[y + 1, x + 1] : new()
            ]
        ];


    /// Meaningless name; this function turns a sequel of stackable IDs to an array that can be used at leditor runtime
    internal static bool[] DecomposeStackables(IEnumerable<int> seq)
    {
        bool[] bools = new bool[22];

        foreach (var i in seq) bools[i] = true;

        return bools;
    }

    /// <summary>
    /// Generic resize method of a 3D array (with the z dimension being exactly 3).
    /// </summary>
    /// <param name="array">The matrix</param>
    /// <param name="newWidth">new matrix width</param>
    /// <param name="newHeight">new matrix height</param>
    /// <param name="layersFill"></param>
    /// <typeparam name="T">a 3-length list (representing the three level layers) of geo IDs to fill extra space with</typeparam>
    /// <returns>a new matrix with <paramref name="newWidth"/> and <paramref name="newHeight"/> as the new dimensions</returns>
    internal static T[,,] Resize<T>(T[,,] array, int newWidth, int newHeight, ReadOnlySpan<T> layersFill)
        where T : notnull, new()
    {
        var width = array.GetLength(1);
        var height = array.GetLength(0);
        
        var newArray = new T[newHeight, newWidth, 3];

        // old height is larger
        if (height > newHeight)
        {
            if (width > newWidth)
            {
                for (int y = 0; y < newHeight; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        newArray[y, x, 0] = array[y, x, 0];
                        newArray[y, x, 1] = array[y, x, 1];
                        newArray[y, x, 2] = array[y, x, 2];
                    }
                }
            }
            else
            {
                for (int y = 0; y < newHeight; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        newArray[y, x, 0] = array[y, x, 0];
                        newArray[y, x, 1] = array[y, x, 1];
                        newArray[y, x, 2] = array[y, x, 2];
                    }

                    for (int x = width; x < newWidth; x++)
                    {
                        newArray[y, x, 0] = layersFill[0];
                        newArray[y, x, 1] = layersFill[1];
                        newArray[y, x, 2] = layersFill[2];
                    }
                }
            }

        }
        // new height is larger or equal
        else
        {
            if (width > newWidth)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        newArray[y, x, 0] = array[y, x, 0];
                        newArray[y, x, 1] = array[y, x, 1];
                        newArray[y, x, 2] = array[y, x, 2];
                    }
                }

                for (int y = height; y < newHeight; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        newArray[y, x, 0] = layersFill[0];
                        newArray[y, x, 1] = layersFill[1];
                        newArray[y, x, 2] = layersFill[2];
                    }
                }
            }
            // new width is larger
            else
            {

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        newArray[y, x, 0] = array[y, x, 0];
                        newArray[y, x, 1] = array[y, x, 1];
                        newArray[y, x, 2] = array[y, x, 2];
                    }

                    for (int x = width; x < newWidth; x++)
                    {
                        newArray[y, x, 0] = layersFill[0];
                        newArray[y, x, 1] = layersFill[1];
                        newArray[y, x, 2] = layersFill[2];
                    }
                }

                for (int y = height; y < newHeight; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        newArray[y, x, 0] = layersFill[0];
                        newArray[y, x, 1] = layersFill[1];
                        newArray[y, x, 2] = layersFill[2];
                    }
                }
            }
        }

        return array;
    }

    internal static TileCell[,,] NewTileMatrix(int width, int height)
    {
        TileCell[,,] matrix = new TileCell[height, width, 3];

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

        return matrix;
    }

    /// <summary>
    /// Determines where the tile preview texture starts in the tile texture.
    /// </summary>
    /// <param name="init">a reference to the tile definition</param>
    /// <returns>a number representing the y-depth value where the tile preview starts</returns>
    internal static int GetTilePreviewStartingHeight(in InitTile init)
    {
        var (width, height) = init.Size;
        var bufferTiles = init.BufferTiles;
        var repeatL = init.Repeat.Length;
        var scale = GLOBALS.Scale;

        var offset = init.Type switch
        {
            InitTileType.VoxelStruct => 1 + scale * ((bufferTiles * 2) + height) * repeatL,
            InitTileType.VoxelStructRockType => 1 + scale * ((bufferTiles * 2) + height),
            InitTileType.Box => scale * height * width + (scale * (height + (2 * bufferTiles))),
            InitTileType.VoxelStructRandomDisplaceVertical => 1 + scale * ((bufferTiles * 2) + height) * repeatL,
            InitTileType.VoxelStructRandomDisplaceHorizontal => 1 + scale * ((bufferTiles * 2) + height) * repeatL,

            _ => 1 + scale * ((bufferTiles * 2) + height) * repeatL
        };

        return offset;
    }

    internal static Rectangle EncloseQuads(Quads quads)
    {
        var nearestX = Math.Min(Math.Min(quads.topLeft.X, quads.topRight.X), Math.Min(quads.bottomLeft.X, quads.bottomRight.X));
        var nearestY = Math.Min(Math.Min(quads.topLeft.Y, quads.topRight.Y), Math.Min(quads.bottomLeft.Y, quads.bottomRight.Y));

        var furthestX = Math.Max(Math.Max(quads.topLeft.X, quads.topRight.X), Math.Max(quads.bottomLeft.X, quads.bottomRight.X));
        var furthestY = Math.Max(Math.Max(quads.topLeft.Y, quads.topRight.Y), Math.Max(quads.bottomLeft.Y, quads.bottomRight.Y));
       
        return new Rectangle(nearestX, nearestY, furthestX - nearestX, furthestY - nearestY);
    }

    internal static Quads RotatePropQuads(Quads quads, float angle)
    {
        // Convert angle to radians

        var radian = float.DegreesToRadians(angle);

        var sinRotation = (float)Math.Sin(radian);
        var cosRotation = (float)Math.Cos(radian);
        
        // Enclose the quads

        var rect = EncloseQuads(quads);
        
        // Get the center of the rectangle

        var center = new Vector2(rect.x + rect.width/2, rect.y + rect.height/2);

        // var center = new Vector2(0, 0);
        
        Vector2 newTopLeft, newTopRight, newBottomRight, newBottomLeft;

        { // Rotate the top left corner
            var x = quads.topLeft.X;
            var y = quads.topLeft.Y;

            var dx = center.X - x;
            var dy = center.Y - y;

            newTopLeft = new Vector2(
                center.X + dx * cosRotation - dy * sinRotation, 
                center.Y + dx * sinRotation + dy * cosRotation
            );
        }
        
        { // Rotate the top right corner
            var x = quads.topRight.X;
            var y = quads.topRight.Y;

            var dx = center.X - x;
            var dy = center.Y - y;

            newTopRight = new Vector2(center.X + dx * cosRotation - dy * sinRotation, center.Y + dx * sinRotation + dy * cosRotation);
        }
        
        { // Rotate the bottom right corner
            var x = quads.bottomRight.X;
            var y = quads.bottomRight.Y;

            var dx = center.X - x;
            var dy = center.Y - y;

            newBottomRight = new Vector2(center.X + dx * cosRotation - dy * sinRotation, center.Y + dx * sinRotation + dy * cosRotation);
        }
        
        { // Rotate the bottom left corner
            var x = quads.bottomLeft.X;
            var y = quads.bottomLeft.Y;

            var dx = center.X - x;
            var dy = center.Y - y;

            newBottomLeft = new Vector2(center.X + dx * cosRotation - dy * sinRotation, center.Y + dx * sinRotation + dy * cosRotation);
        }
        
        return (newTopLeft, newTopRight, newBottomRight, newBottomLeft);
    }
}

/// <summary>
/// Functions that are called each frame; Must only be called after window initialization and in drawing mode.
/// </summary>
internal static class Printers
{
    internal static void DrawTextureQuads(
        Texture texture, 
        (
            Vector2 topRight, 
            Vector2 topLeft, 
            Vector2 bottomLeft, 
            Vector2 bottomRight
        ) quads
    )
    {
        RlGl.rlSetTexture(texture.id);

        RlGl.rlBegin(0x0007);
        RlGl.rlColor4ub(WHITE.r, WHITE.g, WHITE.b, WHITE.a);

        RlGl.rlTexCoord2f(1.0f, 0.0f);
        RlGl.rlVertex2f(quads.topRight.X, quads.topRight.Y);
        
        RlGl.rlTexCoord2f(0.0f, 0.0f);
        RlGl.rlVertex2f(quads.topLeft.X, quads.topLeft.Y);
        
        RlGl.rlTexCoord2f(0.0f, 1.0f);
        RlGl.rlVertex2f(quads.bottomLeft.X, quads.bottomLeft.Y);
        
        RlGl.rlTexCoord2f(1.0f, 1.0f);
        RlGl.rlVertex2f(quads.bottomRight.X, quads.bottomRight.Y);
        
        RlGl.rlTexCoord2f(1.0f, 0.0f);
        RlGl.rlVertex2f(quads.topRight.X, quads.topRight.Y);
        RlGl.rlEnd();

        RlGl.rlSetTexture(0);
    }
    internal static void DrawTilePreview(
        ref InitTile init, 
        ref Texture texture, 
        ref Color color, 
        (int x, int y) position
    )
    {
        var uniformLoc = GetShaderLocation(GLOBALS.Shaders.TilePreview, "inputTexture");
        var colorLoc = GetShaderLocation(GLOBALS.Shaders.TilePreview, "highlightColor");
        var heightStartLoc = GetShaderLocation(GLOBALS.Shaders.TilePreview, "heightStart");
        var heightLoc = GetShaderLocation(GLOBALS.Shaders.TilePreview, "height");
        var widthLoc = GetShaderLocation(GLOBALS.Shaders.TilePreview, "width");

        var startingTextureHeight = Utils.GetTilePreviewStartingHeight(init);
        float calcStartingTextureHeight = (float)startingTextureHeight / (float)texture.height;
        float calcTextureHeight = (float)(init.Size.Item2 * GLOBALS.PreviewScale) / (float)texture.height;
        float calcTextureWidth = (float)(init.Size.Item1 * GLOBALS.PreviewScale) / (float)texture.width;

        BeginShaderMode(GLOBALS.Shaders.TilePreview);
        SetShaderValueTexture(GLOBALS.Shaders.TilePreview, uniformLoc, texture);
        SetShaderValue(GLOBALS.Shaders.TilePreview, colorLoc, new System.Numerics.Vector4(color.r, color.g, color.b, 1), ShaderUniformDataType.SHADER_UNIFORM_VEC4);
        SetShaderValue(GLOBALS.Shaders.TilePreview, heightStartLoc, calcStartingTextureHeight, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        SetShaderValue(GLOBALS.Shaders.TilePreview, heightLoc, calcTextureHeight, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        SetShaderValue(GLOBALS.Shaders.TilePreview, widthLoc, calcTextureWidth, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);

        DrawTexturePro(
            texture,
            new(0, 0, texture.width, texture.height),
            new(position.x * GLOBALS.PreviewScale, position.y * GLOBALS.PreviewScale, init.Size.Item1 * GLOBALS.PreviewScale, init.Size.Item2 * GLOBALS.PreviewScale),
            RayMath.Vector2Scale(Utils.GetTileHeadOrigin(ref init), GLOBALS.PreviewScale),
            0,
            new(255, 255, 255, 255)
        );
        EndShaderMode();
    }
    
    /// <summary>
    /// Draws a camera (used in the camera editor)
    /// </summary>
    /// <param name="origin">the top left corner of the camera</param>
    /// <param name="quads">camera quads</param>
    /// <param name="camera">the page camera</param>
    /// <param name="index">when not -1, it'll be displayed at the top-left corner of the camera to visually differentiate cameras more easily</param>
    /// <returns>two boolean values that signal whether the camera was click/dragged</returns>
    internal static (bool clicked, bool hovered) DrawCameraSprite(
        System.Numerics.Vector2 origin,
        CameraQuads quads,
        Camera2D camera,
        int index = -1)
    {
        var mouse = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
        var hover = Raylib.CheckCollisionPointCircle(mouse, new(origin.X + GLOBALS.EditorCameraWidth / 2, origin.Y + GLOBALS.EditorCameraHeight / 2), 50);
        var biggerHover = Raylib.CheckCollisionPointRec(mouse, new(origin.X, origin.Y, GLOBALS.EditorCameraWidth, GLOBALS.EditorCameraHeight));

        System.Numerics.Vector2 pointOrigin1 = new(origin.X, origin.Y),
            pointOrigin2 = new(origin.X + GLOBALS.EditorCameraWidth, origin.Y),
            pointOrigin3 = new(origin.X, origin.Y + GLOBALS.EditorCameraHeight),
            pointOrigin4 = new(origin.X + GLOBALS.EditorCameraWidth, origin.Y + GLOBALS.EditorCameraHeight);

        if (biggerHover)
        {
            DrawRectangleV(
                origin,
                new(GLOBALS.EditorCameraWidth, GLOBALS.EditorCameraHeight),
                new(0, 255, 150, 70)
            );
        }
        else
        {
            Raylib.DrawRectangleV(
                origin,
                new(GLOBALS.EditorCameraWidth, GLOBALS.EditorCameraHeight),
                new(0, 255, 0, 70)
            );
        }

        if (index != -1)
        {
            Raylib.DrawText(
                $"{index}",
                (int)origin.X + 10,
                (int)origin.Y + 10,
                20,
                new(255, 255, 255, 255)
            );
        }

        Raylib.DrawRectangleLinesEx(
            new(origin.X, origin.Y, GLOBALS.EditorCameraWidth, GLOBALS.EditorCameraHeight),
            4f,
            new(255, 255, 255, 255)
        );

        Raylib.DrawRectangleLinesEx(
            new(origin.X, origin.Y, GLOBALS.EditorCameraWidth, GLOBALS.EditorCameraHeight),
            2f,
            new(0, 0, 0, 255)
        );

        Raylib.DrawCircleLines(
            (int)(origin.X + GLOBALS.EditorCameraWidth / 2),
            (int)(origin.Y + GLOBALS.EditorCameraHeight / 2),
            50,
            new(0, 0, 0, 255)
        );

        if (hover)
        {
            Raylib.DrawCircleV(
                new(origin.X + GLOBALS.EditorCameraWidth / 2, origin.Y + GLOBALS.EditorCameraHeight / 2),
                50,
                new Color(255, 255, 255, 100)
            );
        }

        Raylib.DrawLineEx(
            new(origin.X + 4, origin.Y + GLOBALS.EditorCameraHeight / 2),
            new(origin.X + GLOBALS.EditorCameraWidth - 4, origin.Y + GLOBALS.EditorCameraHeight / 2),
            4f,
            new(0, 0, 0, 255)
        );

        Raylib.DrawRectangleLinesEx(
            new(
                origin.X + 190,
                origin.Y + 20,
                51 * GLOBALS.Scale,
                40 * GLOBALS.Scale - 40
            ),
            4f,
            new(255, 0, 0, 255)
        );

        var quarter1 = new Rectangle(origin.X - 150, origin.Y - 150, GLOBALS.EditorCameraWidth / 2 + 150, GLOBALS.EditorCameraHeight / 2 + 150);
        var quarter2 = new Rectangle(GLOBALS.EditorCameraWidth / 2 + origin.X, origin.Y - 150, GLOBALS.EditorCameraWidth / 2 + 150, GLOBALS.EditorCameraHeight / 2 + 150);
        var quarter3 = new Rectangle(origin.X - 150, origin.Y + GLOBALS.EditorCameraHeight / 2, GLOBALS.EditorCameraWidth / 2 + 150, GLOBALS.EditorCameraHeight / 2 + 150);
        var quarter4 = new Rectangle(GLOBALS.EditorCameraWidth / 2 + origin.X, GLOBALS.EditorCameraHeight / 2 + origin.Y, GLOBALS.EditorCameraWidth / 2 + 150, GLOBALS.EditorCameraHeight / 2 + 150);

        if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT)) GLOBALS.CamScaleMode = false;

        if (Raylib.CheckCollisionPointRec(mouse, quarter1))
        {

            if ((Raylib.CheckCollisionPointCircle(mouse, RayMath.Vector2Add(quads.TopLeft, pointOrigin1), 10) || GLOBALS.CamScaleMode) &&
                Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                GLOBALS.CamScaleMode = true;

                quads.TopLeft = RayMath.Vector2Subtract(mouse, pointOrigin1);
            }

            Raylib.DrawCircleV(RayMath.Vector2Add(quads.TopLeft, origin), 10, new(0, 255, 0, 255));
        }


        if (Raylib.CheckCollisionPointRec(mouse, quarter2))
        {
            if ((Raylib.CheckCollisionPointCircle(mouse, RayMath.Vector2Add(quads.TopRight, pointOrigin2), 10) || GLOBALS.CamScaleMode) &&
                Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                GLOBALS.CamScaleMode = true;
                quads.TopRight = RayMath.Vector2Subtract(mouse, pointOrigin2);
            }

            Raylib.DrawCircleV(RayMath.Vector2Add(quads.TopRight, pointOrigin2), 10, new(0, 255, 0, 255));
        }

        if (Raylib.CheckCollisionPointRec(mouse, quarter3))
        {
            if ((Raylib.CheckCollisionPointCircle(mouse, RayMath.Vector2Add(quads.BottomRight, pointOrigin3), 10) || GLOBALS.CamScaleMode) &&
                Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                GLOBALS.CamScaleMode = true;
                quads.BottomRight = RayMath.Vector2Subtract(mouse, pointOrigin3);
            }

            Raylib.DrawCircleV(RayMath.Vector2Add(quads.BottomRight, pointOrigin3), 10, new(0, 255, 0, 255));
        }

        if (Raylib.CheckCollisionPointRec(mouse, quarter4))
        {
            if ((Raylib.CheckCollisionPointCircle(mouse, RayMath.Vector2Add(quads.BottomLeft, pointOrigin4), 10) || GLOBALS.CamScaleMode) &&
                Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                GLOBALS.CamScaleMode = true;
                quads.BottomLeft = RayMath.Vector2Subtract(mouse, pointOrigin4);
            }

            Raylib.DrawCircleV(RayMath.Vector2Add(quads.BottomLeft, pointOrigin4), 10, new(0, 255, 0, 255));
        }

        return (hover && Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT), biggerHover);
    }

    /// <summary>
    /// Draws the texture of the tile, layer by layer, from the bottom up.
    /// </summary>
    /// <param name="texture">a reference to the tile texture</param>
    /// <param name="init">a reference to the tile definition</param>
    /// <param name="center">the center origin of the target position to draw on</param>
    /// <param name="quads">target placement quads</param>
    internal static void DrawTileAsProp(
        ref Texture texture,
        ref InitTile init,
        ref Vector2 center,
        Span<Vector2> quads
    )
    {
        var scale = GLOBALS.Scale;

        var layerHeight = (init.Size.Item2 + (init.BufferTiles * 2)) * scale;
        float calLayerHeight = (float)layerHeight / (float)texture.height;
        var textureCutWidth = (init.Size.Item1 + (init.BufferTiles * 2)) * scale;
        float calTextureCutWidth = (float)textureCutWidth / (float)texture.width;

        var textureLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "inputTexture");
        var layerNumLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "layerNum");
        var layerHeightLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "layerHeight");
        var layerWidthLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "layerWidth");

        BeginShaderMode(GLOBALS.Shaders.Prop);

        SetShaderValueTexture(GLOBALS.Shaders.Prop, textureLoc, texture);
        SetShaderValue(GLOBALS.Shaders.Prop, layerNumLoc, init.Repeat.Length, ShaderUniformDataType.SHADER_UNIFORM_INT);
        SetShaderValue(GLOBALS.Shaders.Prop, layerHeightLoc, calLayerHeight, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        SetShaderValue(GLOBALS.Shaders.Prop, layerWidthLoc, calTextureCutWidth, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        
        DrawTexturePoly(
            texture,
            center,
            quads,
            [new(1, 0), new(0, 0), new(0, 1), new(1, 1), new(1, 0)],
            5,
            WHITE
        );
        
        EndShaderMode();
    }
    
    /// <summary>
    /// Draws the texture of the tile, layer by layer, from the bottom up.
    /// </summary>
    /// <param name="texture">a reference to the tile texture</param>
    /// <param name="init">a reference to the tile definition</param>
    /// <param name="center">the center origin of the target position to draw on</param>
    /// <param name="quads">target placement quads</param>
    internal static void DrawTileAsProp(
        ref Texture texture,
        ref InitTile init,
        (
            Vector2 topLeft,
            Vector2 topRight,
            Vector2 bottomRight,
            Vector2 bottomLeft
        ) quads
    )
    {
        var scale = GLOBALS.Scale;

        var layerHeight = (init.Size.Item2 + (init.BufferTiles * 2)) * scale;
        float calLayerHeight = (float)layerHeight / (float)texture.height;
        var textureCutWidth = (init.Size.Item1 + (init.BufferTiles * 2)) * scale;
        float calTextureCutWidth = (float)textureCutWidth / (float)texture.width;

        var textureLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "inputTexture");
        var layerNumLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "layerNum");
        var layerHeightLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "layerHeight");
        var layerWidthLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "layerWidth");

        BeginShaderMode(GLOBALS.Shaders.Prop);

        SetShaderValueTexture(GLOBALS.Shaders.Prop, textureLoc, texture);
        SetShaderValue(GLOBALS.Shaders.Prop, layerNumLoc, init.Repeat.Length, ShaderUniformDataType.SHADER_UNIFORM_INT);
        SetShaderValue(GLOBALS.Shaders.Prop, layerHeightLoc, calLayerHeight, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        SetShaderValue(GLOBALS.Shaders.Prop, layerWidthLoc, calTextureCutWidth, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        
        DrawTextureQuads(texture, quads);
        
        EndShaderMode();
    }
    
    internal static void DrawTileAsPropColored(
        ref Texture texture, 
        ref InitTile init, 
        ref Vector2 center, 
        Span<Vector2> quads,
        Color tint,
        int depth = 0
    )
    {
        var scale = GLOBALS.Scale;

        if (init.Type == InitTileType.Box)
        {
            var height = (init.Size.Item2 + init.BufferTiles*2) * scale;
            var offset = new Vector2(init.Size.Item2 > 1 ? GLOBALS.Scale : 0, scale * init.Size.Item1 * init.Size.Item2);
            
            float calcHeight = (float)height / (float)texture.height;
            Vector2 calcOffset = RayMath.Vector2Divide(offset, new(texture.width, texture.height));
            float calcWidth = (float)init.Size.Item1 * GLOBALS.Scale / texture.width;
            
            var textureLoc = GetShaderLocation(GLOBALS.Shaders.ColoredBoxTileProp, "inputTexture");

            var widthLoc = GetShaderLocation(GLOBALS.Shaders.ColoredBoxTileProp, "width");
            var heightLoc = GetShaderLocation(GLOBALS.Shaders.ColoredBoxTileProp, "height");
            var offsetLoc = GetShaderLocation(GLOBALS.Shaders.ColoredBoxTileProp, "offset");
            var colorLoc = GetShaderLocation(GLOBALS.Shaders.ColoredBoxTileProp, "tint");
            var depthLoc = GetShaderLocation(GLOBALS.Shaders.ColoredBoxTileProp, "depth");

            BeginShaderMode(GLOBALS.Shaders.ColoredBoxTileProp);

            SetShaderValueTexture(GLOBALS.Shaders.ColoredBoxTileProp, textureLoc, texture);
            
            SetShaderValue(GLOBALS.Shaders.ColoredBoxTileProp, widthLoc, calcWidth, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
            
            SetShaderValue(GLOBALS.Shaders.ColoredBoxTileProp, heightLoc, calcHeight,
                ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
            
            SetShaderValue(GLOBALS.Shaders.ColoredBoxTileProp, offsetLoc, calcOffset,
                ShaderUniformDataType.SHADER_UNIFORM_VEC2);
            
            SetShaderValue(GLOBALS.Shaders.ColoredBoxTileProp, colorLoc,
                new Vector4(tint.r / 255f, tint.g / 255f, tint.b / 255f, 1.0f),
                ShaderUniformDataType.SHADER_UNIFORM_VEC4);
            
            SetShaderValue(GLOBALS.Shaders.ColoredBoxTileProp, depthLoc, depth, ShaderUniformDataType.SHADER_UNIFORM_INT);

            DrawTexturePoly(
                texture,
                center,
                quads,
                [new(1, 0), new(0, 0), new(0, 1), new(1, 1), new(1, 0)],
                5,
                WHITE
            );
            EndShaderMode();
        }
        else
        {
            var layerHeight = (init.Size.Item2 + (init.BufferTiles * 2)) * scale;
            float calLayerHeight = (float)layerHeight / (float)texture.height;
            var textureCutWidth = (init.Size.Item1 + (init.BufferTiles * 2)) * scale;
            float calTextureCutWidth = (float)textureCutWidth / (float)texture.width;

            var textureLoc = GetShaderLocation(GLOBALS.Shaders.ColoredTileProp, "inputTexture");
            var layerNumLoc = GetShaderLocation(GLOBALS.Shaders.ColoredTileProp, "layerNum");
            var layerHeightLoc = GetShaderLocation(GLOBALS.Shaders.ColoredTileProp, "layerHeight");
            var layerWidthLoc = GetShaderLocation(GLOBALS.Shaders.ColoredTileProp, "layerWidth");
            var colorLoc = GetShaderLocation(GLOBALS.Shaders.ColoredTileProp, "tint");
            var depthLoc = GetShaderLocation(GLOBALS.Shaders.ColoredTileProp, "depth");

            BeginShaderMode(GLOBALS.Shaders.ColoredTileProp);

            SetShaderValueTexture(GLOBALS.Shaders.ColoredTileProp, textureLoc, texture);
            SetShaderValue(GLOBALS.Shaders.ColoredTileProp, layerNumLoc, init.Type == InitTileType.VoxelStructRockType ? 1 : init.Repeat.Length,
                ShaderUniformDataType.SHADER_UNIFORM_INT);
            SetShaderValue(GLOBALS.Shaders.ColoredTileProp, layerHeightLoc, calLayerHeight,
                ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
            SetShaderValue(GLOBALS.Shaders.ColoredTileProp, layerWidthLoc, calTextureCutWidth,
                ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
            SetShaderValue(GLOBALS.Shaders.ColoredTileProp, colorLoc,
                new Vector4(tint.r / 255f, tint.g / 255f, tint.b / 255f, 1.0f),
                ShaderUniformDataType.SHADER_UNIFORM_VEC4);
            SetShaderValue(GLOBALS.Shaders.ColoredTileProp, depthLoc, depth, ShaderUniformDataType.SHADER_UNIFORM_INT);

            DrawTexturePoly(
                texture,
                center,
                quads,
                [new(1, 0), new(0, 0), new(0, 1), new(1, 1), new(1, 0)],
                5,
                WHITE
            );
            EndShaderMode();
        }
    }

    internal static void DrawProp(
        InitPropBase init, 
        ref Texture texture, 
        ref Vector2 center,
        Span<Vector2> quads,
        int variation = -1)
    {
        switch (init)
        {
            case InitVariedStandardProp variedStandard:
                DrawVariedStandardProp(variedStandard, ref texture, ref center, quads, variation);
                break;
            
            case InitStandardProp standard:
                DrawStandardProp(standard, ref texture, ref center, quads);
                break;
            
            case InitVariedSoftProp variedSoft:
                DrawVariedSoftProp(variedSoft, ref texture, ref center, quads, variation);
                break;
            
            case InitSoftProp soft:
                DrawSoftProp(soft, ref texture, ref center, quads);
                break;
            
            case InitVariedDecalProp variedDecal:
                DrawVariedDecalProp(variedDecal, ref texture, ref center, quads, variation);
                break;
            
            case InitSimpleDecalProp:
                DrawSimpleDecalProp(ref texture, ref center, quads);
                break;
        }
    }
    
    internal static void DrawStandardProp(
        InitStandardProp init, 
        ref Texture texture, 
        ref Vector2 center,
        Span<Vector2> quads
    )
    {
        var layerHeight = (float)texture.height / (float)init.Repeat.Length;
        var calcLayerHeight = layerHeight / texture.height;
        var calcWidth = (float) init.Size.x * GLOBALS.Scale / texture.width;

        calcWidth = calcWidth > 1.00000f ? 1.0f : calcWidth;
        
        var textureLoc = GetShaderLocation(GLOBALS.Shaders.StandardProp, "inputTexture");
        var layerNumLoc = GetShaderLocation(GLOBALS.Shaders.StandardProp, "layerNum");
        var layerHeightLoc = GetShaderLocation(GLOBALS.Shaders.StandardProp, "layerHeight");
        var widthLoc = GetShaderLocation(GLOBALS.Shaders.StandardProp, "width");

        BeginShaderMode(GLOBALS.Shaders.StandardProp);

        SetShaderValueTexture(GLOBALS.Shaders.StandardProp, textureLoc, texture);
        SetShaderValue(GLOBALS.Shaders.StandardProp, layerNumLoc, init.Repeat.Length, ShaderUniformDataType.SHADER_UNIFORM_INT);
        SetShaderValue(GLOBALS.Shaders.StandardProp, layerHeightLoc, calcLayerHeight, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        SetShaderValue(GLOBALS.Shaders.StandardProp, widthLoc, calcWidth, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        
        DrawTexturePoly(
            texture, 
            center, 
            quads, 
            [new(1, 0), new(0, 0), new(0, 1), new(1, 1), new(1, 0)], 
            5, 
            WHITE
        );
        EndShaderMode();
    }

    internal static void DrawVariedStandardProp(
        InitVariedStandardProp init, 
        ref Texture texture, 
        ref Vector2 center,
        Span<Vector2> quads,
        int variation
    )
    {
        var layerHeight = (float) init.Size.y * GLOBALS.Scale;
        var variationWidth = (float) init.Size.x * GLOBALS.Scale;
        
        var calcLayerHeight = layerHeight / texture.height;
        var calcVariationWidth = variationWidth / texture.width;
        
        var textureLoc = GetShaderLocation(GLOBALS.Shaders.VariedStandardProp, "inputTexture");
        
        var layerNumLoc = GetShaderLocation(GLOBALS.Shaders.VariedStandardProp, "layerNum");
        var layerHeightLoc = GetShaderLocation(GLOBALS.Shaders.VariedStandardProp, "layerHeight");
        var variationWidthLoc = GetShaderLocation(GLOBALS.Shaders.VariedStandardProp, "varWidth");
        var variationLoc = GetShaderLocation(GLOBALS.Shaders.VariedStandardProp, "variation");

        BeginShaderMode(GLOBALS.Shaders.VariedStandardProp);

        SetShaderValueTexture(GLOBALS.Shaders.VariedStandardProp, textureLoc, texture);
       
        SetShaderValue(GLOBALS.Shaders.VariedStandardProp, layerNumLoc, init.Repeat.Length, ShaderUniformDataType.SHADER_UNIFORM_INT);
        SetShaderValue(GLOBALS.Shaders.VariedStandardProp, layerHeightLoc, calcLayerHeight, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        SetShaderValue(GLOBALS.Shaders.VariedStandardProp, variationWidthLoc, calcVariationWidth, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        SetShaderValue(GLOBALS.Shaders.VariedStandardProp, variationLoc, variation, ShaderUniformDataType.SHADER_UNIFORM_INT);
        
        DrawTexturePoly(
            texture, 
            center, 
            quads, 
            [new(1, 0), new(0, 0), new(0, 1), new(1, 1), new(1, 0)], 
            5, 
            WHITE
        );
        EndShaderMode();
    }

    internal static void DrawSoftProp(
        InitSoftProp init, 
        ref Texture texture, 
        ref Vector2 center,
        Span<Vector2> quads
    )
    {
        var textureLoc = GetShaderLocation(GLOBALS.Shaders.SoftProp, "inputTexture");

        BeginShaderMode(GLOBALS.Shaders.SoftProp);

        SetShaderValueTexture(GLOBALS.Shaders.SoftProp, textureLoc, texture);
        
        DrawTexturePoly(
            texture, 
            center, 
            quads, 
            [new(1, 0), new(0, 0), new(0, 1), new(1, 1), new(1, 0)], 
            5, 
            WHITE
        );
        EndShaderMode();
    }
    
    internal static void DrawVariedSoftProp(
        InitVariedSoftProp init, 
        ref Texture texture, 
        ref Vector2 center,
        Span<Vector2> quads,
        int variation
    )
    {
        var calcHeight = (float) init.SizeInPixels.y / texture.height;
        var calcVariationWidth = (float) init.SizeInPixels.x / texture.width;

        var offset = init.Colorize == 1 && init.Variations > 0 ? new Vector2((float) texture.width - init.SizeInPixels.x, (float) texture.height/2 - init.SizeInPixels.y) : new(0, 0);
        
        var calcOffset = RayMath.Vector2Divide(offset, new(texture.width, texture.height));
        
        var textureLoc = GetShaderLocation(GLOBALS.Shaders.VariedSoftProp, "inputTexture");

        var offsetLoc = GetShaderLocation(GLOBALS.Shaders.VariedSoftProp, "offset");

        var heightLoc = GetShaderLocation(GLOBALS.Shaders.VariedSoftProp, "height");
        var variationWidthLoc = GetShaderLocation(GLOBALS.Shaders.VariedSoftProp, "varWidth");
        var variationLoc = GetShaderLocation(GLOBALS.Shaders.VariedSoftProp, "variation");

        BeginShaderMode(GLOBALS.Shaders.VariedSoftProp);

        SetShaderValue(GLOBALS.Shaders.VariedSoftProp, offsetLoc, calcOffset, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
        
        SetShaderValueTexture(GLOBALS.Shaders.VariedSoftProp, textureLoc, texture);

        SetShaderValue(GLOBALS.Shaders.VariedSoftProp, variationWidthLoc, calcVariationWidth,
            ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        SetShaderValue(GLOBALS.Shaders.VariedSoftProp, heightLoc, calcHeight, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        SetShaderValue(GLOBALS.Shaders.VariedSoftProp, variationLoc, variation, ShaderUniformDataType.SHADER_UNIFORM_INT);

        DrawTexturePoly(
            texture, 
            center, 
            quads, 
            [
                new(1, 0), 
                new(0, 0), 
                new(0, 1), 
                new(1, 1), 
                new(1, 0)
            ], 
            5, 
            WHITE
        );
        EndShaderMode();
    }
    
    internal static void DrawSimpleDecalProp(
        ref Texture texture, 
        ref Vector2 center,
        Span<Vector2> quads
    )
    {
        var textureLoc = GetShaderLocation(GLOBALS.Shaders.SimpleDecalProp, "inputTexture");

        BeginShaderMode(GLOBALS.Shaders.SimpleDecalProp);

        SetShaderValueTexture(GLOBALS.Shaders.SimpleDecalProp, textureLoc, texture);
        
        DrawTexturePoly(
            texture, 
            center, 
            quads, 
            [new(1, 0), new(0, 0), new(0, 1), new(1, 1), new(1, 0)], 
            5, 
            WHITE
        );
        EndShaderMode();
    }
    
    internal static void DrawVariedDecalProp(
        InitVariedDecalProp init, 
        ref Texture texture, 
        ref Vector2 center,
        Span<Vector2> quads,
        int variation
    )
    {
        var calcHeight = (float) init.SizeInPixels.y / texture.height;
        var calcVariationWidth = (float) init.SizeInPixels.x / texture.width;

        var textureLoc = GetShaderLocation(GLOBALS.Shaders.VariedSoftProp, "inputTexture");

        var heightLoc = GetShaderLocation(GLOBALS.Shaders.VariedSoftProp, "height");
        var variationWidthLoc = GetShaderLocation(GLOBALS.Shaders.VariedSoftProp, "varWidth");
        var variationLoc = GetShaderLocation(GLOBALS.Shaders.VariedSoftProp, "variation");

        BeginShaderMode(GLOBALS.Shaders.VariedSoftProp);

        SetShaderValueTexture(GLOBALS.Shaders.VariedSoftProp, textureLoc, texture);

        SetShaderValue(GLOBALS.Shaders.VariedSoftProp, variationWidthLoc, calcVariationWidth,
            ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        SetShaderValue(GLOBALS.Shaders.VariedSoftProp, heightLoc, calcHeight, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        SetShaderValue(GLOBALS.Shaders.VariedSoftProp, variationLoc, variation, ShaderUniformDataType.SHADER_UNIFORM_INT);

        DrawTexturePoly(
            texture, 
            center, 
            quads, 
            [
                new(1, 0), 
                new(0, 0), 
                new(0, 1), 
                new(1, 1), 
                new(1, 0)
            ], 
            5, 
            WHITE
        );
        EndShaderMode();
    }

    /// <summary>
    /// Draws an individual tile geo-spec based on the ID.
    /// </summary>
    /// <param name="id">geo-tile ID</param>
    /// <param name="origin">the top-left corner to start drawing</param>
    /// <param name="scale">the scale of the drawing</param>
    /// <param name="color">the color of the sprite</param>
    internal static void DrawTileSpec(int id, System.Numerics.Vector2 origin, int scale, Color color)
    {
        switch (id)
        {
            // air
            case 0:
                DrawRectangleLinesEx(
                    new(origin.X + 10, origin.Y + 10, scale - 20, scale - 20),
                    2,
                    color
                );
                break;

            // solid
            case 1:
                DrawRectangleV(origin, RayMath.Vector2Scale(new(1, 1), scale), color);
                break;

            // slopes
            case 2:
                DrawTriangle(
                    origin,
                    new(origin.X, origin.Y + scale),
                    new(origin.X + scale, origin.Y + scale),
                    color
                );
                break;

            case 3:
                DrawTriangle(
                    new(origin.X + scale, origin.Y),
                    new(origin.X, origin.Y + scale),
                    new(origin.X + scale, origin.Y + scale),
                    color
                );
                break;

            case 4:
                DrawTriangle(
                    origin,
                    new(origin.X, origin.Y + scale),
                    new(origin.X + scale, origin.Y),
                    color
                );
                break;
            case 5:
                DrawTriangle(
                    origin,
                    new(origin.X + scale, origin.Y + scale),
                    new(origin.X + scale, origin.Y),
                    color
                );
                break;

            // platform
            case 6:
                DrawRectangleV(
                    origin,
                    new(scale, scale / 2),
                    color
                );
                break;

            // shortcut entrance
            case 7: break;

            // glass
            case 9: break;
        }
    }
}