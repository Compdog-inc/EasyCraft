using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EasyCraft.UnitTests.Mathf
{
    [TestClass]
    public class Min
    {
        [TestMethod]
        public void Min_Returns_2_35_VS_5_87()
        {
            float input1 = 2.35f;
            float input2 = 5.87f;
            float expected = 2.35f;

            Assert.AreEqual(expected, engine.Mathf.Min(input1, input2));
        }

        [TestMethod]
        public void Min_Returns_2_35_VS_2_35()
        {
            float input1 = 2.35f;
            float input2 = 2.35f;
            float expected = 2.35f;

            Assert.AreEqual(expected, engine.Mathf.Min(input1, input2));
        }
    }
}
