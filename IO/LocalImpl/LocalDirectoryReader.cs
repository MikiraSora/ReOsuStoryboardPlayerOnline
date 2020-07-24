using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ReOsuStoryboardPlayerOnline.IO.LocalImpl
{
    public class LocalDirectoryReader : IDirectoryReader
    {
        public IEnumerable<string> EnumeratePath(string pattern)
        {
            throw new NotImplementedException();
        }

        public Stream ReadFile(string path)
        {
            throw new NotImplementedException();
        }
    }
}
