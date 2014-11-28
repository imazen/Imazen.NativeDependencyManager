using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imazen.NativeDependencyManager.BinaryParsers.Mach
{
    class IntegerReader:IIntegerReader
    {

        BinaryReader s;
        bool reverse;
        public IntegerReader(BinaryReader s, bool reverseEndian)
        {
            this.s = s;
            this.reverse = reverseEndian;
        }
        public uint ReadUInt32()
        {
            var bytes = s.ReadBytes(4);
            if (reverse) Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes,0);
        }

        public int ReadInt32()
        {
            var bytes = s.ReadBytes(4);
            if (reverse) Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
    }
}
