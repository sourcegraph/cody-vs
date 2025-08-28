using JsonSubTypes;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;


namespace Cody.Core.Agent.Protocol
{
    [JsonConverter(typeof(JsonSubtypes), "Status")]
    [JsonSubtypes.KnownSubType(typeof(ProtocolAuthenticatedAuthStatus), "authenticated")]
    [JsonSubtypes.KnownSubType(typeof(ProtocolUnauthenticatedAuthStatus), "unauthenticated")]
    public abstract class ProtocolAuthStatus
    {
        public string Status { get; set; }

        public bool Authenticated { get; set; }

        public string Endpoint { get; set; }

        public bool PendingValidation { get; set; }

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
