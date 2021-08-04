using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EasyCraft.UnitTests.Mathf
{
    [TestClass]
    public class PerlinNoise
    {
        [TestMethod]
        public void SameValue_X()
        {
            float inputX = 2.454f;

            float output1 = engine.Mathf.PerlinNoise(inputX);
            float output2 = engine.Mathf.PerlinNoise(inputX);

            Assert.AreEqual(output1, output2);
        }

        [TestMethod]
        public void SameValue_X_Y()
        {
            float inputX = 2.454f;
            float inputY = 5.328f;

            float output1 = engine.Mathf.PerlinNoise(inputX, inputY);
            float output2 = engine.Mathf.PerlinNoise(inputX, inputY);

            Assert.AreEqual(output1, output2);
        }

        [TestMethod]
        public void DifferentValue_X()
        {
            float inputX1 = 2.454f;
            float inputX2 = 5.549f;

            float output1 = engine.Mathf.PerlinNoise(inputX1);
            float output2 = engine.Mathf.PerlinNoise(inputX2);

            Assert.AreNotEqual(output1, output2);
        }

        [TestMethod]
        public void DifferentValue_X_Y()
        {
            float inputX1 = 2.454f;
            float inputX2 = 5.549f;
            float inputY1 = 5.328f;
            float inputY2 = 9.325f;

            float output1 = engine.Mathf.PerlinNoise(inputX1, inputY1);
            float output2 = engine.Mathf.PerlinNoise(inputX2, inputY2);

            Assert.AreNotEqual(output1, output2);
        }
    }
}
