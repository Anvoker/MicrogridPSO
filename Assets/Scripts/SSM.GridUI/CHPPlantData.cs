using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SSM.Grid;

namespace SSM.GridUI
{
    public class CHPPlantData : MonoBehaviour
    {
        public Microgrid microgrid;
        public Toggle stateOnToggle;
        public Toggle stateOffToggle;
        public InputField ratedPowerInput;
        public InputField minUpTimeInput;
        public InputField minDownTimeInput;
        public InputField aInput;
        public InputField bInput;
        public InputField cInput;

        public void OnEnable()
        {
            microgrid = microgrid ?? (Microgrid)FindObjectOfType(typeof(Microgrid));
            GetData();
        }

        private void GetData()
        {/*
            GetBoolForToggle(stateOnToggle, stateOffToggle, microgrid.input.chp.state);
            ratedPowerInput.text = microgrid.input.chp.ratedPower.ToString("0.00000");
            minUpTimeInput.text = microgrid.input.chp.minUpTime.ToString("0.00000");
            minDownTimeInput.text = microgrid.input.chp.minDownTime.ToString("0.00000");
            aInput.text = microgrid.input.chp.costA.ToString("0.00000");
            bInput.text = microgrid.input.chp.costB.ToString("0.00000");
            cInput.text = microgrid.input.chp.costC.ToString("0.00000");
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

        public void SetRatedPower(InputField i)
        {
            //microgrid.input.chp.ratedPower = float.Parse(i.text);
        }

        public void SetMinUpTime(InputField i)
        {
            //microgrid.input.chp.minUpTime = float.Parse(i.text);
        }

        public void SetMinDownTime(InputField i)
        {
            //microgrid.input.chp.minDownTime = float.Parse(i.text);
        }

        public void SetCostA(InputField i)
        {
            //microgrid.input.chp.costA = float.Parse(i.text);
        }

        public void SetCostB(InputField i)
        {
            //microgrid.input.chp.costB = float.Parse(i.text);
        }

        public void SetCostC(InputField i)
        {
            //microgrid.input.chp.costC = float.Parse(i.text);
        }
    }
}
