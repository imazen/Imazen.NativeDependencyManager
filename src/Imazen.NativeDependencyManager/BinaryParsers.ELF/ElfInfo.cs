using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imazen.NativeDependencyManager.BinaryParsers.ELF
{
    internal class ElfInfo
    {
        internal ElfInfo(ElfFormat format, ElfInstructionSet instructionSet, ElfEndian endian, ElfAbi abi, ElfFlags flags)
        {
            Format = format;
            Abi = abi;
            InstructionSet = instructionSet;
            Flags = flags;
            Endian = endian;
        }
        internal ElfFormat Format { get; private set; }
        internal ElfInstructionSet InstructionSet { get; private set; }
        internal ElfAbi Abi { get; private set; }
        internal ElfFlags Flags { get; private set; }

        internal ElfEndian Endian { get; private set; }
    }
    enum ElfEndian
    {
        Little = 1,
        Big = 2

    }
    enum ElfFormat
    {
        Elf32 = 1,
        Elf64 = 2
    }
    enum ElfInstructionSet
    {
        SPARC = 0x02,
        x86 = 0x03,
        MIPS = 0x08,
        PowerPC = 0x14,
        ARM = 0x28,
        SuperH = 0x2A,
        IA_64 = 0x32,
        x86_64 = 0x3E,
        AArch64 = 0xB7
    }
    enum ElfAbi
    {
        Any = 0,
        HP_UX = 0x01,
        NetBSD = 0x02,
        Linux = 0x03,
        Solaris = 0x06,
        AIX = 0x07,
        IRIX = 0x08,
        FreeBSD = 0x09,
        OpenBSD = 0x0C
    }

    enum ElfFlags
    {
        Relocatable = 1,
        Executable = 2,
        Shared = 3,
        Core = 4
    }
}
