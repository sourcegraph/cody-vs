using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Runtime.Serialization;


namespace Cody.Core.Agent.Protocol
{
    [JsonConverter(typeof(ProtocolAuthStatusConverter))]
    public abstract class ProtocolAuthStatus
    {
        public StatusEnum Status { get; set; }

        public bool Authenticated { get; set; }

        public string Endpoint { get; set; }

        public bool PendingValidation { get; set; }

        public enum StatusEnum
        {
            [EnumMember(Value = "authenticated")] Authenticated,
            [EnumMember(Value = "unauthenticated")] Unauthenticated
        }
    }

    public class ProtocolAuthenticatedAuthStatus : ProtocolAuthStatus
    {
        public string Username { get; set; }

        public bool? IsFireworksTracingEnabled { get; set; }

        public bool? HasVerifiedEmail { get; set; }

        public bool? RequiresVerifiedEmail { get; set; }

        public string PrimaryEmail { get; set; }

        public string DisplayName { get; set; }

        public string AvatarURL { get; set; }

        public OrganizationsParams[] Organizations { get; set; }
    }

    public class ProtocolUnauthenticatedAuthStatus : ProtocolAuthStatus
    {
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
        public string Name { get; set; }
        public string Id { get; set; }
    }

    public class AuthError
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public string Stack { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool ShowTryAgain { get; set; }
    }


}
