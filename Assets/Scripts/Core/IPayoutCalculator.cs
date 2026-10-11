using System.Collections.Generic;
using SlotGame.Data;

namespace SlotGame.Core
{
    /// <summary>
    /// Contract for evaluating slot machine results and determining payouts.
    /// </summary>
    public interface IPayoutCalculator
    {
        /// <summary>
        /// Evaluates whether the given reel outcome constitutes a win and calculates the awarded payout.
        /// Win rule: All reel symbol indices must match identically.
        /// Payout formula: bet * winningSymbol.PayoutMultiplier.
        /// </summary>
        SlotResult Evaluate(int[] outcome, IReadOnlyList<SymbolData> symbols, int bet);
    }
}
