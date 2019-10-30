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
using Fathym.Design.Factory;

namespace LCU.State.API.DataApps.ConfigManager
{
    [Serializable]
    [DataContract]
    public class ToggleAddingAppRequest
    {
    }

    public static class ToggleAddingApp
    {
        [FunctionName("ToggleAddingApp")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return await req.Manage<ToggleAddingAppRequest, ConfigManagerState, ConfigManagerStateHarness>(log, async (mgr, reqData) =>
            {
                log.LogInformation($"Toggling Adding App");

                return await mgr.ToggleAddNew(AddNewTypes.App);
            });
        }
    }
}
