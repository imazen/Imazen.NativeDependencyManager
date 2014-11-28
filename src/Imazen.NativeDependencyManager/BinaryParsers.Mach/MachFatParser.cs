using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imazen.NativeDependencyManager.BinaryParsers.Mach
{
    class MachFatParser
    {
        const uint FAT_MAGIC = 0xcafebabe;
        const uint FAT_CIGAM = 0xbebafeca;

        /// <summary>
        /// Returns null if this is not a Fat binary, or if the endianess is wrong. 
        /// May return an empty list if the file is invalid, such that the supported architecture count is 0
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public IEnumerable<MachInfo> Parse(IIntegerReader s)
        {
            //Magic byte
            if (s.ReadUInt32() != FAT_MAGIC) return null;
            //architecture count
            var count = s.ReadUInt32();
            var list = new List<MachInfo>((int)count);
            for (var i = 0; i < count; i++)
                list.Add(ParseSingleArch(s));
            return list;
        }

        MachInfo ParseSingleArch(IIntegerReader s)
        {
            //CPU specifier
            var cpu = s.ReadInt32();
            //CPU subtype
            var subtype = s.ReadInt32();
            //offset, size, and alignment of obj. file
            s.ReadUInt32(); s.ReadUInt32(); s.ReadUInt32();
            return new MachInfo(MachHeaderType.MachMulti, cpu, subtype,null);
        }
    }
}
