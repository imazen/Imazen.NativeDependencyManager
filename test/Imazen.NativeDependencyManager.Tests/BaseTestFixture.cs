using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

using Mono.Cecil.PE;

namespace Mono.Cecil.Tests
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

        internal Image GetResourceImage(string name)
        {
            using (var fs = new FileStream(GetAssemblyResourcePath(name, GetType().Assembly), FileMode.Open, FileAccess.Read))
                return ImageReader.ReadImageFrom(fs);
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
