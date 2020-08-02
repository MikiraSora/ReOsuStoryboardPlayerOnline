using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Kernel;
using ReOsuStoryboardPlayer.Core.Optimzer;
using ReOsuStoryboardPlayer.Core.Optimzer.DefaultOptimzer;
using ReOsuStoryboardPlayer.Core.Parser.Collection;
using ReOsuStoryboardPlayer.Core.Parser.Reader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ReOsuStoryboardPlayerOnline.Storyboard
{
    public static class StoryboardHelper
    {
        static StoryboardHelper()
        {
            StoryboardOptimzerManager.AddOptimzer(new ParserStaticOptimzer());
            StoryboardOptimzerManager.AddOptimzer(new RuntimeOptimzer());
            StoryboardOptimzerManager.AddOptimzer(new ConflictCommandRecoverOptimzer());
        }

        public static StoryboardUpdater ParseStoryboard(Stream osuFileStream,Stream osbFileStream)
        {
            var objects = CombineStoryboardObjects(ParseStoryboardFile(osbFileStream), ParseStoryboardFile(osuFileStream));
            var instance = new StoryboardUpdater(objects);
            return instance;
        }

        static List<StoryboardObject> ParseStoryboardFile(Stream fileStream)
        {
            using var osbFileReader = new ReOsuStoryboardPlayer.Core.Parser.Stream.OsuFileReader(fileStream);
            var variables = new VariableCollection(new VariableReader(osbFileReader).EnumValues());
            var sbReader = new StoryboardReader(new EventReader(osbFileReader, variables));

            var list = sbReader.EnumValues().OfType<StoryboardObject>().Select(x => {
                x.ImageFilePath = x.ImageFilePath.ToLower();
                return x;
            }).ToList();

            foreach (var obj in list)
                obj.CalculateAndApplyBaseFrameTime();

            StoryboardOptimzerManager.Optimze(2857, list);

            return list;
        }

        static List<StoryboardObject> CombineStoryboardObjects(List<StoryboardObject> osb_list, List<StoryboardObject> osu_list)
        {
            List<StoryboardObject> result = new List<StoryboardObject>();

            Add(Layout.Background);
            Add(Layout.Fail);
            Add(Layout.Pass);
            Add(Layout.Foreground);

            int z = 0;

            foreach (var obj in result)
                obj.Z = z++;

            return result;

            void Add(Layout layout)
            {
                result.AddRange(osu_list.Where(x => x.layout == layout));//先加osu
                result.AddRange(osb_list.Where(x => x.layout == layout).Select(x =>
                {
                    x.FromOsbFile = true;
                    return x;
                }));//后加osb覆盖
            }
        }
    }
}
