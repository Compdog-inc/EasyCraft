using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EasyCraft.UnitTests.Mathf
{
    [TestClass]
    public class Max
    {
        [TestMethod]
        public void Max_Returns_5_87_VS_2_35()
        {
            float input1 = 5.87f;
            float input2 = 2.35f;
            float expected = 5.87f;

            Assert.AreEqual(expected, engine.Mathf.Max(input1, input2));
        }

        [TestMethod]
        public void Max_Returns_5_78_VS_5_78()
        {
            float input1 = 5.87f;
            float input2 = 5.87f;
            float expected = 5.87f;

            Assert.AreEqual(expected, engine.Mathf.Max(input1, input2));
        }
    }
}
