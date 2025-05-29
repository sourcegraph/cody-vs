using Microsoft.VisualStudio.Text.Differencing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Completions.Completions
{
    public class VsDifference
    {
        private static StringDifferenceOptions options = new StringDifferenceOptions(StringDifferenceTypes.Word, 2, false);

        public static List<Difference> FindDifferences(string oldText, string newText, ITextDifferencingService textDifferencingService)
        {
            var results = new List<Difference>();
            var diffs = textDifferencingService.DiffStrings(oldText, newText, options);
            foreach (var diff in diffs.Differences)
            {
                var spanOld = diffs.LeftDecomposition.GetSpanInOriginal(diff.Left);
                var spanNew = diffs.RightDecomposition.GetSpanInOriginal(diff.Right);
                var removedText = oldText.Substring(spanOld.Start, spanOld.Length);
                var addedText = newText.Substring(spanNew.Start, spanNew.Length);
                var result = new Difference(removedText, addedText, spanOld.Start);
                results.Add(result);
            }

            return results;
        }
    }
}
