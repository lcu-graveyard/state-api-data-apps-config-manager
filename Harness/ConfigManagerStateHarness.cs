using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Fathym;
using Fathym.Design.Singleton;
using LCU.Graphs.Registry.Enterprises.Apps;
using LCU.Graphs.Registry.Enterprises.IDE;
using LCU.Personas.Client.Applications;
using LCU.StateAPI;
using LCU.State.API.DataApps.ConfigManager.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace LCU.State.API.DataApps.ConfigManager.Harness
{
    public class ConfigManagerStateHarness : LCUStateHarness<ConfigManagerState>
    {
        #region Fields
        protected readonly string container;

        protected readonly ApplicationManagerClient appMgr;

        const string lcuPathRoot = "_lcu";
        #endregion

        #region Properties
        #endregion

        #region Constructors
        public ConfigManagerStateHarness(HttpRequest req, ILogger log, ConfigManagerState state)
            : base(req, log, state)
        {
            this.container = "Default";

            this.appMgr = req.ResolveClient<ApplicationManagerClient>(log);
        }
        #endregion

        #region API Methods
        public virtual async Task<ConfigManagerState> Ensure()
        {
            state.Applications = new List<Application>();

            if (!state.AppType.HasValue)
                state.AppType = DAFAppTypes.View;

            if (state.ActiveApp != null)
                await SetActiveApp(state.ActiveApp);

            return state;
        }

        public virtual async Task<ConfigManagerState> LoadApplications()
        {
            var apps = await appMgr.ListApplications(details.EnterpriseAPIKey);

            state.Applications = apps.Model.Where(app => app.Container == "lcu-data-apps").ToList();

            state.ActiveApp = state.Applications.FirstOrDefault(app => app.ID == state.ActiveApp?.ID);

            return state;
        }

        public virtual async Task<ConfigManagerState> LoadAppView()
        {
            if (state.ActiveApp != null)
            {
                var apps = await appMgr.ListDAFApplications(details.EnterpriseAPIKey, state.ActiveApp.ID);

                state.ActiveDAFApp = apps?.Model?.FirstOrDefault()?.JSONConvert<DAFApplicationConfiguration>();

                if (state.ActiveDAFApp != null)
                {
                    if (state.ActiveDAFApp.Metadata.ContainsKey("APIRoot"))
                        await SetViewType(DAFAppTypes.API);
                    else if (state.ActiveDAFApp.Metadata.ContainsKey("Redirect"))
                        await SetViewType(DAFAppTypes.Redirect);
                    else if (state.ActiveDAFApp.Metadata.ContainsKey("BaseHref"))
                        await SetViewType(DAFAppTypes.View);
                }
            }
            else
                state.ActiveDAFApp = null;

            return state;
        }

        public virtual async Task<ConfigManagerState> SaveDAFApp(DAFApplicationConfiguration dafApp)
        {
            if (state.ActiveApp != null)
            {
                if (state.AppType != DAFAppTypes.API)
                {
                    if (dafApp.Metadata.ContainsKey("APIRoot"))
                        dafApp.Metadata.Remove("APIRoot");

                    if (dafApp.Metadata.ContainsKey("InboundPath"))
                        dafApp.Metadata.Remove("InboundPath");

                    if (dafApp.Metadata.ContainsKey("Methods"))
                        dafApp.Metadata.Remove("Methods");

                    if (dafApp.Metadata.ContainsKey("Security"))
                        dafApp.Metadata.Remove("Security");
                }

                if (state.AppType != DAFAppTypes.Redirect)
                {
                    if (dafApp.Metadata.ContainsKey("Redirect"))
                        dafApp.Metadata.Remove("Redirect");
                }

                if (state.AppType != DAFAppTypes.View)
                {
                    if (dafApp.Metadata.ContainsKey("BaseHref"))
                        dafApp.Metadata.Remove("BaseHref");

                    if (dafApp.Metadata.ContainsKey("NPMPackage"))
                        dafApp.Metadata.Remove("NPMPackage");

                    if (dafApp.Metadata.ContainsKey("PackageVersion"))
                        dafApp.Metadata.Remove("PackageVersion");
                }

                var dafAppResp = await appDev.SaveDAFApps(new[] { dafApp }.ToList(), state.ActiveApp.ID, details.EnterpriseAPIKey);

                state.ActiveDAFApp = dafAppResp.Model?.FirstOrDefault();
            }

            return await SetActiveApp(state.ActiveApp);
        }

        public virtual async Task<ConfigManagerState> SaveDataApp(Application app)
        {
            var appResp = await appDev.SaveApp(app, details.Host, "lcu-data-apps", details.EnterpriseAPIKey);

            return await SetActiveApp(appResp.Model);
        }

        public virtual async Task<ConfigManagerState> SetActiveApp(Application app)
        {
            await ToggleAddNew(AddNewTypes.None);

            state.ActiveApp = app;

            return await LoadAppView();
        }

        public virtual async Task<ConfigManagerState> SetViewType(DAFAppTypes appType)
        {
            state.AppType = appType;

            return state;
        }

        public virtual async Task<ConfigManagerState> ToggleAddNew(AddNewTypes type)
        {
            state.ActiveApp = null;

            switch (type)
            {
                case AddNewTypes.App:
                    state.AddingApp = !state.AddingApp;
                    break;

                case AddNewTypes.None:
                    state.AddingApp = false;
                    break;
            }

            return state;
        }
        #endregion

        #region Helpers
        #endregion
    }

    public enum AddNewTypes
    {
        None,
        App,
    }
}