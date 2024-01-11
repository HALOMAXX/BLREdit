using BLREdit.API.Utils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;

namespace BLREditTests
{
    [TestClass]
    public class BLREditVersionTests
    {
        static List<(BLREditVersion, BLREditVersion, bool, bool, bool, bool)> TestScenarios { get; } = new List<(BLREditVersion, BLREditVersion, bool, bool, bool, bool)>()
        {
            (new BLREditVersion("v0.0.0"), new BLREditVersion("v0.0.1"), false, true, false, true),
            (new BLREditVersion("v0.0.0"), new BLREditVersion("v0.1.0"), false, true, false, true),
            (new BLREditVersion("v0.0.0"), new BLREditVersion("v1.0.0"), false, true, false, true),

            (new BLREditVersion("v0.0.1"), new BLREditVersion("v0.0.0"), true, false, true, false),
            (new BLREditVersion("v0.1.0"), new BLREditVersion("v0.0.0"), true, false, true, false),
            (new BLREditVersion("v1.0.0"), new BLREditVersion("v0.0.0"), true, false, true, false),

            (new BLREditVersion("v0.0.0"), new BLREditVersion("v0.0.0-beta.1"), false, true, false, true),
            (new BLREditVersion("v0.0.0-beta.1"), new BLREditVersion("v0.0.0-beta.2"), false, true, false, true),
            (new BLREditVersion("v0.0.0-beta.1"), new BLREditVersion("v0.0.0-beta.1"), false, false, true, true),

            (new BLREditVersion("v0.0.0-beta.1"), new BLREditVersion("v0.0.0"), true, false, true, false),
            (new BLREditVersion("v0.0.0-beta.2"), new BLREditVersion("v0.0.0-beta.1"), true, false, true, false),
            (new BLREditVersion("v0.0.0-beta.1"), new BLREditVersion("v0.0.0-beta.1"), false, false, true, true),
        };

        [TestMethod]
        public void GreaterThen()
        {
            foreach (var test in TestScenarios)
            {
                var result = test.Item1 > test.Item2;
                Assert.IsTrue(result == test.Item3, $"Failed: {test.Item1} > {test.Item2} == {test.Item3} Result:{result}");
            }
        }

        [TestMethod]
        public void LessThen()
        {
            foreach (var test in TestScenarios)
            {
                var result = test.Item1 < test.Item2;
                Assert.IsTrue(result == test.Item4, $"Failed: {test.Item1} < {test.Item2} == {test.Item4} Result:{result}");
            }
        }

        [TestMethod]
        public void GreaterOrEqualsThen()
        {
            foreach (var test in TestScenarios)
            {
                var result = test.Item1 >= test.Item2;
                Assert.IsTrue(result == test.Item5, $"Failed: {test.Item1} >= {test.Item2} == {test.Item5} Result:{result}");
            }
        }
        [TestMethod]
        public void LessOrEqualsThen()
        {
            foreach (var test in TestScenarios)
            {
                var result = test.Item1 <= test.Item2;
                Assert.IsTrue(result == test.Item6, $"Failed: {test.Item1} <= {test.Item2} == {test.Item6} Result:{result}");
            }
        }
    }
}
