using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using LCU.State.API.DataApps.ConfigManager.Models;
using LCU.State.API.DataApps.ConfigManager.Harness;

namespace LCU.State.API.DataApps.ConfigManager
{
    [Serializable]
    [DataContract]
    public class SetVisibilityFlowRequest
    {
        [DataMember]
        public virtual string Flow { get; set; }
    }

    public static class SetVisibilityFlow
    {
        [FunctionName("SetVisibilityFlow")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return await req.Manage<SetVisibilityFlowRequest, ConfigManagerState, ConfigManagerStateHarness>(log, async (mgr, reqData) =>
            {
                return await mgr.SetVisibilityFlow(reqData.Flow);
            });
        }
    }
}
