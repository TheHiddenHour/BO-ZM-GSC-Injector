using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Ionic.Zlib;

namespace Black_Ops_Zombies_GSC_Injector
{
    class Program
    {
        static string ProjectDirectory;
        static List<string> Project;

        static void Main(string[] args)
        {
            while(true)
            {
                Console.Clear();
                //Check parameter count
                if (args.Length < 1)
                {
                    Console.Write("Enter a directory or file to inject (no quotes): ");
                    ProjectDirectory = Console.ReadLine();
                }
                else
                    ProjectDirectory = args[0];

                //Connect and attach ps3
                if (!PS3.ConnectPS3())
                    break;

                //Initiate program
                if (ProjectDirectory == "r") //reset scripts
                {
                    PS3.ResetScripts();

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
                    byte[] hook_buffer = CompileScript(Properties.Resources._cheat);
                    PS3.InjectRawfile(0x00E92738, 0x02000000, hook_buffer);

                    //Compile and inject project contents maps\_dev.gsc
                    byte[] proj_buffer = CompileScript(Encoding.UTF8.GetBytes(projectContents));
                    uint proj_bufferAddr = 0x02000000 + (uint)hook_buffer.Length;
                    PS3.InjectRawfile(0x00E9281C, proj_bufferAddr, proj_buffer);

                    Console.WriteLine("Project injected");
                }
                else if (File.Exists(ProjectDirectory)) //arg is a file
                {
                    //Compile and inject modified maps\_cheat.gsc
                    byte[] hook_buffer = CompileScript(Properties.Resources._cheat);
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
