using System.Collections.Generic;

namespace BO_ZM_GSC_Injector {
    class Config {
        public int API;
        public _Hook Hook = new _Hook();
        public _Injection Injection = new _Injection();

        public class _Hook {
            public string Path { get; set; }
            public uint Pointer { get; set; }
            public _Default Default = new _Default();
            public _Custom Custom = new _Custom();
            
            public class _Default {
                public uint Buffer { get; set; }
                public uint Length { get; set; }
            }

            public class _Custom {
                public uint Buffer { get; set; }
            }
        }

        public class _Injection {
            public uint Pointer { get; set; }
            public _Default Default = new _Default();

            public class _Default {
                public uint Buffer { get; set; }
                public uint Length { get; set; }
            }
        }

        public new string ToString {
            get {
                List<string> info = new List<string>();
                info.Add("Hook Path: " + Hook.Path);
                info.Add("Hook Pointer: 0x" + Hook.Pointer.ToString("X"));
                info.Add("Hook Buffer D: 0x" + Hook.Default.Buffer.ToString("X") + " C: 0x" + Hook.Custom.Buffer.ToString("X"));
                info.Add("Hook Length D: 0x" + Hook.Default.Length.ToString("X"));
                info.Add("Inj. Pointer: 0x" + Injection.Pointer.ToString("X"));
                info.Add("Inj. Buffer D: 0x" + Injection.Default.Buffer.ToString("X"));
                info.Add("Inj. Length D: 0x" + Injection.Default.Length.ToString("X"));

                string result = "";
                foreach(string entry in info) {
                    result += entry + '\n';
                }

                return result;
            }
        }
    }
}
