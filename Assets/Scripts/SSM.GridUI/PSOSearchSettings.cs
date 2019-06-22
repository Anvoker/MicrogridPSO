using UnityEngine;
using UnityEngine.UI;
using System;
using System.Globalization;
using TMPro;
using SSM.Grid;

namespace SSM.GridUI
{
    public class PSOSearchSettings : MonoBehaviour
    {
        public TMP_InputField inputFieldSearchSteps;
        public TMP_InputField inputFieldRuns;
        public TMP_InputField inputFieldIterations;
        public TMP_InputField inputFieldParticles;
        public TMP_InputField inputFieldRandomSeed;

        private MicrogridAlgorithm mgAlgorithm;

        public void SetSearchSteps(string input)
        {
            mgAlgorithm.optionsPSO.stepCount
                = Math.Max(1, int.Parse(input));
        }

        public void SetRuns(string input)
        {
            mgAlgorithm.optionsPSO.runCount
                = Math.Max(1, int.Parse(input));
        }

        public void SetIterations(string input)
        {
            mgAlgorithm.optionsPSO.iterCount
                = Math.Max(1, int.Parse(input));
        }

        public void SetParticles(string input)
        {
            mgAlgorithm.optionsPSO.particleCount
                = Math.Max(1, int.Parse(input));
        }

        public void SetRandomSeed(string input)
        {
            mgAlgorithm.optionsPSO.randomSeed
                = Math.Max(0, int.Parse(input));
        }

        protected void Awake()
        {
            mgAlgorithm = FindObjectOfType<MicrogridAlgorithm>();
        }

        protected void Start()
        {
            UpdateFields();
        }

        protected void OnEnable()
        {
            inputFieldSearchSteps.onEndEdit.AddListener(SetSearchSteps);
            inputFieldRuns.onEndEdit.AddListener(SetRuns);
            inputFieldIterations.onEndEdit.AddListener(SetIterations);
            inputFieldParticles.onEndEdit.AddListener(SetParticles);
            inputFieldRandomSeed.onEndEdit.AddListener(SetRandomSeed);
        }

        protected void OnDisable()
        {
            inputFieldSearchSteps.onEndEdit.RemoveListener(SetSearchSteps);
            inputFieldRuns       .onEndEdit.RemoveListener(SetRuns);
            inputFieldIterations .onEndEdit.RemoveListener(SetIterations);
            inputFieldParticles  .onEndEdit.RemoveListener(SetParticles);
            inputFieldRandomSeed .onEndEdit.RemoveListener(SetRandomSeed);
        }

        private void UpdateFields()
        {
            var o  = mgAlgorithm.optionsPSO;
            var ci = CultureInfo.InvariantCulture;

            inputFieldSearchSteps.text = o.stepCount.ToString("G", ci);
            inputFieldRuns.text        = o.runCount.ToString("G", ci);
            inputFieldIterations.text  = o.iterCount.ToString("G", ci);
            inputFieldParticles.text   = o.particleCount.ToString("G", ci);
            inputFieldRandomSeed.text  = o.randomSeed?.ToString("G", ci);
        }
    }
}
