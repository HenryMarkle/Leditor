using Serilog.Core;
// ReSharper disable MemberCanBePrivate.Global

namespace Leditor.Pages;

#nullable enable

internal sealed class Pager(Serilog.ILogger logger, Context context) : IDisposable
{
    private Serilog.ILogger Logger { get; set; } = logger;
    private Context Context { get; set; } = context;

    private readonly List<EditorPage?> _pages = [];

    private ExceptionPage? _exceptionPage;
    private EditorPage? _defaultPage;

    private EditorPage? _previousPage;
    private EditorPage? _currentPage;

    public EditorPage CurrentPage
    {
        get
        {
            if (_currentPage is null)
            {
                if (_defaultPage is null)
                {
                    if (_exceptionPage is null) throw new NullReferenceException("The program has unexpectedly run out of pages. Even the exception the page was not registered");
                    
                    _exceptionPage.Exception = new Exception("The program has unexpectedly run out of pages");
                    
                    _currentPage = _exceptionPage;
                    return _currentPage;
                }

                return _defaultPage;
            }

            return _currentPage;
        }

        private set
        {
            _previousPage = _currentPage;
            _currentPage = value;
        }
    }

    /// <summary>
    /// Clears and re-initializes the registry
    /// </summary>
    /// <param name="capacity">the maximum number of pages allowed</param>
    public void Init(int capacity)
    {
        _pages.Clear();
        _pages.Capacity = capacity;
        _pages.AddRange(new EditorPage?[capacity]);
        
        Logger.Debug($"Pager was initialized with a capacity of {capacity}");
    }

    /// <summary>
    /// Registers a page dedicated to displaying a fatal exception
    /// </summary>
    /// <typeparam name="TPage">the exception page</typeparam>
    public void RegisterException<TPage>()
        where TPage : ExceptionPage, new()
    {
        _exceptionPage = new TPage { Logger = Logger, Context = Context };
        
        Logger.Debug("Registered an exception page");
    }

    /// <summary>
    /// Adds a fallback page
    /// </summary>
    /// <param name="id">page index</param>
    /// <typeparam name="TPage">the page to register</typeparam>
    /// <exception cref="ArgumentException">if a page with the same id was already registered</exception>
    /// <exception cref="ArgumentOutOfRangeException">the id is invalid</exception>
    /// <exception cref="InvalidOperationException">when <see cref="Init"/> was not called properly</exception>
    public void RegisterDefault<TPage>(int id)
        where TPage : EditorPage, new()
    {
        if (_pages is []) throw new InvalidOperationException("The pager capacity was not set");
        
        if (_pages[id] is not null) 
            throw new ArgumentException($"Page with id {id} is already registered");
        
        _defaultPage = new TPage { Logger = Logger, Context = Context };
        _pages[id] = _defaultPage;
        
        Logger.Debug($"Registered page with id {id} as the default fallback");
    }
    
    /// <summary>
    /// Adds a fallback page
    /// </summary>
    /// <param name="id">page index</param>
    /// <typeparam name="TPage">the page to register</typeparam>
    /// <returns>false if a page with the same id was already registered</returns>
    public bool TryRegisterDefault<TPage>(int id)
        where TPage : EditorPage, new()
    {
        if (id < 0 || id >= _pages.Count) return false;
        
        if (_pages[id] is not null) return false;
        
        _defaultPage = new TPage { Logger = Logger, Context = Context };
        _pages[id] = _defaultPage;
        
        Logger.Debug($"Registered page with id {id} as the default fallback");
        return true;
    }

    /// <summary>
    /// Registers a new page with an id/index
    /// </summary>
    /// <param name="id">the index of the page</param>
    /// <typeparam name="TPage">the page to register</typeparam>
    /// <exception cref="ArgumentException">if a page with the same id was already registered</exception>
    /// <exception cref="ArgumentOutOfRangeException">the id is invalid</exception>
    /// <exception cref="InvalidOperationException">when <see cref="Init"/> was not called properly</exception>
    public void Register<TPage>(int id)
        where TPage : EditorPage, new()
    {
        if (_pages is []) throw new InvalidOperationException("The pager capacity was not set");
        
        if (_pages[id] is not null) 
            throw new ArgumentException($"Page with id {id} is already registered");

        _currentPage = new TPage { Logger = Logger, Context = Context };
        _pages[id] = _currentPage;
        
        Logger.Debug($"Registered page with id {id}");
    }

    /// <summary>
    /// Registers a new page with an id/index
    /// </summary>
    /// <param name="id">the index of the page</param>
    /// <typeparam name="TPage">the page to register</typeparam>
    /// <returns>false if a page with the same id was already registered</returns>
    public bool TryRegister<TPage>(int id)
        where TPage : EditorPage, new()
    {
        if (id < 0 || id >= _pages.Count) return false;
        if (_pages[id] is not null) return false;
        
        _currentPage = new TPage { Logger = Logger, Context = Context };
        _pages[id] = _currentPage;
        
        Logger.Debug($"Registered page with id {id}");

        return true;
    }

    /// <summary>
    /// Navigates to the page with the given id
    /// </summary>
    /// <param name="id">the id of the page</param>
    /// <exception cref="ArgumentOutOfRangeException">invalid id</exception>
    /// <exception cref="InvalidOperationException">when <see cref="Init"/> was not called properly</exception>
    public void To(int id)
    {
        if (_pages is []) throw new InvalidOperationException("The pager capacity was not set");
        
        var page = _pages[id];

        if (page is null) throw new ArgumentException($"No page registered with id {id}");

        Logger.Debug($"Navigating to page {id}");

        CurrentPage = page;
        
        PagePushed.Invoke(id, page);
    }

    /// <summary>
    /// Tries to navigates to the page with the given id
    /// </summary>
    /// <param name="id">the id of the page</param>
    /// <returns>false if the id is invalid</returns>
    public bool TryTo(int id)
    {
        if (id < 0 || id >= _pages.Count) return false;
        
        var page = _pages[id];

        if (page is null)
        {
            Logger.Error($"No page registered with id {id}");
            return false;
        }
        
        Logger.Debug($"Navigating to page {id}");

        CurrentPage = page;
        
        PagePushed.Invoke(id, page);

        return true;
    }

    /// <summary>
    /// Navigates back to the previous page
    /// </summary>
    /// <exception cref="NullReferenceException">the previous and default pages were null</exception>
    public void Back()
    {
        Logger.Debug("Navigating back");
        
        CurrentPage = _previousPage 
                      ?? _defaultPage 
                      ?? throw new NullReferenceException("No previous or default page was set");
        
        PreviousPagePushed.Invoke(CurrentPage);
    }
    
    /// <summary>
    /// Tries to navigate back to the previous page
    /// </summary>
    /// <returns>false if no previous or default page was available</returns>
    public bool TryBack()
    {
        Logger.Debug("Navigating back");
        
        if (_previousPage is null)
        {
            if (_defaultPage is null) return false;

            _previousPage = _defaultPage;
        }

        CurrentPage = _previousPage;
        
        PreviousPagePushed.Invoke(CurrentPage);
        return true;
    }
    
    #region Events

    public delegate void PagePushedEventHandler(int id, EditorPage e);

    public delegate void PreviousPagePushedEventHandler(EditorPage e);

    public event PagePushedEventHandler PagePushed;
    public event PreviousPagePushedEventHandler PreviousPagePushed;
    #endregion
    
    #region DisposablePattern
    public bool Disposed { get; private set; }

    public void Dispose()
    {
        if (Disposed) return;
        
        Logger.Debug("Disposing Pager..");
        
        Disposed = true;

        foreach (var page in _pages) page?.Dispose();

        _currentPage?.Dispose();
        _defaultPage?.Dispose();
        _previousPage?.Dispose();
        _exceptionPage?.Dispose();
        
        _defaultPage = _previousPage = _currentPage = null;
        _exceptionPage = null;
        
        _pages.Clear();
        
        GC.SuppressFinalize(this);
    }

    ~Pager()
    {
        if (!Disposed) throw new InvalidOperationException("Pager was not disposed by consumer");
    }
    #endregion
}

internal sealed class Pager<TKey>(Serilog.ILogger logger, Context context) : IDisposable
    where TKey : IEquatable<TKey>
{
    private Serilog.ILogger Logger { get; set; } = logger;
    private Context Context { get; set; } = context;
    
    private readonly Dictionary<TKey, EditorPage> _pages = new();

    private EditorPage? _currentPage;

    private EditorPage? DefaultPage { get; set; }
    private ExceptionPage? ExceptionPage { get; set; }
    private EditorPage? PreviousPage { get; set; }
    
    public EditorPage CurrentPage 
    { 
        get => _currentPage ?? DefaultPage ?? throw new NullReferenceException("No pages"); 
        private set => _currentPage = value;
    }

    public void RegisterDefault<TPage>(TKey id)
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
    
    public void Register<TPage>(TKey id)
        where TPage : EditorPage, new()
    {
        _pages.Add(id, new TPage { Context = Context, Logger = Logger });
    }

    public void Push(TKey id)
    {
        var page = _pages[id];
        
        PreviousPage = CurrentPage;
        CurrentPage = page;
        
        PageUpdated.Invoke(id, page);
    }

    public bool TryPush(TKey id)
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
        CurrentPage = PreviousPage ?? throw new NullReferenceException("No default page registered"); ;
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

    public delegate void PageUpdateEventHandler(TKey id, EditorPage e);
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