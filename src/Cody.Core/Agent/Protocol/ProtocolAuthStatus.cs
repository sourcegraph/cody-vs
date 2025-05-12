using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;


namespace Cody.Core.Agent.Protocol
{
    [JsonConverter(typeof(ProtocolAuthStatusConverter))]
    public abstract class ProtocolAuthStatus
    {
        [JsonProperty("status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public StatusEnum Status { get; set; }

        [JsonProperty("authenticated")]
        public bool Authenticated { get; set; }

        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }

        [JsonProperty("pendingValidation")]
        public bool PendingValidation { get; set; }

        public enum StatusEnum
        {
            [EnumMember(Value = "authenticated")] Authenticated,
            [EnumMember(Value = "unauthenticated")] Unauthenticated
        }
    }

    public class ProtocolAuthenticatedAuthStatus : ProtocolAuthStatus
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("isFireworksTracingEnabled")]
        public bool? IsFireworksTracingEnabled { get; set; }

        [JsonProperty("hasVerifiedEmail")]
        public bool? HasVerifiedEmail { get; set; }

        [JsonProperty("requiresVerifiedEmail")]
        public bool? RequiresVerifiedEmail { get; set; }

        [JsonProperty("primaryEmail")]
        public string PrimaryEmail { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("avatarURL")]
        public string AvatarURL { get; set; }

        [JsonProperty("organizations")] public List<OrganizationsParams> Organizations { get; set; }
    }

    public class ProtocolUnauthenticatedAuthStatus : ProtocolAuthStatus
    {
        [JsonProperty("error")]
        public AuthError Error { get; set; }
    }

    public class ProtocolAuthStatusConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ProtocolAuthStatus);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);

            // Get the status discriminator
            var status = jsonObject["status"]?.ToString();

            // Create the appropriate concrete type based on the discriminator
            ProtocolAuthStatus result;
            if (status == "authenticated")
                result = new ProtocolAuthenticatedAuthStatus();
            else if (status == "unauthenticated")
                result = new ProtocolUnauthenticatedAuthStatus();
            else
                throw new Exception($"Unknown discriminator {status}");

            serializer.Populate(jsonObject.CreateReader(), result);

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JObject.FromObject(value, serializer).WriteTo(writer);
        }
    }

    public class OrganizationsParams
    {
        // not used
    }

    public class AuthError
    {
        // not used
    }


}
