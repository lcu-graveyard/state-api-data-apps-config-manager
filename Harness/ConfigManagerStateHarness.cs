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

                state.ActiveView = apps?.Model?.FirstOrDefault()?.JSONConvert<DAFViewConfiguration>();
            }
            else
                state.ActiveView = null;

            return state;
        }

        public virtual async Task<ConfigManagerState> SaveAppView(DAFViewConfiguration view)
        {
            if (state.ActiveApp != null)
            {
                var dafAppResp = await appDev.SaveDAFView(view, state.ActiveApp.ID, details.EnterpriseAPIKey);

                state.ActiveView = dafAppResp.Model;
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