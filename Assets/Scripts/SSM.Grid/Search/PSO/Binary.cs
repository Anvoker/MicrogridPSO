using System;
using System.Runtime.CompilerServices;

namespace SSM.Grid.Search.PSO
{
    public static class Binary
    {
        /// <summary>
        /// Updates the velocity of a particle. Mutates the given velocity
        /// array.
        /// </summary>
        /// <param name="p">Position of the particle.</param>
        /// <param name="v">Velocity of the particle.</param>
        /// <param name="pbest">Personal best position.</param>
        /// <param name="gbest">Global best position.</param>
        /// <param name="prob_w">Inertial probability. The probability that 
        /// the particle's previous velocity will be used to determine its 
        /// new velocity or if it will be ignored.</param>
        /// <param name="prob_b">Craziness probability. The probability that 
        /// the particle's new velocity will be set to a random value.</param>
        /// <param name="genCount">Number of generators</param>
        /// <param name="random">PRNG instance.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UpdateVel(
            int[] p, 
            int[] v,
            int[] pbest,
            int[] gbest, 
            float prob_w, 
            float prob_b, 
            int genCount,
            Random random)
        {
            // Number of possible generator on/off state combinations
            // This is equivalent to 2^genCount.
            //
            // We want to use this as the upper cap to our random number,
            // because going beyond this number means representing generators
            // that don't exist as being on.
            int nStates = 1 << genCount;

            for (int i = 0; i < p.Length; i++)
            {
                // Random bitmask that controls how much pbest affects the 
                // current particle velocity. 
                // r1 == 0 then 
                //     pbest won't affect the current particle velocity at all,
                // r1 == nStates then 
                //     The particle will head directly towards pbest.
                // Anything in between means only some bits change at a time.
                int r1 = random.Next(0, nStates);

                // Random bitmask that controls how much gbest affects the 
                // current particle velocity. 
                // r1 == 0 then 
                //     gbest won't affect the current particle velocity at all,
                // r1 == nStates then 
                //     The particle will head directly towards gbest.
                // Anything in between means only some bits change at a time.
                int r2 = random.Next(0, nStates);

                // Inertial switch.
                // If w==1
                //    Inertia is applied.
                //    The particle's previous velocity affects its new velocity.
                // if w==0
                //    Inertia isn't applied.
                //    The particle's previous velocity does not affect its new velocity.
                //
                // This is a way of simulating inertia in binary spaces.
                // Normally inertia in real space just acts as a coefficient
                // for velocity, increasing or reducing it.
                //
                // But in a binary space, we can only move from 0 to 1 or vice
                // versa, so it's not possible to have a velocity different
                // from 0 or 1.
                //
                // We use this inertial switch to sometimes completely ignore 
                // the particle's current velocity. Averaged over several 
                // iterations, this approximates the behaviour of inertial 
                // weight of [0...1] in real space.
                int w = random.NextDouble() > prob_w ? 0 : 1;

                // Craziness switch.
                // If c==1, craziness is applied
                // if c==0, craziness isn't applied.
                //
                // When craziness is applied, random noise is added to the
                // velocity by XORing it.
                int c = random.NextDouble() > prob_b ? 0 : 1;

                // Craziness random velocity.
                int cn = random.Next(0, nStates);

                v[i] = 
                    ((w * v[i]) // Inertia & Velocity
                        | (r1 & (pbest[i] ^ p[i])) // Adjustment towards pbest
                //              ( Changed bits  )
                //        ( Masked changed bits  )
                        | (r2 & (gbest[i] ^ p[i])) // Adjustment towards gbest
                //              ( Changed bits  )
                //        ( Masked changed bits  )
                    )
                    ^ c * cn; // Craziness
            }
        }

        /// <summary>
        /// Updates the position of a particle using its velocity. Mutates
        /// the given position array.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="v"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UpdatePos(int[] p, int[] v)
        {
            int d1 = p.Length;
            for (int i = 0; i < d1; i++)
            {
                p[i] = p[i] ^ v[i];
            }
        }
    }
}