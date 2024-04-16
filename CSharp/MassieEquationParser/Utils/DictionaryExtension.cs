using System.Collections.Generic;

namespace Scot.Massie.EquationParser.Utils
{
    public static class DictionaryExtension
    {
        /// <summary>
        /// Sets a value in a nested dictionary. If the inner dictionary does not already exist, it will be created.
        /// </summary>
        /// <param name="dict">The outer dictionary to add a key/key/value set to.</param>
        /// <param name="outerKey">The key in the outer dictionary.</param>
        /// <param name="innerKey">The key in the inner dictionary.</param>
        /// <param name="value">The value to set at the outer and inner key.</param>
        /// <typeparam name="K">The type of the outer key.</typeparam>
        /// <typeparam name="KInner">The type of the inner key.</typeparam>
        /// <typeparam name="V">The type being set.</typeparam>
        /// <typeparam name="TInnerDict">The type of the inner dictionary.</typeparam>
        /// <example>
        /// After calling `doot.SetInNestedDictionary("k1", "k2", myValue)`, `myValue` will be at `doot["k1"]["k2"]`
        /// </example>
        // ReSharper disable InconsistentNaming
        internal static void SetInNestedDictionary<K, KInner, V, TInnerDict>(this IDictionary<K, TInnerDict> dict,
                                                                             K                               outerKey,
                                                                             KInner                          innerKey,
                                                                             V                               value)
            where TInnerDict : IDictionary<KInner, V>, new()
        // ReSharper restore InconsistentNaming
        {
            if(!dict.TryGetValue(outerKey, out var innerDict))
                dict[outerKey] = innerDict = new TInnerDict();

            innerDict[innerKey] = value;
        }
    }
}
