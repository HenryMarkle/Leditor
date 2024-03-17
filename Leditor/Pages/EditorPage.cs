using Leditor.RL;

namespace Leditor.Pages;

internal abstract class EditorPage : IDrawable
{
    public abstract void Draw();
    
    public Serilog.ILogger Logger { get; set; }
    public Context Context { get; set; }

    internal EditorPage()
    {
        
    }

    internal EditorPage(Serilog.ILogger logger, Context context)
    {
        Logger = logger;
        Context = context;
    }
}

internal abstract class ExceptionPage : EditorPage
{
    public Exception Exception { get; set; }
}
