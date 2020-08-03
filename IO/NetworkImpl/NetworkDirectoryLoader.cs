using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReOsuStoryboardPlayerOnline.IO.NetworkImpl
{
    public class NetworkDirectoryLoader : IDirectoryLoader
    {
        private Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();

        public IDirectoryReader GetReader()
        {
            if (files is null)
                throw new ObjectDisposedException(ToString());

            return new NetworkDirectoryReader(files);
        }
                
        public void AddFile(string path,byte[] content)
        {
            if (files is null)
                throw new ObjectDisposedException(ToString());

            path = path.Replace("/", "\\").ToLower();
            files[path] = content;
        }

        public void Dispose()
        {
            files = null;
        }
    }
}
