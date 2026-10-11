using System.Collections.Generic;
using UnityEngine;

namespace SlotGame.Data
{
    /// <summary>
    /// Configuration asset containing slot machine settings, symbols, balance, and reel timings.
    /// </summary>
    [CreateAssetMenu(fileName = "SlotGameConfig", menuName = "Slot Game/Game Config", order = 2)]
    public class SlotGameConfig : ScriptableObject
    {
        [Header("Symbols")]
        [Tooltip("The collection of symbols available on the slot reels.")]
        [SerializeField] private List<SymbolData> symbols = new List<SymbolData>();

        [Header("Balance & Betting")]
        [Tooltip("Initial wallet balance given to the player at startup.")]
        [SerializeField] private int startingBalance = 1000;

        [Tooltip("Initial bet amount.")]
        [SerializeField] private int defaultBet = 10;

        [Tooltip("Minimum allowable bet.")]
        [SerializeField] private int minBet = 5;

        [Tooltip("Maximum allowable bet.")]
        [SerializeField] private int maxBet = 100;

        [Tooltip("Step increment/decrement for bet adjustments.")]
        [SerializeField] private int betStep = 5;

        [Header("Reel Spin Animation Timing")]
        [Tooltip("Base duration in seconds before the first reel begins stopping.")]
        [SerializeField] private float baseSpinDuration = 1.4f;

        [Tooltip("Delay in seconds between successive reels stopping.")]
        [SerializeField] private float reelStopStagger = 0.45f;

        [Tooltip("Vertical scrolling speed in pixels per second.")]
        [SerializeField] private float reelSpinSpeed = 1400f;

        [Tooltip("Height of each symbol slot in pixels.")]
        [SerializeField] private float symbolHeight = 96f;

        public IReadOnlyList<SymbolData> Symbols => symbols;
        public int StartingBalance => startingBalance;
        public int DefaultBet => defaultBet;
        public int MinBet => minBet;
        public int MaxBet => maxBet;
        public int BetStep => betStep;
        public float BaseSpinDuration => baseSpinDuration;
        public float ReelStopStagger => reelStopStagger;
        public float ReelSpinSpeed => reelSpinSpeed;
        public float SymbolHeight => symbolHeight;

        public void SetSymbols(List<SymbolData> symbolList)
        {
            symbols = symbolList;
        }
    }
}
