
using System.Collections.Generic;
using System.Runtime.Serialization;
using LCU.Graphs.Registry.Enterprises.Apps;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LCU.State.API.DataApps.ConfigManager.Models
{
    [DataContract]
    public class ConfigManagerState
    {
        [DataMember]
        public virtual Application ActiveApp { get; set; }

        [DataMember]
        public virtual DAFApplicationConfiguration ActiveDAFApp { get; set; }

        [DataMember]
        public virtual bool AddingApp { get; set; }

        [DataMember]
        public virtual List<Application> Applications { get; set; }

        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        public virtual DAFAppTypes? AppType { get; set; }

        [DataMember]
        public virtual bool Loading { get; set; }
    }

    [DataContract]
    public enum DAFAppTypes
    {
        [EnumMember]
        View,
        
        [EnumMember]
        API,
        
        [EnumMember]
        Redirect
    }
}