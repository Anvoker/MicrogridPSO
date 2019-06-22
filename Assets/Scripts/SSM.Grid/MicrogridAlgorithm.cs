using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using SSM.Grid.Search.Exhaustive;
using SSM.Grid.Search.PSO;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace SSM.Grid
{
    public class MicrogridAlgorithm : MonoBehaviour
    {
        public UCAlgorithms UCAlgorithm
        {
            get { return ucAlgorithm; }
            set { ucAlgorithm = value; }
        }

        public EDAlgorithms EDAlgorithm
        {
            get { return edAlgorithm; }
            set { edAlgorithm = value; }
        }

        public event EventHandler<StopwatchStoppedEventArgs> UCStopwatchStopped;
        public event EventHandler<StopwatchStoppedEventArgs> EDStopwatchStopped;
        public event EventHandler<RunStatusEventArgs>        UCStatusChanged;
        public event EventHandler<CompletionEventArgs>       UCCompletionChanged;
        public event EventHandler<PSOProgressEventArgs>      UCPSOProgressChanged;

        [SerializeField]
        private UCAlgorithms ucAlgorithm;

        [SerializeField]
        private EDAlgorithms edAlgorithm;

        public delegate Task<int[,]> UCMethod(MGInput input, float[] p_target);
        public delegate float[,] EDMethod(MGInput input, float[] p_target, int[,] u_thr);

        private Dictionary<UCAlgorithms, UCMethod> ucEnumToMethod;
        private Dictionary<EDAlgorithms, EDMethod> edEnumToMethod;
        private static int executionID = 0;

        public Search.PSO.Options optionsPSO;

        protected void Awake()
        {
            var random     = new System.Random();
            int randomSeed = random.Next();

            optionsPSO = new Search.PSO.Options(randomSeed, 2, 180, 48, 24);

            ucEnumToMethod = new Dictionary<UCAlgorithms, UCMethod>()
            {
                { UCAlgorithms.PSO, UCPSearch },
            };

            edEnumToMethod = new Dictionary<EDAlgorithms, EDMethod>()
            {
                { EDAlgorithms.SingleCriterionMaxPower, SingleCriterionED },
            };
        }

        protected void Update()
        {
            while (invokeQueue.Count > 0)
            {
                if (invokeQueue.TryTake(out IInvocation e))
                {
                    e.Invoke();
                }
            }
        }

        public async Task<MGOutput> Calculate(MGInput input)
        {
#if !UNITY_WEBGL || (UNITY_WEBGL && UNITY_SSM_WEBGL_THREADING_CAPABLE)
            var r = await Calculate(input,
                ucEnumToMethod[ucAlgorithm],
                edEnumToMethod[edAlgorithm]);
#else
            var r = Calculate(input,
                ucEnumToMethod[ucAlgorithm],
                edEnumToMethod[edAlgorithm]).Result;
#endif
            return r;
        }

        private BlockingCollection<IInvocation> invokeQueue 
            = new BlockingCollection<IInvocation>();

        private interface IInvocation
        {
            void Invoke();
        }

        private struct Invocation<T> : IInvocation where T : EventArgs
        {
            public EventHandler<T> handler;
            public object sender;
            public T eventArgs;

            public void Invoke() => handler?.Invoke(sender, eventArgs);
        }

        private void QueueForMainThread<T>(EventHandler<T> e, object sender, T args) where T : EventArgs
        {
            invokeQueue.Add(new Invocation<T>()
            {
                handler = e,
                sender = sender,
                eventArgs = args
            });
        }


        private async Task<int[,]> UCPSearch(MGInput input, float[] p_target)
        {
#if !UNITY_WEBGL || (UNITY_WEBGL && UNITY_SSM_WEBGL_THREADING_CAPABLE)
            var r = await PSOSearch.UCPSearch(input, optionsPSO, p_target, OnProgress);
#else
            var r = PSOSearch.UCPSearch(input, optionsPSO, p_target, OnProgress).Result;
#endif
            executionID++;
            return r;
        }

        private void OnProgress(
            int runIndex, 
            int iterIndex, 
            int[] currentIterPerRun, 
            IterationSnapshot[,] snapshots)
        {
                float completion = 0.0f;

                for (int iRun = 0; iRun < optionsPSO.runCount; iRun++)
                {
                    completion += (float)currentIterPerRun[iRun]
                        / optionsPSO.iterCount
                        / optionsPSO.runCount;
                }

                QueueForMainThread(UCCompletionChanged, this,
                                    new CompletionEventArgs(completion));

                QueueForMainThread(UCPSOProgressChanged, this,
                    new PSOProgressEventArgs(
                        optionsPSO,
                        executionID,
                        runIndex,
                        iterIndex,
                        currentIterPerRun,
                        snapshots));
        }

        private async Task<MGOutput> Calculate(MGInput m, UCMethod uc, EDMethod ed)
        {
            var p_res = new float[m.tCount];

            var p_res_excess = new float[m.tCount];
            var p_excess = new float[m.tCount];

            for (int t = 0; t < m.tCount; t++)
            {
                p_res[t] = m.p_w[t] + m.p_pv[t];
            }

            float[] c_bat    = new float[m.tCount];
            float[] p_target = new float[m.tCount];
            float[] p_bat    = new float[m.tCount];
            float[] e_bat    = new float[m.tCount];
            float[] soc      = new float[m.tCount];

            soc[0]   = m.soc_ini;
            e_bat[0] = m.e_bat_max * soc[0];

            for (int t = 0; t < m.tCount; t++)
            {
                // Renewables aren't enough to meet demand.
                if (m.p_load[t] > p_res[t])
                {
                    p_res_excess[t] = 0.0f;

                    // Battery is usable.
                    if (soc[t] > m.soc_min)
                    {
                        c_bat[t]    = -1.0f;
                        p_target[t] = Mathf.Max(0.0f, m.p_load[t] - p_res[t] - m.p_bat_max);
                    }
                    // Battery is not usable, demand isn't met.
                    else
                    {
                        c_bat[t]    = 0.0f;
                        p_target[t] = Mathf.Max(0.0f, m.p_load[t] - p_res[t]);
                    }
                }
                // Renewables are enough to meet demand.
                else
                {
                    p_target[t] = 0.0f;
                    p_res_excess[t] = p_res[t] - m.p_load[t];

                    // Battery is chargeable.
                    if (soc[t] < m.soc_max)
                    {
                        c_bat[t] = 1.0f;
                    }
                    else
                    {
                        c_bat[t] = 0.0f;
                    }
                }

                if (c_bat[t] == 1.0f)
                {
                    // Charge the battery wiwth whichever is smallest between
                    // the battery's maximum power and the available surplus.
                    p_bat[t] = Mathf.Max(-m.p_bat_max, m.p_load[t] - p_res[t]);
                }
                else if (c_bat[t] == -1.0f)
                {
                    // Draw whichever is smallest between the battery's
                    // maximum power and the unmet demand.
                    p_bat[t] = Mathf.Min(m.p_bat_max, m.p_load[t] - p_res[t]);
                }
                else
                {
                    p_bat[t] = 0.0f;
                }

                if (t < m.tCount - 1)
                {
                    e_bat[t + 1] = e_bat[t] - (p_bat[t] / 60.0f);
                    soc[t + 1]   = e_bat[t + 1] / m.e_bat_max;
                }
            }

            // Unit Commitment
            var stopWatch = new Stopwatch();

            QueueForMainThread(UCStatusChanged, this, 
                new RunStatusEventArgs(RunStatus.Started));
            stopWatch.Start();

#if !UNITY_WEBGL || (UNITY_WEBGL && UNITY_SSM_WEBGL_THREADING_CAPABLE)
            int[,] u_thr = await uc(m, p_target);
#else
            int[,] u_thr = uc(m, p_target).Result;
#endif

            stopWatch.Stop();
            QueueForMainThread(UCStatusChanged, this, 
                new RunStatusEventArgs(RunStatus.FinishedRunning));

            UCStopwatchStopped?.Invoke(this, new StopwatchStoppedEventArgs(stopWatch.ElapsedMilliseconds));

            // Economic Dispatch
            float[,] p_thr = ed(m, p_target, u_thr);

            // Derive misc. variables
            float[] p_sum     = new float[m.tCount];
            float[] e_sys     = new float[m.tCount];
            float[] e_thr_sum = new float[m.tCount];
            float[] p_thr_sum = new float[m.tCount];
            float[] p_sys     = new float[m.tCount];
            float[] c_sys     = new float[m.tCount];
            float[,] c_thr    = new float[m.tCount, m.genCount];
            float[] c_thr_sum = new float[m.tCount];
            float[] p_thr_max_sum = new float[m.tCount];
            float c_thr_total = 0.0f;
            float c_sys_total = 0.0f;
            float e_thr_total = 0.0f;
            float e_sys_total = 0.0f;

            for (int t = 0; t < m.tCount; t++)
            {
                for (int i = 0; i < m.genCount; i++)
                {
                    p_thr_max_sum[t] += m.p_thr_max[i] * u_thr[t, i];
                    p_thr_sum[t] += p_thr[t, i];
                    e_thr_sum[t] += p_thr[t, i] / 60.0f;
                                                //EUR for 1 hour of functioning
                    c_thr[t, i] = u_thr[t, i] * MGHelper.ThermalGenerator.GetCost(
                        p_thr[t, i],
                        m.thr_c_a[i],
                        m.thr_c_b[i],
                        m.thr_c_c[i]) / 60.0f;
                    c_thr_sum[t] += c_thr[t, i];
                }

                p_sum[t] = p_res[t] + p_bat[t] + p_thr_sum[t];

                if (m.p_load[t] - p_sum[t] < 0)
                {
                    if (m.canSell)
                    {
                        p_sys[t] = m.p_load[t] - p_sum[t];
                    }
                    else
                    {
                        p_sys[t] = 0.0f;
                        p_excess[t] = p_sum[t] - m.p_load[t];
                    }
                }
                else
                {
                    if (m.canBuy)
                    {
                        p_sys[t] = m.p_load[t] - p_sum[t];
                    }
                    else
                    {
                        p_sys[t] = 0.0f;
                        p_excess[t] = p_sum[t] - m.p_load[t];
                    }
                }

                e_sys[t]     = p_sys[t] / 60.0f;
                c_sys[t]     = e_sys[t] * m.price[t];
                e_sys_total += e_sys[t];
                c_thr_total += c_thr_sum[t];
                c_sys_total += c_sys[t];
                e_thr_total += e_thr_sum[t];
            }

            return new MGOutput
            {
                u_thr              = u_thr,
                p_sum              = p_sum,
                p_res              = p_res,
                p_thr              = p_thr,
                e_thr_sum          = e_thr_sum,
                p_thr_sum          = p_thr_sum,
                p_thr_max_sum      = p_thr_max_sum,
                p_bat              = p_bat,
                p_sys              = p_sys,
                p_target           = p_target,
                soc                = soc,
                c_bat              = c_bat,
                e_sys_total        = e_sys_total,
                e_thr_total        = e_thr_total,
                c_sys              = c_sys,
                c_thr              = c_thr,
                c_thr_sum          = c_thr_sum,
                c_sys_total        = c_sys_total,
                c_thr_total        = c_thr_total
            };
        }

        public static float[,] SingleCriterionED(
            MGInput m,
            float[] p_target,
            int[,] u_thr)
        {
            var p_thr = new float[m.tCount, m.genCount];
            var indexToCost = new Dictionary<int, float>(m.genCount);

            for (int i = 0; i < m.genCount; i++)
            {
                // EUR for 1 hour of functioning
                float cost = MGHelper.ThermalGenerator.GetCost(
                    m.p_thr_max[i],
                    m.thr_c_a[i],
                    m.thr_c_b[i],
                    m.thr_c_c[i]);

                // EUR/MWh
                float alpha = cost / m.p_thr_max[i];
                indexToCost.Add(i, alpha);
            }

            var orderedByCost = indexToCost.OrderBy(x => x.Value).ToDictionary(x => x.Key, y => y.Value);

            return SingleCriterionED(m, p_target, u_thr, p_thr, orderedByCost, 0, m.tCount);
        }

        public static float[,] SingleCriterionED(
            MGInput m, 
            float[] p_target, 
            int[,] u_thr,
            float[,] p_thr,
            Dictionary<int, float> orderedByCost,
            int t1,
            int t2)
        {
            for (int t = t1; t < t2; t++)
            {
                float p_thr_remaining = p_target[t];

                foreach (KeyValuePair<int, float> kvp in orderedByCost)
                {
                    int i = kvp.Key;
                    float alpha = kvp.Value;

                    if (u_thr[t, i] == 1)
                    {
                        if (m.canSell)
                        {
                            if (alpha < m.price[t])
                            {
                                p_thr[t, i] = m.p_thr_max[i];
                                p_thr_remaining -= p_thr[t, i];
                                continue;
                            }
                        }

                        if (m.canBuy)
                        {
                            if (alpha > m.price[t])
                            {
                                p_thr[t, i] = 0.0f;
                                continue;
                            }
                        }

                        p_thr[t, i] = Mathf.Clamp(m.p_thr_max[i], 0.0f, p_thr_remaining);
                        p_thr_remaining -= p_thr[t, i];
                    }
                    else
                    {
                        p_thr[t, i] = 0.0f;
                    }
                }
            }

            return p_thr;
        }
    }
}