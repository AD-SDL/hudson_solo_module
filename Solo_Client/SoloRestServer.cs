using Grapevine;
using Hudson.SoloSoft.Communications;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
            // Parse action inputs
            string action_handle = context.Request.QueryString["action_handle"];
            Dictionary<string, string> args = JsonConvert.DeserializeObject<Dictionary<string, string>>(context.Request.QueryString["action_vars"]);
            Dictionary<string, string> action_response;

            // Inject Dependencies
            string state = _server.Locals.GetAs<string>("state");
            SoloClient client = _server.Locals.GetAs<SoloClient>("client");
            string executablePath = _server.Locals.GetAs<string>("executablePath");
            string tipsFilePath = _server.Locals.GetAs<string>("tipsFilePath");
            string instrument_status;

            if (state == ModuleStatus.BUSY)
            {
                action_response = UtilityFunctions.ActionResponse(StepStatus.FAILED, "", "Module is Busy");
                await context.Response.SendResponseAsync(JsonConvert.SerializeObject(action_response));
            }

            // If Module isn't busy, try to run the action
            try
            {
                _server.Locals.TryUpdate("state", ModuleStatus.BUSY, _server.Locals.GetAs<string>("state"));
                switch (action_handle)
                {
                    case "run_protocol":
                        // check if SOLOSoft already running
                        Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(executablePath));

                        // If SOLOSoft not already running, open it 
                        if (processes.Length == 0)
                        {
                            Console.Out.WriteLine("SOLOSoft not open");
                            Process.Start(executablePath);
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
                        instrument_status = client.RunCommand("LOAD " + temp_file_path);
                        Console.Out.WriteLine(instrument_status);
                        instrument_status = client.RunCommand("RUN " + temp_file_path);
                        Console.Out.WriteLine(instrument_status);

                        try
                        {
                            // monitor status of running protocol, sleep until run complete
                            instrument_status = client.RunCommand("GETSTATUS");
                            while (instrument_status != "IDLE")
                            {
                                Thread.Sleep(10000);
                                instrument_status = client.RunCommand("GETSTATUS");
                            }

                            // close SOLOSoft at end of protocol run
                            processes = Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(executablePath));
                            if (processes.Length == 0)
                            {
                                Console.Out.WriteLine("SOLOSoft is already closed");
                            }
                            else
                            {
                                Process soloSoft = processes[0];
                                soloSoft.Kill();
                                Console.Out.WriteLine("Process Killed: " + executablePath);
                                Thread.Sleep(3000);
                            }

                            // Send response if protocol complete at SoloSoft closed
                            action_response = UtilityFunctions.ActionResponse(StepStatus.SUCCEEDED, "Ran SOLO protocol", "");
                            Console.Out.WriteLine("Action Finished: run_protocol");
                        }
                        catch (Exception ex)
                        {
                            // SOLOSoft likely crashed
                            Console.Out.WriteLine(ex.ToString());
                            Console.Out.WriteLine("SOLOSoft likely crashed");
                            _server.Locals.TryUpdate("state", ModuleStatus.ERROR, _server.Locals.GetAs<string>("state"));
                            action_response = UtilityFunctions.ActionResponse(StepStatus.FAILED, "", "Could not run SOLO protocol");
                        }
                        break;

                    case "refill_tips":
                        try
                        {
                            int tipsPosition = Int32.Parse(args["position"]);
                            string[] lines = File.ReadAllLines(tipsFilePath);  // read the contents of the CSV file
                            if (tipsPosition > 0 && tipsPosition <= lines.Length)  // check if the target row exists
                            {
                                string[] columns = lines[tipsPosition - 1].Split(',');  // split csv string into columns(locate 3rd column)
                                columns[2] = columns[2].Replace("0", "1"); // column 2 (3rd column) is edited to replace tips
                                string updatedLine = string.Join(",", columns);  // join the columns back into a line
                                lines[tipsPosition - 1] = updatedLine;  // update the line in the CSV file
                                File.WriteAllLines(tipsFilePath, lines);  // write the updated contents back to the CSV file
                                action_response = UtilityFunctions.ActionResponse(StepStatus.SUCCEEDED, "Ran SOLO refill tips", "");
                            }
                            else
                            {
                                Console.WriteLine("Invalid tip refill position.");
                                _server.Locals.TryUpdate("state", ModuleStatus.ERROR, _server.Locals.GetAs<string>("state"));
                                action_response = UtilityFunctions.ActionResponse(StepStatus.FAILED, "", "Invalid tip refill position");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.Out.WriteLine(ex.ToString());
                            _server.Locals.TryUpdate("state", ModuleStatus.ERROR, _server.Locals.GetAs<string>("state"));
                            action_response = UtilityFunctions.ActionResponse(StepStatus.FAILED, "", "Could not refill SOLO tips");
                        }
                        break;

                    default:
                        Console.Out.WriteLine("Unknown action: " + action_handle);
                        action_response = UtilityFunctions.ActionResponse(StepStatus.FAILED, "", "Unknown action: " + action_handle);
                        break;
                }
                _server.Locals.TryUpdate("state", ModuleStatus.IDLE, _server.Locals.GetAs<string>("state"));
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.ToString());
                _server.Locals.TryUpdate("state", ModuleStatus.ERROR, _server.Locals.GetAs<string>("state"));
                action_response = UtilityFunctions.ActionResponse(StepStatus.FAILED, "", "Step failed: " + ex.ToString());
            }

            await context.Response.SendResponseAsync(JsonConvert.SerializeObject(action_response));

        }
    }
}


