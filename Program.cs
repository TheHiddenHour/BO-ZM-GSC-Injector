using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using Ionic.Zlib;
using PS3Lib;

namespace BO_ZM_GSC_Injector {
    class Program {
        private static string _project_dir;
        private static Config _config;
        private static List<string> _project;
        private static PS3API _PS3;

        static void Main(string[] args) {
            /*
             * Check and create default files
             */
            // Create _cheat.gsc in working directory if it doesnt exist 
            if(!File.Exists("_cheat.gsc")) {
                byte[] cheat_buffer = Properties.Resources._cheat;
                File.WriteAllBytes("_cheat.gsc", cheat_buffer);
            }
            // Create config.json in working directory if it doesnt exist 
            if(!File.Exists("config.json")) {
                Config config = new Config();
                config.API = 0; // 0 = TMAPI, 1 = CCAPI 
                config.Hook.Path = "_cheat.gsc";
                config.Hook.Pointer = 0x00E92738; // _cheat.gsc pointer address in rawfile table 
                config.Hook.Default.Buffer = 0x30368E40; // Default _cheat.gsc buffer address stored at the pointer 
                config.Hook.Default.Length = 0x0000092E; // Default _cheat.gsc length, not the modified one 
                config.Hook.Custom.Buffer = 0x02000000; // Modified _cheat.gsc buffer address to be set at the pointer 
                config.Injection.Pointer = 0x00E9281C; // _dev.gsc pointer address in rawfile table 
                config.Injection.Default.Buffer = 0x3037A8C0; // Default _dev.gsc buffer address stored at the pointer 
                config.Injection.Default.Length = 0x00000040; // Default _dev.gsc length, not the modified one 
                /*NOTE: There is no custom injection buffer address because we just grab the one for the hook and add the length of the hook script to it*/
                // Write config.json to disk 
                WriteConfig(config);
            }
            // Load config and set it for the current session 
            _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
            /*
             * Parameter parsing
             */
            if(args.Length > 0) {
                switch(args[0]) {
                    default: // Inject project dir 
                        _project_dir = args[0];
                        // Check if project dir exists
                        if(!Directory.Exists(_project_dir)) {
                            Console.WriteLine("ERROR: Directory doesn't exist");
                            return;
                        }
                        // Check if 'main.gsc' exists in dir 
                        if(!File.Exists(_project_dir + @"\main.gsc")) {
                            Console.WriteLine("ERROR: 'main.gsc' does not exist in root of project directory");
                            return;
                        }
                        break;
                    case "change-api":
                    case "api":
                        _config.API = (_config.API == 0) ? 1 : 0;
                        WriteConfig(_config);
                        Console.WriteLine("Changed active API to " + ((_config.API == 0) ? "TMAPI" : "CCAPI"));
                        return;
                    case "reset":
                    case "r":
                        UpdateRawfileTable(_config.Hook.Pointer, _config.Hook.Default.Buffer, _config.Hook.Default.Length);
                        UpdateRawfileTable(_config.Injection.Pointer, _config.Injection.Default.Buffer, _config.Injection.Default.Length);
                        break;
                }
            }
            else {
                Console.WriteLine("ERROR: No project directory defined");
                return;
            }
            /*
             * Connect and attach PS3
             */
            _PS3 = new PS3API(SelectAPI.TargetManager);
            if(!_PS3.ConnectTarget()) {
                Console.WriteLine("ERROR: Could not connect to target");
                return;
            }

            if(!_PS3.AttachProcess()) {
                Console.WriteLine("ERROR: Could not attach to process");
                return;
            }
            /*
             * Project creation
             */
            _project = Directory.GetFiles(_project_dir, "*.gsc", SearchOption.AllDirectories).ToList();
            // Iterate through each file in project 
            for(int i = 0; i < _project.Count; i++) {
                /*Syntax check*/
                // File is not empty 
                string data = File.ReadAllText(_project[i]);
                if(!string.IsNullOrWhiteSpace(data)) {
                    // Check if any errors were returned 
                    string err = GSCGrammar.CheckSyntax(File.ReadAllText(_project[i]));
                    if(!string.IsNullOrWhiteSpace(err)) {
                        Console.WriteLine("ERROR: Syntax on line " + err + " in " + _project[i]);
                        return;
                    }
                }
                // Move 'main.gsc' to top of project list 
                if(_project[i] == _project_dir + @"\main.gsc") {
                    string pop = _project[i];
                    _project.RemoveAt(i);
                    _project.Insert(0, pop);
                }
            }
            foreach(string element in _project) { Console.WriteLine(element.Replace(_project_dir, "")); }
            /*
             * Plaintext buffer creation
             */
            string ptbuffer = string.Join("\n", _project.Select(x => File.ReadAllText(x)));
            /*
             * Compression and injection
             */
            byte[] hook_buffer = CompileScript(File.ReadAllBytes(_config.Hook.Path));
            byte[] inj_buffer = CompileScript(Encoding.ASCII.GetBytes(ptbuffer));
            // Inject files 
            InjectScript(_config.Hook.Pointer, _config.Hook.Custom.Buffer, hook_buffer);
            InjectScript(_config.Injection.Pointer, _config.Hook.Custom.Buffer + (uint)inj_buffer.Length, inj_buffer);
            /*
             * Done
             */
            Console.WriteLine("Successfully injected scripts");
        }

        static void WriteConfig(Config config) {
            string serialized_config = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText("config.json", serialized_config);
        }

        static void UpdateRawfileTable(uint ptrAddr, uint buffAddr, uint buffLen) {
            // Write new buffer address to pointer address 
            _PS3.Extension.WriteUInt32(ptrAddr, buffAddr);
            _PS3.Extension.WriteUInt32(ptrAddr - (uint)4, buffLen);
        }

        static void InjectScript(uint ptrAddr, uint buffAddr, byte[] buffer) {
            // Update rawfile table 
            UpdateRawfileTable(ptrAddr, buffAddr, (uint)buffer.Length);
            // Write buffer to memory 
            _PS3.SetMemory(buffAddr, buffer);
        }

        static byte[] CompileScript(byte[] script) {
            // Convert script string to byte array and append null byte to end 
            MemoryStream stream = new MemoryStream();
            stream.Write(script, 0, script.Length);
            stream.WriteByte(0x00);
            // Create header data 
            byte[] buffer = ZlibStream.CompressBuffer(stream.ToArray());
            byte[] len = BitConverter.GetBytes(script.Length + 1).Reverse().ToArray();
            byte[] clen = BitConverter.GetBytes(buffer.Length).Reverse().ToArray();
            // Write header and script to stream 
            stream.SetLength(0); // Reset stream 
            stream.Write(len, 0, len.Length);
            stream.Write(clen, 0, clen.Length);
            stream.Write(buffer, 0, buffer.Length);

            return stream.ToArray();
        }
    }
}