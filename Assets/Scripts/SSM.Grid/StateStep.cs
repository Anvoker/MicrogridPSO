using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSM.Grid
{
    public struct StateStep : IEquatable<StateStep>
    {
        int state;
        int step;

        public StateStep(int state, int step)
        {
            this.state = state;
            this.step = step;
        }

        public bool Equals(StateStep other)
        {
            return state == other.state && step == other.step;
        }
    }
}
