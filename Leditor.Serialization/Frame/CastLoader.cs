namespace Leditor.Serialization.Frame;

using Leditor.Data;

using static Raylib_cs.Raylib;
using Raylib_cs;

public class CastLoader
{
    protected Serilog.ILogger? Logger { get; init; }

    protected readonly Task<(Image image, string name)>[] _images;
    protected readonly Task _allImages;

    public bool IsReady => _allImages.IsCompletedSuccessfully;
    public bool IsLoaded { get; protected set; }
    public bool IsCompleted { get; protected set; }

    protected int _taskCursor;

    protected readonly List<(string lib, string name, Texture2D texture)> _parsed = [];
    protected CastLibrary[] _libs = [];
    protected Task? _libTask;

    public int TotalProgress { get; protected set; }
    public int Progress { get; protected set; }
    
    public CastLoader(string folder, Serilog.ILogger? logger = null)
    {
        logger?.Information("[CastLoader] Begin loading cast members");

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

        if (IsLoaded) return _libTask?.IsCompleted ?? false;

        if (_taskCursor >= _images.Length)
        {
            _libTask = Task.Run(() => {
                var groups =  _parsed.GroupBy(p => p.lib);

                List<CastLibrary> libs = new(groups.Count());

                foreach (var g in groups) libs.Add(new() {
                    Name = g.Key,
                    Members = g.ToDictionary(g => g.name, g => new CastMember(g.name, 0, g.texture))
                });

                _libs = [..libs];
            });

            IsLoaded = true;
            return false;
        }

        var (image, name) = _images[_taskCursor].Result;

        var segments = name.Split('_');

        if (segments.Length != 3)
        {
            Logger?.Error("[CastLoader::LoadNext] Invalid cast member name");
        }
        else
        {
            var lib = segments[0];
            // var offset = segments[1];
            var memberName = segments[2];

            if (!string.IsNullOrEmpty(memberName))
            {
                _parsed.Add((lib, memberName, LoadTextureFromImage(image)));
            }
        }
        
        _taskCursor++;
        Progress++;
        UnloadImage(image);
        return false;
    }

    public CastLibrary[] GetLibs()
    {
        IsCompleted = true;
        
        return _libs;
    }
}