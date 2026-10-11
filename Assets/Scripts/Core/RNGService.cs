using System;
using System.Collections.Generic;
using SlotGame.Data;

namespace SlotGame.Core
{
    /// <summary>
    /// Centralized RNG Service responsible for generating slot spin outcomes.
    /// Uses System.Random with optional seed injection for deterministic test validation.
    /// Note: Designed for local educational game simulation; not intended for real-money gaming certification.
    /// </summary>
    public class RNGService : IRNGService
    {
        private readonly Random _random;

        public RNGService()
        {
            _random = new Random();
        }

        public RNGService(int seed)
        {
            _random = new Random(seed);
        }

        /// <summary>
        /// Generates a complete outcome array of symbol indices for the specified number of reels.
        /// Outcome is selected based on symbol weights.
        /// </summary>
        public int[] GenerateOutcome(int reelCount, IReadOnlyList<SymbolData> symbols)
        {
            if (reelCount <= 0)
                throw new ArgumentException("Reel count must be greater than zero.", nameof(reelCount));

            if (symbols == null || symbols.Count == 0)
                throw new ArgumentException("Symbols list cannot be null or empty.", nameof(symbols));

            int totalWeight = 0;
            for (int i = 0; i < symbols.Count; i++)
            {
                int w = symbols[i] != null && symbols[i].Weight > 0 ? symbols[i].Weight : 1;
                totalWeight += w;
            }

            int[] outcome = new int[reelCount];
            for (int r = 0; r < reelCount; r++)
            {
                int randomRoll = _random.Next(0, totalWeight);
                int cumulative = 0;
                int selectedIndex = 0;

                for (int s = 0; s < symbols.Count; s++)
                {
                    int w = symbols[s] != null && symbols[s].Weight > 0 ? symbols[s].Weight : 1;
                    cumulative += w;
                    if (randomRoll < cumulative)
                    {
                        selectedIndex = s;
                        break;
                    }
                }

                outcome[r] = selectedIndex;
            }

            return outcome;
        }
    }
}
