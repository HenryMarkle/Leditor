﻿namespace Leditor;

#nullable enable

internal sealed class TileLoader : IDisposable
{
    #region DisposePattern
    public bool Disposed { get; private set; }

    public void Dispose()
    {
        if (Disposed) return;
        Disposed = true;

        _tilePackTasks = [];
        _tilePacks.Clear();
    }
    #endregion
    
    private struct TilePackInfo
    {
        internal string Directory { get; init; }
        internal (string name, Data.Color)[] Categories { get; init; }
        internal Data.Tiles.TileDefinition[][] Tiles { get; init; }
        internal RL.Managed.Texture2D[][] Textures { get; init; }
    }

    internal bool Done { get; private set; }
    internal int TotalProgress { get; private set; }

    internal bool Started => _started;
    internal bool PackLoadCompleted => _packLoadCompleted;
    internal bool TextureLoadCompleted => _textureLoadCompleted;
    internal bool DexBuildCompleted => _dexBuildCompleted;
    
    private readonly string _initName;
    
    private bool _started;
    private bool _packLoadCompleted;
    private bool _textureLoadCompleted;
    private bool _dexBuildCompleted;

    private int _tilePackLoadWaitCursor;

    private int _texturePackCursor;
    private int _textureCategoryCursor;
    private int _textureCursor;

    private int _tilePackCursor;
    private int _tileCategoryCursor;
    private int _tileCursor;
    
    private readonly List<string> _packDirs = [];
    private Task<TilePackInfo>[] _tilePackTasks = [];
    private readonly List<TilePackInfo> _tilePacks = [];

    private readonly Data.Tiles.TileDexBuilder _builder = new();

    internal TileLoader(IEnumerable<string> initDirs, string initName = "Init.txt")
    {
        _initName = initName;
        
        foreach (var directory in initDirs)
        {
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException($"Tiles directory not found: {directory}");

            if (!File.Exists(Path.Combine(directory, _initName))) 
                throw new FileNotFoundException($"{_initName} file not found in directory {directory}");
            
            _packDirs.Add(directory);
        }
    }
    
    internal void Start()
    {
        if (_started) return;

        _tilePackTasks = _packDirs.Select(directory => Task.Factory.StartNew(() =>
        {
            Task<((string, Data.Color)[] categories, Data.Tiles.TileDefinition[][] tiles)> task = 
                Serialization.TileImporter.ParseInitAsync(Path.Combine(directory, _initName));
            
            task.Wait();

            var textureArray = new RL.Managed.Texture2D[task.Result.tiles.Length][];
            
            for (var c = 0; c < task.Result.tiles.Length; c++)
            {
                var length = task.Result.tiles[c].Length;
                
                textureArray[c] = new RL.Managed.Texture2D[length];

                TotalProgress += length * 2;
            }
            
            return new TilePackInfo
            {
                Directory = directory,
                Categories = task.Result.Item1,
                Tiles = task.Result.Item2,
                Textures = textureArray
            };
        })).ToArray();

        TotalProgress += _tilePackTasks.Length;

        _started = true;
    }

    /// <summary>
    /// Proceeds a single step
    /// </summary>
    /// <returns>true if the process is done; otherwise false</returns>
    /// <exception cref="InvalidOperationException">When calling the function before calling <see cref="Start"/></exception>
    internal bool Proceed()
    {
        if (Done) return true;
        
        if (!_started) throw new InvalidOperationException("TileLoader hasn't started yet");
        
        if (!_packLoadCompleted)
        {
            if (_tilePackLoadWaitCursor == _tilePackTasks.Length)
            {
                _packLoadCompleted = true;
                return false;
            }

            var task = _tilePackTasks[_tilePackLoadWaitCursor++];

            task.Wait();
            
            _tilePacks.Add(task.Result);
            
            return false;
        }

        if (!_textureLoadCompleted)
        {
            if (_textureCursor == _tilePacks[_texturePackCursor].Textures[_textureCategoryCursor].Length)
            {
                _textureCursor = 0;
                _textureCategoryCursor++;
            }
            
            if (_textureCategoryCursor == _tilePacks[_texturePackCursor].Textures.Length)
            {
                _textureCategoryCursor = 0;
                _textureCursor = 0;
                _texturePackCursor++;
            }
            
            if (_texturePackCursor == _tilePacks.Count)
            {
                _textureLoadCompleted = true;
                return false;
            }

            

            var pack = _tilePacks[_texturePackCursor];
            var tile = pack.Tiles[_textureCategoryCursor][_textureCursor];

            pack.Textures[_textureCategoryCursor][_textureCursor] = 
                new RL.Managed.Texture2D(Path.Combine(pack.Directory, $"{tile.Name}.png"));

            _textureCursor++;
            return false;
        }

        if (!_dexBuildCompleted)
        {
            if (_tileCursor == _tilePacks[_tilePackCursor].Textures[_tileCategoryCursor].Length)
            {
                _tileCursor = 0;
                _tileCategoryCursor++;
            }
            
            if (_tileCategoryCursor == _tilePacks[_tilePackCursor].Textures.Length)
            {
                _tileCategoryCursor = 0;
                _tileCursor = 0;
                _tilePackCursor++;
            }
            
            if (_tilePackCursor == _tilePacks.Count)
            {
                _dexBuildCompleted = true;
                return false;
            }

            var pack = _tilePacks[_tilePackCursor];
            var category = pack.Categories[_tileCategoryCursor];
            var tile = pack.Tiles[_tileCategoryCursor][_tileCursor];
            var texture = pack.Textures[_tileCategoryCursor][_tileCursor];

            if (_tileCursor == 0) _builder.Register(category.name, category.Item2);

            _builder.Register(category.name, tile, texture);

            _tileCursor++;
            return false;
        }

        Done = true;
        
        return true;
    }

    internal Task<Data.Tiles.TileDex> Build()
    {
        if (!Done) throw new InvalidOperationException("Loading isn't done yet");

        return Task.Factory.StartNew(() => _builder.Build());
    }
}