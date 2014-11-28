using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imazen.NativeDependencyManager.BinaryParsers.ELF
{
    
    internal class ElfParser
    {
         static byte[] magic_elf = new byte[]{0x7F, 0x45,0x4C, 0x46};
        internal ElfInfo Read(Stream r)
        {
            byte[] buf = new byte[20];
            if (r.Read(buf,0,20) < 20) return null;
            return Read(buf);
         }

        internal ElfInfo Read(byte[] header){
            if (header.Length < 20) return null;
            //https://en.wikipedia.org/wiki/Executable_and_Linkable_Format
        
            //0x7F followed by ELF in ASCII; these four bytes constitute the magic number.
            if (header[0] != magic_elf[0] || 
                header[1] != magic_elf[1] ||
                header[2] != magic_elf[2] ||
                header[3] != magic_elf[3]) return null;

            
            //1 byte; elf format. 1=32, 2 = 64;
            var format = (ElfFormat)header[4];
            //1 byte; endianess. 1=little, 2=big. Affects 
            var endian = (ElfEndian)header[5];
            //1 byte; 1 for the original version of ELF.
            
            //1 byte; target operating system ABI.
            var abi = (ElfAbi)header[7];
            //8 bytes; padding
            //2 bytes; relocatable=1, executable=2, shared=3, or core=4
            var flags = (ElfFlags)(endian == ElfEndian.Little ? header[16] : header[17]);
            //2 bytes; Target instruction set
            var instruction = (ElfInstructionSet)(endian == ElfEndian.Little ? header[18] : header[19]);

            return new ElfInfo(format, instruction, endian, abi, flags);
        }

    }
}
