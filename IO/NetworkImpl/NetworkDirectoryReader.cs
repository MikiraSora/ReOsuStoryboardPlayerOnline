using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ReOsuStoryboardPlayerOnline.IO.NetworkImpl
{
    public class NetworkDirectoryReader : IDirectoryReader
    {
        private IReadOnlyDictionary<string, byte[]> files;

        public NetworkDirectoryReader(IReadOnlyDictionary<string, byte[]> files)
        {
            this.files = files;
        }

        public IEnumerable<string> EnumeratePath(string pattern)
        {
            //将wide pattern转换成简单的正则
            var regExpr = Regex.Escape(pattern.Replace("*", "285702857").Replace("?", "285712857")).Replace("285702857",".*").Replace("285712857",".");
            var reg = new Regex(regExpr);

            return files.Keys.Where(x => reg.Match(x).Success);
        }

        public Stream ReadFile(string path)
        {
            return new MemoryStream(files[path]);
        }
    }
}
