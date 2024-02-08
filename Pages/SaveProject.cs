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

    private Task<string>? _saveFileDialog;
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
            
            _logger.Debug($"Saving to {GLOBALS.ProjectPath}");
            await File.WriteAllTextAsync(Path.Combine(GLOBALS.ProjectPath, GLOBALS.Level.ProjectName+".txt"), str);

            result = new(true);
        }
        catch (Exception e)
        {
            result = new(false, e);
        }

        return result;
    }
    private async Task<SaveProjectResult> SaveProjectAsync(string path)
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
            
            var parent = Directory.GetParent(_saveFileDialog!.Result)?.FullName ?? GLOBALS.ProjectPath;
            var name = Path.GetFileNameWithoutExtension(path);
                    
            ExportImage(image, Path.Combine(parent, name+".png"));
            
            UnloadImage(image);

            var str = await strTask;
            
            _logger.Debug($"Saving to {GLOBALS.ProjectPath}");
            await File.WriteAllTextAsync(path, str);

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

                if (_saveFileDialog is null)
                {
                    if (_saveResult!.IsCompleted)
                    {
                        GLOBALS.Page = 1;
                        RayGui.GuiUnlock();
                    }
                }
                else
                {
                    if (!_saveFileDialog.IsCompleted)
                    {
                        EndDrawing();
                        return;
                    }
                    if (string.IsNullOrEmpty(_saveFileDialog.Result))
                    {
                        RayGui.GuiUnlock();
                        EndDrawing();
                        return;
                    }

                    var path = _saveFileDialog.Result;

                    if (_saveResult is null)
                    {
                        _saveResult = SaveProjectAsync(path);
                        EndDrawing();
                        return;
                    }
                    if (!_saveResult.IsCompleted)
                    {
                        EndDrawing();
                        return;
                    }

                    var result = _saveResult.Result;

                    if (!result.Success)
                    {
                        RayGui.GuiUnlock();
                        EndDrawing();
                        #if DEBUG
                        if (result.Exception is not null) _logger.Error($"Failed to save project: {result.Exception}");
                        #endif
                        _saveResult = null;
                        _saveFileDialog = null;
                        return;
                    }
                    
                    var parent = Directory.GetParent(_saveFileDialog.Result)?.FullName;
                    
                    GLOBALS.ProjectPath = parent ?? GLOBALS.ProjectPath;
                    GLOBALS.Level.ProjectName = Path.GetFileNameWithoutExtension(_saveFileDialog.Result);

                    _saveFileDialog = null;
                    _saveResult = null;
                    RayGui.GuiUnlock();
                    EndDrawing();
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

                var saveAsPressed = RayGui.GuiButton(
                    new(
                        width / 2f - 150,
                        height / 2f + 50,
                        300,
                        40),
                    "Save As");
                
                if (saveAsPressed)
                {
                    _saveFileDialog = Utils.SetFilePathAsync();
                    RayGui.GuiLock();
                }

                var cancelSavePressed = RayGui.GuiButton(
                    new(
                        width / 2f - 150,
                        height / 2f + 100,
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