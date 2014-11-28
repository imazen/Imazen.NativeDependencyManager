using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imazen.NativeDependencyManager.BinaryParsers.Mach
{
    internal class MachInfo
    {
        internal MachInfo(MachHeaderType headerType, int cpu_type, int cpu_subtype, Nullable<MachFileType> file_type)
        {
            HeaderType = headerType;
            this.CpuType = (MachCpuType)cpu_type;
            this.cpu_subtype = cpu_subtype;
            this.FileType = file_type;
        }
        internal MachHeaderType HeaderType { get; private set; }
        internal Nullable<MachFileType> FileType { get; private set; }

        internal MachCpuType CpuType { get; private set; }
        internal int cpu_subtype { get; private set; }


        /// <summary>
        /// Architecture bits are present in cpu_type, visible with 0xff000000 mask.
        /// True if the 64 bit ABI bit is on. Valid with x86, ARM, and PowerPC
        /// </summary>
        internal bool CPU_ABI64 { get { return (CpuType & MachCpuType.ABI_64_Flag) != 0; } }


    }

    [Flags]
    internal enum MachCpuType : int
    {
        ABI_64_Flag = 0x01000000,
        Any = -1,
        Vax = 1,
        Mc680x0 = 6,
        x86 = 7,
        x86_64 = 7 | ABI_64_Flag,
        MC98000 = 10,
        HPPA = 11,
        ARM = 12,
        ARM_64 = 12 | ABI_64_Flag,
        MC88000 = 13,
        SPARC = 14,
        I860 = 15,
        PowerPC = 18,
        PowerPC_64 = 18 | ABI_64_Flag
    }

    internal enum MachHeaderType
    {
        Mach32,
        Mach64,
        MachMulti
    }

    internal enum MachFileType
    {
        /// <summary>
        /// relocatable object file
        /// </summary>
        Object = 0x1,
        /// <summary>
        /// demand paged executable file
        /// </summary>
        Execute = 0x2,
        /// <summary>
        ///  fixed VM shared library file
        /// </summary>
        FVMLib = 0x3,
        /// <summary>
        /// core file
        /// </summary>
        Core = 0x4,
        /// <summary>
        ///  preloaded executable file
        /// </summary>
        Preload = 0x5,
        /// <summary>
        /// dynamically bound shared library
        /// </summary>
        Dylib = 0x6,
        /// <summary>
        /// dynamic link editor
        /// </summary>
        Dylinker = 0x7,
        /// <summary>
        /// dynamically bound bundle file
        /// </summary>
        Bundle = 0x8,
        /// <summary>
        /// shared library stub for stati
        /// </summary>
        Dylib_stub = 0x9,
        /// <summary>
        /// companion file with only debug
        /// </summary>
        Dsym = 0xa,
        /// <summary>
        /// x86_64 kexts
        /// </summary>
        Kext_bundle = 0xb

    }

}
