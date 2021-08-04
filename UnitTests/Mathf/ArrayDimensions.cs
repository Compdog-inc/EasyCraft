using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SharpDX;

namespace EasyCraft.UnitTests.Mathf
{
    [TestClass]
    public class ArrayDimensions
    {
        [TestMethod]
        public void VEC2_TO_INDEX()
        {
            Vector2 input = new Vector2(5, 3);
            int expected = 26;
            int width = 7;
            try
            {
                Assert.AreEqual(expected, engine.Mathf.GetIndexFromVector2(input, width));
            } catch(ArgumentOutOfRangeException)
            {
                Assert.Fail("Argument out of range exception thrown.");
                return;
            }
        }

        [TestMethod]
        public void VEC3_TO_INDEX()
        {
            Vector3 input = new Vector3(5, 3, 9);
            int expected = 845;
            int width = 7;
            int height = 13;
            try
            {
                Assert.AreEqual(expected, engine.Mathf.GetIndexFromVector3(input, width, height));
            }
            catch (ArgumentOutOfRangeException)
            {
                Assert.Fail("Argument out of range exception thrown.");
                return;
            }
        }

        [TestMethod]
        public void VEC2_ARRAY()
        {
            int width = 7;
            int height = 3;
            int[] array = new int[width * height];

            try
            {
                array[engine.Mathf.GetIndexFromVector2(new Vector2(0, 0), width)] = 3;
                Assert.AreEqual(3, array[0]);

                array[engine.Mathf.GetIndexFromVector2(new Vector2(3, 0), width)] = 6;
                Assert.AreEqual(6, array[3]);

                array[engine.Mathf.GetIndexFromVector2(new Vector2(3, 2), width)] = 8;
                Assert.AreEqual(8, array[17]);
            }
            catch (ArgumentOutOfRangeException)
            {
                Assert.Fail("Argument out of range exception thrown.");
                return;
            }
        }

        [TestMethod]
        public void INDEX_TO_VEC2()
        {
            int width = 7;
            int index = 0;
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    try
                    {
                        Assert.AreEqual(new Vector2(x, y), engine.Mathf.GetVector2FromIndex(index, width));
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        Assert.Fail("Argument out of range exception thrown.");
                        return;
                    }
                    index++;
                }
            }
        }

        [TestMethod]
        public void INDEX_TO_VEC3()
        {
            int width = 7;
            int height = 3;
            int index = 0;
            for (int z = 0; z < 6; z++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        try
                        {
                            Assert.AreEqual(new Vector3(x, y, z), engine.Mathf.GetVector3FromIndex(index, width, height));
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            Assert.Fail("Argument out of range exception thrown.");
                            return;
                        }
                        index++;
                    }
                }
            }
        }

        [TestMethod]
        public void VEC3_ARRAY()
        {
            int width = 7;
            int height = 3;
            int depth = 6;
            int[] array = new int[width * height * depth];

            try
            {
                array[engine.Mathf.GetIndexFromVector3(new Vector3(0, 0, 0), width, height)] = 3;
                Assert.AreEqual(3, array[0]);

                array[engine.Mathf.GetIndexFromVector3(new Vector3(3, 0, 0), width, height)] = 6;
                Assert.AreEqual(6, array[3]);

                array[engine.Mathf.GetIndexFromVector3(new Vector3(3, 1, 0), width, height)] = 8;
                Assert.AreEqual(8, array[10]);

                array[engine.Mathf.GetIndexFromVector3(new Vector3(3, 2, 4), width, height)] = 12;
                Assert.AreEqual(12, array[101]);
            }
            catch (ArgumentOutOfRangeException)
            {
                Assert.Fail("Argument out of range exception thrown.");
                return;
            }
        }

        [TestMethod]
        public void Exception_When_X_Negative_V2()
        {
            Vector2 input = new Vector2(-4, 3);
            int width = 7;
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => engine.Mathf.GetIndexFromVector2(input, width));
        }

        [TestMethod]
        public void Exception_When_X_Negative_V3()
        {
            Vector3 input = new Vector3(-4, 3, 9);
            int width = 7;
            int height = 13;
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => engine.Mathf.GetIndexFromVector3(input, width, height));
        }

        [TestMethod]
        public void Exception_When_Y_Negative_V3()
        {
            Vector3 input = new Vector3(4, -7, 9);
            int width = 7;
            int height = 13;
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => engine.Mathf.GetIndexFromVector3(input, width, height));
        }

        [TestMethod]
        public void Exception_When_X_BiggerThan_Width_V2()
        {
            Vector2 input = new Vector2(10, 3);
            int width = 7;
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => engine.Mathf.GetIndexFromVector2(input, width));
        }

        [TestMethod]
        public void Exception_When_X_BiggerThan_Width_V3()
        {
            Vector3 input = new Vector3(10, 3, 9);
            int width = 7;
            int height = 13;
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => engine.Mathf.GetIndexFromVector3(input, width, height));
        }

        [TestMethod]
        public void Exception_When_X_BiggerThan_Width_And_Y_BiggerThan_Height_V3()
        {
            Vector3 input = new Vector3(10, 26, 9);
            int width = 7;
            int height = 13;
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => engine.Mathf.GetIndexFromVector3(input, width, height));
        }
    }
}
