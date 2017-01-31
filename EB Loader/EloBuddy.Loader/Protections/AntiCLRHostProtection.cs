using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EloBuddy.Loader.Utils;


namespace EloBuddy.Loader.Protections
{
    // ReSharper disable once InconsistentNaming
    internal class AntiCLRHostProtection : Protection
    {
        public override string Name
        {
            get { return "Anti CLR Host Protection"; }
        }

        protected internal override void Initialize()
        {
            //

            new AntiCLRProtectionPhase().Execute();
        }

        internal class AntiCLRProtectionPhase : ProtectionPhase
        {
            protected internal override void Execute()
            {
                var functionList = new List<string>
                {
                    "CLRCreateInstance"
                };

                foreach (var functionName in functionList)
                {
                    var functionAddress = NativeImports.GetProcAddress(NativeImports.GetModuleHandle("mscoree"),
                        functionName);

                    if (!this.ProtectNative(functionAddress))
                    {
                        // Unused.
                    }
                }
            }

            private bool ProtectNative(IntPtr address)
            {
                try
                {
                    uint oldProtect;

                    NativeImports.VirtualProtect(address, 8, 0x40, out oldProtect);

                    unsafe
                    {
                        *(long*) ((void*) address) = -8007118662488031304L;
                    }

                    Marshal.WriteIntPtr(address, 1, new IntPtr(0x7F7F7F7F));

                    NativeImports.VirtualProtect(address, 8, oldProtect, out oldProtect);

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}
