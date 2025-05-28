using System;
using System.Collections.Generic;
using System.Linq;

public class StringDifference2
{
    public static List<Difference> FindDifferences(string oldText, string newText)
    {
        if (oldText == null) oldText = string.Empty;
        if (newText == null) newText = string.Empty;

        var differences = new List<Difference>();
        var lcs = GetLongestCommonSubsequence(oldText, newText);

        int oldIndex = 0;
        int newIndex = 0;

        foreach (var match in lcs)
        {
            // Handle deletions before the match
            if (oldIndex < match.OldIndex)
            {
                string removedText = oldText.Substring(oldIndex, match.OldIndex - oldIndex);

                // Check if there are also insertions at the same position
                if (newIndex < match.NewIndex)
                {
                    string addedText = newText.Substring(newIndex, match.NewIndex - newIndex);
                    differences.Add(new Difference(removedText, addedText, oldIndex));
                }
                else
                {
                    differences.Add(new Difference(removedText, string.Empty, oldIndex));
                }
            }
            // Handle insertions before the match
            else if (newIndex < match.NewIndex)
            {
                string addedText = newText.Substring(newIndex, match.NewIndex - newIndex);
                differences.Add(new Difference(string.Empty, addedText, oldIndex));
            }

            // Move past the common part
            oldIndex = match.OldIndex + match.Length;
            newIndex = match.NewIndex + match.Length;
        }

        // Handle remaining text at the end
        if (oldIndex < oldText.Length || newIndex < newText.Length)
        {
            string remainingOld = oldIndex < oldText.Length ? oldText.Substring(oldIndex) : string.Empty;
            string remainingNew = newIndex < newText.Length ? newText.Substring(newIndex) : string.Empty;

            if (!string.IsNullOrEmpty(remainingOld) || !string.IsNullOrEmpty(remainingNew))
            {
                differences.Add(new Difference(remainingOld, remainingNew, oldIndex));
            }
        }

        return differences;
    }

    private static List<CommonSubstring> GetLongestCommonSubsequence(string oldText, string newText)
    {
        var matches = new List<CommonSubstring>();

        // Find all common substrings
        var commonSubstrings = FindCommonSubstrings(oldText, newText);

        // Select non-overlapping substrings that maximize coverage
        commonSubstrings = commonSubstrings.OrderByDescending(x => x.Length).ToList();

        foreach (var substring in commonSubstrings)
        {
            bool overlaps = false;
            foreach (var existing in matches)
            {
                if (IsOverlapping(substring, existing))
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                matches.Add(substring);
            }
        }

        return matches.OrderBy(x => x.OldIndex).ToList();
    }

    private static List<CommonSubstring> FindCommonSubstrings(string oldText, string newText)
    {
        var substrings = new List<CommonSubstring>();

        for (int i = 0; i < oldText.Length; i++)
        {
            for (int j = 0; j < newText.Length; j++)
            {
                if (oldText[i] == newText[j])
                {
                    int length = 1;
                    while (i + length < oldText.Length &&
                           j + length < newText.Length &&
                           oldText[i + length] == newText[j + length])
                    {
                        length++;
                    }

                    if (length > 0)
                    {
                        substrings.Add(new CommonSubstring(i, j, length));
                    }
                }
            }
        }

        return substrings;
    }

    private static bool IsOverlapping(CommonSubstring a, CommonSubstring b)
    {
        bool oldOverlap = (a.OldIndex < b.OldIndex + b.Length) && (b.OldIndex < a.OldIndex + a.Length);
        bool newOverlap = (a.NewIndex < b.NewIndex + b.Length) && (b.NewIndex < a.NewIndex + a.Length);
        return oldOverlap || newOverlap;
    }

    private class CommonSubstring
    {
        public int OldIndex { get; }
        public int NewIndex { get; }
        public int Length { get; }

        public CommonSubstring(int oldIndex, int newIndex, int length)
        {
            OldIndex = oldIndex;
            NewIndex = newIndex;
            Length = length;
        }
    }
}
