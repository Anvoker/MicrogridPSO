namespace SSM.Grid.Search.PSO
{
    /// <summary>
    /// A particle struct suitable for for applying PSO to a Microgrid.
    /// </summary>
    public struct MGParticle
    {
        /// <summary>
        /// The position of the particle represented as an array of integers
        /// where each integer encodes the on/off state of all generators
        /// at a given time step. The index tracks which time step the state
        /// belongs to.
        /// </summary>
        public int[] pos;

        /// <summary>
        /// The velocity of the particle represented as an array of integers
        /// where each integer encodes the on/off state of all generators
        /// at a given time step. The index tracks which time step the state
        /// belongs to.
        /// </summary>
        public int[] vel;

        /// <summary>
        /// The position of the best configuration the particle encountered
        /// so far.
        /// </summary>
        public int[] pBest;

        /// <summary>
        /// The fitness of the best configuration the particle encountered
        /// so far..
        /// </summary>
        public float fitnessBest;
    }
}