using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class Model
    {
        public string Id { get; set; }

        public ModelUsage[] Usage { get; set; }

        public ModelContextWindow ContextWindow { get; set; }

        public string Provider { get; set; }

        public string Title { get; set; }

        public ModelTag[] Tags { get; set; }

        public ModelRef ModelRef { get; set; }
    }

    public enum ModelUsage
    {
        [EnumMember(Value = "chat")]
        Chat,
        [EnumMember(Value = "edit")]
        Edit,
        [EnumMember(Value = "autocomplete")]
        Autocomplete,
        [EnumMember(Value = "unlimitedChat")]
        Unlimited
    }

    public enum ModelTag
    {
        // UI Groups
        Power,
        Speed,
        Balanced,
        Other,

        // Statuses
        Recommended,
        Deprecated,
        Experimental,
        Waitlist, // join waitlist
        [EnumMember(Value = "on-waitlist")]
        OnWaitlist,
        [EnumMember(Value = "early-access")]
        EarlyAccess,
        Internal,

        // Tiers - the level of access to the model
        Pro,
        Free,
        Enterprise,

        // Origins - where the model comes from
        Gateway,
        BYOK,
        Local,
        Ollama,
        Dev,

        // Additional Info about the model. e.g. capabilities
        [EnumMember(Value = "stream-disabled")]
        StreamDisabled, // Model does not support streaming
        Vision, // Model supports vision capabilities
        Reasoning, // Model supports reasoning capabilities
        Tools, // Model supports tools capabilities
        Default, // Default Model
        Unlimited, // Models with unlimited usage when rate limited
    }
}
