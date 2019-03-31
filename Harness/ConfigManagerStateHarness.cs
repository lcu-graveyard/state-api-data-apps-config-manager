using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Fathym;
using Fathym.Design.Singleton;
using LCU.Graphs.Registry.Enterprises;
using LCU.Runtime;
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

        const string lcuPathRoot = "_lcu";
        #endregion

        #region Properties
        #endregion

        #region Constructors
        public ConfigManagerStateHarness(HttpRequest req, ILogger log, ConfigManagerState state)
            : base(req, log, state)
        {
            this.container = "Default";
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
            var apps = await appGraph.ListApplications(details.EnterpriseAPIKey);

            state.Applications = apps.Where(app => app.Container == "lcu-data-apps").ToList();

            return state;
        }

        public virtual async Task<ConfigManagerState> LoadAppView()
        {
            if (state.ActiveApp != null)
            {
                var apps = await appGraph.GetDAFApplications(details.EnterpriseAPIKey, state.ActiveApp.ID);

                state.ActiveView = apps?.FirstOrDefault()?.JSONConvert<DAFViewConfiguration>();
            }
            else
                state.ActiveView = null;

            return state;
        }

        public virtual async Task<ConfigManagerState> SaveAppView(DAFViewConfiguration view)
        {
            if (state.ActiveApp != null)
            {
                view.ApplicationID = state.ActiveApp.ID;

                //  TODO:   Probably need to expose this as an advanced setting in the future or something
                if (view.BaseHref.IsNullOrEmpty())
                    view.BaseHref = state.ActiveApp.PathRegex.TrimEnd('*') + '/';

                if (view.ID.IsEmpty() && view.Priority <= 0)
                    view.Priority = 500;

                var status = Status.Success;

                log.LogInformation($"Saving DAF App: {view.ToJSON()}");

                status = await unpackView(view, details.EnterpriseAPIKey);

                if (status)
                {
                    var dafApp = appGraph.SaveDAFApplication(details.EnterpriseAPIKey, view.JSONConvert<DAFApplicationConfiguration>()).Result;
                }
            }

            return await SetActiveApp(state.ActiveApp);
        }

        public virtual async Task<ConfigManagerState> SaveDataApp(Application app)
        {
            app.EnterprisePrimaryAPIKey = details.EnterpriseAPIKey;

            if (app.Hosts.IsNullOrEmpty())
                app.Hosts = new List<string>();

            if (!app.Hosts.Contains(details.Host))
                app.Hosts.Add(details.Host);

            if (app.ID.IsEmpty() && app.Priority <= 0 && !state.Applications.IsNullOrEmpty())
                app.Priority = state.Applications.Max(a => a.Priority) + 500;

            app.Container = "lcu-data-apps";

            if (!app.PathRegex.EndsWith("*"))
                app.PathRegex = $"{app.PathRegex}*";

            app = await appGraph.Save(app);

            return await SetActiveApp(app);
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