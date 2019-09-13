
using System.Collections.Generic;
using System.Runtime.Serialization;
using LCU.Graphs.Registry.Enterprises.Apps;

namespace LCU.State.API.DataApps.ConfigManager.Models
{
    [DataContract]
    public class ConfigManagerState
    {
        [DataMember]
        public virtual bool AddingApp { get; set; }

        [DataMember]
        public virtual List<Application> Applications { get; set; }

        [DataMember]
        public virtual Application ActiveApp { get; set; }

        [DataMember]
        public virtual DAFViewConfiguration ActiveView { get; set; }

        [DataMember]
        public virtual bool Loading { get; set; }
    }
}