using System;

namespace Scot.Massie.EquationParser.Utils
{
    public static class DoubleExtension
    {
        // See: https://newtonexcelbach.com/2012/01/07/comparing-floating-point-numbers/
        private static readonly double DefaultDoubleEqualityMargin = Math.Pow(10, -14);
        
        /// <summary>
        /// Determines if two double values are equal to within a margin of error.
        /// </summary>
        /// <remarks>Where the margin of error isn't specified, 10^-14 is used. <see href=/></remarks>
        /// <param name="d">The first value.</param>
        /// <param name="other">The other value.</param>
        /// <returns>
        /// True if the two values are equal to within a margin of error. Otherwise, false.
        /// </returns>
        internal static bool EqualsWithMargin(this double d, double other)
        {
            return EqualsWithMargin(d, other, DefaultDoubleEqualityMargin);
        }

        /// <summary>
        /// Determines if two double values are equal to within a margin of error.
        /// </summary>
        /// <param name="d">The first value.</param>
        /// <param name="other">The other value.</param>
        /// <param name="margin">
        /// The margin of error. The other value can be up to this far away from the first value and still be considered
        /// to be equal.
        /// </param>
        /// <see href="https://newtonexcelbach.com/2012/01/07/comparing-floating-point-numbers/"/>
        /// <returns>
        /// True if the two values are equal to within the specified margin of error. Otherwise, false.
        /// </returns>
        internal static bool EqualsWithMargin(this double d, double other, double margin)
        {
            return (other > (d - margin)) && (other < (d + margin));
        }
    }
}
