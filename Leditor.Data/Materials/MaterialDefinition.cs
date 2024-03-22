using Leditor.Data.Generic;

namespace Leditor.Data.Materials;

public record MaterialDefinition(string Name) : IIdentifiable<string>;