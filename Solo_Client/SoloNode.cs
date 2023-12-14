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

        [Option(Description = "Server Hostname")]
        public string Hostname { get; set; } = "+";

        [Option(Description = "Server Port")]
        public int Port { get; } = 2003;  

        [Option(Description = "Whether or not to simulate the instrument (note: if the instrument is connected, this does nothing)")]
        public bool Simulate { get; } = true;

        public string state = ModuleStatus.INIT; 
        
        private static SoloClient client;
        private IRestServer server;

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


