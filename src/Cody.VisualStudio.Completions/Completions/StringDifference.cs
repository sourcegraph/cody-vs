using System;
using System.Collections.Generic;
using System.Text;

public class StringDifference
{
    public static List<Difference> FindDifferences(string str1, string str2)
    {
        if (str1 == null) throw new ArgumentNullException(nameof(str1));
        if (str2 == null) throw new ArgumentNullException(nameof(str2));

        List<Difference> differences = new List<Difference>();

        // Use the Longest Common Subsequence (LCS) approach
        int[,] lcsMatrix = ComputeLCSMatrix(str1, str2);

        // Backtrack to find differences
        int i = str1.Length;
        int j = str2.Length;

        StringBuilder str1Diff = new StringBuilder();
        StringBuilder str2Diff = new StringBuilder();

        while (i > 0 || j > 0)
        {
            if (i > 0 && j > 0 && str1[i - 1] == str2[j - 1])
            {
                // Characters match, move diagonally
                i--;
                j--;

                // If we have collected differences, add them
                if (str1Diff.Length > 0 || str2Diff.Length > 0)
                {
                    differences.Add(new Difference(
                        str1Diff.ToString(),
                        str2Diff.ToString(),
                        i + 1
                    ));

                    str1Diff.Clear();
                    str2Diff.Clear();
                }
            }
            else if (j > 0 && (i == 0 || lcsMatrix[i, j - 1] >= lcsMatrix[i - 1, j]))
            {
                // Character added in str2
                j--;
                str2Diff.Insert(0, str2[j]);
            }
            else if (i > 0 && (j == 0 || lcsMatrix[i, j - 1] < lcsMatrix[i - 1, j]))
            {
                // Character removed from str1
                i--;
                str1Diff.Insert(0, str1[i]);
            }
        }

        // Add any remaining differences
        if (str1Diff.Length > 0 || str2Diff.Length > 0)
        {
            differences.Add(new Difference(
                str1Diff.ToString(),
                str2Diff.ToString(),
                0
            ));
        }

        // Reverse the list to get differences in order from start to end
        differences.Reverse();

        return differences;
    }

    private static int[,] ComputeLCSMatrix(string str1, string str2)
    {
        int[,] lcs = new int[str1.Length + 1, str2.Length + 1];

        for (int i = 1; i <= str1.Length; i++)
        {
            for (int j = 1; j <= str2.Length; j++)
            {
                if (str1[i - 1] == str2[j - 1])
                {
                    lcs[i, j] = lcs[i - 1, j - 1] + 1;
                }
                else
                {
                    lcs[i, j] = Math.Max(lcs[i - 1, j], lcs[i, j - 1]);
                }
            }
        }

        return lcs;
    }
}
