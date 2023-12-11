using Grapevine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// old imports 
using Hudson.SoloSoft.Communications;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics.Eventing.Reader;
using System.Management.Instrumentation;

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
            //TODO
            await context.Response.SendResponseAsync("TODO result"); 

        }
    }
}


