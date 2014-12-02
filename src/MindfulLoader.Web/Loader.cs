using Imazen.NativeDependencyManager;
using Imazen.NativeDependencyManager.BinaryParsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MindfulLoader.Web
{
    public class Loader
    {
        private static IEnumerable<string> GetDylibsInFolder(string dir)
        {
            var validExtensions = new string[] {"", ".dll", ".so", ".dylib" };

            return Directory.GetFiles(dir)
                .Where(file => validExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase));

        }
        private async static Task<Tuple<string,IBinaryInfo>[]> ParseBinaryInfo(IEnumerable<string> files){
            var tasks = files.Select(path => new BinaryParser().ReadBinaryInfo(path));
            await Task.WhenAll(tasks);
            return tasks.Select(t => t.Result).Zip(files, (info, path) => new Tuple<string, IBinaryInfo>(path, info)).ToArray();
        }
        public static void PreApplicationStart()
        {
            var files = GetDylibsInFolder(HttpRuntime.BinDirectory);

            var parsedFiles = ParseBinaryInfo(files).Result;

            var incompatible = parsedFiles.Where(t => !new CompatChecker().IsCompatible(t.Item2));

        }
    }
}
