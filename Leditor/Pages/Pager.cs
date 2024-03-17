namespace Leditor.Pages;

#nullable enable

internal sealed class Pager(Serilog.ILogger logger, Context context, int rollBackLimit = 2)
{
    private Serilog.ILogger Logger { get; set; } = logger;
    private Context Context { get; set; } = context;
    
    private readonly Dictionary<string, EditorPage> _pages = new();

    private EditorPage? _defaultPage;
    private ExceptionPage? _exceptionPage;

    private readonly LinkedList<EditorPage> _history = new();
    private LinkedListNode<EditorPage>? _currentPage;

    internal int RollbackLimit { get; init; } = rollBackLimit;
    
    internal EditorPage Current => _currentPage?.Value 
                                   ?? _defaultPage 
                                   ?? throw new Exception("No default page was registered");

    
    /// <summary>
    /// Adds the specified page and moves to it
    /// </summary>
    /// <param name="page"></param>
    private void Push(EditorPage page)
    {
        _history.AddLast(page);
        _currentPage = _history.Last;
    }
    
    /// <summary>
    /// Clears history and adds the default page.
    /// </summary>
    /// <exception cref="NullReferenceException">if no default page was set</exception>
    private void Default()
    {
        _history.Clear();
        Push(_defaultPage!);    
    }

    /// <summary>
    /// Clears history and moves to the exception page
    /// </summary>
    /// <param name="e">The exception to display</param>
    /// <exception cref="NullReferenceException">No exception page was registered</exception>
    private void Fatal(Exception e)
    {
        if (_exceptionPage is null) throw new NullReferenceException("No exception page was registered");
        
        _exceptionPage.Exception = e;
        
        _history.Clear();
        _history.AddFirst(_exceptionPage);
        _currentPage = _history.Last;
    }
    
    /// <summary>
    /// Pushes a page with a given id.
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="KeyNotFoundException">if no page with given id was registered</exception>
    internal void Push(string id)
    {
        if (_currentPage?.Next is not null)
        {
            _currentPage.Next.Value = _pages[id];
            _currentPage = _currentPage.Next;
            return;
        }

        Push(_pages[id]);
        
        Logger.Information($"Pushed page \"{id}\"");
    }

    /// <summary>
    /// Moves to the previous page
    /// </summary>
    internal void Back()
    {
        _currentPage = _currentPage?.Previous;
    }

    /// <summary>
    /// Removes the current page from history, and moves to the previous page.
    /// </summary>
    internal void Pop()
    {
        if (_history.Count == 0) return;
        
        _history.RemoveLast();
        _currentPage = _currentPage?.Previous;
    }

    /// <summary>
    /// Registers a new page with a given <paramref name="id"/>.
    /// </summary>
    /// <param name="id"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    internal Pager Register<T>(string id) where T : EditorPage, new()
    {
        _pages.Add(id, new T { Logger = Logger, Context = Context });

        return this;
    }

    /// <summary>
    /// Registers a page and sets it as the default page
    /// </summary>
    /// <param name="id"></param>
    /// <typeparam name="T"></typeparam>
    internal Pager RegisterDefault<T>(string id) where T : EditorPage, new()
    {
        var page = new T { Logger = Logger, Context = Context };
        
        _pages.Add(id, page);
        _defaultPage = page;
        
        return this;
    }

    /// <summary>
    /// Registers a page that is used for displaying a fatal exception
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal Pager RegisterFatal<T>() where T : ExceptionPage, new()
    {
        _exceptionPage = new T
        {
            Context = Context,
            Logger = Logger
        };
        
        return this;
    }
    
    /// <summary>
    /// Sets a default page, clears history and pushes the default page.
    /// </summary>
    internal Pager WithDefault<T>() where T : EditorPage, new()
    {
        _defaultPage = new T { Logger = Logger, Context = Context };
        
        Default();

        return this;
    }

    /// <summary>
    /// Sets context for all pages. <see cref="WithDefault{T}"/> must be called beforehand.
    /// </summary>
    /// <param name="context"></param>
    internal Pager WithContext(Context context)
    {
        foreach (var (_, page) in _pages)
        {
            page.Context = context;
        }

        _defaultPage!.Context = context;

        return this;
    }

    /// <summary>
    /// Sets logger for all pages. <see cref="WithDefault{T}"/> must be called beforehand.
    /// </summary>
    /// <param name="logger"></param>
    internal Pager WithLogger(Serilog.ILogger logger)
    {
        foreach (var (_, page) in _pages)
        {
            page.Logger = logger;
        }

        _defaultPage!.Logger = logger;

        return this;
    }

    /// <summary>
    /// Sets context and logger to all pages, and sets a default page.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="logger"></param>
    internal Pager With<TDefaultPage>(Context context, Serilog.ILogger logger) 
        where TDefaultPage : EditorPage, new()
    {
        _defaultPage = new TDefaultPage
        {
            Context = context,
            Logger = logger
        };

        Default();

        foreach (var (_, page) in _pages)
        {
            page.Logger = logger;
            page.Context = context;
        }

        return this;
    }
}