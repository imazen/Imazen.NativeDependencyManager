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
        public void CSharp()
        {
            var x86 = GetResourceImage("Csharp_x86.dll");
            var x86_unsafe = GetResourceImage("Csharp_x86_unsafe.dll");
            var x64 = GetResourceImage("Csharp_x64.dll");
            var x64_unsafe = GetResourceImage("Csharp_x64_unsafe.dll");
            var anycpu = GetResourceImage("Csharp_AnyCPU.dll");
            var anycpu_unsafe = GetResourceImage("Csharp_AnyCPU_unsafe.dll");

            foreach (var i in new[] { x86, x86_unsafe, x64, x64_unsafe, anycpu, anycpu_unsafe })
            {
                Assert.AreEqual(TargetRuntime.Net_4_0, i.ParseRuntime);
                Assert.True((ModuleAttributes.ILOnly & i.Attributes) != 0); //They should all be IL only

                Assert.False((ModuleAttributes.Preferred32Bit & i.Attributes) != 0);
                Assert.False((ModuleAttributes.StrongNameSigned & i.Attributes) != 0);
                Assert.False((ModuleAttributes.Value_16 & i.Attributes) != 0);
            }
            foreach (var i in new[] { x86, x86_unsafe })
            {
                Assert.True((ModuleAttributes.Required32Bit & i.Attributes) != 0);
            }
            foreach (var i in new[] { x86, x86_unsafe, anycpu, anycpu_unsafe })
            {
                Assert.AreEqual(TargetArchitecture.I386, i.Architecture);
            }
            foreach (var i in new[] {x64, x64_unsafe })
            {
                Assert.AreEqual(TargetArchitecture.AMD64, i.Architecture);
            }
        }

        [Test]

        public void TestNative()
        {
            var native_x86 = GetResourceImage("NativeCpp_Release_Win32.dll");
            var native_x64 = GetResourceImage("NativeCpp_Release_x64.dll");

            Assert.AreEqual(ModuleAttributes.None, native_x86.Attributes);
            Assert.AreEqual(ModuleAttributes.None, native_x64.Attributes);
            Assert.AreEqual(TargetRuntime.NotDotNet, native_x86.ParseRuntime);
            Assert.AreEqual(TargetRuntime.NotDotNet, native_x64.ParseRuntime);
            Assert.AreEqual(TargetArchitecture.AMD64, native_x64.Architecture);
            Assert.AreEqual(TargetArchitecture.I386, native_x86.Architecture);


        }

        [Test]
        public void CppCli_2_0()
        {
            var cppcli2 = GetResourceImage("cppcli.dll");

            Assert.AreEqual(TargetRuntime.Net_2_0, cppcli2.ParseRuntime);
            Assert.AreEqual(TargetArchitecture.I386, cppcli2.Architecture);
            Assert.AreEqual(ModuleAttributes.Value_16, cppcli2.Attributes);
        }


        [Test]
        public void CppCli_4_Win_32()
        {

            var w32 = GetResourceImage("CppCli_Release_Win32.dll");
            var w32_safe = GetResourceImage("CppCli_Release_Win32_Safe.dll");
            var w32_pure = GetResourceImage("CppCli_Release_Win32_Pure.dll");

            foreach (var i in new[] { w32, w32_safe, w32_pure })
            {
                Assert.AreEqual(TargetRuntime.Net_4_0, i.ParseRuntime);
                Assert.AreEqual(TargetArchitecture.I386, i.Architecture);
            }

            Assert.AreEqual(ModuleAttributes.Value_16, w32.Attributes);
            Assert.AreEqual(ModuleAttributes.ILOnly , w32_safe.Attributes);

            Assert.AreEqual(ModuleAttributes.ILOnly | ModuleAttributes.Required32Bit, w32_pure.Attributes);


        }

        [Test]
        public void CppCli_4_x64()
        {
            var x64 = GetResourceImage("CppCli_Release_x64.dll");
            var x64_safe = GetResourceImage("CppCli_Release_x64_Safe.dll");
            var x64_pure = GetResourceImage("CppCli_Release_x64_Pure.dll");
            
            foreach (var i in new[] { x64, x64_safe, x64_pure })
            {
                Assert.AreEqual(TargetRuntime.Net_4_0, i.ParseRuntime);
            }

            Assert.AreEqual(TargetArchitecture.AMD64, x64.Architecture);
            Assert.AreEqual(ModuleAttributes.Value_16, x64.Attributes);
            
            
            Assert.AreEqual(TargetArchitecture.I386, x64_safe.Architecture); //AnyCPU??
            Assert.AreEqual(ModuleAttributes.ILOnly, x64_safe.Attributes);

            Assert.AreEqual(TargetArchitecture.AMD64, x64_pure.Architecture);
            Assert.AreEqual(ModuleAttributes.ILOnly, x64_pure.Attributes);

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
