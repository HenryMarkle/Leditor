using System.Numerics;
using Leditor.Data.Tiles;

namespace Leditor;

#nullable enable

// TODO: A replacement of GLOBALS
public sealed class Context(Serilog.ILogger logger, TileDex tileDex) : IDisposable
{
    #region DisposePattern
    public bool Disposed { get; private set; }

    /// <summary>
    /// Must be called within GL context
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;

        Disposed = true;
        
        Raylib.UnloadRenderTexture(_lightMap);
        Raylib.UnloadRenderTexture(_levelRender);
        TileDex.Dispose();
    }

    ~Context()
    {
        if (!Disposed) throw new InvalidOperationException("Context was not disposed by the consumer");
    }
    #endregion

    private readonly Serilog.ILogger? _logger = logger;
    private int _page;
    private TileDex _tileDex = tileDex;
    private LevelState _level = new(72, 43, (12, 3, 12, 5));
    private string _projectPath = string.Empty;
    private Gram _gram = new(100);
    
    private RenderTexture2D _lightMap = Raylib.LoadRenderTexture(72 * 20 + 300, 43 * 20 + 300);
    private RenderTexture2D _levelRender = Raylib.LoadRenderTexture(72 * 20, 43 * 20);
    
    /// <summary>
    /// Must be set within GL context
    /// </summary>
    public RenderTexture2D LightMap
    {
        get => _lightMap;
        private set
        {
            Raylib.UnloadRenderTexture(_lightMap);
            _lightMap = value;
            _logger?.Debug("Light map is updated");
        }
    }

    public RenderTexture2D LevelRender
    {
        get => _levelRender;
        private set
        {
            Raylib.UnloadRenderTexture(_levelRender);
            _levelRender = value;
            _logger?.Debug("Level render texture is updated");
        }
    }
    
    public string ProjectPath
    {
        get => _projectPath;
        private set
        {
            _projectPath = value;
            _logger?.Debug($"Project path is set to {value}");
        }
    }

    public LevelState Level
    {
        get => _level;
        set
        {
            _level = value;
            _logger?.Debug("Level state is updated");
        }
    }
    public int Layer { get; set; }
    
    // TODO: Remove this
    public int[] CamQuadLocks { get; set; } = [0, 0];
    
    // TODO: And this too
    public int CamLock { get; set; }
    public int PreviousPage { get; private set; }
    public int Page
    {
        get => _page;
        set
        {
            PageUpdated.Invoke(_page, value);
            PreviousPage = _page;
            _page = value;
            _logger?.Debug($"Page is set to {value}");
        }
    }
    public Camera2D Camera { get; set; } = new() { Zoom = 1 };
    public TileDex TileDex
    {
        get => _tileDex;
        set
        {
            _tileDex = value;
            _logger?.Debug("Tile dex was updated");
        }
    }
    public Gram Gram
    {
        get => _gram;
        set
        {
            _gram = value;
            _logger?.Debug("Gram was updated");
        }
    }

    /// <summary>
    /// Must be called within GL drawing context
    /// </summary>
    /// <param name="path">The path to the project file ".txt"</param>
    /// <returns></returns>
    public async Task<bool> LoadProjectAsync(string path)
    {
        var result = await Utils.LoadProjectAsync(path);

        if (!result.Success) return false;
        
        GLOBALS.Level.Import(
            result.Width, 
            result.Height,
            (result.BufferTiles.Left, result.BufferTiles.Top, result.BufferTiles.Right, result.BufferTiles.Bottom),
            result.GeoMatrix!,
            result.TileMatrix!,
            result.MaterialColorMatrix!,
            result.Effects,
            result.Cameras,
            result.PropsArray!,
            result.LightSettings,
            result.LightMode,
            result.DefaultTerrain,
            result.Seed,
            result.WaterLevel,
            result.WaterInFront,
            result.DefaultMaterial,
            result.Name
        );

        CamQuadLocks = new int[result.Cameras.Count];
        
        var lightMapTexture = Raylib.LoadTextureFromImage(result.LightMapImage);

        var textureWidth = result.Width * 20;
        var textureHeight = result.Height * 20;
        
        LightMap = Raylib.LoadRenderTexture(textureWidth + 300, textureHeight + 300);

        Raylib.BeginTextureMode(LightMap);
        Raylib.DrawTextureRec(
            lightMapTexture,
            new Rectangle(0, 0, lightMapTexture.Width, lightMapTexture.Height),
            new Vector2(0, 0),
            Color.White
        );
        Raylib.EndTextureMode();
        
        Raylib.UnloadImage(result.LightMapImage);
        Raylib.UnloadTexture(lightMapTexture);

        LevelRender = Raylib.LoadRenderTexture(textureWidth, textureHeight);

        var parent = Directory.GetParent(path)?.FullName;
        ProjectPath = parent ?? ProjectPath;
        Level.ProjectName = Path.GetFileNameWithoutExtension(path);
        
        ProjectLoaded.Invoke(this, EventArgs.Empty);

        return true;
    }

    public async Task CreateProjectAsync(
        int width,
        int height,
        (int left, int top, int right, int bottom) padding,
        int[] geoIdFill)
    {
        await Task.Factory.StartNew(() => Level.New(width, height, padding, geoIdFill));

        LightMap = Raylib.LoadRenderTexture(width * 20 + 300, height * 20 + 300);
        LevelRender = Raylib.LoadRenderTexture(width * 20, height * 20);

        ProjectPath = string.Empty;
        Level.ProjectName = "New Level";
        
        ProjectCreated.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler ProjectCreated; 
    public event EventHandler ProjectLoaded;
    
    public delegate void PageUpdateHandler(int previous, int @next);
    public event PageUpdateHandler PageUpdated;
}