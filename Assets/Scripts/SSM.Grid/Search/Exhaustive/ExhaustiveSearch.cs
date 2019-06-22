using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static SSM.Grid.MGHelper;

namespace SSM.Grid.Search.Exhaustive
{
    [Serializable]
    public class ExhSearch
    {
        public Options options;

        public ExhSearch(Options options)
        {
            this.options = options;
        }

        public int[,] UCPSearch(
            MGInput m, 
            float[] p_target,
            Action<float> progressCallback)
        {
            // Exhaustive Search is very expensive so we use a coarser step.
            // But we'll need to translate from the coarse step to the
            // orginal step so we'll store the translation coefficient
            // in stepSize.
            int stepCount = options.stepCount;
            int stepSize = m.tCount / stepCount;
            int taskCount = options.taskCount;

            // Get the total number of possible combinations.
            long nCombinations = (long)1 << (m.genCount * stepCount);

            // Out fitness variable.
            float lowestCost = Mathf.Infinity;

            // Pre-calculate fuel costs at max power because we'll use them
            // later to calculate a presumptive total cost.
            float[] cost_thr_at_max = ThermalGenerator.GetCostAtMaxPower(
                m.genCount, m.p_thr_max, m.thr_c_a, m.thr_c_b, m.thr_c_c, 60.0f);

            // We only need to verify min uptime/downtime constraints if our
            // step size is smaller than the the biggest min u/dtime. Otherwise
            // our step is large enough that the algorithm will never be in a
            // situation where it's trying to switch a generator that can't be
            // switched due to min downtime/uptime constraints.
            bool[] isValidationNeeded = IsMinTimeVerificationNeeded(
                m.genCount, stepSize, m.thr_min_dtime, m.thr_min_utime);

            // t1 and t2 define the interval of time this step occupies.
            var stepInterval = new Tuple<int, int>[stepCount];
            for (int iStep = 0; iStep < stepCount; iStep++)
            {
                stepInterval[iStep] = Tuple.Create(
                    iStep * stepSize, 
                    (iStep + 1) * stepSize - 1);
            }

            // It's expensive to compute the cost of the amount of power the
            // microgrid needs to purchase inside the main loop. There are only
            // 2^n * stepCount combinations possible which end up being
            // redundantly recalculated a lot if done in the main loop.
            // So we compute all of them ahead of time and store them in a
            // dictionary.
            Dictionary<StateStep, float> purchaseCostDict
                = ConstructCostDictionary(m, stepCount, stepSize, 400, p_target);

            // Running the search in threads makes pruning high cost
            // combinations happen earlier and more often because a low cost
            // variant is discovered earlier on.
            long bestComb = -1;
            var tasks = new Task[taskCount];
            long combSegmentSize = nCombinations / taskCount;

            var taskParams = new TaskParams(
                p_target, 
                stepCount,
                stepSize, 
                stepInterval, 
                isValidationNeeded,
                cost_thr_at_max,
                purchaseCostDict);

            var taskProgress = new float[taskCount];

            void progressCallbackInner(int id, float completion)
            {
                taskProgress[id] = completion;
                float totalProgress = 0;
                for (int i = 0; i < taskProgress.Length; i++)
                {
                    totalProgress += taskProgress[i];
                }
                progressCallback(totalProgress / taskCount);
            }

            for (int iTask = 0; iTask < taskCount; iTask++)
            {
                taskProgress[iTask] = 0.0f;
                long searchStart = iTask * combSegmentSize;
                long searchEnd = (iTask + 1) * combSegmentSize;
                int taskID = iTask;
                var task = Task.Run(() => SearchTask(
                    m,
                    searchStart, 
                    searchEnd, 
                    ref bestComb, 
                    ref lowestCost,
                    taskParams, 
                    progressCallbackInner, 
                    taskID));
                tasks[iTask] = task;
            }

            var waiter = Task.WhenAll(tasks.ToArray());
            waiter.Wait();

            // Write out the step based state array
            int[,] u_thr_best_step = new int[m.genCount, m.tCount];
            for (int iStep = 0; iStep < stepCount; iStep++)
            {
                for (int iGen = m.genCount - 1; iGen >= 0; iGen--)
                {
                    u_thr_best_step[iGen, iStep] = (int)(bestComb % 2);
                    bestComb /= 2;
                }
            }

            // Translate step-based state array to original time based
            // state array.
            int[,] u_thr_best = new int[m.genCount, m.tCount];
            for (int t = 0; t < m.tCount; t++)
            {
                int currStep = t / stepSize;
                for (int iGen = 0; iGen < m.genCount; iGen++)
                {
                    u_thr_best[iGen, t] = u_thr_best_step[iGen, currStep];
                }
            }

            return u_thr_best;
        }

        private static void SearchTask(MGInput m,
            long CombStart, 
            long CombEnd,
            ref long globalBestComb, 
            ref float globalLowestCost,
            TaskParams p, 
            Action<int, float> progressCallback, 
            int taskID)
        {
            // An array we use to store a candidate solution.
            int[,] u_thr_candidate = new int[m.genCount, p.stepCount];

            int[] states = new int[m.genCount];

            long localBestComb;
            float localLowestCost = Mathf.Infinity;

            float[] dtime_norm = new float[m.genCount];
            float[] utime_norm = new float[m.genCount];
            for (int iGen = 0; iGen < m.genCount; iGen++)
            {
                dtime_norm[iGen] = m.thr_min_dtime[iGen] / p.stepSize;
                utime_norm[iGen] = m.thr_min_utime[iGen] / p.stepSize;
            }

            for (long iComb = CombStart; iComb < CombEnd; iComb++)
            {
                long currComb = iComb;
                float c_total = 0.0f;
                // 1 1 1 1 0 1 1 1 0 0 0 1 1 0 0 1
                // We assume the state is valid until proven otherwise.
                bool isStateValid = true;

                for (int iStep = 0; iStep < p.stepCount; iStep++)
                {
                    float c_thr_sum_step = 0.0f;

                    currComb = IntToBinary(currComb, m.genCount, states);

                    for (int ithr = 0; ithr < m.genCount; ithr++)
                    {
                        // Get the binary digit representing the on/off sate
                        // for our current thr and step.
                        u_thr_candidate[ithr, iStep] = states[ithr];

                        if (!isStateValid)
                        {
                            break;
                        }

                        // The total cost incurred by the thrs over the time
                        // interval of the step.
                        c_thr_sum_step += states[ithr] * p.cost_thr_at_max[ithr]
                            * p.stepSize;

                        // The total power the thrs can give at any t moment
                        // during the step. It's constant throughout the step
                        // because the state of units is constant throughout
                        // the time interval of the step.

                    } // END thr

                    int ii = BinaryToInt(states);
                    var id = new StateStep(ii, iStep);
       

                    c_total += c_thr_sum_step + p.purchaseDict[id];

                    // Pruning. If the total cost is already higher than the
                    // lowest cost then there's no way this combination can
                    // be the optimal one, so we abandon verifying it and break.
                    if (c_total > globalLowestCost)
                    {
                        break;
                    }
                } // END Step

                for (int ithr = 0; ithr < m.genCount; ithr++)
                {
                    if (!MGHelper.IsStateValid(GetRow(u_thr_candidate, ithr),
                        dtime_norm[ithr], utime_norm[ithr], p.stepCount))
                    {
                        isStateValid = false;
                        break;
                    }
                }

                if (isStateValid)
                {
                    if (c_total < localLowestCost)
                    {
                        progressCallback(taskID,
                            (float)((double)(iComb - CombStart)
                            / (double)(CombEnd - CombStart)));
                        localLowestCost = c_total;
                        localBestComb = iComb;

                        if (localLowestCost < Volatile.Read(ref globalLowestCost))
                        {
                            Interlocked.Exchange(ref globalLowestCost, localLowestCost);
                            Interlocked.Exchange(ref globalBestComb, localBestComb);
                        }
                    }
                }

            } // END Combination
        }
    }
}