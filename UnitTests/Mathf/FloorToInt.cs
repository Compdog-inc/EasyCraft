using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EasyCraft.UnitTests.Mathf
{
    [TestClass]
    public class FloorToInt
    {
        [TestMethod]
        public void FloorToInt_Returns_Bottom_When_Bottom()
        {
            float input = 3.324f;
            int expected = 3;

            Assert.AreEqual(expected, engine.Mathf.FloorToInt(input));
        }

        [TestMethod]
        public void FloorToInt_Returns_Bottom_When_Middle()
        {
            float input = 3.5f;
            int expected = 3;

            Assert.AreEqual(expected, engine.Mathf.FloorToInt(input));
        }

        [TestMethod]
        public void FloorToInt_Returns_Bottom_When_Top()
        {
            float input = 3.896f;
            int expected = 3;

            Assert.AreEqual(expected, engine.Mathf.FloorToInt(input));
        }

        [TestMethod]
        public void FloorToInt_Returns_Same_When_Int()
        {
            float input = 3f;
            int expected = 3;

            Assert.AreEqual(expected, engine.Mathf.FloorToInt(input));
        }
    }
}
