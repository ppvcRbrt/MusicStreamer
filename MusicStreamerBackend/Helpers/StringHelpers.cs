namespace MusicStreamerBackend.Helpers;
public static class StringHelpers
{
    public static double Similarity(string? source, string? target, bool ignoreCase = true)
    {
        if (string.IsNullOrEmpty(source) && string.IsNullOrEmpty(target))
            return 1.0;

        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
            return 0.0;

        int maxLen = Math.Max(source.Length, target.Length);
        int distance = LevenshteinDistance(source, target, ignoreCase);

        return 1.0 - (double)distance / maxLen;
    }

    /// <summary>
    /// Calculates the Levenshtein distance between two strings.
    /// Returns the minimum number of single-character edits (insertions, deletions, substitutions)
    /// required to change one string into the other.
    /// Uses O(min(n,m)) space optimization with two single-dimension arrays.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="target">The target string.</param>
    /// <param name="ignoreCase">If true, performs case-insensitive comparison.</param>
    /// <returns>The Levenshtein distance between the two strings.</returns>
    public static int LevenshteinDistance(string? source, string? target, bool ignoreCase = true)
    {
        if (string.IsNullOrEmpty(source))
            return target?.Length ?? 0;

        if (string.IsNullOrEmpty(target))
            return source.Length;

        return LevenshteinDistance(source.AsSpan(), target.AsSpan(), ignoreCase);
    }

    /// <summary>
    /// Calculates the Levenshtein distance between two character spans.
    /// Zero-allocation overload for performance-critical paths.
    /// Uses O(min(n,m)) space optimization.
    /// </summary>
    private static int LevenshteinDistance(ReadOnlySpan<char> source, ReadOnlySpan<char> target, bool ignoreCase = true)
    {
        if (source.IsEmpty)
            return target.Length;

        if (target.IsEmpty)
            return source.Length;

        // Ensure we iterate over the shorter dimension for the column array (space optimization)
        if (source.Length < target.Length)
        {
            var temp = source;
            source = target;
            target = temp;
        }

        int targetLength = target.Length;
        int[] previousRow = new int[targetLength + 1];
        int[] currentRow = new int[targetLength + 1];

        // Initialize the previous row (equivalent to distance[0, j] = j)
        for (int j = 0; j <= targetLength; j++)
            previousRow[j] = j;

        for (int i = 1; i <= source.Length; i++)
        {
            currentRow[0] = i;

            for (int j = 1; j <= targetLength; j++)
            {
                int cost = ignoreCase
                    ? (char.ToUpperInvariant(source[i - 1]) == char.ToUpperInvariant(target[j - 1]) ? 0 : 1)
                    : (source[i - 1] == target[j - 1] ? 0 : 1);

                currentRow[j] = Math.Min(
                    Math.Min(
                        previousRow[j] + 1,      // deletion
                        currentRow[j - 1] + 1),  // insertion
                    previousRow[j - 1] + cost);   // substitution
            }

            // Swap rows
            (previousRow, currentRow) = (currentRow, previousRow);
        }

        // Result is in previousRow because we swapped after the last iteration
        return previousRow[targetLength];
    }
    
}
