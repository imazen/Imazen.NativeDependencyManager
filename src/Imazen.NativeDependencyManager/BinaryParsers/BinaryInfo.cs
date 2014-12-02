using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imazen.NativeDependencyManager.BinaryParsers
{
   
    public enum BinaryStructure
    {
        /// <summary>
        /// All .NET binaries and all windows binaries are in PE form.
        /// Horribly ineffecient - requires parsing >15kb
        /// </summary>
        PE = 1,
        /// <summary>
        /// Linux and most operating systems (excluding iOS/OSX) use this standard. Only 20 bytes are needed.
        /// </summary>
        ELF = 2,
        /// <summary>
        /// OS X and iOS binaries use this standard. Only 88 bytes are needed to read all arch info.
        /// </summary>
        Mach = 3
    }
    public enum BinaryKind
    {
        Dll = 1,
        Executable = 2,
        Other = 0xf0
    }


    [Flags]
    public enum BinaryClrFlags
    {
        None = 0,
        /// <summary>
        /// If true, only IL is present (although it could be unsafe IL)
        /// </summary>
        ILOnly = 1,
        /// <summary>
        /// If true, can only be run on 
        /// </summary>
        Required32Bit = 2,
        /// <summary>
        /// Present if strongly named
        /// </summary>
        StrongNameSigned = 8,
        /// <summary>
        /// Present on mixed-mode C++/CLI assemblies
        /// </summary>
        NativeEntryPoint = 0x10,
        TraceDebugData = 0x00010000,
        /// <summary>
        /// Allows .exe files to start on Windows ARM systems.
        /// Also makes AnyCPU programs run in 32-bit mode on 64-bit environments.
        /// </summary>
        Preferred32Bit = 0x00020000
    }

    public class BinaryInfo : IBinaryInfo
    {
        public BinaryInfo(BinaryKind kind, BinaryStructure format, ICollection<InstructionSets> nativeTargets,
            bool isDotNet, BinaryClrFlags clrFlags, string dotNetString)
        {
            Kind = kind;
            Structure = format;
            NativeTargets = nativeTargets;
            IsDotNet = isDotNet;
            ClrFlags = clrFlags;
            DotNetVersionString = dotNetString;
        }
        public BinaryKind Kind { get; private set; }
        public BinaryStructure Structure { get; private set; }
        public ICollection<InstructionSets> NativeTargets { get; private set; }

        public bool IsDotNet { get; private set; }

        public BinaryClrFlags ClrFlags { get; private set; }

        public string DotNetVersionString { get; private set; }

        public bool HasTarget(InstructionSets target)
        {
            return NativeTargets.Contains(target);
        }
    }

}
