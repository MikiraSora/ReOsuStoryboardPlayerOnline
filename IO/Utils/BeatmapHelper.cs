using ReOsuStoryboardPlayerOnline.IO.NetworkImpl;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ReOsuStoryboardPlayerOnline.IO.Utils
{
    public static class BeatmapHelper
    {
        public static async Task<IDirectoryReader> LoadNetworkResources(int sid)
        {
            var first = sid / 10000;
            var second = sid % 10000;
            var url = $"https://cmcc.sayobot.cn:25225/beatmaps/{first}/{second}/novideo";

            var Client = new HttpClient();

            var content = await Client.GetByteArrayAsync(url);
            var zipDecoder = new ZipArchive(new MemoryStream(content));

            var loader = new NetworkDirectoryLoader();

            void ExtractEntry(ZipArchiveEntry entry)
            {
                var stream = new MemoryStream();
                entry.Open().CopyTo(stream);
                stream.Seek(0, SeekOrigin.Begin);
                var buffer = stream.ToArray();

                loader.AddFile(entry.FullName, buffer);
            }

            foreach (var entry in zipDecoder.Entries)
            {
                ExtractEntry(entry);
            }

            return loader.GetReader();
        }

        public static async Task<IDirectoryReader> LoadLocalResources()
        {
            return null;
        }
    }
}
