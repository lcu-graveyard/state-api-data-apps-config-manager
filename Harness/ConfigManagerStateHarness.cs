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
        protected readonly ApplicationGraph appGraph;

        protected readonly string container;

        protected readonly EnterpriseGraph entGraph;

        const string lcuPathRoot = "_lcu";
        #endregion

        #region Properties
        #endregion

        #region Constructors
        public ConfigManagerStateHarness(HttpRequest req, ILogger log, ConfigManagerState state)
            : base(req, log, state)
        {
            appGraph = req.LoadGraph<ApplicationGraph>();

            this.container = "Default";

            entGraph = req.LoadGraph<EnterpriseGraph>();
        }
        #endregion

        #region API Methods
        public virtual async Task<ConfigManagerState> Ensure()
        {
            return state;
        }
        
        public virtual async Task<ConfigManagerState> LoadApplications()
        {
            var apps = await appGraph.ListApplications(details.EnterpriseAPIKey);

            state.Applications = apps.Where(app => app.Container == "lcu-data-apps").ToList();

            return state;
        }
        
        public virtual async Task<ConfigManagerState> SetActiveApp(Application app)
        {
            state.ActiveApp = app;

            return state;
        }

        public virtual async Task<ConfigManagerState> SetVisibilityFlow(string flow)
        {
            state.VisibilityFlow = flow;

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