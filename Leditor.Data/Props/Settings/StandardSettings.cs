﻿namespace Leditor.Data.Props.Settings;

public class StandardSettings(int renderOrder = 0, int seed = 0, int renderTime = 0) 
    : PropSettings(renderOrder, seed, renderTime) {
        public override PropSettings Clone() => new StandardSettings(RenderOrder, Seed, RenderTime);
    }