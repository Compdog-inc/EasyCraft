using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EasyCraft.UnitTests.Mathf
{
    [TestClass]
    public class Abs
    {
        [TestMethod]
        public void Abs_Returns_4_32_When_4_32()
        {
            float input = 4.32f;
            float expected = 4.32f;

            Assert.AreEqual(expected, engine.Mathf.Abs(input));
        }

        [TestMethod]
        public void Abs_Returns_4_32_When_M4_32()
        {
            float input = -4.32f;
            float expected = 4.32f;

            Assert.AreEqual(expected, engine.Mathf.Abs(input));
        }

        [TestMethod]
        public void Abs_Returns_0_When_0()
        {
            float input = 0;
            float expected = 0;

            Assert.AreEqual(expected, engine.Mathf.Abs(input));
        }
    }
}
