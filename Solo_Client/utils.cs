﻿using System.Collections.Generic;

namespace SoloNode
{
    public static class ModuleStatus
    {
        public const string
            INIT = "INIT",
            IDLE = "IDLE",
            BUSY = "BUSY",
            ERROR = "ERROR",
            UNKNOWN = "UNKNOWN";
    }

    public static class StepStatus
    {
        public const string
            IDLE = "idle",
            RUNNING = "running",
            SUCCEEDED = "succeeded",
            FAILED = "failed";
    }

    public static class UtilityFunctions
    {
        public static Dictionary<string, string> ActionResponse(string action_response = StepStatus.IDLE, string action_msg = "", string action_log = "")
        {
            Dictionary<string, string> response = new Dictionary<string, string>
            {
                ["action_response"] = action_response,
                ["action_msg"] = action_msg,
                ["action_log"] = action_log
            };
            return response;
        }

    }

}