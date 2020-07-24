using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ReOsuStoryboardPlayerOnline.IO
{
    public interface IDirectoryReader
    {
        Stream ReadFile(string path);

        IEnumerable<string> EnumeratePath(string pattern);
    }
}
