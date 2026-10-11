using System.Collections.Generic;
using SlotGame.Data;

namespace SlotGame.Core
{
    /// <summary>
    /// Service interface for generating random slot machine spin outcomes.
    /// </summary>
    public interface IRNGService
    {
        /// <summary>
        /// Generates an array of symbol indices (one per reel).
        /// </summary>
        /// <param name="reelCount">Total number of reels.</param>
        /// <param name="symbols">Collection of available symbols with their configured weights.</param>
        /// <returns>Array of symbol indices corresponding to each reel.</returns>
        int[] GenerateOutcome(int reelCount, IReadOnlyList<SymbolData> symbols);
    }
}
