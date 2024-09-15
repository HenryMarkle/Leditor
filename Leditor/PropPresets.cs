using System.Text.Json;
using System.Text.Json.Serialization;
using Leditor.Data.Props.Legacy;
using Leditor.Serialization.Parser;
using Pidgin;

namespace Leditor;

public struct PropPreset {
    public string Label { get; set; }
    public List<Prop_Legacy> Props { get; set; }

    public PropPreset(string label, List<Prop_Legacy> props) {
        Label = label;
        Props = props;
    }
}

public struct PropPresetMeta {
    public string Label { get; set; }
    public string Props { get; set; }

    public PropPresetMeta(string label, string props) {
        Label = label;
        Props = props;
    }
}

public sealed class PropPresets
{
    private int _presetCount;

    public List<PropPreset> SavedProps { get; set; }

    public PropPresets(List<PropPresetMeta> presets) {
        List<PropPreset> imported = new(presets.Count);

        foreach (var preset in presets) {
            var parsed = LingoParser.Expression.ParseOrThrow(preset.Props);
            var props = Serialization.Importers.GetProps(parsed);
            imported.Add(new(preset.Label, props));
        }

        SavedProps = imported;
    }

    public PropPresets() {
        SavedProps = [];
        _presetCount = SavedProps.Count;
    }

    public PropPresets(IEnumerable<PropPreset> presets) {
        SavedProps = [..presets];
        _presetCount = SavedProps.Count;
    }

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
            .ToList();

        if (string.IsNullOrEmpty(label)) label = $"{++_presetCount}";

        SavedProps.Add(new(label, clones));
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

    public List<PropPresetMeta> Export() {
        List<PropPresetMeta> meta = new(SavedProps.Count);

        foreach (var preset in SavedProps) {
            var exported = Serialization.Exporters.Export([..preset.Props]);
        
            meta.Add(new(preset.Label, exported));
        }

        return meta;
    }
}