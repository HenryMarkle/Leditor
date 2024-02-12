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

    private bool _askForPath;
    private bool _failedToSave;
    
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
            
            var parent = Directory.GetParent(path)?.FullName ?? GLOBALS.ProjectPath;
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

                if (_askForPath)
                {
                    
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
                            _failedToSave = true;
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
                    if (_saveResult is null)
                    {
                        _saveResult = SaveProjectAsync(Path.Combine(GLOBALS.ProjectPath, GLOBALS.Level.ProjectName+".txt"));
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
                        _failedToSave = true;
                        RayGui.GuiUnlock();
                        EndDrawing();
                        #if DEBUG
                        if (result.Exception is not null) _logger.Error($"Failed to save project: {result.Exception}");
                        #endif
                        _saveResult = null;
                        _saveFileDialog = null;
                        return;
                    }
                    
                    _saveFileDialog = null;
                    _saveResult = null;
                    RayGui.GuiUnlock();
                    EndDrawing();
                }
            }
            else
            {
                ClearBackground(new(170, 170, 170, 255));

                if (_failedToSave)
                {
                    var alertRect = new Rectangle(0, 0, 600, 40);
                    var closeAlertRect = alertRect with { width = 30, height = 30, X = alertRect.X+alertRect.width - 35, Y = alertRect.Y+5};
                    
                    DrawRectangleRec(alertRect, RED);
                    DrawText("Failed to save project (check logs)", alertRect.X + 5, alertRect.Y + 10, 25, WHITE);
                    DrawText("X", closeAlertRect.X, closeAlertRect.Y, 35, WHITE);

                    if (CheckCollisionPointRec(GetMousePosition(), closeAlertRect))
                    {
                        SetMouseCursor(MouseCursor.MOUSE_CURSOR_POINTING_HAND);
                        if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
                        {
                            _failedToSave = false;
                            SetMouseCursor(MouseCursor.MOUSE_CURSOR_DEFAULT);
                        }
                    }
                    else
                    {
                        SetMouseCursor(MouseCursor.MOUSE_CURSOR_DEFAULT);
                    }
                }

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
                   /*fixed (byte* bytes = _projectNameBufferBytes)
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
                   }*/
                   
                   DrawText(GLOBALS.Level.ProjectName,width / 2f - 150, height / 2f - 90, 20, BLACK);
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
                    if (string.IsNullOrEmpty(GLOBALS.ProjectPath))
                    {
                        _askForPath = true;
                        _saveFileDialog = Utils.SetFilePathAsync();
                    }
                    else
                    {
                        _askForPath = false;
                    }
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
                    _askForPath = true;
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