using System.Numerics;
using System.Text;

namespace Leditor.FileExplorer;

using ImGuiNET;

#nullable enable

public class FileX : IDisposable
{
    protected RL.Managed.Texture2D _fileIcon;
    protected RL.Managed.Texture2D _folderIcon;
    
    #region DisposePattern
    public bool Disposed { get; protected set; }

    public void Dispose()
    {
        if (Disposed) return;
        Disposed = true;

        _fileIcon.Dispose();
        _folderIcon.Dispose();
    }

    ~FileX()
    {
        if (!Disposed) throw new InvalidOperationException($"{GetType()} was not disposed by consumer");
    }
    #endregion
    
    public string Directory { get; set; }
    
    /// <summary>
    /// The name of the file without the extension
    /// </summary>
    public string FileName { get; set; }
    
    /// <summary>
    /// The full path of the file
    /// </summary>
    public string FilePath { get; set; }
    
    public string[] Filters { get; set; }
    
    public Vector2 WindowSize { get; protected set; }

    private const int BufSize = 300;

    private byte[] _pathBuffer;
    private string[] _entriesBuffer;

    protected void CopyToBuffer(string s)
    {
        var bytes = Encoding.UTF8.GetBytes(s);
        var minSize = Math.Min(bytes.Length, _pathBuffer.Length);

        for (var i = 0; i < minSize; i++) _pathBuffer[i] = bytes[i];
    }

    protected string CopyFromBuffer() => Encoding.UTF8.GetString(_pathBuffer);

    public FileX()
    {
        Directory = GLOBALS.Paths.ExecutableDirectory;
        _pathBuffer = Encoding.UTF8.GetBytes(Directory);
        Filters = [];
        FileName = string.Empty;
        FilePath = string.Empty;
        
        _pathBuffer = new byte[BufSize];
        CopyToBuffer(Directory);
        
        _entriesBuffer = System.IO.Directory
            .GetFileSystemEntries(Directory)
            .Select(e => Path.GetFileNameWithoutExtension(e) ?? e)
            .ToArray();
        
        _fileIcon = new RL.Managed.Texture2D(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "file icon.png"));
        _folderIcon = new RL.Managed.Texture2D(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "folder icon.png"));
    }
    
    protected enum DrawResult { Ok, Cancel, None }
    protected enum DrawPurpose { OpenFile, SaveFile }

    protected DrawResult Draw(DrawPurpose purpose)
    {
        // ImGui.OpenPopup("##FileExplorer");

        var result = DrawResult.None;
        
        ImGuiExt.CenterNextWindow(ImGuiCond.Appearing);
        ImGui.SetNextWindowSizeConstraints(new Vector2(0f, ImGui.GetTextLineHeight() * 30.0f), Vector2.One * 9999f);
        var opened = ImGui.Begin("File Explorer##FileExplorer",  ImGuiWindowFlags.NoCollapse);
        
        WindowSize = ImGui.GetWindowSize();

        if (opened)
        {
            ImGui.Button("Up");
            ImGui.SameLine();
            
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            ImGui.InputText("##Path", _pathBuffer, (uint)_pathBuffer.Length-1);
            
            ImGui.Spacing();
            
            if (ImGui.BeginListBox("##Entries", ImGui.GetContentRegionAvail() - new Vector2(0, 30)))
            {
                foreach (var entry in _entriesBuffer)
                {
                    ImGui.Selectable(entry);
                }
                ImGui.EndListBox();
            }

            if (ImGui.Button("Cancel")) result = DrawResult.Cancel;
            ImGui.SameLine();
            if (ImGui.Button("Ok")) result = DrawResult.Ok;
            
            ImGui.EndPopup();
        }

        return result;
    }
    
    public bool? OpenFile()
    {
        var result = Draw(DrawPurpose.OpenFile);
        
        if (result == DrawResult.Ok) return true;
        if (result == DrawResult.Cancel) return false;
        if (result == DrawResult.None) return null;
        
        return null;
    }
}