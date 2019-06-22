using System;
using System.Linq;
using System.Collections.Generic;
using WindowManager;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SSM.Grid;
using SSM.GridUI;

namespace SSM.UI
{
    public class DataPresetPickerWindow : UIWindow
    {
        [SerializeField] private RectTransform generatorContainer;

        [SerializeField] private ThermalGeneratorEntry generatorEntryPrototype;

        [SerializeField] private Button submitButton;

        [SerializeField] private TMP_InputField loadMinIF;
        [SerializeField] private TMP_InputField loadAverageIF;
        [SerializeField] private TMP_InputField loadPeakIF;
        [SerializeField] private TMP_InputField renewableMinIF;
        [SerializeField] private TMP_InputField renewableAverageIF;
        [SerializeField] private TMP_InputField renewablePeakIF;
        [SerializeField] private TMP_InputField batteryRatedPowerIF;
        [SerializeField] private TMP_InputField batteryRatedEnergyIF;
        [SerializeField] private TMP_InputField maxThermalPowerIF;
        [SerializeField] private TMP_InputField loadMultIF;
        [SerializeField] private TMP_InputField renewableMultIF;

        [SerializeField] private Toggle canBuyToggle;
        [SerializeField] private Toggle canSellToggle;

        private List<ThermalGeneratorEntry> generatorEntries = new List<ThermalGeneratorEntry>();

        private Microgrid microgrid;
        private MGInput mgInput;
        private float loadMultiplier = 7.0f;
        private float renewableMultiplier = 3.0f;
        private float batteryRatedPower;
        private float batteryRatedEnergy;
        private bool canBuy = false;
        private bool canSell = false;

        public void Show(MGInput defaultMGInput)
        {
            mgInput               = defaultMGInput;
            mgInput.thr_c_a       = new float[0];
            mgInput.thr_c_b       = new float[0];
            mgInput.thr_c_c       = new float[0];
            mgInput.thr_min_dtime = new float[0];
            mgInput.thr_min_utime = new float[0];
            mgInput.p_thr_max     = new float[0];
            mgInput.genCount      = 0;

            batteryRatedPowerIF.SetTextWithoutNotify(mgInput.p_bat_max.ToString("F3"));
            batteryRatedEnergyIF.SetTextWithoutNotify(mgInput.e_bat_max.ToString("F3"));
            batteryRatedPower = mgInput.p_bat_max;
            batteryRatedEnergy = mgInput.e_bat_max;

            base.Show();
        }

        public new void Show()
        {
            mgInput = microgrid.Input;

            batteryRatedPowerIF.SetTextWithoutNotify(mgInput.p_bat_max.ToString("F3"));
            batteryRatedEnergyIF.SetTextWithoutNotify(mgInput.e_bat_max.ToString("F3"));
            batteryRatedPower = mgInput.p_bat_max;
            batteryRatedEnergy = mgInput.e_bat_max;

            base.Show();
        }

        protected void OnEnable()
        {
            loadMultIF.SetTextWithoutNotify(loadMultiplier.ToString());
            renewableMultIF.SetTextWithoutNotify(renewableMultiplier.ToString());
        }

        protected void Awake()
        {
            microgrid = FindObjectOfType<Microgrid>();

            submitButton.onClick.AddListener(Submit);
            canBuyToggle.onValueChanged.AddListener(b => canBuy = b);
            canSellToggle.onValueChanged.AddListener(b => canSell = b);

            canBuyToggle.isOn = canBuy;
            canSellToggle.isOn = canSell;

            loadMultIF.richText = false;
            loadMultIF.contentType = TMP_InputField.ContentType.DecimalNumber;
            loadMultIF.onValueChanged.AddListener(s => SetLoadMultiplier(s));

            renewableMultIF.richText = false;
            renewableMultIF.contentType = TMP_InputField.ContentType.DecimalNumber;
            renewableMultIF.onValueChanged.AddListener(s => SetRenewableMultiplier(s));

            batteryRatedPowerIF.richText = false;
            batteryRatedPowerIF.contentType = TMP_InputField.ContentType.DecimalNumber;
            batteryRatedPowerIF.onValueChanged.AddListener(s => batteryRatedPower = float.Parse(s));

            batteryRatedEnergyIF.richText = false;
            batteryRatedEnergyIF.contentType = TMP_InputField.ContentType.DecimalNumber;
            batteryRatedEnergyIF.onValueChanged.AddListener(s => batteryRatedEnergy = float.Parse(s));

            loadAverageIF.interactable = false;
            loadAverageIF.contentType = TMP_InputField.ContentType.DecimalNumber;

            loadPeakIF.interactable = false;
            loadPeakIF.contentType = TMP_InputField.ContentType.DecimalNumber;

            loadMinIF.interactable = false;
            loadMinIF.contentType = TMP_InputField.ContentType.DecimalNumber;

            renewableAverageIF.interactable = false;
            renewableAverageIF.contentType = TMP_InputField.ContentType.DecimalNumber;

            renewablePeakIF.interactable = false;
            renewablePeakIF.contentType = TMP_InputField.ContentType.DecimalNumber;

            renewableMinIF.interactable = false;
            renewableMinIF.contentType = TMP_InputField.ContentType.DecimalNumber;

            foreach (var gen in ThermalGeneratorPresets.EnumeratePresets())
            {
                AddGeneratorEntry(gen.Key, gen.Value);
            }

            foreach (var gen in ThermalGeneratorPresets.presetGroup[0])
            {
                var i = generatorEntries.FindIndex(x => x.generatorName == gen.Item1);
                if (i > 0)
                {
                    generatorEntries[i].SetQuantity(gen.Item2);
                }
            }
        }

        protected override void Setup()
        {
            loadMinIF.text = GetLoadMin().ToString("F3");
            loadAverageIF.text = GetLoadAverage().ToString("F3");
            loadPeakIF.text = GetLoadPeak().ToString("F3");
            renewableMinIF.text = GetRenewableMin().ToString("F3");
            renewableAverageIF.text = GetRenewableAverage().ToString("F3");
            renewablePeakIF.text = GetRenewablePeak().ToString("F3");
        }

        private float GetLoadPeak() => mgInput.p_load.Length > 0 
            ? mgInput.p_load.Max() * loadMultiplier : 0.0f;

        private float GetLoadAverage() => mgInput.p_load.Length > 0 
            ? mgInput.p_load.Sum() * loadMultiplier / mgInput.tCount : 0.0f;

        private float GetLoadMin()     => mgInput.p_load.Length > 0 
            ? mgInput.p_load.Min() * loadMultiplier : 0.0f;

        private float GetRenewablePeak() => 
            mgInput.p_w.Length > 0 && mgInput.p_pv.Length > 0 
            ? (mgInput.p_w.Max() + mgInput.p_pv.Max()) * renewableMultiplier
            : 0.0f;

        private float GetRenewableAverage() 
            => mgInput.p_w.Length > 0 && mgInput.p_pv.Length > 0 
            ? (mgInput.p_w.Sum() + mgInput.p_pv.Sum()) * renewableMultiplier / mgInput.tCount
            : 0.0f;

        private float GetRenewableMin()    
            => mgInput.p_w.Length > 0 && mgInput.p_pv.Length > 0 
            ? (mgInput.p_w.Min() + mgInput.p_pv.Min()) * renewableMultiplier
            : 0.0f;

        private void SetLoadMultiplier(string s)
        {
            loadMultiplier = float.Parse(s);
            loadAverageIF.text = GetLoadAverage().ToString("F3");
            loadPeakIF.text = GetLoadPeak().ToString("F3");
            loadMinIF.text = GetLoadMin().ToString("F3");
        }

        private void SetRenewableMultiplier(string s)
        {
            renewableMultiplier = float.Parse(s);
            renewableAverageIF.text = GetRenewableAverage().ToString("F3");
            renewablePeakIF.text = GetRenewablePeak().ToString("F3");
            renewableMinIF.text = GetRenewableMin().ToString("F3");
        }

        private void Submit()
        {
            for (int i = 0; i < mgInput.tCount; i++)
            {
                mgInput.p_load[i] *= loadMultiplier;
                mgInput.p_w[i]    *= renewableMultiplier;
                mgInput.p_pv[i]   *= renewableMultiplier;
            }

            mgInput.genCount      = generatorEntries.Sum(x => x.Quantity);
            mgInput.thr_c_a       = new float[mgInput.genCount];
            mgInput.thr_c_b       = new float[mgInput.genCount];
            mgInput.thr_c_c       = new float[mgInput.genCount];
            mgInput.thr_min_dtime = new float[mgInput.genCount];
            mgInput.thr_min_utime = new float[mgInput.genCount];
            mgInput.p_thr_max     = new float[mgInput.genCount];

            int iGen = 0;
            for (int i = 0; i < generatorEntries.Count; i++)
            {
                var g = generatorEntries[i];
                for (int j = 0; j < generatorEntries[i].Quantity; j++)
                {
                    mgInput.thr_c_a[iGen]       = g.generator.a;
                    mgInput.thr_c_b[iGen]       = g.generator.b;
                    mgInput.thr_c_c[iGen]       = g.generator.c;
                    mgInput.thr_min_dtime[iGen] = g.generator.minDTime;
                    mgInput.thr_min_utime[iGen] = g.generator.minUTime;
                    mgInput.p_thr_max[iGen]     = g.generator.ratedPower;
                    iGen++;
                }
            }

            mgInput.p_bat_max = batteryRatedPower;
            mgInput.e_bat_max = batteryRatedEnergy;
            mgInput.canBuy = canBuy;
            mgInput.canSell = canSell;

            microgrid.NotifyLoaded();
            var uiInputs = FindObjectsOfType<MicrogridInputFields>();
            for (int i = 0; i < uiInputs.Length; i++)
            {
                uiInputs[i].MicrogridToAllUI();
            }

            loadMultiplier = 1.0f;
            renewableMultiplier = 1.0f;
            Close();

        }

        private void GetThermalMaxPower()
        {
            float p = 0.0f;
            foreach (var ge in generatorEntries)
            {
                p += ge.generator.ratedPower * ge.Quantity;
            }

            maxThermalPowerIF.text = p.ToString("F3");
        }

        private void AddGeneratorEntry(string generatorName, ThermalGenerator generator)
        {
            bool generatorExists = false;
            foreach (var ge in generatorEntries)
            {
                if (string.Equals(ge.generatorName, generatorName))
                {
                    generatorExists = true;
                    break;
                }
            }

            if (!generatorExists)
            {
                var obj = Instantiate(generatorEntryPrototype, generatorContainer);
                obj.OnDeleteButton += DeleteGeneratorEntry;
                obj.OnQuantityChange += (s, e) => GetThermalMaxPower();
                obj.SetEntry(generatorName, generator, 0);
                generatorEntries.Add(obj);
            }
        }

        private void DeleteGeneratorEntry(object sender, ThermalGeneratorEntry e)
        {
            bool generatorExists = false;
            foreach (var ge in generatorEntries)
            {
                if (string.Equals(ge.generatorName, e.generatorName))
                {
                    generatorExists = true;
                    break;
                }
            }

            if (generatorExists)
            {
                generatorEntries.Remove(e);
                Destroy(e.gameObject);
            }
        }
    }
}
