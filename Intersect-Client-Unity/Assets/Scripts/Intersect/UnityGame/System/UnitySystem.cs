using Intersect.Client.Framework.Sys;
using System;

namespace Intersect.Client.UnityGame.System
{
    internal class UnitySystem : GameSystem
    {
        public readonly long StartTime;

        public long TotalMilliseconds;

        public UnitySystem()
        {
            StartTime = DateTime.UtcNow.Ticks;
        }

        public override long GetTimeMs()
        {
            return TotalMilliseconds;
        }

        public override long GetTimeMsExact()
        {
            return (DateTime.UtcNow.Ticks - StartTime) / TimeSpan.TicksPerMillisecond;
        }

        public override void Update()
        {
            TotalMilliseconds = (DateTime.UtcNow.Ticks - StartTime) / TimeSpan.TicksPerMillisecond;
        }
    }
}