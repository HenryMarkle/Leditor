namespace Leditor.Renderer;

using Serilog;
using Raylib_cs;
using static Raylib_cs.Raylib;

public class Program
{
    public static void Main(string[] args)
    {
        //
        var executableDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) 
            ?? throw new Exception("unable to retreive current executable's path");
        
        var folders = new Folders
        {
            Executable = executableDir,
        };

        var files = new Files {
            Logs = Path.Combine(executableDir, "logs", "logs.txt")
        };
        //

        using var logger = new LoggerConfiguration()
            .WriteTo
            .File(
                files.Logs,
                fileSizeLimitBytes: 50000000,
                rollOnFileSizeLimit: true
            )
            .CreateLogger();

        //

        SetTargetFPS(60);
        
        //---------------------------------------------------------
        InitWindow(1000, 600, "Henry's Renderer");
        //---------------------------------------------------------

        while (!WindowShouldClose()) {
            BeginDrawing();
            {
                ClearBackground(Color.Gray);
            }
            EndDrawing();
        }

        CloseWindow();
    }
}