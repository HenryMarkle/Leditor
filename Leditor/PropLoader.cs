using Leditor.Data.Props.Definitions;
using Leditor.Data.Tiles;
using Color = Leditor.Data.Color;

namespace Leditor;

#nullable enable

internal sealed class PropLoader : IDisposable
{
    #region DisposePattern
    public bool Disposed { get; private set; }

    public void Dispose()
    {
        if (Disposed) return;
        Disposed = true;

        _propPackTasks = [];
        _propPacks.Clear();
    }
    #endregion
    
    private readonly struct PropPackInfo
    {
        internal string Directory { get; init; }
        internal (string name, Data.Color)[] Categories { get; init; }
        internal PropDefinition[][] Props { get; init; }
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

    private int _propPackLoadWaitCursor;

    private int _texturePackCursor;
    private int _textureCategoryCursor;
    private int _textureCursor;

    private int _propPackCursor;
    private int _propCategoryCursor;
    private int _propCursor;
    
    private readonly List<string> _packDirs = [];
    private Task<PropPackInfo>[] _propPackTasks = [];
    private readonly List<PropPackInfo> _propPacks = [];

    private readonly Data.Props.PropDexBuilder _builder = new();

    internal PropLoader(IEnumerable<string> initDirs, string initName = "Init.txt")
    {
        _initName = initName;
        
        foreach (var directory in initDirs)
        {
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException($"Props directory not found: {directory}");

            if (!File.Exists(Path.Combine(directory, _initName))) 
                throw new FileNotFoundException($"{_initName} file not found in directory {directory}");
            
            _packDirs.Add(directory);
        }
    }

    
    internal void Start()
    {
        if (_started) return;

        _propPackTasks = _packDirs.Select(directory => Task.Factory.StartNew(() =>
        {
            Task<((string, Color)[] categories, PropDefinition[][] props)> task =
                Serialization.PropImporter.ParseInitAsync(Path.Combine(directory, _initName));
            
            task.Wait();

            var textureArray = new RL.Managed.Texture2D[task.Result.props.Length][];
            
            for (var c = 0; c < task.Result.props.Length; c++)
            {
                var length = task.Result.props[c].Length;
                
                textureArray[c] = new RL.Managed.Texture2D[length];

                TotalProgress += length * 2;
            }
            
            return new PropPackInfo
            {
                Directory = directory,
                Categories = task.Result.categories,
                Props = task.Result.props,
                Textures = textureArray
            };
        })).ToArray();

        TotalProgress += _propPackTasks.Length;

        _started = true;
    }

    /// <summary>
    /// Include defined tiles that only need textures. 
    /// Must be called right after <see cref="Start"/>
    /// </summary>
    /// <param name="category"></param>
    /// <param name="props"></param>
    /// <param name="textureDir"></param>
    internal void IncludeDefined((string name, Color color) category, PropDefinition[] props, string textureDir) 
    {
        if (!_started) throw new InvalidOperationException("TileLoader hasn't started yet");;

        var pack = new PropPackInfo {
            Categories = [ category ],
            Props = [ props ],
            Directory = textureDir,
            Textures = [ new RL.Managed.Texture2D[props.Length] ]
        };

        TotalProgress += props.Length * 2 + 1;

        _propPackTasks = [.._propPackTasks, Task.FromResult(pack) ];
    }

    internal void IncludeTiles(TileDex dex) 
    {
        if (!_started) throw new InvalidOperationException("TileLoader hasn't started yet");

        _builder.RegisterTiles(dex.OrderedTileAsPropCategories, dex.OrderedTilesAsProps, true);
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
            if (_propPackLoadWaitCursor == _propPackTasks.Length)
            {
                _packLoadCompleted = true;
                return false;
            }

            var task = _propPackTasks[_propPackLoadWaitCursor++];

            task.Wait();
            
            _propPacks.Add(task.Result);
            
            return false;
        }

        if (!_textureLoadCompleted)
        {
            if (_textureCursor == _propPacks[_texturePackCursor].Textures[_textureCategoryCursor].Length)
            {
                _textureCursor = 0;
                _textureCategoryCursor++;
            }
            
            if (_textureCategoryCursor == _propPacks[_texturePackCursor].Textures.Length)
            {
                _textureCategoryCursor = 0;
                _textureCursor = 0;
                _texturePackCursor++;
            }
            
            if (_texturePackCursor == _propPacks.Count)
            {
                _textureLoadCompleted = true;
                return false;
            }

            

            var pack = _propPacks[_texturePackCursor];
            var tile = pack.Props[_textureCategoryCursor][_textureCursor];

            pack.Textures[_textureCategoryCursor][_textureCursor] = 
                new RL.Managed.Texture2D(Path.Combine(pack.Directory, $"{tile.Name}.png"));

            _textureCursor++;
            return false;
        }

        if (!_dexBuildCompleted)
        {
            if (_propCursor == _propPacks[_propPackCursor].Textures[_propCategoryCursor].Length)
            {
                _propCursor = 0;
                _propCategoryCursor++;
            }
            
            if (_propCategoryCursor == _propPacks[_propPackCursor].Textures.Length)
            {
                _propCategoryCursor = 0;
                _propCursor = 0;
                _propPackCursor++;
            }
            
            if (_propPackCursor == _propPacks.Count)
            {
                _dexBuildCompleted = true;
                return false;
            }

            var pack = _propPacks[_propPackCursor];
            var category = pack.Categories[_propCategoryCursor];
            var prop = pack.Props[_propCategoryCursor][_propCursor];
            var texture = pack.Textures[_propCategoryCursor][_propCursor];

            if (_propCursor == 0) _builder.Register(category.name, category.Item2);

            _builder.Register(category.name, prop);

            _propCursor++;
            return false;
        }

        Done = true;
        
        return true;
    }

    internal Task<Data.Props.PropDex> Build()
    {
        if (!Done) throw new InvalidOperationException("Loading isn't done yet");

        return Task.Factory.StartNew(() => _builder.Build());
    }
}