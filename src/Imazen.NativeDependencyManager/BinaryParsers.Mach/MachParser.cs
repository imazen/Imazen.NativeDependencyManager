using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imazen.NativeDependencyManager.BinaryParsers.Mach
{
    class MachParser
    {

        internal IEnumerable<MachInfo> Parse(BinaryReader s)
        {
            var list = new List<MachInfo>();
            foreach (var reverseEndian in new[]{true,false}){
                var reader = new IntegerReader(s,reverseEndian);
                s.BaseStream.Seek(0, SeekOrigin.Begin);
                var info = new MachSingleParser().Parse(reader);
                if (info != null) list.Add(info);
                s.BaseStream.Seek(0, SeekOrigin.Begin);
                var multi = new MachFatParser().Parse(reader);
                if (multi != null) list.AddRange(multi);
            }
            return list.Count() > 0 ? list : null;
        }
    }
}
