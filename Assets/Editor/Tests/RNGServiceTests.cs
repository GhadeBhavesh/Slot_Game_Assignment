using System.Collections.Generic;
using NUnit.Framework;
using SlotGame.Core;
using SlotGame.Data;
using UnityEngine;

namespace SlotGame.Editor.Tests
{
    [TestFixture]
    public class RNGServiceTests
    {
        private List<SymbolData> _testSymbols;

        [SetUp]
        public void Setup()
        {
            _testSymbols = new List<SymbolData>();

            for (int i = 0; i < 4; i++)
            {
                SymbolData s = ScriptableObject.CreateInstance<SymbolData>();
                s.Initialize($"Symbol_{i}", $"Symbol {i}", null, multiplier: (i + 1) * 5, symbolWeight: (i + 1) * 5);
                _testSymbols.Add(s);
            }
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
        public void GenerateOutcome_ReturnsCorrectReelCount()
        {
            RNGService rng = new RNGService();
            int[] outcome = rng.GenerateOutcome(3, _testSymbols);

            Assert.IsNotNull(outcome);
            Assert.AreEqual(3, outcome.Length);
        }

        [Test]
        public void GenerateOutcome_IndicesAreWithinValidRange()
        {
            RNGService rng = new RNGService();

            for (int spin = 0; spin < 50; spin++)
            {
                int[] outcome = rng.GenerateOutcome(3, _testSymbols);
                for (int r = 0; r < outcome.Length; r++)
                {
                    Assert.GreaterOrEqual(outcome[r], 0);
                    Assert.Less(outcome[r], _testSymbols.Count);
                }
            }
        }

        [Test]
        public void GenerateOutcome_WithSeed_IsDeterministic()
        {
            int seed = 12345;
            RNGService rngA = new RNGService(seed);
            RNGService rngB = new RNGService(seed);

            int[] outcomeA = rngA.GenerateOutcome(3, _testSymbols);
            int[] outcomeB = rngB.GenerateOutcome(3, _testSymbols);

            CollectionAssert.AreEqual(outcomeA, outcomeB, "Identical seeds must yield identical outcomes.");
        }
    }
}
