﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReOsuStoryboardPlayerOnline.IO
{
    public interface IDirectoryLoader : IDisposable
    {
        IDirectoryReader GetReader();
    }
}
