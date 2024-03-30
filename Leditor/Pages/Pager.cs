using Serilog.Core;

namespace Leditor.Pages;

#nullable enable

internal sealed class Pager(Serilog.ILogger logger, Context context) : IDisposable
{
    private Serilog.ILogger Logger { get; set; } = logger;
    private Context Context { get; set; } = context;
    
    private readonly Dictionary<string, EditorPage> _pages = new();

    private EditorPage? _currentPage;

    private EditorPage? DefaultPage { get; set; }
    private ExceptionPage? ExceptionPage { get; set; }
    private EditorPage? PreviousPage { get; set; }
    
    public EditorPage CurrentPage 
    { 
        get => _currentPage ?? DefaultPage ?? throw new NullReferenceException("No pages"); 
        private set => _currentPage = value;
    }

    public void RegisterDefault<TPage>(string id)
        where TPage : EditorPage, new()
    {
        var page = new TPage { Context = Context, Logger = Logger };
        
        _pages.Add(id, page);
        DefaultPage = page;
    }

    public void RegisterException<TPage>()
        where TPage : ExceptionPage, new()
    {
        ExceptionPage = new TPage { Context = Context, Logger = Logger };
    }
    
    public void Register<TPage>(string id)
        where TPage : EditorPage, new()
    {
        _pages.Add(id, new TPage { Context = Context, Logger = Logger });
    }

    public void Push(string id)
    {
        var page = _pages[id];
        
        PreviousPage = CurrentPage;
        CurrentPage = page;
        
        PageUpdated.Invoke(id, page);
    }

    public bool TryPush(string id)
    {
        if (_pages.TryGetValue(id, out var page))
        {
            PreviousPage = CurrentPage;
            CurrentPage = page;
        
            PageUpdated.Invoke(id, page);

            return true;
        }

        return false;
    }

    public void Back()
    {
        CurrentPage = PreviousPage;
        PreviousPage = null;
    }

    public bool TryBack()
    {
        if (PreviousPage is not { } p) return false;
        
        CurrentPage = p;
        PreviousPage = null;
        return true;
    }

    public void WithContext(Context context)
    {
        foreach (var (_, page) in _pages) page.Context = context;
    }

    public void WithLogger(Logger logger)
    {
        foreach (var (_, page) in _pages) page.Logger = logger;
    }

    public delegate void PageUpdateEventHandler(string id, EditorPage e);
    public event PageUpdateEventHandler PageUpdated;
    
    #region DisposablePattern
    public bool Disposed { get; private set; }
    public void Dispose()
    {
        if (Disposed) return;

        Disposed = true;
        
        foreach (var (_, page) in _pages) page.Dispose();
    }

    ~Pager()
    {
        if (!Disposed) throw new InvalidOperationException("Pager not disposed by consumer");
    }
    #endregion
}