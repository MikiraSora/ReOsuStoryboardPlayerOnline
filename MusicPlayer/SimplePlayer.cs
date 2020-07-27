using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace ReOsuStoryboardPlayerOnline.MusicPlayer
{
    public class SimplePlayer : IMusicPlayer
    {
        private readonly Stopwatch sw = new Stopwatch();
        private float offsetMs = 0;

        public float GetCurrentTime()
        {
            return sw.ElapsedMilliseconds + offsetMs;
        }

        public void Jump(float time)
        {
            offsetMs = time - sw.ElapsedMilliseconds;
        }

        public void Pause()
        {
            sw.Stop();
        }

        public void Play()
        {
            sw.Start();
        }

        public void Stop()
        {
            offsetMs = 0;
            sw.Reset();
        }
    }
}
