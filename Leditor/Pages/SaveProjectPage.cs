using System.Numerics;
using System.Text;
using ImGuiNET;
using Leditor.Pages;
using Pidgin;
using rlImGui_cs;
using static Raylib_cs.Raylib;

namespace Leditor.Pages;

#nullable enable

// TODO: localize instantiaion
internal class SaveProjectPage : EditorPage
{
    public override void Dispose()
    {
        if (Disposed) return;
        Disposed = true;
        _upTexture.Dispose();
        _homeTexture.Dispose();
        _folderTexture.Dispose();
        _newFolderTexture.Dispose();
        _fileTexture.Dispose();

        _upBlackTexture.Dispose();
        _homeBlackTexture.Dispose();
        _folderBlackTexture.Dispose();
        _newFolderBlackTexture.Dispose();
        _fileBlackTexture.Dispose();
    }

    internal event EventHandler? ProjectLoaded;

    private bool _uiLocked;

    private Task<string>? _openFileDialog;
    private Task<LoadFileResult>? _loadFileTask;
    
    private RL.Managed.Texture2D _upTexture;
    private RL.Managed.Texture2D _homeTexture;
    private RL.Managed.Texture2D _folderTexture;
    private RL.Managed.Texture2D _newFolderTexture;
    private RL.Managed.Texture2D _fileTexture;
    
    private RL.Managed.Texture2D _upBlackTexture;
    private RL.Managed.Texture2D _homeBlackTexture;
    private RL.Managed.Texture2D _folderBlackTexture;
    private RL.Managed.Texture2D _newFolderBlackTexture;
    private RL.Managed.Texture2D _fileBlackTexture;

    //
    private bool _modalMode;
    private bool _duplicateFolderName;
    private string _newFolderName;
    private bool _currentDirExists;
    private bool _duplicateName;
    private string _projectName;
    private string _currentDir;
    private (string path, string name)[] _dirEntries;

    private int _currentIndex = -1;

    private void NavigateToDir(string path) {
        if (!Directory.Exists(path)) return;

        _currentDir = path;
        _dirEntries = Directory
            .GetFileSystemEntries(path)
            .Select(e => {
                var attrs = File.GetAttributes(e);

                return (e, (attrs & FileAttributes.Directory) == FileAttributes.Directory);
            })
            .Where(e => e.Item2)
            .Select(e => {
                return (e.Item1, Path.GetFileNameWithoutExtension(e.Item1));
            })
            .ToArray();
    }

    private void NavigateUp() {
        var parentDir = Directory.GetParent(_currentDir);

        if (parentDir is null or { Exists: false }) return;

        NavigateToDir(parentDir.FullName);
    }

    private bool ProjectNameExists(string name) {
        return Directory
            .GetFiles(_currentDir)
            .Where(f => f.EndsWith(".txt"))
            .Select(Path.GetFileNameWithoutExtension)
            .Contains(name);
    }

    private bool FolderExists(string name) {
        return Directory.GetDirectories(_currentDir).Select(Path.GetFileNameWithoutExtension).Contains(name);
    }
    //

    private record struct SaveProjectResult(bool Success, Exception? Exception = null);

    private async Task<SaveProjectResult> SaveProjectAsync(string path)
    {
        SaveProjectResult result;
        
        try
        {
            var strTask = Serialization.Exporters.ExportAsync(GLOBALS.Level);

            var str = await strTask;
            
            Logger.Debug($"Saving to {GLOBALS.ProjectPath}");
            await File.WriteAllTextAsync(path, str);

            result = new(true);
        }
        catch (Exception e)
        {
            result = new(false, e);
        }

        return result;
    }

    private Task<SaveProjectResult>? _saveResult;
    private bool _failedToSave;

    internal SaveProjectPage()
    {
        _newFolderName = string.Empty;
        _projectName = string.Empty;
        _currentDir = GLOBALS.Paths.ProjectsDirectory;
        _dirEntries = Directory
            .GetFileSystemEntries(_currentDir)
            .Select(e => {
                var attrs = File.GetAttributes(e);

                return (e, (attrs & FileAttributes.Directory) == FileAttributes.Directory);
            })
            .Where(e => e.Item2)
            .Select(e => {
                return (e.Item1, Path.GetFileNameWithoutExtension(e.Item1));
            })
            .ToArray();

        _currentDirExists = Directory.Exists(_currentDir);
        _duplicateName = ProjectNameExists(_projectName);

        _upTexture = new(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "up icon.png"));
        _homeTexture = new(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "home icon.png"));
        _folderTexture = new(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "folder icon.png"));
        _newFolderTexture = new(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "new folder icon.png"));
        _fileTexture = new(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "file icon.png"));
    
        _upBlackTexture = new(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "up icon black.png"));
        _homeBlackTexture = new(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "home icon black.png"));
        _folderBlackTexture = new(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "folder icon black.png"));
        _newFolderBlackTexture = new(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "new folder icon black.png"));
        _fileBlackTexture = new(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "file icon black.png"));
    

    }

    ~SaveProjectPage() {
        if (!Disposed) throw new InvalidOperationException("SaveProjectPage was not disposed by consumer");
    }

    public override void Draw()
    {
        if (GLOBALS.Page != 12) {
            _projectName = GLOBALS.Level.ProjectName;
            _currentDir = GLOBALS.ProjectPath;
            GLOBALS.Page = 12;
        }

        if (!_uiLocked && !_modalMode) {
            if (_dirEntries.Length > 0 && (IsKeyPressed(KeyboardKey.Down))) {
                _currentIndex++;

                Utils.Restrict(ref _currentIndex, 0, _dirEntries.Length - 1);
            }

            if (_dirEntries.Length > 0 && (IsKeyPressed(KeyboardKey.Up))) {
                _currentIndex--;

                Utils.Restrict(ref _currentIndex, 0, _dirEntries.Length - 1);
            }

            if (_dirEntries.Length > 0 && IsKeyPressed(KeyboardKey.Enter)) {
                NavigateToDir(_dirEntries[_currentIndex].path);
            } 
        }

        BeginDrawing();
        {
            if (_uiLocked)
            {
                var path = Path.Combine(_currentDir, _projectName+".txt");

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
                    _uiLocked = false;
                    GLOBALS.LockNavigation = false;
                    EndDrawing();
                    #if DEBUG
                    if (result.Exception is not null) Logger.Error($"Failed to save project: {result.Exception}");
                    #endif
                    _saveResult = null;
                    return;
                }
                
                // export light map
                {
                    var image = LoadImageFromTexture(GLOBALS.Textures.LightMap.Texture);

                    unsafe
                    {
                        ImageFlipVertical(&image);
                    }

                    var parent = Directory.GetParent(path)?.FullName ?? GLOBALS.ProjectPath;
                    var name = Path.GetFileNameWithoutExtension(path);

                    ExportImage(image, Path.Combine(parent, name + ".png"));

                    UnloadImage(image);
                }

                {
                    var parent = Directory.GetParent(path)?.FullName;

                    GLOBALS.ProjectPath = parent ?? GLOBALS.ProjectPath;
                    GLOBALS.Level.ProjectName = _projectName;

                    _saveResult = null;
                    _uiLocked = false;
                    GLOBALS.LockNavigation = false;

                    // Export level image to cache
                    if (!Directory.Exists(Path.Combine(GLOBALS.Paths.CacheDirectory, "levelpreviews"))) {
                        Directory.CreateDirectory(Path.Combine(GLOBALS.Paths.CacheDirectory, "levelpreviews"));
                    }

                    using var levelImg = Printers.GenerateLevelReviewImage();

                    ExportImage(levelImg, Path.Combine(GLOBALS.Paths.CacheDirectory, "levelpreviews", GLOBALS.Level.ProjectName+".png"));

                    //

                    EndDrawing();
                }

                GLOBALS.Page = GLOBALS.PreviousPage;
            }
            else
            {
                if (GLOBALS.Settings.GeneralSettings.DarkTheme) ClearBackground(new Color(100, 100, 100, 255));
                else ClearBackground(new Color(170, 170, 170, 255));

                #region ImGui
                rlImGui.Begin();

                var winSize = new Vector2(GetScreenWidth() - 80, GetScreenHeight() - 80);


                if (ImGui.Begin("Start##StartupWindow", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoDocking)) {
                    ImGui.SetWindowSize(winSize);
                    ImGui.SetWindowPos(new(40, 40));
                    
                    var upALevelClicked = rlImGui.ImageButtonSize("##Up", GLOBALS.Settings.GeneralSettings.DarkTheme ? _upTexture : _upBlackTexture, new(20, 20));
                    ImGui.SameLine();
                    
                    var homeClicked = rlImGui.ImageButtonSize("##Home", GLOBALS.Settings.GeneralSettings.DarkTheme ? _homeTexture : _homeBlackTexture, new(20, 20));
                    ImGui.SameLine();

                    if (!_currentDirExists) ImGui.BeginDisabled();

                    var newFolderClicked = rlImGui.ImageButtonSize("##NewFolder", GLOBALS.Settings.GeneralSettings.DarkTheme ? _newFolderTexture : _newFolderBlackTexture, new(20, 20));
                    ImGui.SameLine();

                    if (!_currentDirExists) ImGui.EndDisabled();
                    
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                    var pathUpdated = ImGui.InputText("##FilePathBuffer", ref _currentDir, 260, ImGuiInputTextFlags.None);

                    var listAvailSpace = ImGui.GetContentRegionAvail();

                    if (ImGui.BeginListBox("##StartPageFileExplorerList", listAvailSpace with { Y = listAvailSpace.Y - 90 })) {
                        
                        for (var i = 0; i < _dirEntries.Length; i++) {

                            rlImGui.ImageSize(GLOBALS.Settings.GeneralSettings.DarkTheme ? _folderTexture : _folderBlackTexture, new(23, 23));
                            ImGui.SameLine();
                            var selected = ImGui.Selectable(_dirEntries[i].name, _currentIndex == i, ImGuiSelectableFlags.None, ImGui.GetContentRegionAvail() with { Y = 20 });

                            if (selected) {
                                if (_currentIndex == i) {
                                    NavigateToDir(_dirEntries[i].path);
                                    _currentDirExists = Directory.Exists(_currentDir);
                                } else {
                                    _currentIndex = i;
                                }

                                _duplicateName = ProjectNameExists(_projectName);
                            }
                        }
                        
                        ImGui.EndListBox();
                    }

                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                    if (ImGui.InputText("##ProjectName", ref _projectName, 260)) _duplicateName = ProjectNameExists(_projectName);

                    if (pathUpdated) {
                        if (!Directory.Exists(_currentDir)) {
                            _dirEntries = [];
                            _currentDirExists = false;
                        } else {
                            NavigateToDir(_currentDir);
                            _currentDirExists = true;
                            _duplicateName = ProjectNameExists(_projectName);
                        }
                    }

                    if (newFolderClicked) {
                        ImGui.OpenPopup("New Folder Name");
                        _modalMode = true;
                    }

                    if (ImGui.BeginPopupModal("New Folder Name")) {
                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                        if (ImGui.InputText("##NewFolderName", ref _newFolderName, 260)) _duplicateFolderName = FolderExists(_newFolderName);

                        if (_duplicateFolderName) ImGui.BeginDisabled();
                        if (ImGui.Button("Create", ImGui.GetContentRegionAvail() with { Y = 20 })) {
                            Directory.CreateDirectory(Path.Combine(_currentDir, _newFolderName));
                            _newFolderName = string.Empty;
                            ImGui.CloseCurrentPopup();
                            _modalMode = false;
                            NavigateToDir(_currentDir);
                        }
                        if (_duplicateFolderName) ImGui.EndDisabled();

                        if (ImGui.Button("Close", ImGui.GetContentRegionAvail() with { Y = 20 })) {
                            ImGui.CloseCurrentPopup();
                            _modalMode = false;
                        }
                        ImGui.EndPopup();
                    }

                    if (string.IsNullOrEmpty(_projectName)) ImGui.BeginDisabled();

                    var saveLevelClicked = ImGui.Button(_duplicateName ? "Override" : "Save", ImGui.GetContentRegionAvail() with { Y = 20 });
                    var cancelClicked = ImGui.Button("Cancel", ImGui.GetContentRegionAvail() with { Y = 20 });

                    if (saveLevelClicked) {
                        if (_duplicateName) { 
                            ImGui.OpenPopup("Confirm Override");
                            _modalMode = true;
                        } else {
                            _uiLocked = true;
                        }
                    }

                    if (cancelClicked) GLOBALS.Page = GLOBALS.PreviousPage;

                    if (ImGui.BeginPopupModal("Confirm Override")) {
                        if (ImGui.Button("Yes", ImGui.GetContentRegionAvail() with { Y = 20 })) {
                            _uiLocked = true;
                            _modalMode = false;
                            ImGui.CloseCurrentPopup();
                        }

                        if (ImGui.Button("Cancel", ImGui.GetContentRegionAvail() with { Y = 20 })) {
                            _modalMode = false;
                            ImGui.CloseCurrentPopup();
                        }
                        
                        ImGui.EndPopup();
                    }

                    if (string.IsNullOrEmpty(_projectName)) ImGui.EndDisabled();

                    //

                    if (homeClicked) {
                        NavigateToDir(GLOBALS.Paths.ProjectsDirectory);

                        _duplicateName = ProjectNameExists(_projectName);
                        _duplicateFolderName = FolderExists(_newFolderName);
                    }

                    if (upALevelClicked) {
                        NavigateUp();

                        _duplicateName = ProjectNameExists(_projectName);
                        _duplicateFolderName = FolderExists(_newFolderName);
                    }

                    ImGui.End();
                }

                if (_failedToSave) ImGui.OpenPopup("Failed to Save");

                if (ImGui.BeginPopupModal("Failed to Save")) {
                    
                    ImGui.Text("An error has occurred while saving the project.");

                    if (ImGui.Button("Ok", ImGui.GetContentRegionAvail() with { Y = 20 })) {
                        ImGui.CloseCurrentPopup();
                        _failedToSave = false;
                    }
                    
                    ImGui.EndPopup();
                }

                rlImGui.End();
                #endregion
            }
        }
        EndDrawing();
    }
}