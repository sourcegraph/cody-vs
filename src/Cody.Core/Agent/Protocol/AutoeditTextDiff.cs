using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class AutoeditTextDiff
    {
        public ModifiedLineInfo[] ModifiedLines { get; set; }
        public RemovedLineInfo[] RemovedLines { get; set; }
        public AddedLineInfo[] AddedLines { get; set; }
        public UnchangedLineInfo[] UnchangedLines { get; set; }
    }

    public enum LineInfoType
    {
        Added,
        Removed,
        Modified,
        Unchanged
    }

    public abstract class LineInfo
    {
        public string Id { get; set; }

        public LineInfoType Type { get; set; }
    }

    public class AddedLineInfo : LineInfo
    {
        public AddedLineInfo()
        {
            Type = LineInfoType.Added;
        }

        public string Text { get; set; }

        public int ModifiedLineNumber { get; set; }
    }

    public class RemovedLineInfo : LineInfo
    {
        public RemovedLineInfo()
        {
            Type = LineInfoType.Removed;
        }

        public string Text { get; set; }

        public int OriginalLineNumber { get; set; }
    }

    public class ModifiedLineInfo : LineInfo
    {
        public ModifiedLineInfo()
        {
            Type = LineInfoType.Modified;
        }

        public string OldText { get; set; }

        public string NewText { get; set; }
        public LineChange[] Changes { get; set; }
        public int OriginalLineNumber { get; set; }
        public int ModifiedLineNumber { get; set; }
    }

    public class LineChange
    {
        public string Id { get; set; }
        public LineChangeType Type { get; set; }
        public Range OriginalRange { get; set; }
        public Range ModifiedRange { get; set; }
        public string Text { get; set; }
    }

    public enum LineChangeType
    {
        Insert,
        Delete,
        Unchanged
    }

    public class UnchangedLineInfo : LineInfo
    {
        public UnchangedLineInfo()
        {
            Type = LineInfoType.Unchanged;
        }

        public string Text { get; set; }

        public int OriginalLineNumber { get; set; }

        public int ModifiedLineNumber { get; set; }
    }
}
