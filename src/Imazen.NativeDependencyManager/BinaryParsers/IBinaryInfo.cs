using System;
namespace Imazen.NativeDependencyManager.BinaryParsers
{
    public interface IBinaryInfo
    {
        BinaryClrFlags ClrFlags { get; }
        string DotNetVersionString { get; }
        bool IsDotNet { get; }
        BinaryKind Kind { get; }
        System.Collections.Generic.ICollection<InstructionSets> NativeTargets { get; }
        BinaryStructure Structure { get; }
    }
}
