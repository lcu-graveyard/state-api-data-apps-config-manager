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
using LCU.Graphs.Registry.Enterprises.Apps;

namespace LCU.State.API.DataApps.ConfigManager
{
    [Serializable]
    [DataContract]
    public class SaveAppViewRequest
    {
        [DataMember]
        public virtual DAFViewConfiguration View { get; set; }
    }

    public static class SaveAppView
    {
        [FunctionName("SaveAppView")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return await req.Manage<SaveAppViewRequest, ConfigManagerState, ConfigManagerStateHarness>(log, async (mgr, reqData) =>
            {
                return await mgr.SaveAppView(reqData.View);
            });
        }
    }
}
