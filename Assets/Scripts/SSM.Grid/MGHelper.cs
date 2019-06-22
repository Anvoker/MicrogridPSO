using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace SSM.Grid
{
    public static class MGHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int[,] Int1DToBin2D(
            int[] intArr,
            int stepCount,
            int genCount)
        {
            var bin2Darr = new int[stepCount, genCount];
            for (int iStep = 0; iStep < stepCount; iStep++)
            {
                for (int iGen = 0; iGen < genCount; iGen++)
                {
                    var bitValue = intArr[iStep] & (1 << genCount - iGen - 1);
                    var state = ((bitValue | (~bitValue + 1)) >> 31) & 1;
                    bin2Darr[iStep, iGen] = state;
                }
            }

            return bin2Darr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int[,] TranslateTimeStep(
            int[,] arr, 
            float stepSize,
            int timeCount, 
            int genCount)
        {
            int[,] arrNew = new int[timeCount, genCount];
 
            for (int iT = 0; iT < timeCount; iT++)
            {
                for (int iGen = 0; iGen < genCount; iGen++)
                {
                    int iStep = Mathf.FloorToInt(iT / stepSize);
                    arrNew[iT, iGen] = arr[iStep, iGen];
                }
            }

            return arrNew;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CantorPairing(int a, int b) 
            => (a + b) * (a + b + 1) / 2 + a;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int[] IntToBinary(int comb, int genCount)
        {
            int[] result = new int[genCount];
            int nTemp = comb;

            for (int i = genCount - 1; i >= 0; i--)
            {
                result[i] = nTemp & 1;
                nTemp >>= 1;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IntToBinary(int comb, int genCount, int[] arr)
        {
            for (int i = genCount - 1; i >= 0; i--)
            {
                arr[i] = comb & 1;
                comb >>= 1;
            }

            return comb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long IntToBinary(long comb, int genCount, int[] arr)
        {
            for (int i = genCount - 1; i >= 0; i--)
            {
                arr[i] = (int)(comb & 1);
                comb >>= 1;
            }

            return comb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinaryToInt(int[] binArr)
        {
            int genCount = binArr.Length;
            int n = 0;
            for (int iGen = 0; iGen < genCount; iGen++)
            {
                n += binArr[iGen] & 1 << (genCount - iGen - 1);
            }

            return n;
        }

        /// <summary>
        /// Helper methods for deriving useful values from thermal generators.
        /// </summary>
        public static class ThermalGenerator
        {
            /// <summary>
            /// Gets the cost of running a generator at maximum power for one
            /// hour.
            /// </summary>
            /// <param name="p">Maximum output power.</param>
            /// <param name="a">First cost equation coefficient. EUR/MW^2</param>
            /// <param name="b">Second cost equation coefficient. EUR/MW</param>
            /// <param name="c">Third cost equation coefficient. EUR</param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float GetCost(float p, float a, float b, float c) 
                => (a * p * p) + (b * p) + c;

            /// <summary>
            /// Gets the cost of running each generator in the given array
            /// for 1 minute at maximum power.
            /// </summary>
            /// <param name="genCount">Number of generators.</param>
            /// <param name="p_thr_max">Maximum output power for each generator.</param>
            /// <param name="a">First cost equation coefficient for each generator.</param>
            /// <param name="b">Second cost equation coefficient for each generator.</param>
            /// <param name="c">Third cost equation coefficient for each generator.</param>
            /// <returns>An array of costs.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float[] GetCostAtMaxPower(
                int genCount,
                float[] p_thr_max, 
                float[] a, 
                float[] b, 
                float[] c,
                float t)
            {
                float[] cost_thr_at_max = new float[genCount];
                for (int iGen = 0; iGen < genCount; iGen++)
                {
                    cost_thr_at_max[iGen] = GetCost(
                        p_thr_max[iGen],
                        a[iGen], 
                        b[iGen],
                        c[iGen]) / t;
                }

                return cost_thr_at_max;
            }

            /// <summary>
            /// Gets alpha for multiple generators. Alpha is the cost of of 
            /// producing one unit of power when running at the maximum capacity 
            /// of the generator.
            /// </summary>
            /// <param name="genCount">Number of generators.</param>
            /// <param name="p_thr_max">Maximum output power for each generator.</param>
            /// <param name="a">First cost equation coefficient for each generator.</param>
            /// <param name="b">Second cost equation coefficient for each generator.</param>
            /// <param name="c">Third cost equation coefficient for each generator.</param>
            /// <returns>An array of alpha coefficients.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float[] GetAlpha(
                int genCount,
                float[] p_thr_max,
                float[] a,
                float[] b,
                float[] c)
            {
                float[] alpha = new float[genCount];
                for (int iGen = 0; iGen < genCount; iGen++)
                {
                    var maxPower = p_thr_max[iGen];
                    var costAtMaxPower = GetCost(maxPower, a[iGen], b[iGen], c[iGen]);
                    alpha[iGen] = costAtMaxPower / maxPower;
                }

                return alpha;
            }
        }


        private static void UnsafeCopy(int[] arr)
        {
            unsafe
            {
                fixed (int* p = &arr[0])
                {
                    uint length = (uint)(arr.Length * sizeof(int));
                    Unsafe.InitBlock(p, 0, length);
                }
            }
        }

        /// <summary>
        /// Ascertains which of the generators need to have their state 
        /// switches verified. When an algorithm decides to change the state of
        /// a generator, this might violate minimum time constraints and the
        /// change would need to be reverted. If a generator has a minimum up 
        /// time or minimum down time smaller than our time step size, then 
        /// we never need to revert the change and therefore don't need to
        /// verify that the state switches are legal.
        /// </summary>
        /// <param name="genCount">Number of generators.</param>
        /// <param name="stepSize">Size of our time step.</param>
        /// <param name="thr_min_dtime">Minimum down time for each generator.</param>
        /// <param name="thr_min_utime">Minimum up time for each generator.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool[] IsMinTimeVerificationNeeded(
            int genCount,
            float stepSize, 
            float[] thr_min_dtime, 
            float[] thr_min_utime)
        {
            bool[] isVerificationNeeded = new bool[genCount];

            for (int iGen = 0; iGen < genCount; iGen++)
            {
                isVerificationNeeded[iGen] =
                        thr_min_dtime[iGen] > stepSize
                     || thr_min_utime[iGen] > stepSize;
            }

            return isVerificationNeeded;
        }

        public static T[] GetCol<T>(T[,] source2DArray, int columnToGet)
        {
            int d1 = source2DArray.GetLength(0);
            int d2 = source2DArray.GetLength(1);

            if (columnToGet >= d2 || columnToGet < 0)
            {
                throw new ArgumentException("Trying to get column index" +
                    "(" + columnToGet + ") which is out of bounds. Source " +
                    "array column length: " + d2);
            }

            var target1DArray = new T[d1];

            for (int i = 0; i < d1; i++)
            {
                target1DArray[i] = source2DArray[i, columnToGet];
            }

            return target1DArray;
        }

        /// <summary>
        /// Precomputes the cost of each possible set of generator states at 
        /// every time step.
        /// </summary>
        /// <param name="m">Microgrid input data.</param>
        /// <param name="genCount">Number of generators.</param>
        /// <param name="stepCount">The number of time steps.</param>
        /// <param name="stepSize">The size of each time step.</param>
        /// <param name="p_target">The amount of power the system needs to supply at a given time step.</param>
        /// <returns>A dictionary mapping a generator state and time step pair 
        /// to the cost of purchasing the amount of power necessary to fulfill 
        /// remaining demand for that specific pair.</returns>
        public static Dictionary<StateStep, float> ConstructCostDictionary(
            MGInput m,
            int stepCount,
            int stepSize,
            float penalty,
            float[] p_target)
        {
            // It's expensive to compute the cost of the amount of power the
            // microgrid needs to purchase inside the main loop. There are only
            // 2^n * stepCount combinations possible which end up being
            // redundantly recalculated a lot if done in the main loop.
            // So we compute all of them ahead of time and store them in a
            // dictionary.

            // t1 and t2 define the interval of time this step occupies.
            var stepInterval = new Tuple<int, int>[stepCount];
            for (int iStep = 0; iStep < stepCount; iStep++)
            {
                stepInterval[iStep] = new Tuple<int, int>(
                    iStep * stepSize,
                    (iStep + 1) * stepSize - 1);
            }

            int nSqr = 1 << m.genCount; //Bitwise squaring.
            int nPurchaseCombs   = nSqr * stepCount;
            var purchaseCostDict = new Dictionary<StateStep, float>(nPurchaseCombs);
            var indexToCost      = new Dictionary<int, float>(m.genCount);
            float[] p_thr        = new float[m.genCount];

            for (int i = 0; i < m.genCount; i++)
            {
                // EUR for 1 hour of functioning
                float cost = ThermalGenerator.GetCost(
                    m.p_thr_max[i],
                    m.thr_c_a[i],
                    m.thr_c_b[i],
                    m.thr_c_c[i]);

                // EUR/MWh
                float alpha = cost / m.p_thr_max[i];
                indexToCost.Add(i, alpha);
            }

            float[] e_thr_remaining = new float[stepSize];
            float[] p_remaining     = new float[stepSize];
            var orderedByCost = indexToCost.OrderBy(x => x.Value).ToDictionary(x => x.Key, y => y.Value);

            for (int iState = 0; iState < nSqr; iState++)
            {
                for (int iStep = 0; iStep < stepCount; iStep++)
                {
                    float c_total = 0.0f;
                    int t1 = stepInterval[iStep].Item1;
                    int t2 = stepInterval[iStep].Item2;

                    for (int t = t1; t < t2; t++)
                    {
                        e_thr_remaining[t - t1] = p_target[t] / 60.0f;
                        p_remaining[t - t1]     = p_target[t];

                        foreach (KeyValuePair<int, float> kvp in orderedByCost)
                        {
                            int iGen     = kvp.Key;
                            float alpha  = kvp.Value;
                            var bitValue = iState & (1 << m.genCount - iGen - 1);
                            var state    = ((bitValue | (~bitValue + 1)) >> 31) & 1;

                            if (state == 1)
                            {
                                if (alpha < m.price[t])
                                {
                                    if (m.canSell)
                                    {
                                        // Since we can sell and our generator 
                                        // makes cheaper energy than the market price,
                                        // just produce at max power.
                                        p_thr[iGen] = m.p_thr_max[iGen];
                                    }
                                    else
                                    {
                                        // Since we can't sell but our generator
                                        // makes cheaper energy than market price,
                                        // we'll produce enough to cover local demand.
                                        p_thr[iGen] = Mathf.Clamp(m.p_thr_max[iGen], 0.0f, p_remaining[t - t1]);
                                    }
                                }
                                else
                                {
                                    if (m.canBuy)
                                    {
                                        // Since we can buy and our generator 
                                        // makes more expensive energy than 
                                        // the market price, produce no power
                                        // and buy.
                                        p_thr[iGen] = 0.0f;
                                    }
                                    else
                                    {
                                        // Since we can't buy, produce enough
                                        // power to cover local demand, regardless
                                        // of market price.
                                        p_thr[iGen] = Mathf.Clamp(m.p_thr_max[iGen], 0.0f, p_remaining[t - t1]);
                                    }
                                }

                                c_total += ThermalGenerator.GetCost(
                                    p_thr[iGen],
                                    m.thr_c_a[iGen],
                                    m.thr_c_b[iGen],
                                    m.thr_c_c[iGen]) / 60.0f;
                                p_remaining[t - t1]     -= p_thr[iGen];
                                e_thr_remaining[t - t1] -= p_thr[iGen] / 60.0f;
                            }
                            else
                            {
                                p_thr[iGen] = 0.0f;
                            }
                        }

                        if (e_thr_remaining[t - t1] < 0.0f)
                        {
                            if (m.canSell)
                            {
                                c_total += e_thr_remaining[t - t1] * m.price[t];
                            }
                        }
                        else
                        {
                            if (m.canBuy)
                            {
                                c_total += e_thr_remaining[t - t1] * m.price[t];
                            }
                            else
                            {
                                c_total += e_thr_remaining[t - t1] * penalty;
                            }
                        }
                    }

                    var id = new StateStep(iState, iStep);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    if (purchaseCostDict.ContainsKey(id)
                        && purchaseCostDict[id] != c_total)
                    {
                        throw new InvalidProgramException(
                            $@"IDs and purchase costs need to have a 1 to 1
                            mapping. There can't be an ID with a different
                            purchase cost. If that happens, the code is wrong
                            somewhere.
                            UID: {id}.
                            Existing Value: {purchaseCostDict[id]}.
                            New Value: {c_total}.");
                    }
#endif

                    purchaseCostDict[id] = c_total;
                }
            }

            return purchaseCostDict;
        }

        public static T[,] SetRow<T>(IList<T> source, T[,] dest2DArray, int iRow)
        {
            if (source.Count != dest2DArray.GetLength(0))
            {
                throw new ArgumentException("Source and destination arrays "
                    + "need to have the same length. Source has "
                    + source.Count + " and destination has "
                    + dest2DArray.GetLength(0) + ".");
            }

            if (iRow < 0 || iRow >= dest2DArray.GetLength(1))
            {
                throw new IndexOutOfRangeException("Row index is negative "
                    + "or higher than the amount of columns the destination "
                    + "array has.");
            }

            for (int i = 0; i < source.Count; i++)
            {
                dest2DArray[iRow, i] = source[i];
            }

            return dest2DArray;
        }

        public static T[,] SetCol<T>(IList<T> source, T[,] dest2DArray, int iCol)
        {
            if (source.Count != dest2DArray.GetLength(0))
            {
                throw new ArgumentException("Source and destination arrays "
                    + "need to have the same length. Source has "
                    + source.Count + " and destination has "
                    + dest2DArray.GetLength(0) + ".");
            }

            if (iCol < 0 || iCol >= dest2DArray.GetLength(1))
            {
                throw new IndexOutOfRangeException("Column index is negative "
                    + "or higher than the amount of columns the destination "
                    + "array has.");
            }

            for (int i = 0; i < source.Count; i++)
            {
                dest2DArray[i, iCol] = source[i];
            }

            return dest2DArray;
        }

        public static T[] GetRow<T>(T[,] source2DArray, int rowIndex)
        {
            int d1 = source2DArray.GetLength(0);
            int d2 = source2DArray.GetLength(1);

            if (rowIndex >= d1 || rowIndex < 0)
            {
                throw new ArgumentException("Trying to get row index" +
                    "(" + rowIndex + ") which is out of bounds. Source " +
                    "array row length: " + d1);
            }

            int byteSize = System.Runtime.InteropServices.Marshal.SizeOf<T>();
            var target1DArray = new T[d2];

            Buffer.BlockCopy(
                src:       source2DArray, 
                srcOffset: byteSize * d2 * rowIndex,
                dst:       target1DArray, 
                dstOffset: 0,
                count:     byteSize * d2);

            return target1DArray;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SumStatesValue(float[] stateValue, int[] state)
        {
            float value = 0.0f;
            for (int i = 0; i < state.Length; i++)
            {
                value += state[i] * stateValue[i];
            }
            return value;
        }

        /// <summary>
        /// Reverts illegal generator state switches that violate minimum
        /// downtime or minimum uptime constraints.
        /// </summary>
        /// <param name="p">On/Off state of the generator at each time step.</param>
        /// <param name="stepCount">Number of time steps.</param>
        /// <param name="dtime_norm">Minimum downtime divided by the size of the time step.</param>
        /// <param name="utime_norm">Minimum uptime divided by the size of the time step.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FixMinTimeConstraints(
            int[] p, 
            int stepCount,
            int genCount, 
            float[] dtime_norm, 
            float[] utime_norm)
        {

            var dwCount = new int[genCount];
            var upCount = new int[genCount];

            for (int iGen = 0; iGen < genCount; iGen++)
            {
                var bitValue = p[0] & (1 << genCount - iGen - 1);
                var state = ((bitValue | (~bitValue + 1)) >> 31) & 1;
                if (state == 0)
                {
                    dwCount[iGen]++;
                }
                else
                {
                    upCount[iGen]++;
                }
            }

            for (int iStep = 1; iStep < stepCount; iStep++)
            {
                for (int iGen = 0; iGen < genCount; iGen++)
                {
                    var bitPrev = p[iStep-1] & (1 << genCount - iGen - 1);
                    var statePrev = ((bitPrev | (~bitPrev + 1)) >> 31) & 1;

                    var bitCurr = p[iStep] & (1 << genCount - iGen - 1);
                    var stateCurr = ((bitCurr | (~bitCurr + 1)) >> 31) & 1;

                    if (stateCurr == 1)
                    {
                        // Trying to turn generator off.
                        if (statePrev == 0)
                        {
                            if (dwCount[iGen] < dtime_norm[iGen])
                            {
                                // Turn off to fix constraint.
                                p[iStep] &= ~(1 << (genCount - iGen - 1));
                                upCount[iGen] = 0;
                                dwCount[iGen]++;
                            }
                            else
                            {
                                dwCount[iGen] = 0;
                                upCount[iGen]++;
                            }
                        }
                        // Generator is and has been on.
                        else
                        {
                            dwCount[iGen] = 0;
                            upCount[iGen]++;
                        }
                    }
                    else
                    {
                        // Trying to turn generator off.
                        if (statePrev == 1)
                        {
                            if (upCount[iGen] < utime_norm[iGen])
                            {
                                // Turn on to fix constraint.
                                p[iStep] |= 1 << (genCount - iGen - 1);
                                dwCount[iGen] = 0;
                                upCount[iGen]++;
                            }
                            else
                            {
                                upCount[iGen] = 0;
                                dwCount[iGen]++;
                            }
                        }
                        // Generator is off and has been off.
                        else
                        {
                            upCount[iGen] = 0;
                            dwCount[iGen]++;
                        }
                    }
                } // END iGen
            } // END iStep
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsStateValid(
            int[] u_thr, 
            float minDTimeNorm,
            float minUTimeNorm, 
            int stepCount)
        {
            if (minDTimeNorm < 1.0f && minUTimeNorm < 1.0f)
            {
                return true;
            }

            int dCount = 0;
            int uCount = 0;

            if (u_thr[0] == 0)
            {
                dCount++;
            }
            else
            {
                uCount++;
            }

            for (int i = 1; i < stepCount; i++)
            {
                if (u_thr[i] == 1)
                {
                    if (u_thr[i - 1] == 0)
                    {
                        if (dCount < minDTimeNorm)
                        {
                            return false;
                        }
                    }

                    dCount = 0;
                    uCount++;
                }
                else
                {
                    if (u_thr[i - 1] == 1)
                    {
                        if (uCount < minUTimeNorm)
                        {
                            return false;
                        }
                    }

                    uCount = 0;
                    dCount++;
                }
            }

            return true;
        }

        public static float GetUDTime(
            int[,] u_thr,
            int iGen, 
            int currentT, 
            float tIncr, 
            bool uptime)
        {
            if (currentT <= 0)
            {
                return 0.0f;
            }

            int state = Convert.ToInt32(uptime);
            int t = currentT - 1;
            float udtime = 0.0f;

            while (t >= 0 && u_thr[iGen, t] == state)
            {
                udtime += tIncr;
                t--;
            }

            return udtime;
        }
    }
}