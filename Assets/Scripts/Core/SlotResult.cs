using SlotGame.Data;

namespace SlotGame.Core
{
    /// <summary>
    /// Represents the evaluated result of a slot machine spin.
    /// </summary>
    public struct SlotResult
    {
        public int[] SymbolIndices { get; }
        public bool IsWin { get; }
        public int Payout { get; }
        public int Bet { get; }
        public SymbolData WinningSymbol { get; }

        public SlotResult(int[] symbolIndices, bool isWin, int payout, int bet, SymbolData winningSymbol)
        {
            SymbolIndices = symbolIndices;
            IsWin = isWin;
            Payout = payout;
            Bet = bet;
            WinningSymbol = winningSymbol;
        }

        public override string ToString()
        {
            string indicesStr = SymbolIndices != null ? string.Join(", ", SymbolIndices) : "empty";
            return $"SlotResult [Indices: ({indicesStr}), IsWin: {IsWin}, Payout: {Payout}, Bet: {Bet}]";
        }
    }
}
