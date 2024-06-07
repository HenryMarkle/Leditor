namespace Leditor;

/// <summary>Attribute used exclusively on classes that implement <see cref="IEditorShortcuts"/></summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
internal class ShortcutName(string name) : Attribute {
    public string Name { get; set; } = name;

    public string Group { get; set; } = "";
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
internal class SettingName(string name) : Attribute {
    public string Name { get; set; } = name;
    public string Group { get; set; } = "";
    public bool Disabled { get; set; }
    public bool Hidden { get; set; }
    public string Description { get; set; } = "";
}

/// <summary>
/// Sets a min and max bounds to the setting.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
internal class IntBounds(int min) : Attribute {
    public int Min { get; set; } = min;
    public int Max { get; set; }
}

/// <summary>
/// Sets a min and max bounds to the setting.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
internal class FloatBounds(float min) : Attribute {
    public float Min { get; set; } = min;
    public float Max { get; set; }
}

/// <summary>
/// Sets a character limit to the string setting.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
internal class StringBounds(uint minLength) : Attribute {
    public uint MinLength { get; set; } = minLength;
    public uint MaxLength { get; set; }
}


/// <summary>
/// Used to enable or disable the setting based on another setting (dependancy).
/// The dependancy setting must be a boolean type.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
internal class SettingDependancy(string settingName) : Attribute {
    public string SettingName { get; set; } = settingName;
}
