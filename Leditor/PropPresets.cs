using System.Text.Json;
using Leditor.Data.Props.Legacy;

namespace Leditor;

internal sealed class PropPresets
{
    private int _presetCount;
    public List<(string Label, Prop_Legacy[] Props)> SavedProps { get; private set; } = [];

    /// <summary>
    /// Adds a collection of props to the saved presets list, while normalizing their quads.
    /// </summary>
    public void Add(IEnumerable<Prop_Legacy> props, string label = "")
    {
        var enclosingRect = Utils.EncloseProps(props.Select(p => p.Quad));
    
        var clones = props
            .AsParallel()
            .Select(p => {
                var clone = p.Clone();

                var newQuad = clone.Quad - enclosingRect.Position;

                clone.Quad = newQuad;

                return clone;
            })
            .ToArray();

        if (string.IsNullOrEmpty(label)) label = $"{++_presetCount}";

        SavedProps.Add((label, clones));
    }

    /// <summary>
    /// Renames a preset.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">If index is invalid</exception>
    public void Rename(int index, string newName)
    {
        var item = SavedProps[index];

        item.Label = newName;

        SavedProps[index] = item;
    }

    public void Remove(int index)
    {
        if (SavedProps.Count > 0 && index >= 0 && index < SavedProps.Count) SavedProps.RemoveAt(index);
    }

    public void Import(string json)
    {
        SavedProps = JsonSerializer.Deserialize<List<(string, Prop_Legacy[])>>(json, new JsonSerializerOptions() { WriteIndented = true }) 
            ?? throw new Exception("Failed to deserialize list");
    }

    public string Export() => JsonSerializer.Serialize(SavedProps, new JsonSerializerOptions() { WriteIndented = true });
}