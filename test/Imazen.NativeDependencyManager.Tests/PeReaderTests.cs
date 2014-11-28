using System;
using System.IO;

using Imazen.NativeDependencyManager.BinaryParsers;
using Imazen.NativeDependencyManager.BinaryParsers.PE;

using NUnit.Framework;

namespace Imazen.NativeDependencyManager.BinaryParsers.Tests
{

    [TestFixture]
    public class ImageReadTests : BaseTestFixture
    {

        [Test]
        public async void CSharp()
        {
            var cs = await Get("Csharp_", "x86.dll","x86_unsafe.dll",
                    "x64.dll","x64_unsafe.dll", "AnyCPU.dll", 
                    "AnyCPU_unsafe.dll");
            var x86 = cs[0];
            var x86_unsafe = cs[1];
            var x64 = cs[2];
            var x64_unsafe = cs[3];
            var anycpu = cs[4];
            var anycpu_unsafe = cs[5];

            foreach (var i in cs)
            {
                //Assert.AreEqual(TargetRuntime.Net_4_0, i.DotNetRuntime);
                Assert.True((BinaryClrFlags.ILOnly & i.ClrFlags) != 0); //They should all be IL only

                Assert.False((BinaryClrFlags.Preferred32Bit & i.ClrFlags) != 0);
                Assert.False((BinaryClrFlags.StrongNameSigned & i.ClrFlags) != 0);
                Assert.False((BinaryClrFlags.NativeEntryPoint & i.ClrFlags) != 0);
            }
            foreach (var i in new[] { x86, x86_unsafe })
            {
                Assert.True((BinaryClrFlags.Required32Bit & i.ClrFlags) != 0);
            }
            foreach (var i in new[] { x86, x86_unsafe, anycpu, anycpu_unsafe })
            {
                Assert.True(i.HasTarget(InstructionSets.x86));
            }
            foreach (var i in new[] {x64, x64_unsafe })
            {
                Assert.True(i.HasTarget(InstructionSets.x86_64));
            }
        }

        [Test]

        public async void TestNative()
        {
            var native_x86 = await GetBinaryInfo("NativeCpp_Release_Win32.dll");
            var native_x64 = await GetBinaryInfo("NativeCpp_Release_x64.dll");

            Assert.AreEqual(BinaryClrFlags.None, native_x86.ClrFlags);
            Assert.AreEqual(BinaryClrFlags.None, native_x64.ClrFlags);
            Assert.False(native_x86.IsDotNet);
            Assert.False(native_x64.IsDotNet);
            Assert.True(native_x64.HasTarget(InstructionSets.x86_64));
            Assert.True(native_x86.HasTarget(InstructionSets.x86));


        }

        [Test]
        public async void CppCli_2_0()
        {
            var cppcli2 = await GetBinaryInfo("cppcli.dll");

            //Assert.AreEqual(TargetRuntime.Net_2_0, cppcli2.DotNetRuntime);
            Assert.True(cppcli2.HasTarget(InstructionSets.x86));
            Assert.AreEqual(BinaryClrFlags.NativeEntryPoint, cppcli2.ClrFlags);
        }


        [Test]
        public async void CppCli_4_Win_32()
        {
            var w32_set = await Get("CppCli_Release_Win32", ".dll","_Safe.dll", "_Pure.dll" );
            var w32 = w32_set[0];
            var w32_safe = w32_set[1];
            var w32_pure = w32_set[2];

            foreach (var i in w32_set)
            {
                //Assert.AreEqual(TargetRuntime.Net_4_0, i.DotNetRuntime);
                Assert.True(i.HasTarget(InstructionSets.x86));
            }

            Assert.AreEqual(BinaryClrFlags.NativeEntryPoint, w32.ClrFlags);
            Assert.AreEqual(BinaryClrFlags.ILOnly , w32_safe.ClrFlags);

            Assert.AreEqual(BinaryClrFlags.ILOnly | BinaryClrFlags.Required32Bit, w32_pure.ClrFlags);


        }

        [Test]
        public async void CppCli_4_x64()
        {
            var x64_set = await Get("CppCli_Release_x64", ".dll", "_Safe.dll", "_Pure.dll");
            var x64 = x64_set[0];
            var x64_safe = x64_set[1];
            var x64_pure = x64_set[2];
            
            foreach (var i in x64_set)
            {
                //Assert.AreEqual(TargetRuntime.Net_4_0, i.DotNetRuntime);
            }

            Assert.True(x64.HasTarget(InstructionSets.x86_64));
            Assert.AreEqual(BinaryClrFlags.NativeEntryPoint, x64.ClrFlags);
            
            
            Assert.True(x64_safe.HasTarget(InstructionSets.x86)); //AnyCPU??
            Assert.AreEqual(BinaryClrFlags.ILOnly, x64_safe.ClrFlags);

            Assert.True(x64_pure.HasTarget(InstructionSets.x86_64));
            Assert.AreEqual(BinaryClrFlags.ILOnly, x64_pure.ClrFlags);

        }

        [Test]
        public async void ImageMetadataVersion()
        {
            var image = await GetBinaryInfo("hello.exe");
            //Assert.AreEqual(TargetRuntime.Net_2_0, image.DotNetRuntime);

            image = await GetBinaryInfo("hello1.exe");
           // Assert.AreEqual(TargetRuntime.Net_1_1, image.DotNetRuntime);
        }

        [Test]
        public async void ImageModuleKind()
        {
            var image = await GetBinaryInfo("hello.exe");
            Assert.AreEqual(BinaryKind.Executable, image.Kind);

            image = await GetBinaryInfo("libhello.dll");
            Assert.AreEqual(BinaryKind.Dll, image.Kind);

            image = await GetBinaryInfo("hellow.exe");
            Assert.AreEqual(BinaryKind.Executable, image.Kind);
        }

       
        [Test]
        public async void X64Module()
        {
            var i = await GetBinaryInfo("hello.x64.exe");
            Assert.True(i.HasTarget(InstructionSets.x86_64));
            Assert.AreEqual(BinaryClrFlags.ILOnly, i.ClrFlags);
           
        }

        [Test]
        public async void IA64Module()
        {
            var i = await GetBinaryInfo("hello.ia64.exe");
            Assert.True(i.HasTarget(InstructionSets.IA64));
            Assert.AreEqual(BinaryClrFlags.ILOnly, i.ClrFlags);
            
        }

        [Test]
        public async void X86Module()
        {
            var i = await GetBinaryInfo("hello.x86.exe");
            Assert.True(i.HasTarget(InstructionSets.x86));
            Assert.AreEqual(BinaryClrFlags.ILOnly | BinaryClrFlags.Required32Bit, i.ClrFlags);
            
        }

        [Test]
        public async void AnyCpuModule()
        {
            var i = await GetBinaryInfo("hello.anycpu.exe");
            Assert.True(i.HasTarget(InstructionSets.x86));
            Assert.AreEqual(BinaryClrFlags.ILOnly, i.ClrFlags);
           
        }

        [Test]
        public async void DelaySignedAssembly()
        {
            var i = await GetBinaryInfo("delay-signed.dll");
            Assert.AreNotEqual(BinaryClrFlags.StrongNameSigned, i.ClrFlags & BinaryClrFlags.StrongNameSigned);
           
        }

        [Test]
        public async void WindowsPhoneNonSignedAssembly()
        {
            var i = await GetBinaryInfo("wp7.dll");
            Assert.AreNotEqual(BinaryClrFlags.StrongNameSigned, i.ClrFlags & BinaryClrFlags.StrongNameSigned);
        }

        
    }
}
