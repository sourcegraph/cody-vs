using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class AuthStatus
    {
        public string Endpoint { get; set; }
        public bool IsDotCom { get; set; }
        public bool IsLoggedIn { get; set; }
        public bool ShowInvalidAccessTokenError { get; set; }
        public bool Authenticated { get; set; }
        public bool HasVerifiedEmail { get; set; }
        public bool RequiresVerifiedEmail { get; set; }
        public bool SiteHasCodyEnabled { get; set; }
        public string SiteVersion { get; set; }
        public bool UserCanUpgrade { get; set; }
        public string Username { get; set; }
        public string PrimaryEmail { get; set; }
        public string DisplayName { get; set; }
        public string AvatarURL { get; set; }
        public int CodyApiVersion { get; set; }
        public ConfigOverwrites ConfigOverwrites { get; set; }
    }
}
