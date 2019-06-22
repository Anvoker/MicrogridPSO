namespace SSM.Grid.Search.PSO
{
    /// <summary>
    /// Contains parameters for how a PSO algorithm should execute.
    /// </summary>
    [System.Serializable]
    public struct Options
    {
        /// <summary>
        /// A random seed that should be used for all random generation within
        /// the PSO algorithm. If null, the algorithm should provide its own
        /// seed.
        /// </summary>
        public int? randomSeed;

        /// <summary>
        /// The number of times the whole algorithm should run. It's useful to
        /// run the entire search multiple times if the PSO search is displaying
        /// high variance.
        /// </summary>
        public int runCount;

        /// <summary>
        /// The number of times the particles are moved and updated. High
        /// iteration counts mean a higher chance for the particles to converge
        /// on a minimum.
        /// </summary>
        public int iterCount;

        /// <summary>
        /// The number of particles generated in the search space. A rule of
        /// thumb is that the number of particles should be 3-4 times the
        /// dimensionality of the problem, but the optimal number varies
        /// significantly from problem to problem.
        /// </summary>
        public int particleCount;

        /// <summary>
        /// The number of steps through the time horizon. The finer the step,
        /// the more exact the result gets, up to a point, but the more time
        /// the algorithm has to take to complete.
        /// </summary>
        public int stepCount;

        public Options(int? randomSeed, int runCount, int iterCount,
            int particleCount, int stepCount)
        {
            this.randomSeed = randomSeed;
            this.runCount = runCount;
            this.iterCount = iterCount;
            this.particleCount = particleCount;
            this.stepCount = stepCount;
        }
    }
}