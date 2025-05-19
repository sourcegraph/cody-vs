using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class AutoeditImageDiff
    {
        public string Dark { get; set; }
        public string Light { get; set; }
        public int PixelRatio { get; set; }
        public AutoeditImageDiffPosition Position { get; set; }
    }

    public class AutoeditImageDiffPosition
    {
        public int Line { get; set; }
        public int Column { get; set; }
    }
}
