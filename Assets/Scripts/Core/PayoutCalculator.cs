using System;
using System.Collections.Generic;
using SlotGame.Data;

namespace SlotGame.Core
{
    /// <summary>
    /// Pure logic service for evaluating win conditions and calculating payouts.
    /// Baseline assignment rule:
    /// - Win condition: ALL three reels must land on the EXACT SAME symbol index.
    /// - Payout formula: bet * winningSymbol.PayoutMultiplier (credited to player's balance).
    /// - Any non-matching combination pays 0.
    /// </summary>
    public class PayoutCalculator : IPayoutCalculator
    {
        public SlotResult Evaluate(int[] outcome, IReadOnlyList<SymbolData> symbols, int bet)
        {
            if (outcome == null || outcome.Length == 0)
                throw new ArgumentException("Outcome array cannot be null or empty.", nameof(outcome));

            if (symbols == null || symbols.Count == 0)
                throw new ArgumentException("Symbols collection cannot be null or empty.", nameof(symbols));

            if (bet <= 0)
                throw new ArgumentOutOfRangeException(nameof(bet), "Bet must be greater than zero.");

            // Check if all reel indices match the first reel's index
            int firstIndex = outcome[0];
            bool allMatch = true;

            for (int i = 1; i < outcome.Length; i++)
            {
                if (outcome[i] != firstIndex)
                {
                    allMatch = false;
                    break;
                }
            }

            if (allMatch && firstIndex >= 0 && firstIndex < symbols.Count)
            {
                SymbolData winningSymbol = symbols[firstIndex];
                int payout = bet * winningSymbol.PayoutMultiplier;
                return new SlotResult(outcome, isWin: true, payout: payout, bet: bet, winningSymbol: winningSymbol);
            }

            return new SlotResult(outcome, isWin: false, payout: 0, bet: bet, winningSymbol: null);
        }
    }
}
