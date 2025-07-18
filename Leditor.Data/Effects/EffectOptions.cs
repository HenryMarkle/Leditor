namespace Leditor.Data.Effects;

public class EffectOptions(string name, IEnumerable<string> options, dynamic choice)
{
    public string Name { get; set; } = name;
    public string[] Options { get; set; } = [.. options];
    public dynamic Choice { get; set; } = choice;

    public override string ToString() => $"{Name}, [{string.Join(", ", Options)}], {Choice}";
}