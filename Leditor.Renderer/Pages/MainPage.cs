namespace Leditor.Renderer.Pages;

using ImGuiNET;
using rlImGui_cs;

using Raylib_cs;
using static Raylib_cs.Raylib;

using System.Numerics;

public sealed class MainPage
{

    private string _projects = "";

    private int _currentProject;

    public string[] ProjectFiless { get; private set; } = [];
    public string[] ProjectNames { get; private set; } = [];

    public required Context Context { get; init; }

    public required string ProjectsFolder
    { 
        get => _projects; 
        set
        {
            if (!Directory.Exists(value)) throw new DirectoryNotFoundException(value);

            _projects = value;

            ProjectFiless = Directory.GetFiles(value);
            ProjectNames = ProjectFiless.Select(f => Path.GetFileNameWithoutExtension(f)!).ToArray();
            _currentProject = 0;
        } 
    }

    public Serilog.ILogger? Logger { get; init; }

    public void Draw()
    {
        ClearBackground(Color.Gray);

        rlImGui.Begin();
        {
            if (ImGui.Begin("Projects", 
                ImGuiWindowFlags.NoCollapse | 
                ImGuiWindowFlags.NoResize | 
                ImGuiWindowFlags.NoMove))
            {
                ImGui.SetWindowSize(new Vector2(GetScreenWidth() - 80, GetScreenHeight() - 80));
                ImGui.SetWindowPos(new Vector2(40, 40));

                if (ImGui.BeginListBox("ProjectsList"))
                {
                    for (var p = 0; p < ProjectNames.Length; p++)
                    {
                        if (ImGui.Selectable(ProjectNames[p], p == _currentProject))
                        {
                            _currentProject = p;
                        }
                    }


                    ImGui.EndListBox();
                }

                ImGui.End();
            }
        }
        rlImGui.End();
    }
}