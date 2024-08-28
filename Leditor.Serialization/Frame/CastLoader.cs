namespace Leditor.Serialization.Frame;

using Leditor.Data;

using static Raylib_cs.Raylib;
using Raylib_cs;

public class CastLoader
{
    private Serilog.ILogger? Logger { get; init; }

    private readonly Task<(Image image, string name)>[] _images;
    private readonly Task _allImages;

    public bool IsReady => _allImages.IsCompleted;
    public bool IsLoaded { get; private set; }
    public bool IsCompleted => _libTask?.IsCompleted == true;

    private int _taskCursor;

    private readonly List<(string lib, string name, Texture2D texture)> _parsed = [];
    private CastLibrary[] _libs = [];
    private Task? _libTask;

    public int TotalProgress { get; private set; }
    public int Progress { get; private set; }
    
    public CastLoader(string folder, Serilog.ILogger? logger = null)
    {
        logger?.Information("[CastLoader] Loading cast members started");

        var files = Directory
            .GetFiles(folder)
            .Where(path => path.EndsWith(".png"))
            .Select(path => {
                var name = Path.GetFileNameWithoutExtension(path);
                return (name, path);
            });

        _images = files
            .Select(f => Task.Factory.StartNew(() => (LoadImage(f.path), f.name)))
            .ToArray();

        _allImages = Task.WhenAll(_images);

        TotalProgress = _images.Length;
    }

    public bool LoadNext()
    {
        if (!IsReady) return false;

        if (IsLoaded) return true;

        if (_taskCursor >= _images.Length)
        {
            _libTask = Task.Factory.StartNew(() => {
                _libs = _parsed
                    .GroupBy(p => p.lib)
                    .Select(g => {
                        var dict = g.ToDictionary(g => g.name, g => new CastMember(g.name, 0, g.texture));
                    
                        return new CastLibrary {
                            Name = g.Key,
                            Members = dict
                        };
                    })
                    .ToArray();
            });

            IsLoaded = true;
            return true;
        }

        var (image, name) = _images[_taskCursor].Result;

        var texture = LoadTextureFromImage(image);
        UnloadImage(image);

        var segments = name.Split('_');

        if (segments.Length != 3)
        {
            UnloadTexture(texture);
            _taskCursor++;

            Logger?.Error("[CastLoader::LoadNext] Invalid cast member name");
        }

        var lib = segments[0];
        // var offset = segments[1];
        var memberName = segments[2];

        _parsed.Add((lib, memberName, texture));

        _taskCursor++;

        Progress++;

        return false;
    }

    public CastLibrary[] GetLibs()
    {
        if (!IsCompleted) throw new InvalidOperationException("Texture loading is not done");

        return _libs;
    }
}