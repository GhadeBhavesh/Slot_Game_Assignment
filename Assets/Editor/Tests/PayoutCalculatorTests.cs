using System.Collections.Generic;
using NUnit.Framework;
using SlotGame.Core;
using SlotGame.Data;
using UnityEngine;

namespace SlotGame.Editor.Tests
{
    [TestFixture]
    public class PayoutCalculatorTests
    {
        private List<SymbolData> _testSymbols;
        private PayoutCalculator _calculator;

        [SetUp]
        public void Setup()
        {
            _calculator = new PayoutCalculator();
            _testSymbols = new List<SymbolData>();

            // Create test symbols with known multipliers
            SymbolData s0 = ScriptableObject.CreateInstance<SymbolData>();
            s0.Initialize("Cherry", "Cherry", null, multiplier: 5, symbolWeight: 10);
            _testSymbols.Add(s0);

            SymbolData s1 = ScriptableObject.CreateInstance<SymbolData>();
            s1.Initialize("Bell", "Bell", null, multiplier: 10, symbolWeight: 8);
            _testSymbols.Add(s1);

            SymbolData s2 = ScriptableObject.CreateInstance<SymbolData>();
            s2.Initialize("Bar", "Bar", null, multiplier: 25, symbolWeight: 5);
            _testSymbols.Add(s2);

            SymbolData s3 = ScriptableObject.CreateInstance<SymbolData>();
            s3.Initialize("Seven", "Seven", null, multiplier: 50, symbolWeight: 2);
            _testSymbols.Add(s3);
        }

        [TearDown]
        public void Teardown()
        {
            foreach (var s in _testSymbols)
            {
                if (s != null) Object.DestroyImmediate(s);
            }
            _testSymbols.Clear();
        }

        [Test]
        public void ThreeMatchingSymbols_AwardsWinningPayout()
        {
            int bet = 10;
            // Outcome where all three reels land on index 1 (Bell, multiplier 10)
            int[] outcome = new int[] { 1, 1, 1 };

            SlotResult result = _calculator.Evaluate(outcome, _testSymbols, bet);

            Assert.IsTrue(result.IsWin, "Three matching symbols should be a win.");
            Assert.AreEqual(100, result.Payout, "Payout should be bet (10) * multiplier (10) = 100.");
            Assert.IsNotNull(result.WinningSymbol);
            Assert.AreEqual("Bell", result.WinningSymbol.SymbolId);
        }

        [Test]
        public void Jackpots_SevenMatch_AwardsTopMultiplier()
        {
            int bet = 20;
            // All three reels land on index 3 (Seven, multiplier 50)
            int[] outcome = new int[] { 3, 3, 3 };

            SlotResult result = _calculator.Evaluate(outcome, _testSymbols, bet);

            Assert.IsTrue(result.IsWin);
            Assert.AreEqual(1000, result.Payout, "Payout should be 20 * 50 = 1000.");
            Assert.AreEqual("Seven", result.WinningSymbol.SymbolId);
        }

        [Test]
        public void TwoMatchingSymbols_AwardsZeroPayout()
        {
            int bet = 10;
            // Near win: two match, one does not (1, 1, 2)
            int[] outcome = new int[] { 1, 1, 2 };

            SlotResult result = _calculator.Evaluate(outcome, _testSymbols, bet);

            Assert.IsFalse(result.IsWin, "Near match (2 symbols) should NOT win per baseline rules.");
            Assert.AreEqual(0, result.Payout, "Payout should be 0 on loss.");
            Assert.IsNull(result.WinningSymbol);
        }

        [Test]
        public void AllDifferentSymbols_AwardsZeroPayout()
        {
            int bet = 10;
            int[] outcome = new int[] { 0, 1, 2 };

            SlotResult result = _calculator.Evaluate(outcome, _testSymbols, bet);

            Assert.IsFalse(result.IsWin);
            Assert.AreEqual(0, result.Payout);
            Assert.IsNull(result.WinningSymbol);
        }
    }
}
