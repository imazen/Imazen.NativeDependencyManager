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

        public static string GetCSharpResourcePath(string name, Assembly assembly)
        {
            return GetResourcePath(Path.Combine("cs", name), assembly);
        }

        public static string GetILResourcePath(string name, Assembly assembly)
        {
            return GetResourcePath(Path.Combine("il", name), assembly);
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

        public static void TestModule(string file, Action<Image> test)
        {
            Run(new ModuleTestCase(file, test));
        }


        private static void Run(TestCase testCase)
        {
            var runner = new TestRunner(testCase);
            runner.RunTest();
            
        }
    }

    abstract class TestCase
    {

    
        public readonly Action<Image> Test;

        public abstract string ModuleLocation { get; }

        protected Assembly Assembly { get { return Test.Method.Module.Assembly; } }

        protected TestCase(Action<Image> test)
        {
            Test = test;
        }
    }

    class ModuleTestCase : TestCase
    {

        public readonly string Module;

        public ModuleTestCase(string module, Action<Image> test)
            : base(test)
        {
            Module = module;
        }

        public override string ModuleLocation
        {
            get { return BaseTestFixture.GetAssemblyResourcePath(Module, Assembly); }
        }
    }

    class TestRunner
    {

        readonly TestCase test_case;

        public TestRunner(TestCase testCase)
        {
            this.test_case = testCase;
        }

        Image GetImage()
        {
            var location = test_case.ModuleLocation;
            using (var stream = new FileStream (location, FileMode.Open, FileAccess.Read, FileShare.Read)){
                return ImageReader.ReadImageFrom(stream);
            }
            
        }
      
        public void RunTest()
        {
            var module = GetImage();
            if (module == null)
                return;

            test_case.Test(module);
        }
    }

}
