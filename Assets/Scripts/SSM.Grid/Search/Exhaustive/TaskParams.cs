using System;
using System.Collections.Generic;

namespace SSM.Grid.Search.Exhaustive
{
    [System.Serializable]
    public struct TaskParams
    {
        public TaskParams(
            float[] p_target, 
            int stepCount,
            int stepSize,
            Tuple<int, int>[] stepInterval,
            bool[] isValidationNeeded,
            float[] cost_thr_at_max,
            Dictionary<StateStep, float> purchaseDict)
        {
            this.p_target = p_target;
            this.stepCount = stepCount;
            this.stepSize = stepSize;
            this.stepInterval = stepInterval;
            this.isValidationNeeded = isValidationNeeded;
            this.cost_thr_at_max = cost_thr_at_max;
            this.purchaseDict = purchaseDict;
        }

        public float[] p_target;
        public readonly int stepCount;
        public readonly int stepSize;
        public readonly Tuple<int, int>[] stepInterval;
        public bool[] isValidationNeeded;
        public float[] cost_thr_at_max;
        public Dictionary<StateStep, float> purchaseDict;
    }
}