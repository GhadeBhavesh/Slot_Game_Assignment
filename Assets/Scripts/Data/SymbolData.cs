using UnityEngine;

namespace SlotGame.Data
{
    /// <summary>
    /// ScriptableObject defining an individual slot symbol, its visual icon, and payout multiplier.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSymbolData", menuName = "Slot Game/Symbol Data", order = 1)]
    public class SymbolData : ScriptableObject
    {
        [Tooltip("Unique identifier for this symbol (e.g. Cherry, Bell, Seven).")]
        [SerializeField] private string symbolId = "Symbol";

        [Tooltip("Friendly display name shown in UI and paytable.")]
        [SerializeField] private string displayName = "Symbol";

        [Tooltip("Sprite icon used on the reel.")]
        [SerializeField] private Sprite icon;

        [Tooltip("Payout multiplier awarded when all 3 reels land on this symbol.")]
        [SerializeField] private int payoutMultiplier = 5;

        [Tooltip("Weight for RNG generation. Higher weight means higher probability of appearing.")]
        [SerializeField] private int weight = 10;

        public string SymbolId => symbolId;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public int PayoutMultiplier => payoutMultiplier;
        public int Weight => weight;

        public void Initialize(string id, string name, Sprite sprite, int multiplier, int symbolWeight = 10)
        {
            symbolId = id;
            displayName = name;
            icon = sprite;
            payoutMultiplier = multiplier;
            weight = symbolWeight;
        }
    }
}
