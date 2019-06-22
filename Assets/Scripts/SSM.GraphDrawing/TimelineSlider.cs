using System;

using UnityEngine;
using UnityEngine.UI;
using SSM.Grid;
using SSM.Grid.Search.PSO;

namespace SSM.GraphDrawing
{
    [RequireComponent(typeof(Slider))]
    public class TimelineSlider : MonoBehaviour
    {
        public IterationSnapshot PSOProgress { get; private set; }
        public EventHandler OnTimelineChanged;

        private Slider slider;
        private PSOTimeline timelineController;
        private bool isSliderSubscribedTo;

        protected void Awake()
        {
            slider = GetComponent<Slider>();
            timelineController = timelineController 
                ?? FindObjectOfType<PSOTimeline>();
        }

        protected void OnEnable()
        {
            PSOProgress = timelineController.GetProgressAtNewest();
            SetupSlider();
            OnTimelineChanged?.Invoke(this, new EventArgs());
            timelineController.PSOTimelineChanged += UpdateSlider;
            SliderSetActive(!timelineController.IsRunning);
        }

        protected void OnDisable()
        {
            timelineController.PSOTimelineChanged -= UpdateSlider;
            SliderSetActive(false);
        }

        protected void OnDestroy()
        {
            timelineController.PSOTimelineChanged -= UpdateSlider;
            SliderSetActive(false);
        }

        private void UpdateSlider(object sender, bool isRunning)
        {
            SliderSetActive(!isRunning);
            SetupSlider();

            if (isRunning)
            {
                PSOProgress = timelineController.GetProgressAtNewest();
            }

            OnTimelineChanged?.Invoke(this, new EventArgs());
        }

        private void SetupSlider()
        {
            slider.minValue = 0;
            slider.maxValue = timelineController.IterCount - 1;
            slider.value = timelineController.CurrentIter;
            slider.wholeNumbers = true;
        }

        private void OnSliderChanged(float f)
        {
            if (!timelineController.IsRunning)
            {
                PSOProgress = timelineController
                    .GetProgressAtIteration(Mathf.RoundToInt(f));
                OnTimelineChanged?.Invoke(this, new EventArgs());
            }
        }

        private void SliderSetActive(bool isActive)
        {
            if (isActive)
            {
                slider.interactable = true;
                if (!isSliderSubscribedTo)
                {
                    slider.onValueChanged.AddListener(OnSliderChanged);
                    isSliderSubscribedTo = true;
                }
            }
            else
            {
                slider.interactable = false;
                if (isSliderSubscribedTo)
                {
                    slider.onValueChanged.RemoveListener(OnSliderChanged);
                    isSliderSubscribedTo = false;
                }
            }
        }
    }
}
