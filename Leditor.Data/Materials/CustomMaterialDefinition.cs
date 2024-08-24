namespace Leditor.Data.Materials;

public sealed class CustomMaterialDefinition : MaterialDefinition
{
    public CustomMaterialDefinition(string name, Color color) : base(name, color, MaterialRenderType.CustomUnified) { }
}