namespace Leditor.Types;

/// <summary>Attribute used exclusively on classes that implement <see cref="IEditorShortcuts"/></summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
internal class ShortcutName(string name) : Attribute {
    public string Name { get; set; } = name;

    public string Group { get; set; } = "";
}