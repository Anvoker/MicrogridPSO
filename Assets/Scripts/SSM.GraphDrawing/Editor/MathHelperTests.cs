using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace SSM.GraphDrawing.Tests
{
    [TestFixture]
    public class MathHelperTests
    {
        [Test, Sequential]
        public void DigitCount(
            [Values(234755.353f, 5.000f, 9999.9f, 0.0f, -234.334f)] float f,
            [Values(6, 1, 4, 1, 3)] int expected)
        {
            int actual = MathHelper.DigitCount(f);
            Assert.AreEqual(expected, actual);
        }

        [Test, Sequential]
        public void RoundAbs_UpPositive(
            [Values(23478.353f, 23478.353f, 23478.353f, 23478.353f, 23478.353f, 23478.353f, 23478.353f, 23478.353f, 23478.353f, 23478.353f)] float number,
            [Values(0, 1, 2, 3, 4, 5, 6, 7, 8, 9)] int digit,
            [Values(23478.353f, 30000f, 24000f, 23500f, 23480f, 23479f, 23478.4f, 23478.36f, 23478.353f, 23478.353f)] float expected)
        {
            float actual = MathHelper.RoundAbs(number, digit, MathHelper.RoundStyle.AlwaysUp);
            Assert.IsTrue(Mathf.Approximately(expected, actual));
        }

        [Test, Sequential]
        public void RoundAbs_UpNegative(
            [Values(-23478.353f, -23478.353f, -23478.353f, -23478.353f, -23478.353f, -23478.353f, -23478.353f, -23478.353f, -23478.353f, -23478.353f)] float number,
            [Values(0, 1, 2, 3, 4, 5, 6, 7, 8, 9)] int digit,
            [Values(-23478.353f, -30000f, -24000f, -23500f, -23480f, -23479f, -23478.4f, -23478.36f, -23478.353f, -23478.353f)] float expected)
        {
            float actual = MathHelper.RoundAbs(number, digit, MathHelper.RoundStyle.AlwaysUp);
            Assert.IsTrue(Mathf.Approximately(expected, actual));
        }

        [Test, Sequential]
        public void RoundAbsDown_Positive(
            [Values(23478.353f, 23478.353f, 23478.353f, 23478.353f, 23478.353f, 23478.353f, 23478.353f, 23478.353f, 23478.353f, 23478.353f)] float number,
            [Values(0, 1, 2, 3, 4, 5, 6, 7, 8, 9)] int digit,
            [Values(23478.353f, 20000f, 23000f, 23400f, 23470f, 23478f, 23478.3f, 23478.35f, 23478.353f, 23478.353f)] float expected)
        {
            float actual = MathHelper.RoundAbs(number, digit, MathHelper.RoundStyle.AlwaysDown);
            Assert.IsTrue(Mathf.Approximately(expected, actual));
        }

        [Test, Sequential]
        public void RoundAbs_DownNegative(
            [Values(-23478.353f, -23478.353f, -23478.353f, -23478.353f, -23478.353f, -23478.353f, -23478.353f, -23478.353f, -23478.353f, -23478.353f)] float number,
            [Values(0, 1, 2, 3, 4, 5, 6, 7, 8, 9)] int digit,
            [Values(-23478.353f, -20000f, -23000f, -23400f, -23470f, -23478f, -23478.3f, -23478.35f, -23478.353f, -23478.353f)] float expected)
        {
            float actual = MathHelper.RoundAbs(number, digit, MathHelper.RoundStyle.AlwaysDown);
            Assert.IsTrue(Mathf.Approximately(expected, actual));
        }

        [Test, Sequential]
        public void RoundUpAbsBelowError(
            [Values(23478.353f, 23478.353f, 23478.353f, 23478.353f, 23478.353f, 23478.353f, 23478.353f, 23478.353f, 23478.353f, 23478.353f)] float number,
            [Values(0.0f, 0.00000001f, 0.0000001f, 0.000001f, 0.00001f, 0.00005f, 0.0001f, 0.001f, 0.025f, 0.3f)] float error,
            [Values(23478.353f, 23478.353f, 23478.353f, 23478.36f, 23478.4f, 23479f, 23480f, 23500f, 24000f, 30000f)] float expected)
        {
            float actual = MathHelper.RoundAbsBelowError(number, error, MathHelper.RoundStyle.AlwaysUp).rounded;
            Assert.AreEqual(expected, actual, 0.01f);
        }

        [Test, Sequential]
        public void GetAxisRounded_MinConstraint(
            [Values(400.0f, 400.0f, 800.0f, 800.0f, 2000.0f, 2000.0f)] float axisPixelLength,
            [Values(0.0f, 25.0f, 2635.2f, 12.3888f, 526.0f, 923.1f)] float minValue,
            [Values(10.0f, 533.0f, 8992.999f, 14.0f, 1123.3f, 925.963f)] float maxValue)
        {
            var result = MathHelper.GetAxisRounded(axisPixelLength, minValue, maxValue, 16.0f, 100.0f, 0.15f);

            /*
            Debug.Log("max: " + result.maxRounded);
            Debug.Log("min: " + result.minRounded);
            Debug.Log("inc: " + result.incr);
            Debug.Log("div: " + result.divisions);
            Debug.Log("spx: " + result.pixelSpacing);
            */

            Assert.LessOrEqual(result.minRounded, minValue);
        }

        [Test, Sequential]
        public void GetAxisRounded_MaxConstraint(
            [Values(400.0f, 400.0f, 800.0f, 800.0f, 2000.0f, 2000.0f)] float axisPixelLength,
            [Values(0.0f, 25.0f, 2635.2f, 12.3888f, 526.0f, 923.1f)] float minValue,
            [Values(10.0f, 533.0f, 8992.999f, 14.0f, 1123.3f, 925.963f)] float maxValue)
        {
            var result = MathHelper.GetAxisRounded(axisPixelLength, minValue, maxValue, 16.0f, 100.0f, 0.15f);
            Assert.GreaterOrEqual(result.maxRounded, maxValue);
        }

        [Test, Sequential]
        public void GetAxisRounded_AxisLengthError(
            [Values(400.0f, 400.0f, 800.0f, 800.0f, 2000.0f, 2000.0f)] float axisPixelLength,
            [Values(0.0f, 25.0f, 2635.2f, 12.3888f, 526.0f, 923.1f)] float minValue,
            [Values(10.0f, 533.0f, 8992.999f, 14.0f, 1123.3f, 925.963f)] float maxValue)
        {
            const float errExpected = 0.15f;
            var result = MathHelper.GetAxisRounded(axisPixelLength, minValue, maxValue, 16.0f, 100.0f, errExpected);
            float errActual = MathHelper.GetPercentErr(maxValue - minValue, result.maxRounded - result.minRounded);

            Assert.LessOrEqual(errActual, errExpected);
        }
    }
}
