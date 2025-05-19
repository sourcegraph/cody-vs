using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class AutocompleteEditItem
    {
        public string Id { get; set; }
        public string InsertText { get; set; }
        public Range Range { get; set; }
        public string OriginalText { get; set; }
    }

    public class AutocompleteEditItemRender
    {
        public AutocompleteEditItemRenderInline Inline { get; set; }
        public AutocompleteEditItemRenderAside Aside { get; set; }
    }

    public class AutocompleteEditItemRenderInline
    {
        public AutoeditChanges[] Changes { get; set; }
    }

    public class AutocompleteEditItemRenderAside
    {
        public AutoeditImageDiff Image { get; set; }
        public AutoeditTextDiff Diff { get; set; }
    }
}
