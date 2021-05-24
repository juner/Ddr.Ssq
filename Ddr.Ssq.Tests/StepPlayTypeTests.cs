using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ddr.Ssq.Tests
{
    [TestClass]
    public class StepPlayTypeTests
    {
        static IEnumerable<object?[]> ToParamTestData
        {
            get
            {
                yield return ToParamTest(new(PlayStyle.Double, PlayDifficulty.Battle), 4120);
                static object?[] ToParamTest(StepPlayType PlayType, short ExpectedParam)
                    => new object?[] { PlayType, ExpectedParam };
            }
        }
        [TestMethod, DynamicData(nameof(ToParamTestData))]
        public void ToParamTest(StepPlayType PlayType, short ExpectedParam)
            => Assert.AreEqual(ExpectedParam, (short)PlayType);
        static IEnumerable<object?[]> FromParamTestData
        {
            get
            {
                yield return FromParamTest(4120, new(PlayStyle.Double, PlayDifficulty.Battle));
                static object?[] FromParamTest(short Param, StepPlayType ExpectedPlayType)
                    => new object?[] { Param, ExpectedPlayType };
            }
        }
        [TestMethod, DynamicData(nameof(FromParamTestData))]
        public void FromParamTest(short Param, StepPlayType ExpectedPlayType)
            => Assert.AreEqual(ExpectedPlayType, StepPlayType.FromParam(Param));
    }
}
