using System.Numerics;
using System.Text.Json;
using ImGuiNET;
using rlImGui_cs;
using static Raylib_cs.Raylib;
using Leditor.Types;


namespace Leditor.Pages;

#nullable enable

internal class SettingsPage : EditorPage
{
    public override void Dispose()
    {
        Disposed = true;
    }
    
    private int _settingsActiveCategory;
    private int _shortcutsActiveCategory;

    private bool _assigningShortcut;
    private bool _showAssignAlertWindow;

    private KeyboardShortcut? _shortcutToAssign;
    private KeyboardShortcut? _shortcutToReset;
    private MouseShortcut? _mouseShortcutToAssign;
    
    private readonly string[] _settingsCategories = [
        "Global",
        "Geometry Editor",
        "Tile Editor",
        "Camera Editor",
        "Light Editor",
        "Effects Editor",
        "Props Editor",
        "Style",
        "Shortcuts"
    ];

    private readonly string[] _shortcutsCategories = [
        "Global",
        "Old Geometry Editor",
        "New Geometry Editor",
        "Tile Editor",
        "Camera Editor",
        "Light Editor",
        "Effects Editor",
        "Props Editor"
    ];
    
    public override void Draw()
    {
        #region Shortcuts
        
        if (_shortcutToAssign is not null || _mouseShortcutToAssign is not null)
        {
            _assigningShortcut = true;
            GLOBALS.LockNavigation = true;
        }

        if (_shortcutToAssign is not null)
        {
            var key = GetKeyPressed();
            
            if (key == (int)KeyboardKey.Escape)
            {
                if (IsKeyDown(KeyboardKey.LeftShift))
                {
                    _shortcutToAssign.Key = KeyboardKey.Null;
                }
                
                _assigningShortcut = false;
                GLOBALS.LockNavigation = false;
                _shortcutToAssign = null;
            }

            if (key != 0 && key != 340 && key != 341 && key != 342 && key != 256 && key != 4)
            {
                _shortcutToAssign!.Key = (KeyboardKey)key;
                _shortcutToAssign.Shift = IsKeyDown(KeyboardKey.LeftShift) || IsKeyDown(KeyboardKey.RightShift);
                _shortcutToAssign.Ctrl = IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.RightControl);
                _shortcutToAssign.Alt = IsKeyDown(KeyboardKey.LeftAlt) || IsKeyDown(KeyboardKey.RightAlt);
                
                _assigningShortcut = false;
                GLOBALS.LockNavigation = false;
                _shortcutToAssign = null;
            }
            
        }
        else if (_shortcutToReset is not null) {
            _shortcutToReset.Key = KeyboardKey.Null;
            _shortcutToReset = null;
        }
        else if (_mouseShortcutToAssign is not null)
        {
            var key = GetKeyPressed();
            var button = -1;

            if (IsMouseButtonPressed(MouseButton.Left)) button = 0;
            if (IsMouseButtonPressed(MouseButton.Middle)) button = 2;
            if (IsMouseButtonPressed(MouseButton.Right)) button = 1;

            if (key == 256)
            {
                _assigningShortcut = false;
                GLOBALS.LockNavigation = false;
                _mouseShortcutToAssign = null;
            }

            if (button != -1 && key != 340 && key != 341 && key != 342 && key != 256)
            {
                _mouseShortcutToAssign!.Button = (MouseButton)button;
                _mouseShortcutToAssign.Shift = IsKeyDown(KeyboardKey.LeftShift) || IsKeyDown(KeyboardKey.RightShift);
                _mouseShortcutToAssign.Ctrl = IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.RightControl);
                _mouseShortcutToAssign.Alt = IsKeyDown(KeyboardKey.LeftAlt) || IsKeyDown(KeyboardKey.RightAlt);
                
                _assigningShortcut = false;
                GLOBALS.LockNavigation = false;
                _mouseShortcutToAssign = null;
            }
            
        }
        
        #endregion
        
        BeginDrawing();
        
        ClearBackground(Color.Gray);

        rlImGui.Begin();
        
        // Navigation bar
                
        if (GLOBALS.Settings.GeneralSettings.Navbar) GLOBALS.NavSignal = Printers.ImGui.Nav(out _);

        if (ImGui.Begin("Settings##EditorSettings", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove))
        {
            ImGui.SetWindowSize(new Vector2(GetScreenWidth() - 80, GetScreenHeight() - 80));
            ImGui.SetWindowPos(new Vector2(40, 40));
            
            ImGui.Columns(2);
            ImGui.SetColumnWidth(0, 200);

            var col1Space = ImGui.GetContentRegionAvail();

            if (ImGui.BeginListBox("##SettingsCategories", col1Space with { Y = col1Space.Y - 120 }))
            {
                for (var category = 0; category < _settingsCategories.Length; category++)
                {
                    var selected = ImGui.Selectable(_settingsCategories[category], category == _settingsActiveCategory);

                    if (selected && !_assigningShortcut) _settingsActiveCategory = category;
                }
                ImGui.EndListBox();
            }

            var resetAllSelected = ImGui.Button("Reset Settings", col1Space with { Y = 20 });
            var saveAllSelected = ImGui.Button("Save Settings", col1Space with { Y = 20 });

            if (resetAllSelected)
            {
                _assigningShortcut = false;
                GLOBALS.LockNavigation = false;
                _shortcutToAssign = null;
                _mouseShortcutToAssign = null;

                GLOBALS.Settings = new Settings();
            }

            if (saveAllSelected)
            {
                try
                {
                    File.WriteAllText(GLOBALS.Paths.SettingsPath,
                        JsonSerializer.Serialize(GLOBALS.Settings, GLOBALS.JsonSerializerOptions));
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to save settings: {e}");
                }
            }
            
            //
            ImGui.NextColumn();
            //

            var col2Space = ImGui.GetContentRegionAvail();

            if (ImGui.BeginChild("##SettingsPanel", col2Space))
            {
                if (_settingsActiveCategory != 7 && _settingsActiveCategory != 8) {
                    var settings = _settingsActiveCategory switch {
                        0 => GLOBALS.Settings.GeneralSettings as object,
                        1 => GLOBALS.Settings.GeometryEditor,
                        2 => GLOBALS.Settings.TileEditor,
                        3 => GLOBALS.Settings.CameraSettings,
                        4 => GLOBALS.Settings.LightEditor,
                        5 => GLOBALS.Settings.EffectsSettings,
                        6 => GLOBALS.Settings.PropEditor,

                        _ => GLOBALS.Settings.GeneralSettings
                    };

                    Printers.ImGui.BindObject(settings);
                } else if (_settingsActiveCategory is 7) {
                    ImGui.ShowStyleEditor();
                } else {
                    if (ImGui.BeginChild("##ShortcutsPanel")) {
                        ImGui.Columns(2);
                        ImGui.SetColumnWidth(0, 200);

                        var avail = ImGui.GetContentRegionAvail();
                        ImGui.SeparatorText("Shortcut Categories");
                        if (ImGui.BeginListBox("##ShortcutCategories", avail with { Y = avail.Y - 90 })) {
                            
                            for (var c = 0; c < _shortcutsCategories.Length; c++) {
                                var selected = ImGui.Selectable(_shortcutsCategories[c], _shortcutsActiveCategory == c);

                                if (selected) _shortcutsActiveCategory = c;
                            }

                            ImGui.EndListBox();
                        }

                        ImGui.Spacing();

                        var resetSelected = ImGui.Button("Reset Shortcuts", col1Space with { Y = 20 });
                        var saveSelected = ImGui.Button("Save Shortcuts", col1Space with { Y = 20 });

                        if (resetSelected)
                        {
                            _assigningShortcut = false;
                            GLOBALS.LockNavigation = false;
                            _shortcutToAssign = null;
                            _mouseShortcutToAssign = null;

                            GLOBALS.Settings.Shortcuts = new Shortcuts();
                        }

                        if (saveSelected)
                        {
                            _assigningShortcut = false;
                            GLOBALS.LockNavigation = false;
                            _shortcutToAssign = null;
                            _mouseShortcutToAssign = null;
                        
                            try
                            {
                                File.WriteAllText(
                                    GLOBALS.Paths.SettingsPath, 
                                    JsonSerializer.Serialize(GLOBALS.Settings, GLOBALS.JsonSerializerOptions)
                                );
                            }
                            catch (Exception e)
                            {
                                Logger.Error($"Failed to save settings: {e}");
                            }
                        }

                        ImGui.NextColumn();

                        if (ImGui.BeginChild("##ActiveShortcutPanel")) {                        
                            var settings = GLOBALS.Settings;

                            var shortcutsContainer = _shortcutsActiveCategory switch { 
                                0 => settings.Shortcuts.GlobalShortcuts as object,
                                1 => settings.Shortcuts.GeoEditor as object,
                                2 => settings.Shortcuts.ExperimentalGeoShortcuts as object,
                                3 => settings.Shortcuts.TileEditor as object,
                                4 => settings.Shortcuts.CameraEditor as object,
                                5 => settings.Shortcuts.LightEditor as object,
                                6 => settings.Shortcuts.EffectsEditor as object,
                                7 => settings.Shortcuts.PropsEditor as object,

                                _ => settings.Shortcuts.GlobalShortcuts as object
                            };

                            var shortcuts = shortcutsContainer
                                .GetType()
                                .GetProperties()
                                .Where(p => p.PropertyType == typeof(KeyboardShortcut) || p.PropertyType == typeof(MouseShortcut))
                                .Select(property => {
                                    var attr = (ShortcutName?) property.GetCustomAttributes(typeof(ShortcutName), false).FirstOrDefault();

                                    var name = attr?.Name ?? property.Name;
                                    var combination = property.GetValue(shortcutsContainer);
                                    var group = attr?.Group ?? "";
                                    var isMouse = property.PropertyType == typeof(MouseShortcut);

                                    return (property, name, combination, group, isMouse);
                                });


                            var mouseShortcuts = shortcuts.Where(s => s.isMouse).GroupBy(s => s.group);

                            if (mouseShortcuts.Any()) ImGui.SeparatorText("Mouse Shortcuts");

                            if (ImGui.BeginTable("##MouseShortcuts", 3, ImGuiTableFlags.RowBg)) {
                                ImGui.TableSetupColumn("Name");
                                ImGui.TableSetupColumn("Combination");

                                ImGui.TableHeadersRow();

                                foreach (var group in mouseShortcuts) {
                                    foreach (var (property, name, combination, _, isMouse) in group) {

                                        ImGui.TableNextRow();

                                        ImGui.TableSetColumnIndex(0);

                                        ImGui.Text(name);
                                        
                                        ImGui.TableSetColumnIndex(1);
                                
                                        var clicked =  ImGui.Button($"{combination}");
                                        // ImGui.SameLine();
                                        // var reset = ImGui.Button("Reset");

                                        if (clicked) {
                                            _mouseShortcutToAssign = (MouseShortcut?)combination;
                                        }

                                        // if (reset) {
                                        //     (combination as MouseShortcut)!.Button = MouseButton.Left;
                                        // }
                                    }
                                }
                                ImGui.EndTable();
                            }


                            //

                            var keyboardShortcuts = shortcuts.Where(s => !s.isMouse).GroupBy(s => s.group);

                            if (keyboardShortcuts.Any()) {
                                ImGui.Spacing();
                                ImGui.SeparatorText("Keyboard Shortcuts");
                                ImGui.Spacing();
                            }


                            var counter = 0;

                            foreach (var group in keyboardShortcuts) {

                                if (!string.IsNullOrEmpty(group.Key)) ImGui.SeparatorText($"{group.Key}");

                                if (ImGui.BeginTable($"##KeyboardShortcuts_{group.Key}", 3, ImGuiTableFlags.RowBg)) {
                                    ImGui.TableSetupColumn("Name");
                                    ImGui.TableSetupColumn("Combination");

                                    ImGui.TableHeadersRow();
                                
                                    foreach (var (property, name, combination, _, isMouse) in group) {
                                        counter++;

                                        ImGui.TableNextRow();

                                        ImGui.TableSetColumnIndex(0);

                                        ImGui.Text(name);

                                        // ImGui.SameLine();

                                        ImGui.TableSetColumnIndex(1);

                                        var clicked =  ImGui.Button($"{combination}##COMBINATION_{counter}");
                                        // ImGui.SameLine();

                                        ImGui.TableSetColumnIndex(2);

                                        var reset = ImGui.Button($"Delete##DELETE_{counter}");

                                        if (clicked) {
                                            _shortcutToAssign = (KeyboardShortcut?)combination;
                                            ImGui.OpenPopup($"Settings Assigning Shortcut Alert {counter}");
                                        }

                                        if (ImGui.BeginPopupModal($"Settings Assigning Shortcut Alert {counter}")) 
                                        {
                                            // var size = ImGui.GetWindowSize();
                                            
                                            // ImGui.SetWindowPos(new Vector2(GetScreenWidth() - size.X, GetScreenHeight() - size.Y) / 2f);
                                            
                                            ImGui.Text("Enter the keyboard combination.");

                                            var cancelClicked = ImGui.Button("Cancel", ImGui.GetContentRegionAvail() with { Y = 20});

                                            if (cancelClicked || _shortcutToAssign is null) 
                                            {
                                                _shortcutToAssign = null;
                                                ImGui.CloseCurrentPopup();
                                            }

                                            ImGui.EndPopup();
                                        }

                                        if (reset) {
                                            _shortcutToReset = (KeyboardShortcut?)combination;
                                        }
                                    }
                                }
                                ImGui.EndTable();
                            }
                            ImGui.EndChild();
                        }
                        ImGui.EndChild();
                    }
                }
                ImGui.EndChild();
            }
            
            ImGui.End();
        }

        rlImGui.End();
        
        EndDrawing();
    }
}