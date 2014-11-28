using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using System.Linq;
using Imazen.NativeDependencyManager.BinaryParsers.PE;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Imazen.NativeDependencyManager.BinaryParsers.Tests
{

    public  abstract class BaseTestFixture
    {

        public static string GetResourcePath(string name, Assembly assembly)
        {
            return Path.Combine(FindResourcesDirectory(assembly), name);
        }

        public static string GetAssemblyResourcePath(string name, Assembly assembly)
        {
            return GetResourcePath(Path.Combine("assemblies", name), assembly);
        }

        internal Task<BinaryInfo[]> Get(string prefix, params string[] suffixes)
        {
            var tasks = suffixes.Select(s => GetBinaryInfo(prefix + s));
            return Task.WhenAll(tasks);
        }

        internal async Task<BinaryInfo> GetBinaryInfo(string name)
        {
            using (var fs = new FileStream(GetAssemblyResourcePath(name, GetType().Assembly), FileMode.Open, FileAccess.Read))
            {
                var r = await new BinaryParser().ReadBinaryInfo(fs);
                Trace.Write("Read " + fs.Position + " bytes from " + name + "\n");
                return r;
            }
        }


        public static string FindResourcesDirectory(Assembly assembly)
        {
            var path = Path.GetDirectoryName(new Uri(assembly.CodeBase).LocalPath);
            while (!Directory.Exists(Path.Combine(path, "Resources")))
            {
                var old = path;
                path = Path.GetDirectoryName(path);
                Assert.AreNotEqual(old, path);
            }

            return Path.Combine(path, "Resources");
        }

    }

}
