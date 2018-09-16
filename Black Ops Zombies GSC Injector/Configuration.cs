namespace Black_Ops_Zombies_GSC_Injector
{
    class Configuration
    {
        public class Default
        {
            public uint HookPointer, HookBuffer, HookLength;
            public uint ProjectPointer, ProjectBuffer, ProjectLength;
        }

        public class Custom
        {
            public uint HookBuffer;
        }

        public string HookPath;
        public Default DefaultInfo = new Default();
        public Custom CustomInfo = new Custom();

        public void SetDefaultHookInfo(uint Pointer, uint Buffer, uint Length)
        {
            DefaultInfo.HookPointer = Pointer;
            DefaultInfo.HookBuffer = Buffer;
            DefaultInfo.HookLength = Length;
        }

        public void SetDefaultProjectInfo(uint Pointer, uint Buffer, uint Length)
        {
            DefaultInfo.ProjectPointer = Pointer;
            DefaultInfo.ProjectBuffer = Buffer;
            DefaultInfo.ProjectLength = Length;
        }

        public void SetCustomHookInfo(uint Buffer)
        {
            CustomInfo.HookBuffer = Buffer;
        }
    }
}
