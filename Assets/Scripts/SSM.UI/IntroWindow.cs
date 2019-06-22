using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowManager;
using UnityEngine;
using UnityEngine.UI;
using SSM.CSV;
using SSM.Grid;

namespace SSM.UI
{
    public class IntroWindow : UIWindow
    {
        public Button buttonLoadFromPreset;
        public Button buttonLoadFromFile;
        public DataPresetPickerWindow presetPickerWindow;
        public TextAsset defaultCSV;

        private Microgrid microgrid;
        private MicrogridFileInput fileManager;

        protected void Awake()
        {
            fileManager = fileManager ?? FindObjectOfType<MicrogridFileInput>();
            microgrid   = microgrid   ?? FindObjectOfType<Microgrid>();
            buttonLoadFromPreset.onClick.AddListener(ShowPresetPicker);
            buttonLoadFromFile.onClick.AddListener(ShowFileBrowser);
        }

        protected override void Setup() { }

        private void ShowPresetPicker()
        {
            CSVHelper.ReadInputFromString(
                defaultCSV.text,
                microgrid,
                MGMisc.GetInputVars());
            presetPickerWindow.Show(microgrid.Input);
        }

        private void ShowFileBrowser()
        {
            Close();
            fileManager.LoadDialog();
        }
    }
}
