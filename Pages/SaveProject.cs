using System.Text;
using Leditor.Leditor.Lingo;
using static Raylib_CsLo.Raylib;

namespace Leditor;

#nullable enable

public class SaveProjectPage(Serilog.Core.Logger logger) : IPage
{
    private readonly Serilog.Core.Logger _logger = logger;
    
    private readonly byte[] _saveProjectPanelBytes = "Save Project"u8.ToArray();
    private byte[] _projectNameBufferBytes = Encoding.ASCII.GetBytes(GLOBALS.Level.ProjectName);

    internal void OnProjectLoaded(object? sender, EventArgs e)
    {
        _projectNameBufferBytes = Encoding.ASCII.GetBytes(GLOBALS.Level.ProjectName);
    }
    
    private record struct SaveProjectResult(bool Success, Exception? Exception = null);

    private Task<SaveProjectResult>? _saveResult;
    
    private async Task<SaveProjectResult> SaveProjectAsync()
    {
        SaveProjectResult result;
        
        try
        {
            var strTask = Exporters.ExportAsync(GLOBALS.Level);

            // export light map
            var image = LoadImageFromTexture(GLOBALS.Textures.LightMap.texture);

            unsafe
            {
                ImageFlipVertical(&image);
            }


            ExportImage(image, Path.Combine(GLOBALS.ProjectPath, $"{GLOBALS.Level.ProjectName}.png"));
            
            UnloadImage(image);

            var str = await strTask;
            
            await File.WriteAllTextAsync(GLOBALS.ProjectPath, str);

            result = new(true);
        }
        catch (Exception e)
        {
            result = new(false, e);
        }

        return result;
    }
    
    public void Draw()
    {
        var width = GetScreenWidth();
        var height = GetScreenHeight();
        
        BeginDrawing();
        {
            if (RayGui.GuiIsLocked())
            {
                ClearBackground(BLACK);
                
                DrawText(
                    "Please wait..", 
                    (width - MeasureText("Please wait..", 50))/2f, 
                    100, 
                    50, 
                    WHITE
                );

                if (_saveResult?.IsCompleted is true)
                {
                    GLOBALS.Page = 1;
                    RayGui.GuiUnlock();
                }
            }
            else
            {
                ClearBackground(new(170, 170, 170, 255));

                unsafe
                {
                   fixed (byte* pt = _saveProjectPanelBytes)
                   {
                       RayGui.GuiPanel(
                           new(
                               width / 2f - 200,
                               height / 2f - 150,
                               400,
                               300
                           ),
                           (sbyte*)pt
                       );
                   } 
                   fixed (byte* bytes = _projectNameBufferBytes)
                   {
                       RayGui.GuiTextBox(
                           new(
                               width / 2f - 150,
                               height / 2f - 90,
                               300,
                               40
                           ),
                           (sbyte*)bytes,
                           20,
                           true
                       );
                   }
                }
                
                var saveClicked = RayGui.GuiButton(
                    new(
                        width / 2f - 150,
                        height / 2f,
                        300,
                        40
                    ),
                    "Save"
                );

                if (saveClicked)
                {
                    GLOBALS.Level.ProjectName = Encoding.UTF8.GetString(_projectNameBufferBytes);
                    _saveResult = SaveProjectAsync();
                    RayGui.GuiLock();
                }

                var cancelSavePressed = RayGui.GuiButton(
                    new(
                        width / 2f - 150,
                        height / 2f + 50,
                        300,
                        40
                    ),
                    "Cancel"
                );

                if (cancelSavePressed) GLOBALS.Page = 1;
            }
        }
        EndDrawing();
    }
}