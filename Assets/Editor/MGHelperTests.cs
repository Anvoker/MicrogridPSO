using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace SSM.Grid.Tests
{
    [TestFixture]
    public class MGHelperTests
    {
        [Test]
        public void IntToBinary()
        {
            int comb = 29;
            int bitCount = 6;
            var actual = MGHelper.IntToBinary(comb, bitCount);
            var expected = new int[] { 0, 1, 1, 1, 0, 1 };

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void BinaryToInt()
        {
            int[] arr = new int[] { 0, 1, 1, 1, 0, 1 };
            var actual = MGHelper.BinaryToInt(arr);
            var expected = 29;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void BitwiseDivisionByTwo()
        {
            int n = 29;
            int bitsCount = 6;

            int[] integerArithmeticResult = new int[bitsCount];
            int nTemp1 = n;

            for (int i = 0; i < bitsCount; i++)
            {
                integerArithmeticResult[i]= nTemp1 % 2;
                nTemp1 /= 2;
            }

            int[] bitwiseArithmeticResult = new int[bitsCount];
            int nTemp2 = n;

            for (int i = 0; i < bitsCount; i++)
            {
                bitwiseArithmeticResult[i] = nTemp2 & 1;
                nTemp2 >>= 1;
            }

            var expectedResult = new int[] { 1, 0, 1, 1, 1, 0};
            CollectionAssert.AreEqual(expectedResult, integerArithmeticResult);
            CollectionAssert.AreEqual(expectedResult, bitwiseArithmeticResult);
        }

        [Test]
        public void UpDownTimeTest()
        {
            var u_chp0 = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            var u_chp1 = new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            var u_chp2 = new int[] { 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 1, 1, 0, 0, 0, 0, 1 };
            var u_chp3 = new int[] { 1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 1, 1, 1, 1, 0, 0, 0, 1, 1, 0, 0, 0, 0, 1 };
            var u_chp4 = new int[] { 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 1, 1, 0, 1, 0, 0, 1 };
            var u_chp5 = new int[] { 1, 1, 1, 1, 0, 0, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1 };
            var u_chp6 = new int[] { 1, 1, 1, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1 };
            var u_chp7 = new int[] { 1, 1, 1, 1, 0, 0, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 1 };
            var u_chp8 = new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1 };

            var actual0 = MGHelper.IsStateValid(u_chp0, 2.25f, 0.0f, 24);
            var actual1 = MGHelper.IsStateValid(u_chp1, 2.25f, 0.0f, 24);
            var actual2 = MGHelper.IsStateValid(u_chp2, 2.25f, 0.0f, 24);
            var actual3 = MGHelper.IsStateValid(u_chp3, 2.25f, 0.0f, 24);
            var actual4 = MGHelper.IsStateValid(u_chp4, 2.25f, 0.0f, 24);
            var actual5 = MGHelper.IsStateValid(u_chp5, 2.00f, 3.0f, 24);
            var actual6 = MGHelper.IsStateValid(u_chp6, 2.00f, 3.0f, 24);
            var actual7 = MGHelper.IsStateValid(u_chp7, 2.00f, 3.0f, 24);
            var actual8 = MGHelper.IsStateValid(u_chp8, 2.00f, 3.0f, 24);

            Assert.AreEqual(true, actual0);
            Assert.AreEqual(true, actual1);
            Assert.AreEqual(true, actual2);
            Assert.AreEqual(false, actual3);
            Assert.AreEqual(false, actual4);
            Assert.AreEqual(true, actual5);
            Assert.AreEqual(false, actual6);
            Assert.AreEqual(false, actual7);
            Assert.AreEqual(true, actual8);
        }

        [Test, Sequential]
        public void GetUDTime_Up(
            [Values(60.0f, 60.0f, 60.0f, 60.0f, 60.0f)] float tIncr,
            [Values(0, 1, 2, 3, 4)] int iRow,
            [Values(11, 3, 0, 10, 5)] int iChp,
            [Values(4.0f, 2.0f, 0.0f, 0.0f, 5.0f)] float expectedCount)
        {
            //chpIndex,t
            var u1 = new int[,]
            {
                { 0, 1, 1, 0, 0, 1, 0, 1, 1, 1, 1, 0 },
                { 0, 1, 1, 0, 0, 1, 0, 1, 1, 1, 1, 1 },
                { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
                { 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 1, 1 },
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0 }
            };

            var actual = MGHelper.GetUDTime(u1, iRow, iChp, tIncr, true);
            Assert.AreEqual(expectedCount * tIncr, actual);
        }

        [Test, Sequential]
        public void GetUDTime_Down(
            [Values(60.0f, 60.0f, 60.0f, 60.0f, 60.0f)] float tIncr,
            [Values(0, 1, 2, 3, 4)] int iRow,
            [Values(11, 5, 11, 10, 0)] int iChp,
            [Values(0.0f, 2.0f, 9.0f, 1.0f, 0.0f)] float expectedCount)
        {
            //chpIndex,t
            var u1 = new int[,]
            {
                { 0, 1, 1, 0, 0, 1, 0, 1, 1, 1, 1, 0 },
                { 0, 1, 1, 0, 0, 1, 0, 1, 1, 1, 1, 1 },
                { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
                { 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 1, 1 },
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0 }
            };

            var actual = MGHelper.GetUDTime(u1, iRow, iChp, tIncr, false);
            Assert.AreEqual(expectedCount * tIncr, actual);
        }

        [Test, Sequential]
        public void GetRowTest([Values(0, 1, 2, 3, 4)] int iRow)
        {
            //chpIndex,t
            var u1 = new int[,]
            {
                { 0, 1, 1, 0, 0, 1, 0, 1, 1, 1, 1, 0 },
                { 0, 1, 1, 0, 0, 1, 0, 1, 1, 1, 1, 1 },
                { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
                { 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 1, 1 },
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0 }
            };

            int[] actualRow = MGHelper.GetRow(u1, iRow);

            for (int j = 0; j < u1.GetLength(1); j++)
            {
                Assert.AreEqual(u1[iRow, j], actualRow[j]);
            }
        }

        [Test]
        public void GetColTest([Values(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11)] int iCol)
        {
            //chpIndex,t
            var u1 = new int[,]
            {
                { 0, 1, 1, 0, 0, 1, 0, 1, 1, 1, 1, 0 },
                { 0, 1, 1, 0, 0, 1, 0, 1, 1, 1, 1, 1 },
                { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
                { 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 1, 1 },
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0 }
            };

            int[] actualCol = MGHelper.GetCol(u1, iCol);

            for (int i = 0; i < u1.GetLength(0); i++)
            {
                Assert.AreEqual(u1[i, iCol], actualCol[i]);
            }
        }
    }
}