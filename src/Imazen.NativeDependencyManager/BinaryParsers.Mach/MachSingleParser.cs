using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imazen.NativeDependencyManager.BinaryParsers.Mach
{
    internal class MachSingleParser
    {

        const uint MH_MAGIC = 0xfeedface;
        const uint MH_CIGAM = 0xcefaedfe;

        const uint MH_MAGIC_64 = 0xfeedfacf;
        const uint MH_CIGAM_64 = 0xcffaedfe;


        internal MachInfo Parse(IIntegerReader s)
        {
            var magic = s.ReadUInt32();
            MachHeaderType type;
            if (magic == MH_MAGIC) type = MachHeaderType.Mach32;
            else if (magic == MH_MAGIC_64) type = MachHeaderType.Mach64;
            else return null;

            var cpu = s.ReadInt32();
            var subtype = s.ReadInt32(); //machine specifier
            var filetype = s.ReadUInt32();
            s.ReadUInt32();//# of load cmds
            s.ReadUInt32();//size of load cms
            var flags = s.ReadUInt32(); //flags

            return new MachInfo(type, cpu, subtype, (MachFileType)filetype);
        }
    }
}
