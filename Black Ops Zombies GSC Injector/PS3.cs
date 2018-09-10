using System;
using PS3Lib;

namespace Black_Ops_Zombies_GSC_Injector
{
    class PS3
    {
        public static PS3API API = new PS3API(SelectAPI.TargetManager);

        public static bool ConnectPS3()
        {
            if (API.ConnectTarget())
            {
                if (!API.AttachProcess())
                {
                    Console.WriteLine("Could not attach to process");
                    return false;
                }
            }
            else
            {
                Console.WriteLine("Could not connect to target");
                return false;
            }

            return true;
        }

        public static void InjectRawfile(uint pointerAddress, uint bufferAddress, byte[] buffer)
        {
            UpdateRawfileTable(pointerAddress, bufferAddress, (uint)buffer.Length);
            API.SetMemory(bufferAddress, buffer);
        }

        public static void UpdateRawfileTable(uint pointerAddress, uint bufferAddress, uint length)
        {
            API.Extension.WriteUInt32(pointerAddress, bufferAddress); //Set buffer address at pointer address
            API.Extension.WriteUInt32(pointerAddress - (uint)4, length); //Set length at pointer address - 4
        }

        public static void ResetScripts()
        {
            UpdateRawfileTable(0x00E92738, 0x30368E40, 0x0000092E); //Reset maps\_cheat.gsc
            UpdateRawfileTable(0x00E9281C, 0x3037A8C0, 0x00000040); //Reset maps\_dev.gsc
        }
    }
}
