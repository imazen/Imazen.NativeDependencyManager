using Imazen.NativeDependencyManager.BinaryParsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imazen.NativeDependencyManager
{
    public class CompatChecker
    {
        EnvironmentProfile env;
        public CompatChecker()
        {
            env = EnvironmentProfile.Current;
        }
        public CompatChecker(EnvironmentProfile profile)
        {
            env = profile;
        }

        public bool IsCompatible(IBinaryInfo b)
        {
            //Only executables and fully dynamic libraries may pass
            if (b.Kind != BinaryKind.Dll && b.Kind != BinaryKind.Executable) return false;
            if (b.Structure == BinaryStructure.PE && b.IsDotNet)
                return clrCompatible(b);
            else
                return nativeCompatible(b);
            
        }

        private bool clrVersionSufficient(IBinaryInfo b)
        {
            return true; //For now; TODO fixme
        }
        private bool cpuCompatible(IBinaryInfo b)
        {
            return b.NativeTargets.Any(t => t == env.CpuCompatibility);
        }
        private bool nativeFormatCompatible(IBinaryInfo b)
        {
            if (b.Structure == BinaryStructure.PE && env.PlatformType == PlatformKind.LikeWindows) return true;
            if (b.Structure == BinaryStructure.Mach && env.PlatformType == PlatformKind.LikeOSX) return true;
            if (b.Structure == BinaryStructure.ELF && env.PlatformType == PlatformKind.LikeLinux) return true;
            return false;
        }

        private bool nativeCompatible(IBinaryInfo b)
        {
            return nativeFormatCompatible(b) && cpuCompatible(b);
        }
        private bool clrCompatible(IBinaryInfo b)
        {
            //Ensure the CLR version itself is sufficent
            if (!clrVersionSufficient(b)) return false;

            //If it's not pure IL, then dlls (usually C++/CLI) must be perfectly compatible with the native platform 
            if ((b.ClrFlags & BinaryClrFlags.NativeEntryPoint) > 0 ||
                ((b.ClrFlags & BinaryClrFlags.ILOnly) == 0))
            {
                return nativeCompatible(b);
            }

            //If the architecture of the PE wrapper works in the current environment, we're good.
            if (cpuCompatible(b)) return true;

            //If not, we need to handle cases where the CLR or Mono abstract it away.
            //32-bit PEs may be Any CPU
            if (b.NativeTargets.First() == InstructionSets.x86){
                //Preferred32Bit enables running on ARM.
                if ((b.ClrFlags & BinaryClrFlags.Preferred32Bit) > 0 &&
                    env.CpuCompatibility == InstructionSets.ARM){
                    return true;
                }
                //When Required 
                if ((b.ClrFlags & BinaryClrFlags.Required32Bit) == 0){
                    //ILOnly and not 32-bit specific? That's Any CPU!
                    //http://msdn.microsoft.com/en-us/library/system.reflection.portableexecutablekinds(v=vs.110).aspx
                    
                    return true; //TODO: Does this really include ARM, ARM64, and IA64?
                }else {
                    //TODO can Required32Bit IL work anywhere except x86?
                    return false;
                }
            }

            return false;

        }

    }
}
