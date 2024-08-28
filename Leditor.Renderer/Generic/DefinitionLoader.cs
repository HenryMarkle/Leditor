namespace Leditor.Renderer.Generic;

using Leditor.Serialization;
using Leditor.Data.Tiles;

using Raylib_cs;
using Leditor.Data.Generic;
using Leditor.Data;

public sealed class DefinitionLoader<T> 
    where T : IIdentifiable<string>, ITexture
{
    private readonly record struct ImagePair(T Tile, Image Image);
    private readonly record struct Pack(ImagePair[] Pairs, string Folder);

    /// <summary>
    /// Is set to true when _loadTask is completed.
    /// </summary>
    public bool IsReady { get; private set; }

    /// <summary>
    /// Indicates that the process is completed.
    /// </summary>
    public bool IsCompleted { get; private set; }

    public int TotalProgress { get; private set; }
    public int Progress { get; private set; }

    /// <summary>
    /// Is set by the constructor.
    /// </summary>
    private Task<List<Pack>>? _loadTask;

    private T[] _resultTiles;

    private int _packCursor;
    private int _tileCursor;
    private int _resultCursor;

    private Serilog.ILogger? Logger { get; init; }

    public DefinitionLoader(
        string[] folders, 
        Func<string, Serilog.ILogger?, Task<T[]>> asyncInitParser, 
        Serilog.ILogger? logger
    )
    {
        logger?.Information("[DefinitionLoader] Begin loading definitions");


        _loadTask = Task.Factory.StartNew(() => {
            List<(string folder, Task<T[]> task)> imported = [];
            var tileCount = 0;
            
            foreach (var folder in folders)
            {
                logger?.Information($"[DefinitionLoader] Loading definition in \"{folder}\"");

                if (!Directory.Exists(folder)) {
                    logger?.Error("[DefinitionLoader] Failed to load init; folder not found: \"{}\"", folder);
                    continue;
                }

                if (!Program.FileExistsInFolder(folder, "init.txt"))
                {
                    logger?.Error("[DefinitionLoader] init.txt was not found in folder \"{folder}\"", folder);
                    continue;
                }

                var initPath = Program.GetFilePathInFolder(folder, "init.txt");

                if (initPath is null) {
                    logger?.Error("[DefinitionLoader] init.txt was not found in folder \"{folder}\"", folder);
                    continue;
                }

                var t = asyncInitParser(initPath, logger);

                imported.Add((folder, t));
            }

            Task.WhenAll(imported.Select(i => i.task)).Wait();

            var filtered = imported
                .Where(t => {
                    if (t.task.IsFaulted) {
                        logger?.Error(t.task.Exception, "[DefinitionLoader] Failed to load and parse init.txt");
                        Console.WriteLine(t.task.Exception);
                    }

                    return t.task.IsCompletedSuccessfully;
                });

            List<Pack> packs = new(imported.Count);

            foreach (var t in filtered)
            {
                var imagePairTasks = t.task.Result
                    .Where(tile => {
                        var exists = Program.FileExistsInFolder(t.folder, $"{tile.Name}.png");

                        if (!exists)
                        {
                            Logger?.Error($"[DefinitionLoader] Could not find image for tile \"{tile.Name}\"; path does not exists: \"{Path.Combine(t.folder, $"{tile.Name}.png")}\"");
                        }

                        return exists;
                    })
                    .Select(tile => {
                        string imagePath = Program.GetFilePathInFolder(t.folder, $"{tile.Name}.png")!;

                        return (tile, Raylib.LoadImage(imagePath));
                    });

                List<ImagePair> imagePairs = new();

                foreach (var i in imagePairTasks)
                {
                    tileCount++;
                    imagePairs.Add(new ImagePair(i.Item1, i.Item2));
                }

                packs.Add(new Pack([..imagePairs], t.folder));
            }

            _resultTiles = new T[tileCount];
            TotalProgress = tileCount;
            
            IsReady = true;

            logger?.Information($"[DefinitionLoader] {tileCount} loaded");

            return packs;        
        });
        
        Logger = logger;
    }

    public bool LoadNext()
    {
        if (!IsReady || _loadTask is null || !_loadTask.IsCompleted) return false;

        if (_loadTask.IsFaulted)
        {
            Logger?.Error(_loadTask.Exception, $"[DefinitionLoader::LoadNext] Awaited load task has faulted");
            return true;
        }

        if (_packCursor >= _loadTask.Result.Count) {
            IsCompleted = true;
            return true;
        }

        var currentPack = _loadTask.Result[_packCursor];

        if (_tileCursor >= currentPack.Pairs.Length)
        {
            _tileCursor = 0;
            _packCursor++;
            return false;
        }

        var currentPair = currentPack.Pairs[_tileCursor];

        var texture = Raylib.LoadTextureFromImage(currentPair.Image);

        currentPair.Tile.Texture = texture;

        Raylib.UnloadImage(currentPair.Image);

        #if DEBUG
        if (_resultCursor >= _resultTiles.Length)
        {
            Logger?.Fatal("[DefinitionLoader::LoadNext] Result cursor was out of bounds");
            throw new IndexOutOfRangeException($"{nameof(_resultCursor)} was out of bounds ({_resultCursor})");
        }
        #endif

        _resultTiles[_resultCursor] = currentPair.Tile;
        _resultCursor++;
        _tileCursor++;

        Progress++;

        return false;
    }

    /// <summary>
    /// Must be called only when IsCompleted is true.
    /// </summary>
    /// <returns></returns>
    public T[] GetResult() => _resultTiles;
}