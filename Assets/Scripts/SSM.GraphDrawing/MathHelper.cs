using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace SSM.GraphDrawing
{
    public static class MathHelper
    {
        public enum RoundStyle
        {
            AlwaysUp,
            AlwaysDown,
            Closest
        }

        public struct AxisResult
        {
            public float minRounded;
            public float maxRounded;
            public float incr;
            public int divisions;
            public float pixelSpacing;
        }

        public struct RoundResult
        {
            public float rounded;
            public int digit;
            public float percentErr;
        }

        public static float NormalizeToRange(float m, float mMin, float mMax, float rangeMin, float rangeMax)
        {
            float mDelta = mMax - mMin;
            float rangeDelta = rangeMax - rangeMin;
            return (m - mMin) / mDelta * rangeDelta + rangeMin;
        }

        public static Vector2 NormalizeToRange(Vector2 m, Vector2 mMin, Vector2 mMax, Vector2 rangeMin, Vector2 rangeMax)
        {
            var mDelta = mMax - mMin;
            var rangeDelta = rangeMax - rangeMin;
            return new Vector2(
                (m.x - mMin.x) / mDelta.x * rangeDelta.x + rangeMin.x,
                (m.y - mMin.y) / mDelta.y * rangeDelta.y + rangeMin.y);
        }

        public static void NormalizeToRange(IList<Vector2> m, IList<Vector2> output, Vector2 mMin, Vector2 mMax, Vector2 rangeMin, Vector2 rangeMax)
        {
            var mDelta = mMax - mMin;
            var rangeDelta = rangeMax - rangeMin;

            for (int i = 0; i < m.Count; i++)
            {
                output[i] = new Vector2(
                    (m[i].x - mMin.x) / mDelta.x * rangeDelta.x + rangeMin.x,
                    (m[i].y - mMin.y) / mDelta.y * rangeDelta.y + rangeMin.y);
            }
        }

        public static Vector2 DenormalizeToRange(Vector2 n, Vector2 mMin, Vector2 mMax, Vector2 rangeMin, Vector2 rangeMax)
        {
            var mDelta = mMax - mMin;
            var rangeDelta = rangeMax - rangeMin;
            return new Vector2(
                (n.x - rangeMin.x) / rangeDelta.x * mDelta.x + mMin.x,
                (n.y - rangeMin.y) / rangeDelta.y * mDelta.y + mMin.y);
        }

        public static float DenormalizeToRange(float n, float mMin, float mMax, float rangeMin, float rangeMax)
        {
            var mDelta = mMax - mMin;
            var rangeDelta = rangeMax - rangeMin;
            return (n - rangeMin) / rangeDelta * mDelta + mMin;
        }

        public static AxisResult GetAxisRounded(float axisPxLength,
            float lowestVal,
            float highestVal,
            float minPxSpacing,
            float maxPxSpacing,
            float errPercent,
            bool isMinFixed = false)
        {
            float valueLength = highestVal - lowestVal;
            float absError = valueLength * errPercent;

            float lowestValRelErr  = GetPercentErr(lowestVal,
                lowestVal + (absError / 2.0f));
            float highestValRelErr = GetPercentErr(highestVal,
                highestVal + (absError / 2.0f));

            float minRounded;
            if (!isMinFixed)
            {
                minRounded = lowestVal >= 0.0f
                    ? RoundAbsBelowError(lowestVal, lowestValRelErr,
                        RoundStyle.AlwaysDown).rounded
                    : RoundAbsBelowError(lowestVal, lowestValRelErr,
                        RoundStyle.AlwaysUp).rounded;
            }
            else
            {
                minRounded = lowestVal;
            }

            int maxNDiv = Mathf
                .FloorToInt(axisPxLength * (1.0f + errPercent) / minPxSpacing);
            int minNDiv = Mathf
                .FloorToInt(axisPxLength / maxPxSpacing);

            float valueLengthRounded = Mathf.Abs(highestVal - minRounded);

            int divBest = 0;
            float errBest = Mathf.Infinity;
            float incrBest = 0.0f;

            for (int i = minNDiv; i < maxNDiv; i++)
            {
                float incr = valueLengthRounded / i;
                float incrRounded = highestVal >= 0.0f
                    ? RoundAbsBelowError(incr, highestValRelErr,
                        RoundStyle.AlwaysUp).rounded
                    : RoundAbsBelowError(incr, highestValRelErr,
                        RoundStyle.AlwaysDown).rounded;
                float maxRounded = minRounded + (incrRounded * i);
                float err = GetPercentErr(valueLength, maxRounded - minRounded);

                if (err < errBest)
                {
                    divBest = i;
                    errBest = err;
                    incrBest = incrRounded;
                }
            }

            return new AxisResult
            {
                minRounded = minRounded,
                maxRounded = minRounded + (incrBest * divBest),
                divisions = divBest,
                incr = incrBest,
                pixelSpacing = axisPxLength / divBest
            };
        }

        public static AxisResult GetAxisRounded2(float axisPxLength,
            float lowestVal,
            float highestVal,
            int minDivisions,
            int maxDivisions,
            float errPercent,
            bool isMinFixed = false)
        {
            float valueLength = highestVal - lowestVal;
            float absError = valueLength * errPercent;

            float lowestValRelErr = GetPercentErr(lowestVal,
                lowestVal + (absError / 2.0f));
            float highestValRelErr = GetPercentErr(highestVal,
                highestVal + (absError / 2.0f));

            float minRounded;
            if (!isMinFixed)
            {
                minRounded = lowestVal >= 0.0f
                    ? RoundAbsBelowError(lowestVal, lowestValRelErr,
                        RoundStyle.AlwaysDown).rounded
                    : RoundAbsBelowError(lowestVal, lowestValRelErr,
                        RoundStyle.AlwaysUp).rounded;
            }
            else
            {
                minRounded = lowestVal;
            }

            float valueLengthRounded = Mathf.Abs(highestVal - minRounded);

            int divBest = 0;
            float errBest = Mathf.Infinity;
            float incrBest = 0.0f;

            for (int i = minDivisions; i < maxDivisions; i++)
            {
                float incr = valueLengthRounded / i;
                float incrRounded = highestVal >= 0.0f
                    ? RoundAbsBelowError(incr, highestValRelErr,
                        RoundStyle.AlwaysUp).rounded
                    : RoundAbsBelowError(incr, highestValRelErr,
                        RoundStyle.AlwaysDown).rounded;
                float maxRounded = minRounded + (incrRounded * i);
                float err = GetPercentErr(valueLength, maxRounded - minRounded);

                if (err < errBest)
                {
                    divBest = i;
                    errBest = err;
                    incrBest = incrRounded;
                }
            }

            return new AxisResult
            {
                minRounded = minRounded,
                maxRounded = minRounded + (incrBest * divBest),
                divisions = divBest,
                incr = incrBest,
                pixelSpacing = axisPxLength / divBest
            };
        }

        public static float GetPercentErr(float original, float approximation)
        {
            if (original == 0)
            {
                return 0.0f;
            }

            return Mathf.Abs((original - approximation) / original);
        }

        /// <summary>
        /// Rounds as much as possible while staying below the specified error.
        /// </summary>
        public static RoundResult RoundAbsBelowError(float f,
            float errPercent, RoundStyle rStyle, float rBase = 1.0f)
        {
            float rounded = f;
            float newErr = 0.0f;
            float previousErr = Mathf.Infinity;
            int digit = 0;

            do
            {
                digit++;
                previousErr = newErr;
                rounded = RoundAbs(f, digit, rStyle, rBase);
            }
            while (((newErr = GetPercentErr(f, rounded)) >= errPercent)
                 && (newErr != previousErr));

            return new RoundResult
            {
                rounded = rounded,
                digit = digit,
                percentErr = newErr
            };
        }

        public static int DigitCount(this float number)
        {
            number = Math.Abs(number);
            int length = 1;
            while ((number /= 10) >= 1)
            {
                length++;
            }

            return length;
        }

        public static int DigitCount(this double number)
        {
            number = Math.Abs(number);
            int length = 1;
            while ((number /= 10) >= 1)
            {
                length++;
            }

            return length;
        }

        public static float RoundAbs(float n, int digit, RoundStyle rStyle,
            float rBase = 1.0f)
        {
            if (digit == 0)
            {
                return n;
            }

            int sign = n < 0.0f ? -1 : 1;
            n *= sign;

            int digitCount = DigitCount(n);

            float mult = rBase * Mathf.Pow(10.0f, digit - digitCount);

            switch (rStyle)
            {
                case RoundStyle.AlwaysDown:
                    return sign * Mathf.Floor(n * mult) / mult;
                case RoundStyle.AlwaysUp:
                    return sign * Mathf.Ceil(n * mult) / mult;
                case RoundStyle.Closest:
                    return sign * Mathf.Round(n * mult) / mult;
                default:
                    throw new System.ComponentModel
                        .InvalidEnumArgumentException(nameof(rStyle),
                        (int)rStyle, rStyle.GetType());
            }
        }

        public static double RoundAbs(double n, int digit, RoundStyle rStyle,
            double rBase = 1.0)
        {
            if (digit == 0)
            {
                return n;
            }

            int sign = n < 0.0f ? -1 : 1;
            n *= sign;

            int digitCount = DigitCount(n);

            double mult = rBase * Math.Pow(10.0, digit - digitCount);

            switch (rStyle)
            {
                case RoundStyle.AlwaysDown:
                    return sign * Math.Floor(n * mult) / mult;
                case RoundStyle.AlwaysUp:
                    return sign * Math.Ceiling(n * mult) / mult;
                case RoundStyle.Closest:
                    return sign * Math.Round(n * mult) / mult;
                default:
                    throw new System.ComponentModel
                        .InvalidEnumArgumentException(nameof(rStyle),
                        (int)rStyle, rStyle.GetType());
            }
        }

        public static Vector2 PolarToCartesian(float angle, float value)
        {
            float x = value * Mathf.Cos(Mathf.Deg2Rad * angle);
            float y = value * Mathf.Sin(Mathf.Deg2Rad * angle);
            return new Vector2(x, y);
        }
    }
}