using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imazen.NativeDependencyManager.BinaryParsers.Mach
{
    public interface IIntegerReader
    {
        UInt32 ReadUInt32();
        Int32 ReadInt32();
    }
}
