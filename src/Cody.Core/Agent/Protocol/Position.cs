using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class Position
    {
        public int Line { get; set; }
        public int Character { get; set; }

        public bool IsPosition(int line, int character) => this.Line == line && this.Character == character;
    }
}
