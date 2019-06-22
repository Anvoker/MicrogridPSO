using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.CompilerServices;

namespace SSM.Grid.Search.PSO
{
    /// <summary>
    /// Runs Particle Swarm Optimization on microgrid data in order to solve
    /// UCP (Unit Commitment Problem).
    /// </summary>
    [Serializable]
    public static class PSOSearch
    {
        public delegate void OnNewSnapshotCallback(
            int runIndex, 
            int iterationIndex, 
            int[] iterationLatestPerRun, 
            IterationSnapshot[,] snapshots);

        private delegate void OnNewSnapshotInnerCallback(
            int runIndex, 
            int iterationIndex, 
            IterationSnapshot snapshot);

        /// <summary>
        /// Executes a search on the provided microgrid data to generate
        /// optimal unit states for the formulated Unit Commitment Problem 
        /// (UCP).
        /// </summary>
        /// <param name="input">Microgrid input data.</param>
        /// <param name="options">Defines the parameters for the PSO algorithm.</param>
        /// <param name="p_target">The local power demand at every moment of time.</param>
        /// <param name="progressCallback">Callback invoked every time the algorithm finishes a new iteration.</param>
        /// <returns>Binary integer array of unit states.</returns>
        public static async Task<int[,]> UCPSearch(
            MGInput input,
            Options options,
            float[] p_target,
            OnNewSnapshotCallback progressCallback)
        {

            // Execute the PSO search.
#if !UNITY_WEBGL || (UNITY_WEBGL && UNITY_SSM_WEBGL_THREADING_CAPABLE)
            var t = new Task<int[]>(() =>
            {
                return UCPExecutePSORuns(
                    input,
                    options,
                    p_target,
                    progressCallback);
            });

            t.Start();
            int[] u_thr_step_1d = await t;
#else
            int[] u_thr_step_1d = UCPExecutePSORuns(
                input,
                options,
                p_target,
                progressCallback);
#endif

            // Map the results from a 1D array to a 2D array where one
            // dimension is the generator states and the other dimension is
            // time, measured in time steps.
            int[,] u_thr_step_2d = MGHelper.Int1DToBin2D(
                u_thr_step_1d, 
                options.stepCount,
                input.genCount);

            // Translate the time dimension from time steps to real time.
            float stepSize = input.tCount / options.stepCount;
            int[,] u_thr = MGHelper.TranslateTimeStep(
                u_thr_step_2d,
                stepSize,
                input.tCount,
                input.genCount);

            return u_thr;
        }

        protected struct PSOResult
        {
            public int[] bestPos;
            public float bestFitness;
            public float[] convergence;
        }

        private static int[] UCPExecutePSORuns(
            MGInput m,
            Options o,
            float[] p_target,
            OnNewSnapshotCallback progressCallback)
        {
            Random random = o.randomSeed.HasValue 
                ? new Random(o.randomSeed.Value) 
                : new Random();

            int runCount         = o.runCount;
            int iterCount        = o.iterCount;
            int pCount           = o.particleCount;
            int stepCount        = o.stepCount;
            int stepSize         = m.tCount / stepCount;
            int[] rBest          = new int[stepCount];
            float rBestFitness   = float.MaxValue;
            float totalIter      = runCount * iterCount;
            var threads          = new Thread[runCount];
            var results          = new PSOResult[runCount];

            Dictionary<StateStep, float> cDict;
            cDict = MGHelper.ConstructCostDictionary(m, stepCount, stepSize, 400.0f, p_target);

            // Precompute an array that maps generator indices to the operating
            // costs for that generator if it ran at maximum power for a 
            // timestep.
            float[] c_thr_max_1m = MGHelper.ThermalGenerator.GetCostAtMaxPower(
                m.genCount, 
                m.p_thr_max, 
                m.thr_c_a, 
                m.thr_c_b, 
                m.thr_c_c,
                stepSize);

            var lockObject = new object();
            var iterLatest = new int[runCount];
            var progressPerRun = new IterationSnapshot[runCount, iterCount];

            for (int iRun = 0; iRun < runCount; iRun++)
            {
                iterLatest[iRun] = -1;
            }

            void progressInner(
                int runIndex, 
                int iterIndex,
                IterationSnapshot progressStruct)
            {
                lock (lockObject)
                {
                    iterLatest[runIndex] = iterLatest[runIndex] < iterIndex 
                        ? iterIndex 
                        : iterLatest[runIndex];

                    progressPerRun[runIndex, iterIndex] = progressStruct;

                    progressCallback?.Invoke(
                        runIndex, 
                        iterIndex, 
                        (int[])iterLatest.Clone(), 
                        (IterationSnapshot[,])progressPerRun.Clone());
                }
            }

#if !UNITY_WEBGL || (UNITY_WEBGL && UNITY_SSM_WEBGL_THREADING_CAPABLE)
            // Run the PSO multiple times in parallel.
            for (int iRun = 0; iRun < runCount; iRun++)
            {
                int _iRun = iRun;
                threads[_iRun] = new Thread(() => 
                {
                    var r = random.Next();
                    results[_iRun] = UCPExecutePSORun(
                        _iRun, m, o, r, p_target, c_thr_max_1m, cDict, progressInner);
                });

                threads[_iRun].IsBackground = true;
                threads[_iRun].Start();
            }

            for (int iRun = 0; iRun < threads.Length; iRun++)
            {
                threads[iRun].Join();
            }
#else
            for (int iRun = 0; iRun < runCount; iRun++)
            {
                var r = random.Next();
                results[iRun] = UCPExecutePSORun(
                        iRun,
                        m,
                        o,
                        r,
                        p_target,
                        c_thr_max_1m,
                        cDict,
                        progressInner);
            }
#endif


            // Pick the best run.
            for (int iRun = 0; iRun < runCount; iRun++)
            {
                PSOResult result = results[iRun];
                if (result.bestFitness < rBestFitness)
                {
                    rBestFitness = result.bestFitness;
                    Array.Copy(result.bestPos, rBest, stepCount);
                }
            }

            return rBest;
        }

        private static PSOResult UCPExecutePSORun(
            int runIndex,
            MGInput mg,
            Options OptsPSO,
            int randomSeed,
            float[] p_target,
            float[] c_thr_max_1m,
            Dictionary<StateStep, float> cDict,
            OnNewSnapshotInnerCallback progressCallback = null)
        {
            var random = new Random(randomSeed);
            int iterCount = OptsPSO.iterCount;
            int pCount    = OptsPSO.particleCount;
            int stepCount = OptsPSO.stepCount;
            int stepSize  = mg.tCount / stepCount;

            // The power demand that needs to be fulfilled at each time step.
            float[] p_target_step_max = new float[stepCount];

            // The min. downtime of each generator normalized to our time step.
            float[] dtime_norm  = new float[mg.genCount];

            // The min. uptime of each generator normalized to our time step.
            float[] utime_norm  = new float[mg.genCount];

            // Initialize min. downtime and uptime arrays.
            for (int iGen = 0; iGen < mg.genCount; iGen++)
            {
                dtime_norm[iGen] = mg.thr_min_dtime[iGen] / stepSize;
                utime_norm[iGen] = mg.thr_min_utime[iGen] / stepSize;
            }

            // Iterate through all time steps and initialize p_target_step_max.
            for (int iStep = 0; iStep < stepCount; iStep++)
            {
                // Start time of the current step.
                int t1 = iStep * stepSize;

                // End time of the current step.
                int t2 = (iStep + 1) * stepSize - 1;

                // Sum up the power demand for every real time unit across
                // our time step.
                for (int t = t1; t < t2; t++)
                {
                    p_target_step_max[iStep] += 
                        UnityEngine.Mathf.Max(
                        p_target_step_max[iStep], 
                        p_target[t]);
                }
            }

            // Alpha - a criterion variable for how cost efficient a generator is.
            // Initialize an array to keep track of the alpha of every generator.
            float[] thr_alpha = MGHelper.ThermalGenerator.GetAlpha(
                mg.genCount, mg.p_thr_max, mg.thr_c_a, mg.thr_c_b, mg.thr_c_c);

            // Get an array of the indices of generators, sorted by their alpha.
            // In other words, if the 5th generator has the 2nd lowest alpha,
            // then thr_sorted_by_alpha[2] == 5.
            var thr_sorted_by_alpha = thr_alpha
                .Select(x => thr_alpha.OrderBy(alpha => alpha).ToList()
                .IndexOf(x)).ToArray();

            // Initialize an array for our particles.
            var p              = InitParticles(pCount, mg.genCount, stepCount, random);

            // Initialize an array for keeping track of the global best at each time step.
            var gBest          = new int[stepCount];

            // Variable for keeping track of the global best.
            float gBestFitness = float.MaxValue;

            // How many iterations have past since we've improved our global best.
            int iterSinceLastGBest = -1;

            // Go through all of our iterations, updating the particles each time.
            for (int iIter = 0; iIter < iterCount; iIter++)
            {
                iterSinceLastGBest++;

                // Percent of iterations completed.
                float completion = iIter / (float)iterCount;

                // Get the probability of inertia being applied to the
                // particle's velocity update.
                //
                // This probability decreases with each iteration, effectively
                // slowing down particles.
                float prob_inertia = UnityEngine.Mathf.Lerp(
                    1.0f, 0.35f, completion);

                // Get the probability of the particle's velocity being updated
                // to a random value.
                //
                // This probability increases with each iteration the swarm
                // goes through without finding a better global best.
                float prob_craziness = UnityEngine.Mathf.Lerp(
                    0.005f, 0.50f, iterSinceLastGBest / 100.0f);

                // Determines if the current iteration is an iteration where
                // it's possible to apply a heuristic adjustment to the swarm.
                //
                // This allows us to apply the adjustment only every n iterations.
                bool isHeuristicIter = iIter % 10 == 0;

                // Probability of a particle undergoing heuristic adjustment 
                // when the iteration is a heuristic adjustment enabled 
                // iteration.
                //
                // This allows us to control, on average, what percentage of
                // particles get affected by the heuristic adjustment.
                float prob_heuristic = 0.25f;

                // Step 1.
                // Iterate through all particles, evaluating their fitness
                // and establishing their personal best.
                for (int iP = 0; iP < pCount; iP++)
                {
                    float fitness = GetFitness(p[iP].pos, stepCount, cDict);

                    if (fitness < p[iP].fitnessBest)
                    {
                        p[iP].fitnessBest = fitness;
                        Array.Copy(p[iP].pos, p[iP].pBest, stepCount);
                    }
                }

                // Step 2.
                // Iterate through all particles, establishing the global best
                // in the swarm.
                for (int iP = 0; iP < pCount; iP++)
                {
                    if (p[iP].fitnessBest < gBestFitness)
                    {
                        gBestFitness = p[iP].fitnessBest;
                        Array.Copy(p[iP].pBest, gBest, stepCount);
                        iterSinceLastGBest = 0;
                    }
                }

                // Step 3.
                // Update the velocity and position of every particle.
                //
                // Optionally apply a heuristic adjustment aimed at speeding 
                // the convergence of the swarm.
                //
                // Revert state switches of generators that would violate the
                // min/max uptime constraints of that generator. This is 
                // necessary because the particles have no knowledge of those 
                // constraionts. So we deal with that simply by undoing
                // illegal state switches.
                for (int iP = 0; iP < pCount; iP++)
                {
                    Binary.UpdateVel(
                        p[iP].pos, 
                        p[iP].vel, 
                        p[iP].pBest, 
                        gBest, 
                        prob_inertia, 
                        prob_craziness, 
                        mg.genCount, 
                        random);

                    Binary.UpdatePos(p[iP].pos, p[iP].vel);

                    /*
                    if (isHeuristicIter)
                    {
                        if (random.NextDouble() < prob_heuristic)
                        {
                            HeuristicAdjustment(
                                mg.genCount, 
                                stepCount,
                                stepSize,
                                p[iP].pos,
                                thr_sorted_by_alpha, 
                                mg.p_thr_max,
                                p_target_step_max);
                        }
                    }
                    */

                    MGHelper.FixMinTimeConstraints(
                        p[iP].pos, stepCount, mg.genCount, dtime_norm, utime_norm);
                }

                var snapshot = new IterationSnapshot(
                    iIter,
                    iterCount,
                    stepCount,
                    mg.genCount,
                    gBestFitness,
                    gBest,
                    p);

                progressCallback?.Invoke(runIndex, iIter, snapshot);
            }


            return new PSOResult
            {
                bestPos = gBest,
                bestFitness = gBestFitness
            };
        }

        private static int[] GenerateInitPosition(
            int genCount, 
            int stepCount,
            System.Random random)
        {
            var pos = new int[stepCount];
            var n = 1 << genCount;

            for (int iStep = 0; iStep < stepCount; iStep++)
            {
                pos[iStep] = random.Next(0, n);
            }

            return pos;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void HeuristicAdjustment(
            int genCount,
            int stepCount, 
            float stepSize,
            int[] p,
            int[] indicesOfGenSortedByAlpha,
            float[] p_thr_max, 
            float[] p_target_step_max)
        {
            int[] states;
            for (int iStep = 0; iStep < stepCount; iStep++)
            {
                states = MGHelper.IntToBinary(p[iStep], genCount);
                float p_thr_step_max = SumGenPower(genCount, stepSize, p_thr_max, states);
                float p_target_remaining = p_target_step_max[iStep] - p_thr_step_max;

                if (p_target_remaining < 0.0f)
                {
                    for (int i = genCount - 1; i >= 0; i--)
                    {
                        int iGen = indicesOfGenSortedByAlpha[i];
                        if (states[iGen] == 1 && p_target_remaining + p_thr_max[iGen] * stepSize < 0.0f)
                        {
                            states[iGen] = 0;
                            p_thr_step_max = SumGenPower(genCount, stepSize, p_thr_max, states);
                            p_target_remaining = p_target_step_max[iStep] - p_thr_step_max;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < genCount; i++)
                    {
                        int iGen = indicesOfGenSortedByAlpha[i];
                        if (states[iGen] == 0)
                        {
                            if (p_target_remaining - p_thr_max[iGen] * stepSize > 0.0f)
                            {
                                states[iGen] = 1;
                                p_thr_step_max = SumGenPower(genCount, stepSize, p_thr_max, states);
                                p_target_remaining = p_target_step_max[iStep] - p_thr_step_max;
                            }
                        }
                    }
                }

                p[iStep] = MGHelper.BinaryToInt(states);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float SumGenPower(
            int genCount, 
            float stepSize,
            float[] power, 
            int[] states)
        {
            float sum = 0.0f;
            for (int iGen = 0; iGen < genCount; iGen++)
            {
                sum += states[iGen] * power[iGen];
            }

            return sum * stepSize;
        }

        /// <summary>
        /// Initializes particles and returns them in an array.
        /// </summary>
        /// <param name="particleCount">Number of particles to initialize.</param>
        /// <param name="generatorCount">Number of generators.</param>
        /// <param name="stepCount">Number of time steps.</param>
        /// <param name="random">PRNG seed.</param>
        /// <returns></returns>
        private static MGParticle[] InitParticles(
            int particleCount,
            int generatorCount, 
            int stepCount, 
            System.Random random)
        {
            var p = new MGParticle[particleCount];
            for (int iP = 0; iP < particleCount; iP++)
            {
                p[iP].pos = GenerateInitPosition(generatorCount, stepCount, random);
                p[iP].vel = new int[stepCount];
                p[iP].pBest = new int[stepCount];
                p[iP].fitnessBest = float.MaxValue;
            }

            return p;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GetFitness(
            int[] p,
            int stepCount,
            Dictionary<StateStep, float> cDict)
        {
            float cost_total = 0.0f;

            for (int iStep = 0; iStep < stepCount; iStep++)
            {
                var id = new StateStep(p[iStep], iStep);
                cost_total += cDict[id];
            }

            return cost_total;
        }
    }
}
