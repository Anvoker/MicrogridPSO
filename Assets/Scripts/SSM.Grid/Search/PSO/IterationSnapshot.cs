using static SSM.Grid.MGHelper;

namespace SSM.Grid.Search.PSO
{
    /// <summary>
    /// A snapshot of the PSO's state taken at a certain iteration.
    /// </summary>
    public class IterationSnapshot
    {
        public float Completion => IterCurrent / (float)IterCount;

        /// <summary>
        /// Number of particles across every run.
        /// </summary>
        public int ParticleCount { get; private set; }

        /// <summary>
        /// Number of time steps.
        /// </summary>
        public int StepCount { get; private set; }

        /// <summary>
        /// Number of generators.
        /// </summary>
        public int GenCount { get; private set; }

        /// <summary>
        /// The index of the iteration this snapshot was taken at.
        /// </summary>
        public int IterCurrent { get; private set; }

        /// <summary>
        /// The maximum iteration index.
        /// </summary>
        public int IterCount { get; private set; }

        /// <summary>
        /// The fitness of the best configuration found by the PSO.
        /// </summary>
        public float GBestFitness { get; private set; }

        /// <summary>
        /// 2D array containing the best configuration found by the PSO
        /// using 1s and 0s to represent on and off states of each 
        /// generator at every time step.
        /// <para>The row index selects a time step.</para>
        /// <para>The column index selects a generator.</para>
        /// </summary>
        public int[,] GBest { get; private set; }

        /// <summary>
        /// 2D array containing the number of particles that "think" a
        /// generator should be on at a specific point in time according
        /// to their respective <see cref="MGParticle.pBest"/>. 
        /// <para>The row index selects a time step.</para>
        /// <para>The column index selects a generator.</para>
        /// </summary>
        public int[,] PBestCount => pBestCount;

        /// <summary>
        /// 2D array containing the number of particles that "think" a
        /// generator should be on at a specific point in time according
        /// to their respective <see cref="MGParticle.pos"/>. 
        /// <para>The row index selects a generator.</para>
        /// <para>The column index selects a time step.</para>
        /// </summary>
        public int[,] PCount => pCount;

        private int[,] pBestCount;
        private int[,] pCount;

        public IterationSnapshot(
            int iterCurrent,
            int iterCount,
            int stepCount,
            int genCount,
            float gBestFitness,
            int[] gBest,
            MGParticle[] p)
        {
            IterCurrent   = iterCurrent;
            IterCount     = iterCount;
            GBestFitness  = gBestFitness;
            ParticleCount = p.Length;
            StepCount     = stepCount;
            GenCount      = genCount;

            GBest      = new int[StepCount, GenCount];
            pBestCount = new int[StepCount, GenCount];
            pCount     = new int[StepCount, GenCount];

            for (int iStep = 0; iStep < StepCount; iStep++)
            {
                for (int iGen = 0; iGen < GenCount; iGen++)
                {
                    var bitGBest = gBest[iStep] & (1 << genCount - iGen - 1);
                    var stateGBest = ((bitGBest | (~bitGBest + 1)) >> 31) & 1;
                    GBest[iStep, iGen] += stateGBest;
                }
            }

            for (int iP = 0; iP < p.Length; iP++)
            {
                int[] pBest = p[iP].pBest;
                int[] pos = p[iP].pos;

                for (int iStep = 0; iStep < StepCount; iStep++)
                {
                    for (int iGen = 0; iGen < GenCount; iGen++)
                    {
                        var bitPBest = pBest[iStep] & (1 << genCount - iGen - 1);
                        var statePBest = ((bitPBest | (~bitPBest + 1)) >> 31) & 1;
                        pBestCount[iStep, iGen] += statePBest;

                        var bitPos = pos[iStep] & (1 << genCount - iGen - 1);
                        var statePos = ((bitPos | (~bitPos + 1)) >> 31) & 1;
                        pCount[iStep, iGen] += statePos;
                    }
                }
            }
        }

        public IterationSnapshot() { }

        public static IterationSnapshot Combine(IterationSnapshot[] snapshots)
        {
            var r = new IterationSnapshot();
            r.IterCount = snapshots[0].IterCount;
            r.IterCurrent = snapshots[0].IterCurrent;
            r.GenCount = snapshots[0].GenCount;
            r.StepCount = snapshots[0].StepCount;
            r.pBestCount = new int[snapshots[0].StepCount, snapshots[0].GenCount];
            r.pCount = new int[snapshots[0].StepCount, snapshots[0].GenCount];
            r.GBestFitness = float.PositiveInfinity;
            r.ParticleCount = 0;

            for (int iShot = 0; iShot < snapshots.Length; iShot++)
            {
                r.ParticleCount += snapshots[iShot].ParticleCount;
                if (r.GBestFitness > snapshots[iShot].GBestFitness)
                {
                    r.GBest = snapshots[iShot].GBest;
                    r.GBestFitness = snapshots[iShot].GBestFitness;
                }

                for (int iStep = 0; iStep < r.StepCount; iStep++)
                {
                    for (int iGen = 0; iGen < r.GenCount; iGen++)
                    {
                        r.PBestCount[iStep, iGen] += snapshots[iShot].PBestCount[iStep, iGen];
                        r.PCount[iStep, iGen] += snapshots[iShot].PCount[iStep, iGen];
                    }
                }
            }

            return r;
        }
    }
}
