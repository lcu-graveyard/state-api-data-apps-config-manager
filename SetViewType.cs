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
    public class SetViewTypeRequest
    {
        [DataMember]
        public virtual DAFAppTypes AppType { get; set; }
    }

    public static class SetViewType
    {
        [FunctionName("SetViewType")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return await req.Manage<SetViewTypeRequest, ConfigManagerState, ConfigManagerStateHarness>(log, async (mgr, reqData) =>
            {
                log.LogInformation($"Setting View Type: {reqData.AppType}");

                return await mgr.SetViewType(reqData.AppType);
            });
        }
    }
}
