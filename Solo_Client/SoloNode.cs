using Grapevine;
using Hudson.SoloSoft.Communications;
using McMaster.Extensions.CommandLineUtils;
using System;


namespace SoloNode
{
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

        // CLI Options
        [Option(Description = "Server Hostname")]
        public string Hostname { get; set; } = "+";

        [Option(Description = "Server Port")]
        public int Port { get; } = 2003;

        // TESTING (Add hardcoded paths as cli parameters)
        [Option(Description = "SOLOSoft Application Path")]
        public string executablePath { get; set; } = @"C:\Program Files (x86)\Hudson Robotics\SoloSoft\SOLOSoft.exe";

        [Option(Description = "SOLOSoft Tip Counts CSV File Path")]
        public string tipsFilePath { get; set; } = @"C:\ProgramData\Hudson Robotics\SoloSoft\SoloSoft\TipCounts.csv";

        [Option(Description = "Temp SOLOSoft Protocols Folder Path", ShortName = "T")]
        public string tempFolderPath { get; set; } = @"C:\labautomation\instructions_wei\";

        // END TESTING  --------------------------------------------------------------------------
 
        [Option(Description = "Whether or not to simulate the instrument (note: if the instrument is connected, this does nothing)")]
        public bool Simulate { get; } = true;

        [Option(Description = "Path to the SOLOSoft executable")]
        public string ExecutablePath { get; } = @"C:\Program Files (x86)\Hudson Robotics\SoloSoft\SOLOSoft.exe";

        [Option(Description = "Path to the Tips File in ProgramData")]
        public string TipsFilePath { get; } = @"C:\ProgramData\Hudson Robotics\SoloSoft\SoloSoft\TipCounts.csv";


        public string state = ModuleStatus.INIT;

        private static SoloClient client;
        private IRestServer server;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by CommandLineApplication.Execute (see above)")]
        private void OnExecute()
        {
            InitializeSoloClient();

            server = RestServerBuilder.UseDefaults().Build();
            string server_url = "http://" + Hostname + ":" + Port.ToString() + "/";
            server.Prefixes.Clear();
            server.Prefixes.Add(server_url);
            server.Locals.TryAdd("state", state);
            server.Locals.TryAdd("client", client);
            server.Locals.TryAdd("executablePath", ExecutablePath);
            server.Locals.TryAdd("tipsFilePath", TipsFilePath);
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
            try
            {
                client = new SoloClient();
                client.Connect(11139);
                state = ModuleStatus.IDLE;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                state = ModuleStatus.ERROR;

            }
        }


    }

}



