using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class ModelAvailabilityStatus
    {
        public Model Model { get; set; }

        public bool IsModelAvailable { get; set; }
    }
}
