using UnityEngine;
using UnityEngine.UI;
using System.Globalization;
using System.Numerics;
using TMPro;
using SSM.Grid;

namespace SSM.GridUI
{
    public class AlgorithmData : MonoBehaviour
    {
        public TMP_InputField inputFieldNoConfig;
        public TMP_InputField inputFieldExecutionTime;
        public Image progressBar;

        private Microgrid mg;
        private MicrogridAlgorithm mgAlgorithm;
        private bool queued;

        private int _stepCount;
        private int _genCount;

        protected void OnEnable()
        {
            mgAlgorithm.UCStopwatchStopped  += UpdateExecutionTimeField;
            mgAlgorithm.UCCompletionChanged += UpdateProgressBar;
            mg.OnCalculated += HideProgressBar;
        }

        protected void OnDisable()
        {
            mgAlgorithm.UCStopwatchStopped  -= UpdateExecutionTimeField;
            mgAlgorithm.UCCompletionChanged -= UpdateProgressBar;
            mg.OnCalculated -= HideProgressBar;
        }

        protected void Awake()
        {
            mgAlgorithm = FindObjectOfType<MicrogridAlgorithm>();
            mg = FindObjectOfType<Microgrid>();
        }

        protected void LateUpdate()
        {
            if (gameObject.activeInHierarchy)
            {
                int stepCount = 0;

                switch (mgAlgorithm.UCAlgorithm)
                {
                    case UCAlgorithms.PSO:
                        stepCount = mgAlgorithm.optionsPSO.stepCount;
                        break;
                    case UCAlgorithms.ExhaustiveSearch:
                        stepCount = mgAlgorithm.optionsPSO.stepCount;
                        break;
                }


                if (_stepCount != stepCount ||
                    _genCount != mg.Input.genCount)
                {
                    var nCombinations =
                        BigInteger.Pow(2, mg.Input.genCount * stepCount);
                    inputFieldNoConfig.text = nCombinations.ToString("E3",
                        CultureInfo.InvariantCulture);
                    _stepCount = stepCount;
                    _genCount  = mg.Input.genCount;
                }
            }
        }

        private void UpdateProgressBar(object sender, CompletionEventArgs e)
        {
            if (!queued)
            {
                mg.QueueInvocation(() =>
                {
                    if (!progressBar.gameObject.activeSelf)
                    {
                        progressBar.gameObject.SetActive(true);
                    }

                    if (progressBar.fillAmount < e.Completion)
                    {
                        progressBar.fillAmount = e.Completion;
                    }

                    queued = false;
                });

                queued = true;
            }
        }

        private void HideProgressBar(object sender,
            Microgrid.CalculatedEventArgs e)
        {
            _HideProgressBar();
            Invoke("_HideProgressBar", 0.1f);
        }

        private void _HideProgressBar()
        {
            if (progressBar.gameObject.activeSelf)
            {
                progressBar.gameObject.SetActive(false);
            }
            progressBar.fillAmount = 0.0f;
        }

        private void UpdateExecutionTimeField(object sender,
            StopwatchStoppedEventArgs args)
        {
            mg.QueueInvocation(() => inputFieldExecutionTime.text
                = args.miliseconds.ToString("G", CultureInfo.InvariantCulture));
        }
    }
}