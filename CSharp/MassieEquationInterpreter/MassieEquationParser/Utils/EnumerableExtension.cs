using System;
using System.Collections.Generic;
using System.Linq;

namespace Scot.Massie.EquationParser.Utils
{
    public static class EnumerableExtension
    {
        /// <summary>
        /// Gets different selections of the same enumeration and combines them into a single enumeration.
        /// </summary>
        /// <param name="enumerable">A sequence of values to invoke transform functions on.</param>
        /// <param name="selectors">A series of transform functions to apply to each source element.</param>
        /// <typeparam name="TSource">The type of the source enumerable.</typeparam>
        /// <typeparam name="TTarget">The type of the resulting enumerable.</typeparam>
        /// <returns></returns>
        internal static IEnumerable<TTarget> SelectAndCombine<TSource, TTarget>(
            this   IEnumerable<TSource>     enumerable,
            params Func<TSource, TTarget>[] selectors)
        {
            foreach(var item in enumerable)
                foreach(var selector in selectors)
                    yield return selector(item);
        }

        /// <summary>
        /// Gets different selections of the same enumeration where those selections are enumerables, and combines all
        /// of those inner enumerables from all selections into a single enumeration.
        /// </summary>
        /// <param name="enumerable">A sequence of values to invoke transform functions on.</param>
        /// <param name="selectors">
        /// A series of transform functions to apply to each source element to produce an enumerable.
        /// </param>
        /// <typeparam name="TSource">The type of the source enumerable.</typeparam>
        /// <typeparam name="TTarget">The type of the resulting enumerable.</typeparam>
        /// <returns></returns>
        internal static IEnumerable<TTarget> SelectAndCombineMany<TSource, TTarget>(
            this   IEnumerable<TSource>                   enumerable,
            params Func<TSource, IEnumerable<TTarget>?>[] selectors)
        {
            foreach(var item in enumerable)
                foreach(var selector in selectors)
                    foreach(var selectedItem in selector(item) ?? Enumerable.Empty<TTarget>())
                        yield return selectedItem;
        }
    }
}
