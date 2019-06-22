using System;
using UnityEngine;
using SSM.Grid.Search.PSO;

namespace SSM.Grid
{
    public class PSOTimeline : MonoBehaviour
    {
        public event EventHandler<bool> PSOTimelineChanged;
        public int ExecutionID    => executionID;
        public Options? OptionPSO => timeline?.OptionsPSO;
        public int GenCount       => timeline != null ? timeline.Snapshots[0, 0].GenCount : -1;
        public int CurrentIter    => timeline != null ? timeline.CurrentIter : -1;
        public int IterCount      => timeline != null ? timeline.OptionsPSO.iterCount : -1;
        public bool IsRunning     => isRunning;

        private bool isRunning;

        private PSOProgressEventArgs timeline;
        private IterationSnapshot[] combined;
        private MicrogridAlgorithm algorithm;

        private int executionID = -1;
        private int lastProcessedIter = -1;

        public IterationSnapshot GetProgressAtIteration(int iter) 
            => timeline != null ? combined[iter] : null;

        public IterationSnapshot GetProgressAtNewest()
            => timeline != null && CurrentIter >= 0 ? combined[CurrentIter] : null;

        protected void Awake()
        {
            algorithm = algorithm ?? FindObjectOfType<MicrogridAlgorithm>();
        }

        protected void OnEnable()
        {
            algorithm.UCPSOProgressChanged += UpdateTimeline;
            algorithm.UCStatusChanged      += UpdateUCStatus;
        }

        protected void OnDisable()
        {
            algorithm.UCPSOProgressChanged -= UpdateTimeline;
            algorithm.UCStatusChanged      -= UpdateUCStatus;
        }

        private void UpdateTimeline(object sender, PSOProgressEventArgs progress)
        {
            if (progress.ExecutionID != executionID)
            {
                combined = new IterationSnapshot[progress.OptionsPSO.iterCount];
                executionID = progress.ExecutionID;
                lastProcessedIter = -1;
            }

            timeline = progress;
            executionID = progress.ExecutionID;

            var currIter = progress.CurrentIter;

            if (currIter > lastProcessedIter)
            {
                Combine(progress, currIter);
            };

            lastProcessedIter = currIter;
            isRunning = lastProcessedIter < progress.OptionsPSO.iterCount - 1;
            PSOTimelineChanged?.Invoke(this, isRunning);
        }

        private void Combine(PSOProgressEventArgs progress, int iIter)
        {
            var snapshotsAcrossRun = MGHelper.GetCol(progress.Snapshots, iIter);
            combined[iIter] = IterationSnapshot.Combine(snapshotsAcrossRun);
        }

        private void UpdateUCStatus(object sender, RunStatusEventArgs e)
        {
            isRunning = e.Status == RunStatus.Started;
            PSOTimelineChanged?.Invoke(this, isRunning);
        }
    }
}
