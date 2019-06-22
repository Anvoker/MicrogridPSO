using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SSM.Grid;
using SSM.UI;

namespace SSM.GridUI
{
    public class SaveLoadVars : MonoBehaviour
    {
        public MicrogridFileInput mgFileInput;
        public List<MicrogridVar> microgridVariables = new List<MicrogridVar>();

        protected void Awake()
        {
            mgFileInput = mgFileInput ?? FindObjectOfType<MicrogridFileInput>();
        }

        public void Save()
        {
            mgFileInput.SaveDialog(microgridVariables.ToArray());
        }

        public void Load()
        {
            mgFileInput.LoadDialog(microgridVariables.ToArray());
        }
    }
}
