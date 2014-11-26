using System;
using System.IO;

using Mono.Cecil;
using Mono.Cecil.PE;

using NUnit.Framework;

namespace Mono.Cecil.Tests
{

    [TestFixture]
    public class ImageReadTests : BaseTestFixture
    {

        [Test]
        public void CppCli()
        {
            TestModule("cppcli.dll", i =>
            {
                Assert.AreEqual(TargetRuntime.Net_2_0, i.ParseRuntime);
                Assert.AreEqual(TargetArchitecture.I386, i.Architecture);
                Assert.AreEqual(ModuleAttributes.Value_16, i.Attributes);
            });


            TestModule("CppCli_Release_x64.dll", i =>
            {
                Assert.AreEqual(TargetRuntime.Net_4_0, i.ParseRuntime);
                Assert.AreEqual(TargetArchitecture.AMD64, i.Architecture);
                Assert.AreEqual(ModuleAttributes.Value_16, i.Attributes);
            });

            TestModule("CppCli_Release_x64_Safe.dll", i =>
            {
                Assert.AreEqual(TargetRuntime.Net_4_0, i.ParseRuntime);
                Assert.AreEqual(TargetArchitecture.I386, i.Architecture); //AnyCPU??
                Assert.AreEqual(ModuleAttributes.ILOnly, i.Attributes);
            });

            TestModule("CppCli_Release_x64_Pure.dll", i =>
            {
                Assert.AreEqual(TargetRuntime.Net_4_0, i.ParseRuntime);
                Assert.AreEqual(TargetArchitecture.AMD64, i.Architecture);
                Assert.AreEqual(ModuleAttributes.ILOnly, i.Attributes);
            });

            TestModule("CppCli_Release_Win32.dll", i =>
            {
                Assert.AreEqual(TargetRuntime.Net_4_0, i.ParseRuntime);
                Assert.AreEqual(TargetArchitecture.I386, i.Architecture);
                Assert.AreEqual(ModuleAttributes.Value_16, i.Attributes);
            });

            TestModule("CppCli_Release_Win32_Safe.dll", i =>
            {
                Assert.AreEqual(TargetRuntime.Net_4_0, i.ParseRuntime);
                Assert.AreEqual(TargetArchitecture.I386, i.Architecture);
                Assert.AreEqual(ModuleAttributes.Value_16, i.Attributes);
            });

            TestModule("CppCli_Release_Win32_Pure.dll", i =>
            {
                Assert.AreEqual(TargetRuntime.Net_4_0, i.ParseRuntime);
                Assert.AreEqual(TargetArchitecture.I386, i.Architecture);
                Assert.AreEqual(ModuleAttributes.Value_16, i.Attributes);
            });

            //TestModule("NativeCpp_Release_x64.dll", i =>
            //{
            //    Assert.AreEqual(TargetRuntime.Net_4_0, i.ParseRuntime);
            //    Assert.AreEqual(TargetArchitecture.AMD64, i.Architecture);
            //    Assert.AreEqual(ModuleAttributes.Value_16, i.Attributes);
            //});

            //TestModule("NativeCpp_Release_Win32.dll", i =>
            //{
            //    Assert.AreEqual(TargetRuntime.Net_4_0, i.ParseRuntime);
            //    Assert.AreEqual(TargetArchitecture.I386, i.Architecture);
            //    Assert.AreEqual(ModuleAttributes.Value_16, i.Attributes);
            //});
        }


        [Test]
        public void ImageSections()
        {
            var image = GetResourceImage("hello.exe");

            Assert.AreEqual(3, image.Sections.Length);
            Assert.AreEqual(".text", image.Sections[0].Name);
            Assert.AreEqual(".rsrc", image.Sections[1].Name);
            Assert.AreEqual(".reloc", image.Sections[2].Name);
        }

        [Test]
        public void ImageMetadataVersion()
        {
            var image = GetResourceImage("hello.exe");
            Assert.AreEqual(TargetRuntime.Net_2_0, image.ParseRuntime);

            image = GetResourceImage("hello1.exe");
            Assert.AreEqual(TargetRuntime.Net_1_1, image.ParseRuntime);
        }

        [Test]
        public void ImageModuleKind()
        {
            var image = GetResourceImage("hello.exe");
            Assert.AreEqual(ModuleKind.Console, image.Kind);

            image = GetResourceImage("libhello.dll");
            Assert.AreEqual(ModuleKind.Dll, image.Kind);

            image = GetResourceImage("hellow.exe");
            Assert.AreEqual(ModuleKind.Windows, image.Kind);
        }

       
        [Test]
        public void X64Module()
        {
            TestModule("hello.x64.exe", i =>
            {
                Assert.AreEqual(TargetArchitecture.AMD64, i.Architecture);
                Assert.AreEqual(ModuleAttributes.ILOnly, i.Attributes);
            });
        }

        [Test]
        public void IA64Module()
        {
            TestModule("hello.ia64.exe", i =>
            {
                Assert.AreEqual(TargetArchitecture.IA64, i.Architecture);
                Assert.AreEqual(ModuleAttributes.ILOnly, i.Attributes);
            });
        }

        [Test]
        public void X86Module()
        {
            TestModule("hello.x86.exe", i =>
            {
                Assert.AreEqual(TargetArchitecture.I386, i.Architecture);
                Assert.AreEqual(ModuleAttributes.ILOnly | ModuleAttributes.Required32Bit, i.Attributes);
            });
        }

        [Test]
        public void AnyCpuModule()
        {
            TestModule("hello.anycpu.exe", i =>
            {
                Assert.AreEqual(TargetArchitecture.I386, i.Architecture);
                Assert.AreEqual(ModuleAttributes.ILOnly, i.Attributes);
            });
        }

        [Test]
        public void DelaySignedAssembly()
        {
            TestModule("delay-signed.dll", i =>
            {
  
                Assert.AreNotEqual(ModuleAttributes.StrongNameSigned, i.Attributes & ModuleAttributes.StrongNameSigned);
                Assert.AreNotEqual(0, i.StrongName.VirtualAddress);
                Assert.AreNotEqual(0, i.StrongName.Size);
            });
        }

        [Test]
        public void WindowsPhoneNonSignedAssembly()
        {
            TestModule("wp7.dll", i =>
            {
                Assert.AreNotEqual(ModuleAttributes.StrongNameSigned, i.Attributes & ModuleAttributes.StrongNameSigned);
                Assert.AreEqual(0, i.StrongName.VirtualAddress);
                Assert.AreEqual(0, i.StrongName.Size);
            });
        }

        [Test]
        public void MetroAssembly()
        {
            TestModule("metro.exe", module =>
            {
                Assert.AreEqual(ModuleCharacteristics.AppContainer, module.Characteristics & ModuleCharacteristics.AppContainer);
            });
        }
    }
}
