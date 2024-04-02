﻿using Leditor.Data.Props.Settings;
using Raylib_cs;

namespace Leditor.Data.Props.Definitions;

public sealed class SoftEffect(string name, int depth, Texture2D texture) : PropDefinition(name, depth, texture)
{
    public override PropSettings NewSettings(int renderOrder = 0, int seed = 0, int renderTime = 0)
        => new SoftEffectSettings(renderOrder, seed, renderTime);
}