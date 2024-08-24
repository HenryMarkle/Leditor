namespace Leditor.Data;

public class EffectOptions(string name, IEnumerable<string> options, dynamic choice)
{
    public string Name { get; set; } = name;
    public string[] Options { get; set; } = [.. options];
    public dynamic Choice { get; set; } = choice;
}