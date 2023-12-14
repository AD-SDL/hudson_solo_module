using Grapevine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Hudson.SoloSoft.Communications;
using System.Diagnostics;
using System.Windows.Forms;

namespace SoloNode
{
	[RestResource]
    public class SoloRestServer
    {
        private readonly IRestServer _server;

        public SoloRestServer(IRestServer server)
        {
            _server = server;
        }

        [RestRoute("Get", "/state")]
        public async Task State(IHttpContext context)
        {
            string state = _server.Locals.GetAs<string>("state");
            Dictionary<string, string> response = new Dictionary<string, string>
            {
                ["State"] = state
            };
            Console.WriteLine(state);
            await context.Response.SendResponseAsync(JsonConvert.SerializeObject(response));
        }


        [RestRoute("Get", "/about")]
        public async Task About(IHttpContext context)
        {
            //TODO
            await context.Response.SendResponseAsync("about");
        }


        [RestRoute("Get", "/resources")]
        public async Task Resources(IHttpContext context)
        {
            //TODO
            await context.Response.SendResponseAsync("resources");
        }


        [RestRoute("Post", "/action")]
        public async Task Action(IHttpContext context)
        {
            string action_handle = context.Request.QueryString["action_handle"];
            string action_vars = context.Request.QueryString["action_vars"];
            Dictionary<string, string> args = JsonConvert.DeserializeObject<Dictionary<string, string>>(action_vars);
            var result = UtilityFunctions.action_response();
            string state = _server.Locals.GetAs<string>("state");
            SoloClient client = _server.Locals.GetAs<SoloClient>("client");
            string programPath = @"C:\Program Files (x86)\Hudson Robotics\SoloSoft\SOLOSoft.exe";  
            string status_code = "0000"; 
            string s = "";  // instrument state

            if (state == ModuleStatus.BUSY)
            {
                result = UtilityFunctions.action_response(StepStatus.FAILED, "", "Module is Busy");
                await context.Response.SendResponseAsync(JsonConvert.SerializeObject(result));
            }
            
            // if module not busy
            try
            {
                _server.Locals.TryUpdate("state", ModuleStatus.BUSY, _server.Locals.GetAs<string>("state"));
                switch (action_handle)
                {
                    case "run_protocol":
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
                        else
                        {
                            Console.Out.WriteLine("SOLOSoft already open");

                        }
                        //collect protocol details and save to temp file 
                        string[] hso_contents = args["hso_contents"].Split('\n');
                        string hso_basename = args["hso_basename"]; 
                        string temp_file_path = "C:\\labautomation\\instructions_wei\\" + args["hso_basename"];
                        File.WriteAllLines(temp_file_path, hso_contents);

                        // Run commands to execute hso protocol on SOLO
                        status_code = client.RunCommand("LOAD " + temp_file_path);
                        Console.Out.WriteLine(status_code);
                        status_code = client.RunCommand("RUN " + temp_file_path);
                        Console.Out.WriteLine(status_code);

                        try
                        {
                            // monitor status of running protocol, sleep until run complete
                            s = client.RunCommand("GETSTATUS");
                            while (s != "IDLE")
                            {
                                Thread.Sleep(10000);
                                s = client.RunCommand("GETSTATUS");
                            }

                            // close SOLOSoft at end of protocol run
                            processes = Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(programPath));
                            if (processes.Length == 0)
                            {
                                Console.Out.WriteLine("SOLOSoft is already closed");
                            }
                            else
                            {
                                Process soloSoft = processes[0];
                                soloSoft.Kill();
                                Console.Out.WriteLine("Process Killed: " + programPath);
                                Thread.Sleep(3000);
                            }

                            // Send response if protocol complete at SoloSoft closed
                            result = UtilityFunctions.action_response(StepStatus.SUCCEEDED, "Ran SOLO protocol", "");
                            Console.Out.WriteLine("Action Finished: run_protocol"); 

                        }
                        catch (Exception ex)
                        {
                            // SOLOSoft likely crashed
                            Console.Out.WriteLine(ex.ToString());
                            Console.Out.WriteLine("SOLOSoft likely crashed");
                            result = UtilityFunctions.action_response(StepStatus.FAILED, "", "Could not run SOLO protocol");
                        }
                        break; 

                    case "refill_tips":
                        // TODO
                        Console.Out.WriteLine("Refill tips called");
                        break;

                    default:
                        Console.Out.WriteLine("Unknown action: " + action_handle);
                        result = UtilityFunctions.action_response(StepStatus.FAILED, "", "Unknown action: " + action_handle);
                        break;
                }
                _server.Locals.TryUpdate("state", ModuleStatus.IDLE, _server.Locals.GetAs<string>("state"));
            }
            catch (Exception ex)
            {
                _server.Locals.TryUpdate("state", ModuleStatus.IDLE, _server.Locals.GetAs<string>("state"));
                Console.Out.WriteLine(ex.ToString());
                result = UtilityFunctions.action_response(StepStatus.FAILED, "", "Step failed: " + ex.ToString());
            }

            await context.Response.SendResponseAsync(JsonConvert.SerializeObject(result)); 

        }
    }
}


