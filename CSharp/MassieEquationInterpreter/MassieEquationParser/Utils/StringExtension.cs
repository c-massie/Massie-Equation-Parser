using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Scot.Massie.EquationParser.Utils
{
    public static class StringExtension
    {
        /// <summary>
        /// Gets call indices in the given string <paramref name="s"/> in which the given string <paramref name="other"/> appears.
        /// </summary>
        /// <remarks>
        /// Instances of <paramref name="other"/> are not counted where they overlap with another instance.
        /// </remarks>
        /// <param name="s">The string that may contain the given other string.</param>
        /// <param name="other">The string to look for.</param>
        /// <param name="startingAt">The index in <paramref name="s"/> to only get indices at or after.</param>
        /// <param name="stringComparison">The comparison method used to look for instances.</param>
        /// <returns></returns>
        public static ICollection<int> AllIndicesOf(this string s, string other, int startingAt, StringComparison stringComparison)
        {
            if(other.Length == 0)
                return Enumerable.Range(0, s.Length + 1).ToList();

            if(startingAt < 0)
                startingAt = 0;
            
            int index = -other.Length + startingAt;
            var results = new List<int>();
            
            while((index = s.IndexOf(other, (index + other.Length), stringComparison)) >= 0)
                results.Add(index);
        
            return results;
        }

        /// <summary>
        /// Splits a string <paramref name="s"/> by a given set of separators <paramref name="separators"/> at a given
        /// set of indices <paramref name="indices"/>.
        /// </summary>
        /// <remarks>
        /// This assumes that the separators and indices lists contain the name number of elements, and that each
        /// element of <paramref name="indices"/> corresponds to an element of <paramref name="separators"/> at the same
        /// index in the list.
        /// </remarks>
        /// <remarks>
        /// This assumes that all separators passed are actually at the index at the same position in the indices list.
        /// </remarks>
        /// <param name="s">The string to be split.</param>
        /// <param name="separators">The separators at the given indices the string should be split by.</param>
        /// <param name="indices">The indices the separators appear at in the string.</param>
        /// <returns>
        /// A list of strings which are the separated portions of the original string, in order. They do not contain the
        /// separators the string was split by.
        /// </returns>
        public static IList<string> SplitBySeparatorsAtIndices(this string s, IList<string> separators, IList<int> indices)
        {
            var result = new List<string>(separators.Count + 1);

            var startOfSection = 0;

            for(int i = 0; i < separators.Count; i++)
            {
                var separator = separators[i];
                var index     = indices[i];
                
                result.Add(s[startOfSection..index]);
                startOfSection = index + separator.Length;
            }
            
            result.Add(s[startOfSection..]);
            return result;
        }

        private static readonly Regex EndingDoubleRegex = new Regex(@"(-)?\d(\.\d)?$", RegexOptions.Singleline);
        
        /// <summary>
        /// Gets all possible doubles a string could be interpreted as ending with.
        /// </summary>
        /// <param name="s">The string that may end in any number of doubles.</param>
        /// <returns>
        /// An enumerable of the doubles the string ends with, paired with their original string representation in the
        /// string, in order from longest to shortest.
        /// </returns>
        public static IEnumerable<(double result, string source)> GetEndingDoublesWithSources(this string s)
        {
            var longestEndingDoubleString = EndingDoubleRegex.Match(s).Value;

            for(int i = 0; i < longestEndingDoubleString.Length; i++)
            {
                var endingDoubleString = longestEndingDoubleString[i..];

                if(double.TryParse(endingDoubleString, out var endingDouble))
                    yield return (endingDouble, endingDoubleString);
            }
        }

        /// <summary>
        /// Gets whether a string <paramref name="s"/> contains another string <paramref name="other"/> at the given
        /// index <paramref name="startingIndex"/>.
        /// </summary>
        /// <param name="s">The string that may contain the other string at the given index.</param>
        /// <param name="other">The other string that may appear in the first.</param>
        /// <param name="startingIndex">The index in the string to check for the other string.</param>
        /// <returns>
        /// True if the string contains the other string starting at the index specified. Otherwise, false.
        /// </returns>
        public static bool ContainsAt(this string s, string other, int startingIndex)
        {
            if(startingIndex + other.Length > s.Length)
                return false;

            for(int i = 0; i < other.Length; i++)
                if(s[startingIndex + i] != other[i])
                    return false;

            return true;
        }

        /// <summary>
        /// Gets whether a string <paramref name="s"/> contains another string <paramref name="other"/> ending at the
        /// given index <paramref name="endingIndex"/>.
        /// </summary>
        /// <param name="s">The string that may contain the other string ending at the given index.</param>
        /// <param name="other">The other string that may appear in the first.</param>
        /// <param name="endingIndex">The index in the string to check for the other string.</param>
        /// <returns></returns>
        public static bool ContainsEndingAt(this string s, string other, int endingIndex)
        {
            if(other.Length > (endingIndex + 1))
                return false;
            
            for(int i = 0; i < other.Length; i++)
                if(s[(endingIndex - other.Length + 1) + i] != other[i])
                    return false;

            return true;
        }
    }
}
