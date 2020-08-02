using ReOsuStoryboardPlayer;
using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Kernel;
using ReOsuStoryboardPlayer.Core.Optimzer;
using ReOsuStoryboardPlayer.Core.Optimzer.DefaultOptimzer;
using ReOsuStoryboardPlayer.Core.Parser.Collection;
using ReOsuStoryboardPlayer.Core.Parser.Reader;
using ReOsuStoryboardPlayerOnline.IO.Utils;
using ReOsuStoryboardPlayerOnline.Storyboard;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace TestConsle
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var bufferSize = 123456;
            var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            var memory = buffer.AsMemory().Slice(0, bufferSize);
            MemoryMarshal.TryGetArray<byte>(memory, out var segment);

            var reader = await BeatmapHelper.LoadNetworkResources(94790);
            Console.WriteLine("Start to select a .osb file and a .osu file (if it exist.)");
            var osbFilePath = reader.EnumeratePath("*.osb").FirstOrDefault();
            var osuFilePath = reader.EnumeratePath("*.osu").FirstOrDefault();


            var updater = StoryboardHelper.ParseStoryboard(reader.ReadFile(osbFilePath), reader.ReadFile(osuFilePath));

            Console.WriteLine(updater.StoryboardObjectList.Count);
        }
    }
}
