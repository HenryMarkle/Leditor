﻿namespace Leditor.Data.Props.Settings;

public class VariedStandardSettings(int renderOrder = 0, int seed = 0, int renderTime = 0,int variation = 0) 
    : PropSettings(renderOrder, seed, renderTime), IVariant
{
    public int Variation { get; set; } = variation;

    public override PropSettings Clone() => new VariedStandardSettings(RenderOrder, Seed, RenderTime);
}