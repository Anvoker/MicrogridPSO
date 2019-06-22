using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

namespace SSM.GraphDrawing
{
    [RequireComponent(typeof(RadarArray))]
    public class RadarArraySliderSource : MonoBehaviour
    {
        public TimelineSlider timelineSlider;
        public PSOVariable variableType;

        private RadarArray radarArray;

        public RadarArray RadarArray
        {
            get
            {
                if (radarArray == null)
                {
                    radarArray = GetComponent<RadarArray>();
                }

                return radarArray;
            }
        }

        public void SetPSOVariable(TMP_Dropdown dropdown)
        {
            variableType = (PSOVariable)dropdown.value;
            SetDataToRadarArray(this, new EventArgs());
        }

        protected void OnEnable()
        {
            SetDataToRadarArray(this, new EventArgs());
            timelineSlider.OnTimelineChanged += SetDataToRadarArray;
        }

        protected void OnDisable()
        {
            timelineSlider.OnTimelineChanged -= SetDataToRadarArray;
        }

        private void SetDataToRadarArray(object sender, EventArgs args)
        {
            if (timelineSlider.PSOProgress != null)
            {
                RadarArray.SetData(GetVariable(variableType));
            }
        }

        private int[,] GetVariable(PSOVariable variable)
        {
            switch(variable)
            {
                case PSOVariable.PBest:
                    return timelineSlider.PSOProgress.PBestCount;

                case PSOVariable.Position:
                    return timelineSlider.PSOProgress.PCount;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    public enum PSOVariable
    {
        PBest,
        Position,
    }
}
