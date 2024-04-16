using System.Collections.Generic;
using Scot.Massie.EquationParser.Equations;

namespace Scot.Massie.EquationParser.EquationSubParsers
{
    internal delegate int DepthAdjuster(int pulledEquationLength);
    
    internal interface IEquationSubParser
    {
        IEquation? Parse(string equationString, IEquationStores stores, int depthRemaining);

        /// <summary>
        /// Attempts to read an equation from the end of a given equation.
        /// </summary>
        /// <param name="equationString">
        /// The equation that may end in another equation parsable by this subparser.
        /// </param>
        /// <param name="stores">The equation stores.</param>
        /// <param name="depthRemaining">The remaining depth.</param>
        /// <param name="depthAdjuster">
        /// Function that accepts a current depth and the length of the equation read from the end, and returns a
        /// recommended new depth that accounts for any prefix operators that may exist before the proposed trailing
        /// equation.
        /// </param>
        /// <returns>
        /// An enumerable of all possible equations that could be read from the end of the given equation according to
        /// this subparser, paired with the text they were parsed from. These should be given in order from longest to
        /// shortest.
        /// </returns>
        IEnumerable<(IEquation equation, string equationSource)> ReadFromEnd(string          equationString,
                                                                             IEquationStores stores,
                                                                             int             depthRemaining,
                                                                             DepthAdjuster   depthAdjuster);
    }
}
