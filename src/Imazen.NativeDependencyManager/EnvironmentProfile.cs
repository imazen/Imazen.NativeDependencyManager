using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Imazen.NativeDependencyManager
{
    public enum PlatformKind{
        LikeWindows,
        LikeOSX,
        LikeLinux
    }
    public enum InstructionSets
    {
        x86 = 1,
        x86_64 = 2,
        ARM = 3,
        ARM_64 = 4,
        IA64 = 5,
        Other = 6
    }

    public class EnvironmentProfile
    {
        //TODO: refactor this to express capabilities instead

        //TODO: verify that resource assemblies are architecture-agnostic https://en.wikipedia.org/wiki/Assembly_(CLI)#Satellite_assemblies
        public PlatformKind PlatformType { get; set; }
        public InstructionSets CpuCompatibility { get; set; }

        public string LocaleString { get; set; }

        public string ClrVersionString { get; set; }


        private static string monoDisplayName()
        {
            Type monoRuntimeType;
            MethodInfo getDisplayNameMethod;
            if ((monoRuntimeType = typeof(object).Assembly.GetType("Mono.Runtime")) != null && (getDisplayNameMethod = monoRuntimeType.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.ExactBinding, null, Type.EmptyTypes, null)) != null)
                return (string)getDisplayNameMethod.Invoke(null, null);
            return null;
        }

        private static PlatformKind currentPlatformKind()
        {
            var plat = Environment.OSVersion.Platform;
            if (plat == PlatformID.Unix || plat == (PlatformID)128) 
                return PlatformKind.LikeLinux;
            if (plat == PlatformID.MacOSX) 
                return PlatformKind.LikeOSX;
            return PlatformKind.LikeWindows;
        }
        public static EnvironmentProfile Current
        {
            get
            {
                string clrVersion = monoDisplayName() ?? System.Environment.Version.ToString();

                //TODO: add ARM and IA64 detection
                var runningInstructionSet = Environment.Is64BitProcess ?  InstructionSets.x86_64 : InstructionSets.x86;

                return new EnvironmentProfile()
                {
                    ClrVersionString = clrVersion,
                    LocaleString = CultureInfo.CurrentCulture.ToString(),
                    CpuCompatibility = runningInstructionSet,
                    PlatformType = currentPlatformKind()
                };

            }
        }
    }
}
