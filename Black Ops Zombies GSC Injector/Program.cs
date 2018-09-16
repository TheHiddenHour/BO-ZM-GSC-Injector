using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Ionic.Zlib;
using Newtonsoft.Json;

namespace Black_Ops_Zombies_GSC_Injector
{
    class Program
    {
        static string ProjectDirectory;
        static List<string> Project;
        static Configuration Config = new Configuration();
        static string ConfigPath = "config.json";

        static void Main(string[] args)
        {
            while(true)
            {
                Console.Clear();
                //Check parameter count
                if (args.Length < 1)
                {
                    Console.Write("Enter a program parameter to use such as 'r' to reset scripts in memory, a directory, or filepath: ");
                    ProjectDirectory = Console.ReadLine();
                }
                else
                    ProjectDirectory = args[0];
                ProjectDirectory = ProjectDirectory.Replace("\"", "");

                //Check for configuration in program directory
                if(!File.Exists(ConfigPath)) //config file does not exist, use defaults
                {
                    /*INFO:
                        If no valid config.json is found, the program will generate on that uses maps\_cheat.gsc
                        as the hook script and maps\_dev.gsc as the project script.*/
                    Config.HookPath = @"resources\_cheat.gsc";
                    /*DEFAULT INFO*/
                    Config.SetDefaultHookInfo(0x00E92738, 0x30368E40, 0x0000092E); //Use maps\_cheat.gsc as the hook script
                    Config.SetDefaultProjectInfo(0x00E9281C, 0x3037A8C0, 0x00000040); //Use maps\_dev.gsc as the project script

                    /*CUSTOM INFO*/
                    Config.SetCustomHookInfo(0x02000000);//maps\_cheat.gsc

                    //Write config to file
                    string serializedConfig = JsonConvert.SerializeObject(Config, Formatting.Indented);
                    File.WriteAllText(ConfigPath, serializedConfig);
                }
                else //config file does exist
                {
                    try { Config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(ConfigPath)); }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Config loading error: {0}", ex.Message);
                        break;
                    }
                }

                //Connect and attach ps3
                if (!PS3.ConnectPS3())
                    break;

                //Initiate program
                if (ProjectDirectory == "r") //reset scripts
                {
                    //PS3.ResetScripts();
                    PS3.UpdateRawfileTable(Config.DefaultInfo.HookPointer, Config.DefaultInfo.HookBuffer, Config.DefaultInfo.HookLength);
                    PS3.UpdateRawfileTable(Config.DefaultInfo.ProjectPointer, Config.DefaultInfo.ProjectBuffer, Config.DefaultInfo.ProjectLength);

                    Console.WriteLine("Scripts reset");
                }
                else if (Directory.Exists(ProjectDirectory)) //arg is a directory
                {
                    //Check to see if main.gsc exists in root of project dir, organize project files list, check for syntax errors, etc...
                    if (!ValidateProject(ProjectDirectory))
                        break;

                    //Print all project files
                    Console.WriteLine("Project files:");
                    Console.ForegroundColor = ConsoleColor.Green;
                    foreach(string file in Project)
                    {
                        string fileName = file.Replace(ProjectDirectory, "");
                        Console.WriteLine(fileName);
                    }
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine();

                    //Combine all project files
                    string projectContents = "";
                    foreach (string file in Project)
                        projectContents += File.ReadAllText(file) + '\n';

                    //Compile and inject modified maps\_cheat.gsc
                    byte[] hook_buffer = CompileScript(File.ReadAllBytes(Config.HookPath));
                    PS3.InjectRawfile(Config.DefaultInfo.HookPointer, Config.CustomInfo.HookBuffer, hook_buffer);

                    //Compile and inject project contents maps\_dev.gsc
                    byte[] proj_buffer = CompileScript(Encoding.UTF8.GetBytes(projectContents));
                    uint proj_bufferAddr = Config.CustomInfo.HookBuffer + (uint)hook_buffer.Length;
                    PS3.InjectRawfile(Config.DefaultInfo.ProjectPointer, Config.DefaultInfo.ProjectBuffer, proj_buffer);

                    Console.WriteLine("Project injected");
                }
                else if (File.Exists(ProjectDirectory)) //arg is a file
                {
                    //Compile and inject modified maps\_cheat.gsc
                    byte[] hook_buffer = CompileScript(File.ReadAllBytes(Config.HookPath));
                    PS3.InjectRawfile(0x00E92738, 0x02000000, hook_buffer);

                    //Compile and inject project contents maps\_dev.gsc
                    byte[] proj_buffer = CompileScript(File.ReadAllBytes(ProjectDirectory));
                    uint proj_bufferAddr = 0x02000000 + (uint)hook_buffer.Length;
                    PS3.InjectRawfile(0x00E9281C, proj_bufferAddr, proj_buffer);

                    Console.WriteLine("File injected");
                }
                else
                {
                    Console.WriteLine("No folder or file was found");
                    Console.ReadLine();
                    continue;
                }

                break;
            }

            Console.Write("\nPress enter to continue...");
            Console.ReadLine();
        }

        static bool ValidateProject(string projectDirectory)
        {
            //Validate that main.gsc exists at root of project dir
            if (!File.Exists(projectDirectory + @"\main.gsc"))
            {
                Console.WriteLine("No main.gsc found in root of project directory");
                return false;
            }

            //Get all files in project dir
            Project = new List<string>(Directory.GetFiles(projectDirectory, "*.*", SearchOption.AllDirectories));

            //Remove non GSC files
            for(int i=0; i<Project.Count; i++)
            {
                if (!Project[i].EndsWith(".gsc"))
                    Project.RemoveAt(i);
            }

            //Add main.gsc to the top of the project
            for(int i=0; i<Project.Count; i++)
            {
                if(Project[i] == projectDirectory + @"\main.gsc")
                {
                    string removedItem = Project[i];
                    Project.RemoveAt(i);
                    Project.Insert(0, removedItem);
                }
            }

            //Check syntax of all files in project
            for(int i=0; i<Project.Count; i++)
            {
                string errorLine = GSCGrammar.CheckSyntax(File.ReadAllText(Project[i]));
                if(errorLine != "")
                {
                    string error = "Bad syntax at line " + errorLine + " in " + Project[i].Replace(projectDirectory, "");
                    Console.WriteLine(error);

                    return false;
                }
            }

            return true;
        }

        static byte[] CompileScript(byte[] script)
        {
            //Store script bytes and null byte into stream
            MemoryStream stream = new MemoryStream();
            stream.Write(script, 0, script.Length);
            stream.WriteByte(0x00);
            //Create compiled script buffer and lengths for header
            byte[] buffer = ZlibStream.CompressBuffer(stream.ToArray());
            byte[] length = BitConverter.GetBytes(script.Length + 1).Reverse().ToArray();
            byte[] comp_length = BitConverter.GetBytes(buffer.Length).Reverse().ToArray();
            //Write compiled script and header to stream
            stream.SetLength(0); //Reset stream
            stream.Write(length, 0, length.Length); //Write first part of header
            stream.Write(comp_length, 0, comp_length.Length); //Write second part of header
            stream.Write(buffer, 0, buffer.Length); //Write compiled script bytes

            return stream.ToArray();
        }
    }
}
