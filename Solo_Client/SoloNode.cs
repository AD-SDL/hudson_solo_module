using Grapevine;
//old imports
using Hudson.SoloSoft.Communications;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace SoloNode
{
    //public class Callback_wrapper <--- what would the eqivalent be for the SOLO?

    class Program
    {
        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        ~Program()
        {
            // execute on close of program
            Console.WriteLine("Exiting...");
            client.Disconnect();  
            server.Stop();  
        }

        [Option(Description = "Server Hostname")]
        public string Hostname { get; set; } = "+";

        [Option(Description = "Server Port")]
        public int Port { get; } = 2003;  

        [Option(Description = "Whether or not to simulate the instrument (note: if the instrument is connected, this does nothing)")]
        public bool Simulate { get; } = true;

        public string state = ModuleStatus.INIT; //why aren't the util functions being pulled in here?
        private static SoloClient client;
        private IRestServer server;
        string programPath = @"C:\Program Files (x86)\Hudson Robotics\SoloSoft\SOLOSoft.exe";


        private void OnExecute()
        {
            InitializeSoloClient();

            server = RestServerBuilder.UseDefaults().Build();
            string server_url = "http://" + Hostname + ":" + Port.ToString() + "/";
            Console.WriteLine(server_url);
            server.Prefixes.Clear();
            server.Prefixes.Add(server_url);
            server.Locals.TryAdd("state", state);
            server.Locals.TryAdd("client", client);
            try
            {
                server.Start();
                Console.WriteLine("Press enter to stop the server");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void InitializeSoloClient()
        {
            SoloClient client = new SoloClient();
            client.Connect(11139);

            // MAYBE THIS SHOULD GO IN ACTION POST (this should happen once per action message)
            // check if SOLOSoft already running
            Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(programPath));

            // If SOLOSoft not already running, open it 
            if (processes.Length == 0)
            {
                Console.Out.WriteLine("SOLOSoft not open");
                Process.Start(programPath);
                Thread.Sleep(6000);  // wait for program to open
                SendKeys.SendWait("{ENTER}");  //press enter to bring up login 
                Thread.Sleep(1000); //wait for login window to appear
                SendKeys.SendWait("{ENTER}");  //press enter to bypass login screen
                Console.Out.WriteLine("SOLOSoft opened");
                Console.Out.WriteLine(client.RunCommand("CLOSEALLFILES"));  // ensure no other SOLO protocols open
                Console.Out.WriteLine("Closed all files");
            }
            // No need to open SOLOSoft if already open 
            else
            {
                Console.Out.WriteLine("SOLOSoft already open");

            }





        }


    }

}


