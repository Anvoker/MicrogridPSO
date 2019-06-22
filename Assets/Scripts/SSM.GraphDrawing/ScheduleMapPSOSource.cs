using System;
using UnityEngine;
using SSM.Grid;

namespace SSM.GraphDrawing
{
    [RequireComponent(typeof(ScheduleMap))]
    public class ScheduleMapPSOSource : MonoBehaviour
    {
        private ScheduleMap scheduleMap;
        private PSOTimeline timelineController;

        public ScheduleMap ScheduleMap => scheduleMap;

        private bool needsUpdate;

        protected void Awake()
        {
            timelineController = timelineController ?? FindObjectOfType<PSOTimeline>();
            scheduleMap = GetComponent<ScheduleMap>();
            timelineController.PSOTimelineChanged += SetDataToScheduleMap;
            SetDataToScheduleMap(this, false);
        }

        protected void OnEnable()
        {
            if (needsUpdate)
            {
                DoUpdate();
            }
        }

        protected void OnDestroy()
        {
            timelineController.PSOTimelineChanged -= SetDataToScheduleMap;
        }

        private void SetDataToScheduleMap(object sender, bool isRunning)
        {
            if (timelineController.ExecutionID < 0 || isRunning)
            {
                return;
            }

            if (!gameObject.activeInHierarchy)
            {
                needsUpdate = true;
                return;
            }

            DoUpdate();
        }

        private void DoUpdate()
        {
            var iterCount = timelineController.IterCount;
            var genCount = timelineController.GenCount;

            scheduleMap.EnsureCount(iterCount, genCount);
            for (int iIter = 0; iIter < iterCount; iIter++)
            {
                var p = timelineController.GetProgressAtIteration(iIter);
                scheduleMap.SetData(iIter, p.GBest);
            }
        }
    }
}
