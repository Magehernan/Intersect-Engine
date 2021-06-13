using System;
using System.Collections.Generic;
using Intersect.Client.Framework.GenericClasses;

namespace Intersect.Client.Framework.Graphics
{
    public abstract class GameRenderer
    {
        public bool HasOverrideResolution => OverrideResolution != Resolution.Empty;

        public Resolution ActiveResolution => new Resolution(PreferredResolution, OverrideResolution);

        public Resolution OverrideResolution { get; set; }

        public Resolution PreferredResolution { get; set; }

        public abstract void Init();

        public abstract void SetView(FloatRect view);

        public abstract FloatRect GetView();

        public abstract int GetScreenWidth();

        public abstract int GetScreenHeight();

        public abstract bool DisplayModeChanged();

        public abstract List<string> GetValidVideoModes();
    }

}
