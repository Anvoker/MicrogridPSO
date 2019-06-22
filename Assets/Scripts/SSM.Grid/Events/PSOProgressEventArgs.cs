using System;
using SSM.Grid.Search.PSO;
using System.Linq;

namespace SSM.Grid
{
    [Serializable]
    public class PSOProgressEventArgs : EventArgs
    {
        public Options OptionsPSO { get; }

        /// <summary>
        /// Gets the execution ID of the search which can uniquely identify
        /// a PSO search.
        /// </summary>
        public int ExecutionID { get; }

        /// <summary>
        /// Gets the index of the run that most recently updated this object.
        /// </summary>
        public int LatestRun { get; }

        /// <summary>
        /// Gets the index of the iteration that most recently updated this
        /// object. This is not necessarily the highest iteration index
        /// among all runs.
        /// </summary>
        public int LatestIter { get; }

        /// <summary>
        /// Gets the index of the latest iteration that was computed by all
        /// runs.
        /// <para>
        /// Example: If run 0 is at iteration 50, run 2 is at iteration 60,
        /// and run 3 is at iteration 45 then this property will return 45.
        /// </para>
        /// </summary>
        public int CurrentIter
        {
            get
            {
                var min = int.MaxValue;
                var count = CurrentIterPerRun.Length;
                for (int i = 0; i < count; i++)
                {
                    if (CurrentIterPerRun[i] < min)
                    {
                        min = CurrentIterPerRun[i];
                    }
                }

                return min;
            }
        }

        /// <summary>
        /// Gets an array containing the current iteration index for each run.
        /// </summary>
        public int[] CurrentIterPerRun { get; }

        /// <summary>
        /// Gets an array of the snapshots of the PSO's state for every PSO
        /// run at every iteration.
        /// <para>The row index selects a run.</para>
        /// <para>The column index selects an iteration.</para>
        /// </summary>
        public IterationSnapshot[,] Snapshots { get; }

        public PSOProgressEventArgs(
            Options optionsPSO,
            int executionID,
            int latestRun,
            int latestIter,
            int[] currentIterPerRun,
            IterationSnapshot[,] snapshots)
        {
            OptionsPSO        = optionsPSO;
            ExecutionID       = executionID;
            LatestRun         = latestRun;
            LatestIter        = latestIter;
            CurrentIterPerRun = currentIterPerRun;
            Snapshots         = snapshots;
        }
    }
}
