using System.Numerics;
using Leditor.Types;
using Leditor.Data.Tiles;
using Pidgin;
using System.Text.Json;
using Leditor.Data.Geometry;
using Leditor.Data.Props.Legacy;
using Leditor.Data.Effects;
using Leditor.Serialization.Parser;

namespace Leditor;

/// A collection of helper functions used across pages
internal static class Utils
{
    internal static void Restrict(ref int value, int min, int max)
    {
        if (value < min) value = min;
        if (value > max) value = max;
    }

    internal static void Restrict(ref int value, int min)
    {
        if (value < min) value = min;
    }

    internal static void Restrict(ref float value, float min, float max)
    {
        if (value < min) value = min;
        if (value > max) value = max;
    }

    internal static void Restrict(ref float value, float min)
    {
        if (value < min) value = min;
    }

    internal static void Cycle(ref int value, int min, int max)
    {
        if (value > max) value = min;
        if (value < min) value = max;
    }

    internal static void Cycle(ref float value, float min, float max)
    {
        if (value > max) value = min;
        if (value < min) value = max;
    }

    internal static void VFlipQuad(ref Data.Quad quad) {
        (quad.TopLeft, quad.BottomLeft) = (quad.BottomLeft, quad.TopLeft);
        (quad.TopRight, quad.BottomRight) = (quad.BottomRight, quad.TopRight);
    }

    internal static void HFlipQuad(ref Data.Quad quad) {
        (quad.TopLeft, quad.TopRight) = (quad.TopRight, quad.TopLeft);
        (quad.BottomLeft, quad.BottomRight) = (quad.BottomRight, quad.BottomLeft);
    }

    internal static bool SpecHasDepth(int[,,] specs, int depth = 1) {
        if (depth < 0 || depth >= specs.GetLength(2)) return false;

        for (var y = 0; y < specs.GetLength(0); y++) {
            for (var x = 0; x < specs.GetLength(1); x++) {
                if (specs[y, x, depth] != -1) return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Calculates all integers between two flaoting points
    /// </summary>
    /// <param name="f1">The first floating point</param>
    /// <param name="f2">The second floating point</param>
    /// <returns>A list of all integers from f1 to f2</returns>
    internal static IEnumerable<int> GetIntsBetweenFloats(float f1, float f2) 
    {
        var i1 = (int)f1;
        var i2 = (int)f2;

        if (i1 > i2) {
            for (var x = i1; x >= i2; x--) yield return x;
        } else {
            for (var x = i1; x <= i2; x++) yield return x;
        }
    }

    internal static IEnumerable<int> GetIntsBetween(int i1, int i2)
    {
        if (i1 > i2) {
            for (var x = i1; x >= i2; x--) yield return x;
        } else {
            for (var x = i1; x <= i2; x++) yield return x;
        }
    }

    internal static IEnumerable<Data.Coords> GetTrianglePathPoints(Data.Coords p1, Data.Coords p2, bool yAxisFirst) 
    {
        if (yAxisFirst)
        {
            var yVecs = GetIntsBetween(p1.Y, p2.Y).Select(y => p1 with { Y = y });

            var lastVec = yVecs.Last();

            var xVecs = GetIntsBetween(p1.X, p2.X).Select(x => lastVec with { X = x });

            return yVecs.Concat(xVecs.Skip(1));
        }
        else
        {
            var xVecs = GetIntsBetween(p1.X, p2.X).Select(x => p1 with { X = x });

            var lastVec = xVecs.Last();

            var yVecs = GetIntsBetween(lastVec.Y, p2.Y).Select(y => lastVec with { Y = y });

            return xVecs.Concat(yVecs.Skip(1));
        }
    }

    internal static void SaveSettings() {
        try {
            var text = JsonSerializer.Serialize(GLOBALS.Settings, GLOBALS.JsonSerializerOptions);
            File.WriteAllText(GLOBALS.Paths.SettingsPath, text);
        } catch {
            // do nothing, I guess
        }
    }

    internal static bool AllEqual(int[] list) => list.All(i => i == list[0]);

    internal static bool AllEqual(IEnumerable<int> list, int item) => list.All(i => i == item);

    internal static bool AllEqual(int a, int b, int c) => a == b && b == c && a == c;

    internal static Geo CopyGeoCell(in Geo cell) => new()
    {
        Type = cell.Type,
        Features = cell.Features,
    };

    internal static int GetPropDepth(in TileDefinition? tile) => tile?.Repeat.Sum() ?? 0;

    internal static int GetPropDepth(in InitPropBase prop) => prop switch
    {
        InitVariedStandardProp v => v.Repeat.Length,
        InitStandardProp s => s.Repeat.Length,
        _ => prop.Depth
    };

    internal static Rectangle CameraCriticalRectangle(int x, int y) =>
        new(x + 190, y + 20, 51 * 20, 40 * 20 - 40);

    internal static Rectangle CameraCriticalRectangle(float x, float y) =>
        new(x + 190, y + 20, 51 * 20, 40 * 20 - 40);

    internal static Rectangle CameraCriticalRectangle(Vector2 origin) =>
        new(origin.X + 190, origin.Y + 20, 51 * 20, 40 * 20 - 40);

    internal static void AppendRecentProjectPath(string path)
    {
        if (GLOBALS.RecentProjects.SingleOrDefault(pair => pair.path == path).path != null) return;

        GLOBALS.RecentProjects.AddFirst((path, Path.GetFileNameWithoutExtension(path)));

        if (GLOBALS.RecentProjects.Count > GLOBALS.RecentProjectsLimit)
            GLOBALS.RecentProjects.RemoveLast();
    }

    internal static Geo[,,] GetGeometryMatrixFromFile(string path) {
        if (!File.Exists(path)) throw new FileNotFoundException("Level file not found", path);
    
        var text = File
            .ReadAllText(path)
            .ReplaceLineEndings()
            .Split(Environment.NewLine);

        var matrixObject = LingoParser.Expression.ParseOrThrow(text[0]);
    
        var matrix = Serialization.Importers.GetGeoMatrix(matrixObject, out _, out _);

        return matrix;
    }

    internal static async Task<LoadFileResult> LoadProjectAsync(string filePath)
    {
        var text = (await File.ReadAllTextAsync(filePath)).ReplaceLineEndings().Split(Environment.NewLine);

        var lightMapFileName = Path.Combine(Path.GetDirectoryName(filePath)!,
            Path.GetFileNameWithoutExtension(filePath) + ".png");

        if (text.Length < 7) return new LoadFileResult();

        var objTask = Task.Run(() =>
            LingoParser.Expression.ParseOrThrow(text[0]));
        var tilesObjTask = Task.Run(() =>
            LingoParser.Expression.ParseOrThrow(text[1]));
        var terrainObjTask = Task.Run(() =>
            LingoParser.Expression.ParseOrThrow(text[4]));
        var obj2Task = Task.Run(() =>
            LingoParser.Expression.ParseOrThrow(text[5]));
        var effObjTask = Task.Run(() =>
            LingoParser.Expression.ParseOrThrow(text[2]));
        var lightObjTask = Task.Run(() =>
            LingoParser.Expression.ParseOrThrow(text[3]));
        var camsObjTask = Task.Run(() =>
            LingoParser.Expression.ParseOrThrow(text[6]));
        var waterObjTask = Task.Run(() =>
            LingoParser.Expression.ParseOrThrow(text[7]));
        
        var propsObjTask = string.IsNullOrEmpty(text[8]) 
            ? null 
            : Task.Run(() => LingoParser.Expression.Parse(text[8]));

        AstNode.Base obj;
        AstNode.Base tilesObj;
        AstNode.Base terrainModeObj;
        AstNode.Base obj2;
        AstNode.Base effObj;
        AstNode.Base lightObj;
        AstNode.Base camsObj;
        AstNode.Base waterObj;
        AstNode.Base? propsObj;

        Exception? propsLoadException = null;

        try {
            obj = await objTask;
        } catch (Exception e) {
            throw new Exception("Failed to parse level project file at line 1", e);
        }

        try {
            tilesObj = await tilesObjTask;
        } catch (Exception e) {
            throw new Exception("Failed to parse level project file at line 2", e);
        }

        try {
            terrainModeObj = await terrainObjTask;
        } catch (Exception e) {
            throw new Exception("Failed to parse level project file at line 5", e);
        }

        try {
            obj2 = await obj2Task;
        } catch (Exception e) {
            throw new Exception("Failed to parse level project file at line 6", e);
        }

        try {
            effObj = await effObjTask;
        } catch (Exception e) {
            throw new Exception("Failed to parse level project file at line 3", e);
        }

        try {
            lightObj = await lightObjTask;
        } catch (Exception e) {
            throw new Exception("Failed to parse level project file at line 4", e);
        }

        try {
            camsObj = await camsObjTask;
        } catch (Exception e) {
            throw new Exception("Failed to parse level project file at line 7", e);
        }

        try {
            waterObj = await waterObjTask;
        } catch (Exception e) {
            throw new Exception("Failed to parse level project file at line 8", e);
        }

        try {
            var parseRes = propsObjTask is null ? null : await propsObjTask;
            propsObj = parseRes?.Success == true ? parseRes.Value : null;
        } catch (Exception e) {
            propsObj = null;
            propsLoadException = e;
            // throw new Exception("Failed to parse level project file at line 9", e);
        }

        var (foundWidth, foundHeight) = Serialization.Importers.GetLevelDimensions(obj2);


        var mtx = Serialization.Importers.GetGeoMatrix(obj, out int givenHeight, out int givenWidth);
        if (foundWidth <= 0 || foundHeight <= 0)
        {
            foundWidth = givenWidth;
            foundHeight = givenHeight;
        }
        // var tlMtx = Serialization.Importers.GetTileMatrix(tilesObj, out _, out _);
        var tlMtx2 = Serialization.TileImporter.GetTileMatrix_NoExcept(tilesObj, GLOBALS.MaterialDex!.DefMap, GLOBALS.TileDex!.DefMap);
        var defaultMaterial = Serialization.Importers.GetDefaultMaterial(tilesObj);
        var buffers = Serialization.Importers.GetBufferTiles(obj2);
        var terrain = Serialization.Importers.GetTerrainMedium(terrainModeObj);
        var lightMode = Serialization.Importers.GetLightMode(obj2);
        var seed = Serialization.Importers.GetSeed(obj2);
        var waterData = Serialization.Importers.GetWaterData(waterObj);
        var effects = Serialization.Importers.GetEffects(effObj, foundWidth, foundHeight);
        var cams = Serialization.Importers.GetCameras(camsObj);

        for (var x = 0; x < foundWidth; x++) {
            for (var y = 0; y < foundHeight; y++) {
                for (var z = 0; z < 3; z++) {
                    var body = tlMtx2[y, x, z];

                    if (body.Type is TileCellType.Body)
                    {
                        var (hx, hy, hz) = body.HeadPosition;

                        hx--;
                        hy--;
                        hz--;
                    
                        if (
                            hx < 0 || hx >= tlMtx2.GetLength(1) ||
                            hy < 0 || hy >= tlMtx2.GetLength(0) ||
                            hz < 0 || hz >= 3) continue;

                        var head = tlMtx2[hy, hx, hz];

                        if (head.Type is not TileCellType.Head) continue;

                        var newBody = body with {
                            TileDefinition = head.TileDefinition,
                            UndefinedName = head.UndefinedName     
                        };

                        tlMtx2[y, x, z] = newBody;
                    }
                }
            }
        }

        Data.Materials.MaterialDefinition defMatFound = GLOBALS.Materials[0][1];

        foreach (var category in GLOBALS.Materials) {
            foreach (var m in category) {
                if (string.Equals(defaultMaterial, m.Name, StringComparison.InvariantCulture)) {
                    defMatFound = m;
                    goto skipMatSearch;
                }
            }
        }

        skipMatSearch:

        // TODO: catch PropNotFoundException
        List<Prop_Legacy> props;

        try {
            props = Serialization.Importers.GetProps(propsObj);
        } catch (Exception e) {
            props = [];
            Console.WriteLine("Failed to load props: "+e);
        }

        var lightSettings = Serialization.Importers.GetLightSettings(lightObj);

        // map material colors

        Data.Color[,,] materialColors = Utils.NewMaterialColorMatrix(givenWidth, givenHeight, new(0, 0, 0, 255));

        for (int y = 0; y < givenHeight; y++)
        {
            for (int x = 0; x < givenWidth; x++)
            {
                for (int z = 0; z < 3; z++)
                {
                    if (x < 0 || x >= tlMtx2.GetLength(1) || y < 0 || y >= tlMtx2.GetLength(0)) continue;
                    var cell = tlMtx2[y, x, z];

                    if (cell.Type is not TileCellType.Material) continue;

                    var materialName = cell.MaterialDefinition?.Name ?? cell.UndefinedName;

                    if (GLOBALS.MaterialColors.TryGetValue(materialName!, out Data.Color color))
                        materialColors[y, x, z] = color;
                }
            }
        }

        // Light map

        var lightMap = File.Exists(lightMapFileName) 
            ? Raylib.LoadImage(lightMapFileName) 
            : Raylib.GenImageColor(givenWidth * 20 + 300, givenHeight * 20 + 300, Color.White);

        //

        return new LoadFileResult
        {
            Success = true,
            Seed = seed,
            WaterLevel = waterData.waterLevel,
            WaterInFront = waterData.waterInFront,
            Width = givenWidth,
            Height = givenHeight,
            DefaultMaterial = defMatFound,
            BufferTiles = buffers,
            GeoMatrix = mtx,
            TileMatrix = tlMtx2,
            LightMode = lightMode,
            DefaultTerrain = terrain,
            MaterialColorMatrix = materialColors,
            Effects = effects,
            LightMapImage = lightMap,
            Cameras = cams,
            PropsArray = props.ToArray(),
            LightSettings = lightSettings,
            Name = Path.GetFileNameWithoutExtension(filePath),
            PropsLoadException = propsLoadException
        };
    }

    public static TileDefinition? PickupTile(int x, int y, int z)
    {
        if (GLOBALS.TileDex is null) return null;
        
        var cell = GLOBALS.Level.TileMatrix[y, x, z];

        if (cell.Type is TileCellType.Head) return cell.TileDefinition;

        if (cell.Type is not TileCellType.Body) return null;
        
        // fetch the head
        var (headX, headY, headZ) = cell.HeadPosition;
        // This is done because Lingo is 1-based index
        var supposedHead = GLOBALS.Level.TileMatrix[headY - 1, headX - 1, headZ - 1];

        if (supposedHead.Type is TileCellType.Head && supposedHead is { TileDefinition: not null }) return supposedHead.TileDefinition;
            
        return null;
    }

    public static (int category, int index) GetTileIndex(in TileDefinition? tile)
    {
        if (tile is null || GLOBALS.TileDex is null) return (-1, -1);

        var categoryFound = GLOBALS.TileDex.TryGetCategory(tile, out var categoryName);

        if (!categoryFound || categoryName is null) return (-1, -1);

        for (var c = 0; c < GLOBALS.TileDex.OrderedCategoryNames.Length; c++)
        {
            if (!string.Equals(GLOBALS.TileDex.OrderedCategoryNames[c], categoryName,
                    StringComparison.InvariantCultureIgnoreCase)) continue;
            
            var tiles = GLOBALS.TileDex.GetTilesOfCategory(categoryName);

            return (c, Array.IndexOf(tiles, tile));
        }

        return (-1, -1);
    }

    public static (int category, int index)? PickupMaterial(int x, int y, int z)
    {
        var cell = GLOBALS.Level.TileMatrix[y, x, z];

        if (cell.Type is TileCellType.Material)
        {
            for (int c = 0; c < GLOBALS.Materials.Length; c++)
            {
                for (int i = 0; i < GLOBALS.Materials[c].Length; i++)
                {
                    if (string.Equals(GLOBALS.Materials[c][i].Name, cell.MaterialDefinition?.Name ?? cell.UndefinedName, StringComparison.Ordinal)) return (c, i);
                }
            }

            return null;
        }

        return null;
    }

    public static bool InBounds<T>(in Data.Coords coords, T[,,] matrix)
    {
        return coords.X >= 0 && coords.X < matrix.GetLength(1) &&
               coords.Y >= 0 && coords.Y < matrix.GetLength(0) &&
               coords.Z >= 0 && coords.Z < matrix.GetLength(2);
    }
    
    public static bool InBounds<T>(in Data.Coords coords, T[,] matrix)
    {
        return coords.X >= 0 && coords.X < matrix.GetLength(1) &&
               coords.Y >= 0 && coords.Y < matrix.GetLength(0);
    }

    internal static void RemoveTile(int mx, int my, int mz)
    {
        while (true)
        {
            var cell = GLOBALS.Level.TileMatrix[my, mx, mz];

            if (cell.Type is TileCellType.Head)
            {
                // tile is undefined
                if (cell.TileDefinition is null)
                {
                    var oldCell = GLOBALS.Level.TileMatrix[my, mx, mz];
                    var newCell = new Tile();

                    GLOBALS.Level.TileMatrix[my, mx, mz] = newCell;

                    return;
                }

                var tileInit = cell.TileDefinition;
                var (width, height) = tileInit.Size;

                var specs = tileInit.Specs;

                // get the "middle" point of the tile
                var head = Utils.GetTileHeadOrigin(tileInit);

                // the top-left of the tile
                var start = Raymath.Vector2Subtract(new(mx, my), head);

                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        var matrixX = x + (int)start.X;
                        var matrixY = y + (int)start.Y;

                        if (matrixX < 0 || matrixX >= GLOBALS.Level.Width || matrixY < 0 || matrixY >= GLOBALS.Level.Height) continue;

                        var spec = specs[y, x, 0];
                        var spec2 = specs[y, x, 1];
                        var spec3 = specs[y, x, 2];

                        if (spec != -1)
                        {
                            var newCell = new Tile();

                            GLOBALS.Level.TileMatrix[matrixY, matrixX, mz] = newCell;
                        }

                        if (mz != 2 && spec2 != -1)
                        {
                            var newCell2 = new Tile();

                            GLOBALS.Level.TileMatrix[matrixY, matrixX, mz + 1] = newCell2;
                        }

                        if (mz == 0 && spec3 != -1)
                        {
                            var newCell3 = new Tile();

                            GLOBALS.Level.TileMatrix[matrixY, matrixX, 2] = newCell3;
                        }
                    }
                }
            }
            else if (cell.Type == TileCellType.Body)
            {
                var (headX, headY, headZ) = cell.HeadPosition;

                // This is done because Lingo is 1-based index
                var supposedHead = GLOBALS.Level.TileMatrix[headY - 1, headX - 1, headZ - 1];

                // if the head was not found, only delete the given tile body
                if (supposedHead.Type is not TileCellType.Head)
                {
                    GLOBALS.Level.TileMatrix[my, mx, mz] = new Tile();
                    return;
                }

                mx = headX - 1;
                my = headY - 1;
                mz = headZ - 1;
                continue;
            }

            return;
        }
    }

    internal static void ForcePlaceTileWithoutGeo(
        in Tile[,,] tileMatrix,
        in TileDefinition init,
        (int x, int y, int z) matrixPosition
    )
    {
        var (mx, my, mz) = matrixPosition;
        var (width, height) = init.Size;
        var specs = init.Specs;

        // get the "middle" point of the tile
        var head = Utils.GetTileHeadOrigin(init);

        // the top-left of the tile
        var start = Raymath.Vector2Subtract(new(mx, my), head);
        
        // Remove pre-existing tile in the way
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var matrixX = x + (int)start.X;
                var matrixY = y + (int)start.Y;
                
                if (
                    !(matrixX >= 0 &&
                      matrixX < tileMatrix.GetLength(1) &&
                      matrixY >= 0 &&
                      matrixY < tileMatrix.GetLength(0))
                ) continue;
                
                var spec = specs[y, x, 0];
                var spec2 = specs[y, x, 1];
                var spec3 = specs[y, x, 2];

                if (spec != -1)
                {
                    if (tileMatrix[matrixY, matrixX, mz].Type is TileCellType.Head)
                        RemoveTile(matrixX, matrixY, mz);
                }

                if (spec2 != -1 && mz != 2)
                {
                    if (tileMatrix[matrixY, matrixX, mz+1].Type is TileCellType.Head)
                        RemoveTile(matrixX, matrixY, mz+1);
                }
            }
        }

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var matrixX = x + (int)start.X;
                var matrixY = y + (int)start.Y;

                // This function depends on the rest of the program to guarantee that all level matrices have the same x and y dimensions
                if (
                    matrixX >= 0 &&
                    matrixX < tileMatrix.GetLength(1) &&
                    matrixY >= 0 &&
                    matrixY < tileMatrix.GetLength(0)
                )
                {
                    var spec = specs[y, x, 0];
                    var spec2 = specs[y, x, 1];
                    var spec3 = specs[y, x, 2];
                    
                    // If it's the tile head
                    if (x == (int)head.X && y == (int)head.Y)
                    {
                        // Place the head of the tile at matrixPosition
                        var newHead = new Tile(init);
                        
                        tileMatrix[my, mx, mz] = newHead;
                    }
                    
                    if (spec != -1)
                    {
                        // If it's the tile head
                        if (!(x == (int)head.X && y == (int)head.Y))
                        {
                            var newCell = new Tile(
                                mx + 1, my + 1, mz + 1,
                                tileMatrix[my + 1, mx + 1, mz + 1].TileDefinition,
                                tileMatrix[my + 1, mx + 1, mz + 1].UndefinedName
                            );
                            
                            tileMatrix[matrixY, matrixX, mz] = newCell;
                        }
                    }
                    
                    if (spec2 != -1 && mz != 2)
                    {
                        var newerCell = new Tile(
                            mx + 1, my + 1, mz + 1,
                            tileMatrix[my + 1, mx + 1, mz + 1].TileDefinition,
                            tileMatrix[my + 1, mx + 1, mz + 1].UndefinedName
                        );
                        
                        tileMatrix[matrixY, matrixX, mz + 1] = newerCell;
                    }
                }
            }
        }
        
        return;
    }

    internal static bool InBounds<T>(int index, T[] array) => index >= 0 && index < array.Length;
    internal static bool InBounds<T>(int x, int y, T[,] matrix) => x >= 0 && x < matrix.GetLength(1) &&
                                                                   y >= 0 && y < matrix.GetLength(0);
    
    internal static bool InBounds<T>(int x, int y, int z, T[,,] matrix) => x >= 0 && x < matrix.GetLength(1) &&
                                                                   y >= 0 && y < matrix.GetLength(0) &&
                                                                   z >= 0 && z < matrix.GetLength(2);

    internal static T[,] Sample<T>(T[,] matrix, int x, int y, int radius)
        where T : struct
    {
        #if DEBUG
        if (!InBounds(x, y, matrix)) throw new ArgumentOutOfRangeException($"X or Y are out of materix range (X:{matrix.GetLength(1)}, Y:{matrix.GetLength(0)})");
        #endif
        
        var sample = new T[radius * 2 + 1, radius * 2 + 1];
        
        for (var i = 0; i < radius * 2 + 1; i++)
        {
            for (var k = 0; k < radius * 2 + 1; k++)
            {
                var mx = x - radius + i;
                var my = y - radius + k;

                if (mx < 0 || mx >= matrix.GetLength(1) ||
                    my < 0 || my >= matrix.GetLength(0))
                {
                    sample[k, i] = default;
                    continue;
                }

                sample[k, i] = matrix[my, mx];
            }
        }

        return sample;
    }
    
    internal static T[,] Sample<T>(T[,,] matrix, int x, int y, int z, int radius)
        where T : struct
    {
        #if DEBUG
        if (!InBounds(x, y, z, matrix)) throw new ArgumentOutOfRangeException($"X or Y are out of materix range (X:{matrix.GetLength(1)}, Y:{matrix.GetLength(0)})");
        #endif
        
        var sample = new T[radius * 2 + 1, radius * 2 + 1];
        
        for (var i = 0; i < radius * 2 + 1; i++)
        {
            for (var k = 0; k < radius * 2 + 1; k++)
            {
                var mx = x - radius + i;
                var my = y - radius + k;

                if (mx < 0 || mx >= matrix.GetLength(1) ||
                    my < 0 || my >= matrix.GetLength(0))
                {
                    sample[k, i] = default;
                    continue;
                }

                sample[k, i] = matrix[my, mx, z];
            }
        }

        return sample;
    }


    internal static (bool left, bool top, bool right, bool bottom) GetConnectionNode<T> (T[,,] matrix, Data.Coords coords, Func<T, bool> predicate)
        where T : struct
    {
        var sample = Sample(matrix, coords.X, coords.Y, coords.Z, 1);

        var left = predicate(sample[1, 0]);
        var top = predicate(sample[0, 1]);
        var right = predicate(sample[1, 2]);
        var bottom = predicate(sample[2, 1]);

        return (left, top, right, bottom);
    }

    public static bool EffectCoversScreen(string effect) => effect switch {
        "BlackGoo" or "Super BlackGoo" => true,
        _ => false
    };
            
    public static int GetEffectBrushStrength(string effect) => effect switch
    {
        "BlackGoo" or "Fungi Flowers" or "Lighthouse Flowers" or
            "Fern" or "Giant Mushroom" or "Sprawlbush" or
            "featherFern" or "Fungus Tree" or "Restore As Scaffolding" or "Restore As Pipes" or "Super BlackGoo" => 100,

        _ => 10
    };

    public static bool IsEffectBruhConstrained(string effect) => effect switch
    {
        "Fungi Flowers" or "Lighthouse Flowers" or "Fern" or "Giant Mushroom" or 
            "Sprawlbush" or "featherFern" or "Fungus Tree" => true,
        _ => false
    };

    public static EffectOptions[] NewEffectOptions(string name)
    {
        EffectOptions[] options = name switch
        {
            "Slime" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("3D", ["Off", "On"], "Off"),
                new("Affect Gradients and Decals", ["Yes", "No"], "Yes")
            ],
            
            "LSime" => [new("3D", ["Off", "On"], "Off")],
            
            "Fat Slime" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("3D", ["Off", "On"], "Off"),
                new("Affect Gradients and Decals", ["Yes", "No"], "Yes")
            ],
            
            "Scales" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All")
            ],
            
            "SlimeX3" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("3D", ["Off", "On"], "Off"),
                new("Affect Gradients and Decals", ["Yes", "No"], "Yes")
            ],
            
            "DecalsOnlySlime" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All")
            ],
            
            "Melt" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Affect Gradients and Decals", ["Yes", "No"], "No")
            ],
            
            "Rust" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("3D", ["Off", "On"], "Off"),
                new("Affect Gradients and Decals", ["Yes", "No"], "No")
            ],
            
            "Barnacles" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("3D", ["Off", "On"], "Off"),
                new("Affect Gradients and Decals", ["Yes", "No"], "No")
            ],
            
            "Colored Barnacles" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("3D", ["Off", "On"], "Off"),
                new("Effect Color", ["EffectColor1", "EffectColor2", "None"], "EffectColor2")
            ],
            
            "Clovers" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("3D", ["Off", "On"], "Off"),
                new("Color", ["Color1", "Color2", "Dead"], "Color2")
            ],
            
            "Ivy" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Color", ["Color1", "Color2", "Dead"], "Color2"),
                new("Color Intensity", ["High", "Medium", "Low", "None", "Random"], "Medium"),
                new("Fruit Density", ["High", "Medium", "Low", "None"], "None"),
                new("Leaf Density", [], new Random().Next(100))
            ],
            
            "Little Flowers" => [
                new("Color", ["Color1", "Color2", "Dead"], "Color2"),
                new("Detail Color", ["Color1", "Color2", "Dead"], "Color2"),
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Rotate", ["Off", "On"], "Off")
            ],
            
            "Erode" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All")
            ],
            
            "Sand" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Effect Color", ["EffectColor1", "EffectColor2", "None"], "None")
            ],
            
            "Super Erode" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
            ],
            
            "Ultra Super Erode" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All")
            ],
            
            "Roughen" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All")
            ],
            
            "Impacts" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All")
            ],
            
            "Super Melt" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("3D", ["Off", "On"], "Off"),
                new("Affect Gradients and Decals", ["Yes", "No"], "No")
            ],
            
            "Destructive Melt" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("3D", ["Off", "On"], "Off"),
                new("Affect Gradients and Decals", ["Yes", "No"], "No")
            ],
            
            "Rubble" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All")
            ],
            
            "Colored Rubble" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Effect Color", ["EffectColor1", "EffectColor2", "None"], "EffectColor2")
            ],
            
            "Fungi Flowers" or "Lighthouse Flowers" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "1")
            ],
            
            "Colored Fungi Flowers" or "Colored Lighthouse Flowers" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "1"),
                new("Color", ["Color1", "Color2", "Dead"], "Color2")
            ],
            
            "Foliage" or "High Grass" or "High Fern" or "Mistletoe" or "Reeds" or "Lavenders" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Color", ["Color1", "Color2", "Dead"], "Color2")
            ],
            
            "Ring Chains" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Effect Color", ["EffectColor1", "EffectColor2", "None"], "None")
            ],
            
            "Assorted Trash" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Effect Color", ["EffectColor1", "EffectColor2", "None"], "None")
            ],
            
            "Fern" or "Giant Mushroom" or "Sprawlbush" or "featherFern" or "Fungus Tree" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "1"),
                new("Color", ["Color1", "Color2", "Dead"], "Color2")
            ],
            
            "Root Grass" or "Growers" or "Cacti" or "Rain Moss" or "Dense Mold" or 
                "Seed Pods" or "Grass" or "Arm Growers" or "Horse Tails" or "Circuit Plants" or 
                "Feather Plants" or "Mini Growers" or "Left Facing Kelp" or "Right Facing Kelp" or 
                "Club Moss" or "Moss Wall" or "Mixed Facing Kelp" or "Bubble Grower" or
                "Storm Plants" or "Seed Grass" or "Hyacinths" or "Orb Plants" or "Dandelions" or "Og Grass" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Color", ["Color1", "Color2", "Dead"], "Color2")
            ],
            
            "Rollers" or "Thorn Growers" or "Garbage Spirals" or "Spinets" or "Small Springs" or "Fuzzy Growers" or
            "Leaf Growers" or "Meat Growers" or "Thunder Growers" or "Ice Growers" or "Grass Growers" or "Fancy Growers" or "Horror Growers" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Color", ["Color1", "Color2", "Dead"], "Color2")
            ],
            
            "Wires" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Fatness", ["1px", "2px", "3px", "random"], "2px")
            ],
            
            "Chains" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Size", ["Small", "FAT"], "Small")
            ],
            
            "Colored Wires" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Fatness", ["1px", "2px", "3px", "random"], "2px"),
                new("Effect Color", ["EffectColor1", "EffectColor2", "None"], "EffectColor2")
            ],
            
            "Colored Chains" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Size", ["Small", "FAT"], "Small"),
                new("Effect Color", ["EffectColor1", "EffectColor2", "None"], "EffectColor2")
            ],
            
            "DarkSlime" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
            ],
            
            "Hang Roots" or "Thick Roots" or "Shadow Plants" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All")
            ],
            
            "Colored Hang Roots" or "Colored Thick Roots" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Color", ["Color1", "Color2", "Dead"], "Color2")
            ],
            
            "Colored Shadow Plants" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Color", ["Color1", "Color2", "Dead"], "Color2")
            ],
            
            "Root Plants" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Color", ["Color1", "Color2", "Dead"], "Color2")
            ],
            
            "Restore As Scaffolding" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All")
            ],
            
            "Restore As Pipes" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All")
            ],
            
            "Ceramic Chaos" => [
                new("Ceramic Color", ["None", "Colored"], "Colored"),
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All")
            ],
            
            "DaddyCorruption" => [
                new("Color", ["Color1", "Color2", "Dead"], "Color2"),
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All")
            ],
            
            "Wastewater Mold" => [
                new("Color", ["Color1", "Color2", "Dead"], "Color2"),
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All")
            ],
            
            "Corruption No Eye" or "Slag" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All")
            ],
            
            "Stained Glass Properties" => [
                new("Variation", ["1", "2", "3"], "1"),
                new("Color 1", ["EffectColor1", "EffectColor2", "None"], "EffectColor1"),
                new("Color 2", ["EffectColor1", "EffectColor2", "None"], "EffectColor2")
            ],

            "Grape Roots" or "Hand Growers" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Color", ["Color1", "Color2", "Dead"], "Color2")
            ],
            
            _ => []
        };

        return [
            new("Delete/Move", ["Delete", "Move Back", "Move Forth"], ""),
            new("Seed", [], new Random().Next(1000)),
            ..options
        ];
    }
    
    public static int GetMiddle(int number)
    {
        if (number < 3) return 0;
        if (number % 2 == 0) return number / 2 - 1;
        return number / 2;
    }

    /// <summary>
    /// Determines the "middle" of a tile, which where the tile head is positioned.
    /// </summary>
    /// <param name="init">a reference to the tile definition</param>
    internal static Vector2 GetTileHeadOrigin(in TileDefinition init)
    {
        var (width, height) = init.Size;
        return new Vector2(GetMiddle(width), GetMiddle(height));
    }

    /// Maps a geo block id to a block texture index
    public static int GetBlockIndex(GeoType id) => id switch
    {
        GeoType.Solid => 0,
        GeoType.SlopeNE => 1,
        GeoType.SlopeNW => 2,
        GeoType.SlopeES => 3,
        GeoType.SlopeSW => 4,
        GeoType.Platform => 5,
        GeoType.ShortcutEntrance => 6,
        GeoType.Glass => 7,

        _ => -1,
    };

    /// Maps a UI texture index to block ID
    public static GeoType GetBlockID(uint index) => index switch
    {
        0 => GeoType.Solid,
        1 => GeoType.Air,
        7 => GeoType.SlopeES,
        3 => GeoType.SlopeNE,
        2 => GeoType.SlopeSW,
        6 => GeoType.Platform,
        30 => GeoType.Glass,
        _ => GeoType.Air
    };


    /// Maps a UI texture index to a stackable ID
    public static GeoFeature GetStackableID(uint index) => index switch
    {
        28 => GeoFeature.PlaceRock,
        29 => GeoFeature.PlaceSpear,
        9 => GeoFeature.CrackedTerrain,
        10 => GeoFeature.HorizontalPole,
        11 => GeoFeature.VerticalPole,
        14 => GeoFeature.ShortcutEntrance,
        15 => GeoFeature.ShortcutPath,
        16 => GeoFeature.DragonDen,
        17 => GeoFeature.RoomEntrance,
        18 => GeoFeature.Bathive,
        19 => GeoFeature.Waterfall,
        20 => GeoFeature.ScavengerHole,
        21 => GeoFeature.WackAMoleHole,
        22 => GeoFeature.GarbageWormHole,
        23 => GeoFeature.WormGrass,
        24 => GeoFeature.ForbidFlyChains,

        _ => GeoFeature.None
    };

    public static int GetStackableTextureIndex(GeoFeature id) => id switch
    {
        GeoFeature.HorizontalPole => 0,
        GeoFeature.VerticalPole => 1,
        GeoFeature.Bathive => 2,
        GeoFeature.ShortcutPath => 3,
        GeoFeature.RoomEntrance => 27,
        GeoFeature.DragonDen => 28,
        GeoFeature.PlaceRock => 18,
        GeoFeature.PlaceSpear => 29,
        GeoFeature.ForbidFlyChains => 30,
        GeoFeature.GarbageWormHole => 16,
        GeoFeature.Waterfall => 19,
        GeoFeature.WackAMoleHole => 20,
        GeoFeature.WormGrass => 21,
        GeoFeature.ScavengerHole => 17,
        
        _ => -1
    };

    /// <summary>
    /// This is used to determine the index of the stackable texture, including the directional ones.
    /// </summary>
    /// <param name="id">the ID of the geo-tile feature</param>
    /// <param name="context">a 3x3 slice of the geo-matrix where the geo-tile feature is in the middle</param>
    /// <returns>the index of the texture in the textures array (GLOBALS.Textures.GeoStackables)</returns>
    public static int GetStackableTextureIndex(int id, Geo[][] context)
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
                ((
                    context[0][0].Features | context[0][1].Features  | context[0][2].Features |
                    context[1][0].Features                           | context[1][2].Features |
                    context[2][0].Features | context[2][1].Features  | context[2][2].Features
                ) & GeoFeature.RoomEntrance)
                
                == GeoFeature.RoomEntrance
            ) return 26;

            var pattern = (
                false, context[0][1][5] ^ context[0][1][6] ^ context[0][1][7] ^ context[0][1][19] ^ context[0][1][21], false,
                context[1][0][5] ^ context[1][0][6] ^ context[1][0][7] ^ context[1][0][19] ^ context[1][0][21], false, context[1][2][5] ^ context[1][2][6] ^ context[1][2][7] ^ context[1][2][19] ^ context[1][2][21],
                false, context[2][1][5] ^ context[2][1][6] ^ context[2][1][7] ^ context[2][1][19] ^ context[2][1][21], false
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
                context[0][0].Type, context[0][1].Type, context[0][2].Type,
                context[1][0].Type, 0,                 context[1][2].Type,
                context[2][0].Type, context[2][1].Type, context[2][2].Type
            );

            directionIndex = geoPattern switch
            {

                (
                    GeoType.Solid,             _, GeoType.Solid,
                    GeoType.Solid,             _, GeoType.Solid,
                    GeoType.Solid, GeoType.Solid, GeoType.Solid
                ) => (context[0][1].Type is GeoType.Air or GeoType.Platform 
                    && !(context[1][0][5] || context[1][0][6] || context[1][0][7] || context[1][0][19] || context[1][0][21])
                    && !(context[1][2][5] || context[1][2][6] || context[1][2][7] || context[1][2][19] || context[1][2][21])) ? directionIndex : 26,

                (
                    GeoType.Solid, GeoType.Solid, GeoType.Solid,
                    GeoType.Solid, _,          _,
                    GeoType.Solid, GeoType.Solid, GeoType.Solid
                ) => (context[1][2].Type is GeoType.Air or GeoType.Platform 
                    && !(context[0][1][5] || context[0][1][6] || context[0][1][7] || context[0][1][19] || context[0][1][21])
                    && !(context[2][1][5] || context[2][1][6] || context[2][1][7] || context[2][1][19] || context[2][1][21])) ? directionIndex : 26,

                (
                    GeoType.Solid, GeoType.Solid, GeoType.Solid,
                    GeoType.Solid,             _, GeoType.Solid,
                    GeoType.Solid,             _, GeoType.Solid
                ) => (context[2][1].Type is GeoType.Air or GeoType.Platform 
                    && !(context[1][0][5] || context[1][0][6] || context[1][0][7] || context[1][0][19] || context[1][0][21])
                    && !(context[1][2][5] || context[1][2][6] || context[1][2][7] || context[1][2][19] || context[1][2][21])) ? directionIndex : 26,

                (
                    GeoType.Solid, GeoType.Solid, GeoType.Solid,
                                _,             _, GeoType.Solid,
                    GeoType.Solid, GeoType.Solid, GeoType.Solid
                ) => (context[1][0].Type is GeoType.Air or GeoType.Platform 
                    && !(context[0][1][5] || context[0][1][6] || context[0][1][7] || context[0][1][19] || context[0][1][21])
                    && !(context[2][1][5] || context[2][1][6] || context[2][1][7] || context[2][1][19] || context[2][1][21])) ? directionIndex : 26,

                _ => 26
            };

            return directionIndex;
        }
        else if (i == -11)
        {
            i = (
                false, context[0][1][11], false,
                context[1][0][11], false, context[1][2][11],
                false, context[2][1][11], false
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
                false, context[0][1].Type == GeoType.Solid, false,
                context[1][0].Type == GeoType.Solid, context[1][1].Type == GeoType.Solid, context[1][2].Type == GeoType.Solid,
                false, context[2][0].Type == GeoType.Solid, false
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
    public static int GetCorrectSlopeID(Geo[][] context)
    {
        var fi = (
            false, context[0][1].Type is GeoType.Solid, false,
            context[1][0].Type is GeoType.Solid, false, context[1][2].Type is GeoType.Solid,
            false, context[2][1].Type is GeoType.Solid, false
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

        if (fi == -1) return -1;

        var ssi = context[0][1].Type is GeoType.SlopeES or GeoType.SlopeNE or GeoType.SlopeNW or GeoType.SlopeSW || 
                  context[1][0].Type is GeoType.SlopeES or GeoType.SlopeNE or GeoType.SlopeNW or GeoType.SlopeSW || 
                  context[1][2].Type is GeoType.SlopeES or GeoType.SlopeNE or GeoType.SlopeNW or GeoType.SlopeSW ||
                  context[2][1].Type is GeoType.SlopeES or GeoType.SlopeNE or GeoType.SlopeNW or GeoType.SlopeSW;

        if (ssi) return -1;

        return fi;
    }

    internal static T[,,] Resize<T>(T[,,] matrix, 
        int left,
        int top,
        int right,
        int bottom)

        where T : new()
    {
        if (left == 0 && top == 0 && right == 0 && bottom == 0) return matrix;
        if (-left == matrix.GetLength(1) || -right == matrix.GetLength(1) ||
            -top == matrix.GetLength(0) || -bottom == matrix.GetLength(0)) return matrix;

        var width = matrix.GetLength(1);
        var height = matrix.GetLength(0);

        var newWidth = width + left + right;
        var newHeight = height + top + bottom;

        var depth = matrix.GetLength(2);

        T[,,] newMatrix = new T[newHeight, newWidth, depth];

        // Default value

        for (var y = 0; y < newHeight; y++) {
            for (var x = 0; x < newWidth; x++) {
                for (var z = 0; z < depth; z++) {
                    newMatrix[y, x, z] = new T();
                }
            }
        } 

        // Copy old matrix to new matrix

        for (var y = 0; y < height; y++) {
            var ny = y + top;

            if (ny >= newHeight) break;
            if (ny < 0) continue;

            for (var x = 0; x < width; x++) {
                var nx = x + left;

                if (nx >= newWidth) break;
                if (nx < 0) continue;

                for (var z = 0; z < depth; z++) {
                    newMatrix[ny, nx, z] = matrix[y, x, z];
                }
            }
        }

        return newMatrix;
    }

    internal static double[,] Resize(
        double[,] matrix,
        
        int left,
        int top,
        int right,
        int bottom
    ) {
        if (left == 0 && top == 0 && right == 0 && bottom == 0) return matrix;
        if (-left == matrix.GetLength(1) || -right == matrix.GetLength(1) ||
            -top == matrix.GetLength(0) || -bottom == matrix.GetLength(0)) return matrix;

        var width = matrix.GetLength(1);
        var height = matrix.GetLength(0);

        var newWidth = width + left + right;
        var newHeight = height + top + bottom;

        double[,] newMatrix = new double[newHeight, newWidth];

        // Copy old matrix to new matrix

        for (var y = 0; y < height; y++) {
            var ny = y + top;

            if (ny >= newHeight) break;
            if (ny < 0) continue;

            for (var x = 0; x < width; x++) {
                var nx = x + left;

                if (nx >= newWidth) break;
                if (nx < 0) continue;

                //

                newMatrix[ny, nx] = matrix[y, x];
            }
        }

        return newMatrix;
    }

    internal static Data.Color[,,] Resize(
        Data.Color[,,] matrix,
        
        int left,
        int top,
        int right,
        int bottom,

        Data.Color layer1Fill,
        Data.Color layer2Fill,
        Data.Color layer3Fill
    ) {
        if (left == 0 && top == 0 && right == 0 && bottom == 0) return matrix;
        if (-left == matrix.GetLength(1) || -right == matrix.GetLength(1) ||
            -top == matrix.GetLength(0) || -bottom == matrix.GetLength(0)) return matrix;

        var width = matrix.GetLength(1);
        var height = matrix.GetLength(0);

        var newWidth = width + left + right;
        var newHeight = height + top + bottom;

        var depth = matrix.GetLength(2);

        Data.Color[,,] newMatrix = new Data.Color[newHeight, newWidth, depth];

        // Default value

        for (var y = 0; y < newHeight; y++) {
            for (var x = 0; x < newWidth; x++) {
                newMatrix[y, x, 0] = layer1Fill;
                newMatrix[y, x, 1] = layer2Fill;
                newMatrix[y, x, 2] = layer3Fill;
            }
        } 

        // Copy old matrix to new matrix

        for (var y = 0; y < height; y++) {
            var ny = y + top;

            if (ny >= newHeight) break;
            if (ny < 0) continue;

            for (var x = 0; x < width; x++) {
                var nx = x + left;

                if (nx >= newWidth) break;
                if (nx < 0) continue;

                //

                newMatrix[ny, nx, 0] = matrix[y, x, 0];
                newMatrix[ny, nx, 1] = matrix[y, x, 1];
                newMatrix[ny, nx, 2] = matrix[y, x, 2];
            }
        }

        return newMatrix;
    }

    internal static Geo[,,] Resize(
        Geo[,,] matrix,
        
        int left,
        int top,
        int right,
        int bottom,

        Geo layer1Fill,
        Geo layer2Fill,
        Geo layer3Fill
    ) {
        if (left == 0 && top == 0 && right == 0 && bottom == 0) return matrix;
        if (-left == matrix.GetLength(1) || -right == matrix.GetLength(1) ||
            -top == matrix.GetLength(0) || -bottom == matrix.GetLength(0)) return matrix;

        var width = matrix.GetLength(1);
        var height = matrix.GetLength(0);

        var newWidth = width + left + right;
        var newHeight = height + top + bottom;

        var depth = matrix.GetLength(2);

        Geo[,,] newMatrix = new Geo[newHeight, newWidth, depth];

        // Default value

        for (var y = 0; y < newHeight; y++) {
            for (var x = 0; x < newWidth; x++) {
                newMatrix[y, x, 0] = layer1Fill;
                newMatrix[y, x, 1] = layer2Fill;
                newMatrix[y, x, 2] = layer3Fill;
            }
        } 

        // Copy old matrix to new matrix

        for (var y = 0; y < height; y++) {
            var ny = y + top;

            if (ny >= newHeight) break;
            if (ny < 0) continue;

            for (var x = 0; x < width; x++) {
                var nx = x + left;

                if (nx >= newWidth) break;
                if (nx < 0) continue;

                //

                newMatrix[ny, nx, 0] = matrix[y, x, 0];
                newMatrix[ny, nx, 1] = matrix[y, x, 1];
                newMatrix[ny, nx, 2] = matrix[y, x, 2];
            }
        }

        return newMatrix;
    }

    internal static Geo[,,] Resize(Geo[,,] array, int width, int height, int newWidth, int newHeight, Geo[] layersFill)
    {

        Geo[,,] newArray = new Geo[newHeight, newWidth, 3];

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

    internal static Data.Color[,,] NewMaterialColorMatrix(int width, int height, Data.Color @default)
    {
        Data.Color[,,] matrix = new Data.Color[height, width, 3];

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
    internal static Geo[][] GetContext(Geo[,,] matrix, int width, int height, int x, int y, int z) =>
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
    internal static Geo[][] GetContext(Geo[,] matrix, int x, int y) =>
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


    [Obsolete]
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

        return newArray;
    }
    
    /// <summary>
    /// Generic resize method of a 3D array (with the z dimension being exactly 3).
    /// </summary>
    /// <param name="array">The matrix</param>
    /// <param name="newWidth">new matrix width</param>
    /// <param name="newHeight">new matrix height</param>
    /// <typeparam name="T">a 3-length list (representing the three level layers) of geo IDs to fill extra space with</typeparam>
    /// <returns>a new matrix with <paramref name="newWidth"/> and <paramref name="newHeight"/> as the new dimensions</returns>
    internal static T[,,] Resize<T>(T[,,] array, int newWidth, int newHeight)

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
                        newArray[y, x, 0] = default!;
                        newArray[y, x, 1] = default!;
                        newArray[y, x, 2] = default!;
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
                        newArray[y, x, 0] = default!;
                        newArray[y, x, 1] = default!;
                        newArray[y, x, 2] = default!;
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
                        newArray[y, x, 0] = default!;
                        newArray[y, x, 1] = default!;
                        newArray[y, x, 2] = default!;
                    }
                }

                for (int y = height; y < newHeight; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        newArray[y, x, 0] = default!;
                        newArray[y, x, 1] = default!;
                        newArray[y, x, 2] = default!;
                    }
                }
            }
        }

        return newArray;
    }
    
    /// <summary>
    /// Generic resize method of a 2D array.
    /// </summary>
    /// <param name="array">The matrix</param>
    /// <param name="newWidth">new matrix width</param>
    /// <param name="newHeight">new matrix height</param>
    /// <typeparam name="T">a 3-length list (representing the three level layers) of geo IDs to fill extra space with</typeparam>
    /// <returns>a new matrix with <paramref name="newWidth"/> and <paramref name="newHeight"/> as the new dimensions</returns>
    internal static T[,] Resize<T>(T[,] array, int newWidth, int newHeight)
    {
        var width = array.GetLength(1);
        var height = array.GetLength(0);
        
        var newArray = new T[newHeight, newWidth];

        // old height is larger
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
                        newArray[y, x] = default!;
                        newArray[y, x] = default!;
                        newArray[y, x] = default!;
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
                        newArray[y, x] = array[y, x];
                        newArray[y, x] = array[y, x];
                        newArray[y, x] = array[y, x];
                    }
                }

                for (int y = height; y < newHeight; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        newArray[y, x] = default!;
                        newArray[y, x] = default!;
                        newArray[y, x] = default!;
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
                        newArray[y, x] = array[y, x];
                        newArray[y, x] = array[y, x];
                        newArray[y, x] = array[y, x];
                    }

                    for (int x = width; x < newWidth; x++)
                    {
                        newArray[y, x] = default!;
                        newArray[y, x] = default!;
                        newArray[y, x] = default!;
                    }
                }

                for (int y = height; y < newHeight; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        newArray[y, x] = default!;
                        newArray[y, x] = default!;
                        newArray[y, x] = default!;
                    }
                }
            }
        }

        return newArray;
    }

    /// <summary>
    /// Determines where the tile preview texture starts in the tile texture.
    /// </summary>
    /// <param name="init">a reference to the tile definition</param>
    /// <returns>a number representing the y-depth value where the tile preview starts</returns>
    internal static int GetTilePreviewStartingHeight(in TileDefinition init)
    {
        var (width, height) = init.Size;
        var bufferTiles = init.BufferTiles;
        var repeatL = init.Repeat.Length;
        var scale = GLOBALS.Scale;

        var offset = init.Type switch
        {
            Data.Tiles.TileType.VoxelStruct => scale * ((bufferTiles * 2) + height) * repeatL,
            Data.Tiles.TileType.VoxelStructRockType => scale * ((bufferTiles * 2) + height),
            Data.Tiles.TileType.Box => scale * height * width + (scale * (height + (2 * bufferTiles))),
            Data.Tiles.TileType.VoxelStructRandomDisplaceVertical => scale * ((bufferTiles * 2) + height) * repeatL,
            Data.Tiles.TileType.VoxelStructRandomDisplaceHorizontal => scale * ((bufferTiles * 2) + height) * repeatL,

            _ => scale * ((bufferTiles * 2) + height) * repeatL
        };

        return offset;
    }

    internal static Rectangle EncloseQuads(Data.Quad quads)
    {
        var nearestX = Math.Min(Math.Min(quads.TopLeft.X, quads.TopRight.X), Math.Min(quads.BottomLeft.X, quads.BottomRight.X));
        var nearestY = Math.Min(Math.Min(quads.TopLeft.Y, quads.TopRight.Y), Math.Min(quads.BottomLeft.Y, quads.BottomRight.Y));

        var furthestX = Math.Max(Math.Max(quads.TopLeft.X, quads.TopRight.X), Math.Max(quads.BottomLeft.X, quads.BottomRight.X));
        var furthestY = Math.Max(Math.Max(quads.TopLeft.Y, quads.TopRight.Y), Math.Max(quads.BottomLeft.Y, quads.BottomRight.Y));
       
        return new Rectangle(nearestX, nearestY, furthestX - nearestX, furthestY - nearestY);
    }
    
    [Obsolete]
    internal static Rectangle EncloseQuads(in Data.Quad quads)
    {
        var nearestX = Math.Min(Math.Min(quads.TopLeft.X, quads.TopRight.X), Math.Min(quads.BottomLeft.X, quads.BottomRight.X));
        var nearestY = Math.Min(Math.Min(quads.TopLeft.Y, quads.TopRight.Y), Math.Min(quads.BottomLeft.Y, quads.BottomRight.Y));

        var furthestX = Math.Max(Math.Max(quads.TopLeft.X, quads.TopRight.X), Math.Max(quads.BottomLeft.X, quads.BottomRight.X));
        var furthestY = Math.Max(Math.Max(quads.TopLeft.Y, quads.TopRight.Y), Math.Max(quads.BottomLeft.Y, quads.BottomRight.Y));
       
        return new Rectangle(nearestX, nearestY, furthestX - nearestX, furthestY - nearestY);
    }
    
    internal static Rectangle EncloseProps(IEnumerable<Data.Quad> quadsList)
    {
        float 
            minX = float.MaxValue, 
            minY = float.MaxValue, 
            maxX = float.MinValue, 
            maxY = float.MinValue;

        foreach (var q in quadsList)
        {
            minX = Math.Min(
                minX, 
                Math.Min(
                    Math.Min(q.TopLeft.X, q.TopRight.X), 
                    Math.Min(q.BottomLeft.X, q.BottomRight.X)
                )
            );
            
            minY = Math.Min(
                minY, 
                Math.Min(
                    Math.Min(q.TopLeft.Y, q.TopRight.Y), 
                    Math.Min(q.BottomLeft.Y, q.BottomRight.Y)
                )
            );
            
            maxX = Math.Max(
                maxX, 
                Math.Max(
                    Math.Max(q.TopLeft.X, q.TopRight.X), 
                    Math.Max(q.BottomLeft.X, q.BottomRight.X)
                )
            );
            
            maxY = Math.Max(
                maxY, 
                Math.Max(
                    Math.Max(q.TopLeft.Y, q.TopRight.Y), 
                    Math.Max(q.BottomLeft.Y, q.BottomRight.Y)
                )
            );
        }
        
        return new Rectangle(minX, minY, maxX - minX, maxY - minY);
    }
    
    internal static Rectangle EncloseProps(Data.Quad[] quadsList)
    {
        float 
            minX = float.MaxValue, 
            minY = float.MaxValue, 
            maxX = float.MinValue, 
            maxY = float.MinValue;

        foreach (var q in quadsList)
        {
            minX = Math.Min(
                minX, 
                Math.Min(
                    Math.Min(q.TopLeft.X, q.TopRight.X), 
                    Math.Min(q.BottomLeft.X, q.BottomRight.X)
                )
            );
            
            minY = Math.Min(
                minY, 
                Math.Min(
                    Math.Min(q.TopLeft.Y, q.TopRight.Y), 
                    Math.Min(q.BottomLeft.Y, q.BottomRight.Y)
                )
            );
            
            maxX = Math.Max(
                maxX, 
                Math.Max(
                    Math.Max(q.TopLeft.X, q.TopRight.X), 
                    Math.Max(q.BottomLeft.X, q.BottomRight.X)
                )
            );
            
            maxY = Math.Max(
                maxY, 
                Math.Max(
                    Math.Max(q.TopLeft.Y, q.TopRight.Y), 
                    Math.Max(q.BottomLeft.Y, q.BottomRight.Y)
                )
            );
        }
        
        return new Rectangle(minX, minY, maxX - minX, maxY - minY);
    }

    internal static Data.Quad RotatePropQuads(Data.Quad quads, float angle)
    {
        // Convert angle to radians

        var radian = float.DegreesToRadians(angle);
        
        var sinRotation = (float)Math.Sin(radian);
        var cosRotation = (float)Math.Cos(radian);
        
        // Enclose the quads

        var rect = EncloseQuads(quads);
        
        // Get the center of the rectangle

        var center = new Vector2(rect.X + rect.Width/2, rect.Y + rect.Height/2);

        // var center = new Vector2(0, 0);
        
        Vector2 newTopLeft, newTopRight, newBottomRight, newBottomLeft;

        { // Rotate the top left corner
            var x = quads.TopLeft.X;
            var y = quads.TopLeft.Y;

            var dx = x - center.X;
            var dy = y - center.Y;

            newTopLeft = new Vector2(
                center.X + dx * cosRotation - dy * sinRotation, 
                center.Y + dx * sinRotation + dy * cosRotation
            );
        }
        
        { // Rotate the top right corner
            var x = quads.TopRight.X;
            var y = quads.TopRight.Y;

            var dx = x - center.X;
            var dy = y - center.Y;

            newTopRight = new Vector2(center.X + dx * cosRotation - dy * sinRotation, center.Y + dx * sinRotation + dy * cosRotation);
        }
        
        { // Rotate the bottom right corner
            var x = quads.BottomRight.X;
            var y = quads.BottomRight.Y;

            var dx = x - center.X;
            var dy = y - center.Y;

            newBottomRight = new Vector2(center.X + dx * cosRotation - dy * sinRotation, center.Y + dx * sinRotation + dy * cosRotation);
        }
        
        { // Rotate the bottom left corner
            var x = quads.BottomLeft.X;
            var y = quads.BottomLeft.Y;

            var dx = x - center.X;
            var dy = y - center.Y;

            newBottomLeft = new Vector2(center.X + dx * cosRotation - dy * sinRotation, center.Y + dx * sinRotation + dy * cosRotation);
        }
        
        return new(newTopLeft, newTopRight, newBottomRight, newBottomLeft);
    }
    
    internal static Data.Quad RotatePropQuads(Data.Quad quads, float angle, Vector2 center)
    {
        // Convert angle to radians

        var radian = float.DegreesToRadians(angle);
        
        var sinRotation = (float)Math.Sin(radian);
        var cosRotation = (float)Math.Cos(radian);
        
        Vector2 newTopLeft, newTopRight, newBottomRight, newBottomLeft;

        { // Rotate the top left corner
            var x = quads.TopLeft.X;
            var y = quads.TopLeft.Y;

            var dx = x - center.X;
            var dy = y - center.Y;
            
            newTopLeft = new Vector2(
                center.X + dx * cosRotation - dy * sinRotation, 
                center.Y + dx * sinRotation + dy * cosRotation
            );
            
            //Console.WriteLine(newTopLeft);
        }
        
        { // Rotate the top right corner
            var x = quads.TopRight.X;
            var y = quads.TopRight.Y;

            var dx = x - center.X;
            var dy = y - center.Y;

            newTopRight = new Vector2(
                center.X + dx * cosRotation - dy * sinRotation, 
                center.Y + dx * sinRotation + dy * cosRotation
            );
        }
        
        { // Rotate the bottom right corner
            var x = quads.BottomRight.X;
            var y = quads.BottomRight.Y;

            var dx = x - center.X;
            var dy = y - center.Y;

            newBottomRight = new Vector2(
                center.X + dx * cosRotation - dy * sinRotation, 
                center.Y + dx * sinRotation + dy * cosRotation
            );
        }
        
        { // Rotate the bottom left corner
            var x = quads.BottomLeft.X;
            var y = quads.BottomLeft.Y;

            var dx = x - center.X;
            var dy = y - center.Y;

            newBottomLeft = new Vector2(
                center.X + dx * cosRotation - dy * sinRotation, 
                center.Y + dx * sinRotation + dy * cosRotation
            );
        }
        
        return new(newTopLeft, newTopRight, newBottomRight, newBottomLeft);
    }
    
    internal static void RotatePoints(float angle, Vector2 center, Vector2[] points) {
        // Convert angle to radians

        var radian = float.DegreesToRadians(angle);
        
        var sinRotation = (float)Math.Sin(radian);
        var cosRotation = (float)Math.Cos(radian);
        
        for (var p = 0; p < points.Length; p++)
        {
            ref var point = ref points[p];

            var dx = point.X - center.X;
            var dy = point.Y - center.Y;

            point.X = center.X + dx * cosRotation - dy * sinRotation;
            point.Y = center.Y + dx * sinRotation + dy * cosRotation;
        }
    }

    internal static Vector2 QuadsCenter(ref Data.Quad quads)
    {
        var rect = EncloseQuads(quads);
        return new(rect.X + rect.Width/2, rect.Y + rect.Height/2);
    }

    internal static Vector2 RectangleCenter(ref Rectangle rectangle) => new(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2);

    [Obsolete]
    internal static void ScaleQuads(ref Data.Quad quads, float factor)
    {
        var enclose = EncloseQuads(quads);
        var center = RectangleCenter(ref enclose);
        
        quads.TopLeft = Raymath.Vector2Add(
            Raymath.Vector2Scale(
                Raymath.Vector2Subtract(quads.TopLeft, center), 
                factor
            ), 
            center
        );
                        
        quads.TopRight = Raymath.Vector2Add(
            Raymath.Vector2Scale(
                Raymath.Vector2Subtract(quads.TopRight, center), 
                factor
            ), 
            center
        );
                        
        quads.BottomLeft = Raymath.Vector2Add(
            Raymath.Vector2Scale(
                Raymath.Vector2Subtract(quads.BottomLeft, center), 
                factor
            ), 
            center
        ); 
                        
        quads.BottomRight = Raymath.Vector2Add(
            Raymath.Vector2Scale(
                Raymath.Vector2Subtract(quads.BottomRight, center), 
                factor
            ), 
            center
        );
    }
    
    internal static void ScaleQuads(ref Data.Quad quads, Vector2 center, float factor)
    {
        quads.TopLeft = Raymath.Vector2Add(
            Raymath.Vector2Scale(
                Raymath.Vector2Subtract(quads.TopLeft, center), 
                factor
            ), 
            center
        );
                        
        quads.TopRight = Raymath.Vector2Add(
            Raymath.Vector2Scale(
                Raymath.Vector2Subtract(quads.TopRight, center), 
                factor
            ), 
            center
        );
                        
        quads.BottomLeft = Raymath.Vector2Add(
            Raymath.Vector2Scale(
                Raymath.Vector2Subtract(quads.BottomLeft, center), 
                factor
            ), 
            center
        ); 
                        
        quads.BottomRight = Raymath.Vector2Add(
            Raymath.Vector2Scale(
                Raymath.Vector2Subtract(quads.BottomRight, center), 
                factor
            ), 
            center
        );
    }

    internal static void ScaleQuadsX(ref Data.Quad quads, Vector2 center, float factor)
    {
        quads.TopLeft = quads.TopLeft with { X = (quads.TopLeft.X - center.X) * factor + center.X };
        quads.BottomLeft = quads.BottomLeft with { X = (quads.BottomLeft.X - center.X) * factor + center.X };
        quads.TopRight = quads.TopRight with { X = (quads.TopRight.X - center.X) * factor + center.X };
        quads.BottomRight = quads.BottomRight with { X = (quads.BottomRight.X - center.X) * factor + center.X };
    }

    internal static void ScaleQuadsY(ref Data.Quad quads, Vector2 center, float factor)
    {
        quads.TopLeft = quads.TopLeft with { Y = (quads.TopLeft.Y - center.Y) * factor + center.Y };
        quads.BottomLeft = quads.BottomLeft with { Y = (quads.BottomLeft.Y - center.Y) * factor + center.Y };
        quads.TopRight = quads.TopRight with { Y = (quads.TopRight.Y - center.Y) * factor + center.Y };
        quads.BottomRight = quads.BottomRight with { Y = (quads.BottomRight.Y - center.Y) * factor + center.Y };
    }

    [Obsolete]
    internal static (Vector2 pA, Vector2 pB) RopeEnds(in Data.Quad quads)
    {
        return (
            Raymath.Vector2Divide(Raymath.Vector2Add(quads.TopLeft, quads.BottomLeft), new(2f, 2f)), 
            Raymath.Vector2Divide(Raymath.Vector2Add(quads.TopRight, quads.BottomRight), new(2f, 2f))
        );
    }
    
    internal static (Vector2 pA, Vector2 pB) RopeEnds(Data.Quad quads)
    {
        return (
            Raymath.Vector2Divide(Raymath.Vector2Add(quads.TopLeft, quads.BottomLeft), new(2f, 2f)), 
            Raymath.Vector2Divide(Raymath.Vector2Add(quads.TopRight, quads.BottomRight), new(2f, 2f))
        );
    }

    internal static (Vector2 left, Vector2 top, Vector2 right, Vector2 bottom) LongSides(in Data.Quad quad)
    {
        var (left, right) = RopeEnds(quad);

        var top = new Vector2((quad.TopLeft.X + quad.TopRight.X)/2f, (quad.TopLeft.Y + quad.TopRight.Y)/2f);
        var bottom = new Vector2((quad.BottomLeft.X + quad.BottomRight.X)/2f, (quad.BottomLeft.Y + quad.BottomRight.Y)/2f);
        
        return (left, top, right, bottom);
    }
    
    internal static Vector2[] GenerateRopePoints(in Vector2 pointA, in Vector2 pointB, int count = 3)
    {
        var distance = Raymath.Vector2Distance(pointA, pointB);

        var delta = distance / count;

        List<Vector2> points = [];
        
        for (var step = 0; step < count; step++) points.Add(Raymath.Vector2MoveTowards(pointA, pointB, delta * step));

        return [..points];
    }

    internal static (int width, int height) GetPropSize(in InitPropBase prop)
    {
        return prop switch
        {
            InitVariedStandardProp variedStandard => (variedStandard.Size.x * 20, variedStandard.Size.y * 20),
            InitStandardProp standard => (standard.Size.x * 20, standard.Size.y * 20),
            InitVariedSoftProp variedSoft => variedSoft.SizeInPixels,
            InitVariedDecalProp variedDecal => variedDecal.SizeInPixels,
            _ => (-1, -1)
        };
    }

    internal static Vector2[] Casteljau(int steps, Vector2[] points) {
        Vector2 GetCasteljauPoint(int r, int i, double t) { 
            if(r == 0) return points[i];

            var p1 = GetCasteljauPoint(r - 1, i, t);
            var p2 = GetCasteljauPoint(r - 1, i + 1, t);

            return new Vector2((int) ((1 - t) * p1.X + t * p2.X), (int) ((1 - t) * p1.Y + t * p2.Y));
        }
        
        List<Vector2> tmp = [];
        
        for (double t = 0, i = 0; i < steps; i++, t = 1f/steps*i) { 
            tmp.Add(GetCasteljauPoint(points.Length-1, 0, t));
        }

        return [.. tmp];
    }


    internal static IEnumerable<(string, string)> GetShortcutStrings(object? obj)
    {
        if (obj is null) return [];

        var properties = obj.GetType().GetProperties();

        List<(string, string)> pairs = [];

        foreach (var property in properties)
        {
            if (property.PropertyType != typeof(KeyboardShortcut) && property.PropertyType != typeof(MouseShortcut)) continue;
            {
                var name = property.Name;
                
                var attr = (ShortcutName?) property
                    .GetCustomAttributes(typeof(ShortcutName), false)
                    .FirstOrDefault();

                var shortcut = property.GetValue(obj);
                    
                pairs.Add((attr?.Name ?? name ?? "Unknown", shortcut?.ToString() ?? ""));
            }
        }

        return pairs;
    }

    internal static Rectangle RecFromTwoVecs(Vector2 p1, Vector2 p2)
    {
        return new Rectangle(
            Math.Min(p1.X, p2.X),
            Math.Min(p1.Y, p2.Y),
            Math.Abs(p1.X - p2.X),
            Math.Abs(p1.Y - p2.Y)
        );
    }
    #nullable disable
}