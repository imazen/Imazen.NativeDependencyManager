using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imazen.NativeDependencyManager.BinaryParsers
{
    public class BinaryParser
    {
        //ELF - 20 bytes
        //Mach 48 bytes for 1 or 2-arch binary, 128 bytes for 6-arch
        //PE - 64 bytes at start of file, then seek, then read, then seek, then read..

        public Task<IBinaryInfo> ReadBinaryInfo(string physicalPath)
        {
            using (var s = File.Open(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                return ReadBinaryInfo(s);
        }

        public async Task<IBinaryInfo> ReadBinaryInfo(Stream s)
        {
            byte[] buffer = new byte[128];
            int count = await s.ReadAsync(buffer, 0, 128);
            if (count < 128) return null;
            s.Seek(0, SeekOrigin.Begin);

            var isPe = buffer[0] == 0x4d && buffer[1] == 0x5a;

            if (isPe){
                var pe = await Task.Run<PE.Image>(() => PE.ImageReader.ReadImageFrom(s, false));
                if (pe != null) return PeInfo(pe);
            }

            var elf = new ELF.ElfParser().Read(buffer);
            if (elf != null) return ElfInfo(elf);
                
            var mach = new Mach.MachParser().Parse(new BinaryReader(new MemoryStream(buffer, false)));
            if (mach != null) return MachInfo(mach);
            
            return null;
        }
        BinaryInfo PeInfo(PE.Image info)
        {
            var kind = (info.Kind == PE.ModuleKind.Dll) ? BinaryKind.Dll : (
                (info.Kind == PE.ModuleKind.Console ||
                info.Kind == PE.ModuleKind.Windows) ? BinaryKind.Executable : (
                BinaryKind.Other));


            return new BinaryInfo(kind, BinaryStructure.PE,
                new[] { info.Architecture }, info.DotNetRuntime != PE.TargetRuntime.NotDotNet,
                info.Attributes, info.DotNetRuntimeVersionString);
        }

        BinaryInfo ElfInfo(ELF.ElfInfo info)
        {
            var kind = (info.Flags == ELF.ElfFlags.Executable) ?
                BinaryKind.Executable : (info.Flags == ELF.ElfFlags.Shared) ? 
                BinaryKind.Dll : BinaryKind.Other;

            return new BinaryInfo(kind, BinaryStructure.ELF,
                new[] { ElfInstructionSet(info.InstructionSet) }, false, BinaryClrFlags.None, null);
        }

        InstructionSets ElfInstructionSet(ELF.ElfInstructionSet e)
        {
            if (e == ELF.ElfInstructionSet.IA_64) return InstructionSets.IA64;
            if (e == ELF.ElfInstructionSet.ARM) return InstructionSets.ARM;
            if (e == ELF.ElfInstructionSet.AArch64) return InstructionSets.ARM_64;
            if (e == ELF.ElfInstructionSet.x86) return InstructionSets.x86;
            if (e == ELF.ElfInstructionSet.x86_64) return InstructionSets.x86_64;
            return InstructionSets.Other;
        }

        BinaryInfo MachInfo(IEnumerable<Mach.MachInfo> info)
        {
            var sets = info.SelectMany(mi => MachInstructionSet(mi.CpuType)).ToArray();
            if (sets.Count() < 1)
            {
                return null;
            }
            var kind = MachKind(info);
            return new BinaryInfo(kind, BinaryStructure.Mach, sets, false, BinaryClrFlags.None, null);
        }

        InstructionSets[] MachInstructionSet(Mach.MachCpuType type)
        {
            if (type == Mach.MachCpuType.ARM) return new[]{InstructionSets.ARM};
            if (type == Mach.MachCpuType.ARM_64) return new[]{InstructionSets.ARM_64};
            if (type == Mach.MachCpuType.x86) return new[]{InstructionSets.x86};
            if (type == Mach.MachCpuType.x86_64) return new[]{InstructionSets.x86_64};
            //When authors claim to manually detect/handle instruction sets,
            //We sign them up for x86, 64, arm, arm_x64.
            if (type == Mach.MachCpuType.Any) return new[] { InstructionSets.ARM, InstructionSets.ARM_64, InstructionSets.x86, InstructionSets.x86_64 };
            return new[] { InstructionSets.Other };
        }

        BinaryKind MachKind(IEnumerable<Mach.MachInfo> info)
        {
            var types = info.Where(mi => mi.FileType.HasValue).Select(mi => mi.FileType.Value).ToArray();
            
            if (types.Length > 0){
                if (types[0] == Mach.MachFileType.Execute) return BinaryKind.Executable;
                if (types[0] == Mach.MachFileType.Dylib) return BinaryKind.Dll;
                return BinaryKind.Other;
            }
            //If there's no explict declaration, then we
            //Pretend it's a dll, since that's what is most likely wanted.
            //Why don't fat binaries have a file type?!?
            return BinaryKind.Dll; 
        }
    }
}
