using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SSM.Grid;

namespace SSM.GridUI
{
    public class NetworkPCCData : MonoBehaviour
    {
        public Microgrid microgrid;
        public Toggle stateOnToggle;
        public Toggle stateOffToggle;
        public InputField dispatchingIntervalInput;
        public InputField clearingPriceInput;

        public void OnEnable()
        {
            microgrid = microgrid ?? (Microgrid)FindObjectOfType(typeof(Microgrid));
            GetData();
        }

        private void GetData()
        {/*
            GetBoolForToggle(stateOnToggle, stateOffToggle, microgrid.input.networkPCC.state);
            dispatchingIntervalInput.text = microgrid.input.networkPCC.dispatchingInterval.ToString("0.00000");
            clearingPriceInput.text = microgrid.input.networkPCC.clearingPrice.ToString("0.00000");
            */
        }

        private void GetBoolForToggle(Toggle on, Toggle off, bool state)
        {
            on.isOn = state;
            off.isOn = !state;
        }

        public void SetStateOn(Toggle toggle)
        {
            //if (toggle.isOn) { microgrid.input.chp.state = true; }
        }

        public void SetStateOff(Toggle toggle)
        {
            //if (toggle.isOn) { microgrid.input.chp.state = false; }
        }

        public void SetDispatchingInterval(InputField i)
        {
            //microgrid.input.networkPCC.dispatchingInterval = float.Parse(i.text);
        }

        public void SetClearingPrice(InputField i)
        {
            //microgrid.input.networkPCC.clearingPrice = float.Parse(i.text);
        }
    }
}
