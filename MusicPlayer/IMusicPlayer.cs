using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReOsuStoryboardPlayerOnline.MusicPlayer
{
    public interface IMusicPlayer
    {
        float GetCurrentTime();
        void Play();
        void Pause();
        void Stop();
        void Jump(float time);
    }
}
