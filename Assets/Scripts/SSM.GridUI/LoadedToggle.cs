using SSM.Grid;
using System;
using UnityEngine;

namespace SSM.GridUI
{
    public class LoadedToggle : MonoBehaviour
    {
        public Microgrid microgrid;
        public UnityEngine.UI.Text label;
        public UnityEngine.UI.Image onTrueImage;
        public UnityEngine.UI.Image onFalseImage;
        public string onTrueText;
        public string onFalseText;
        public LoadState loadState;

        public enum LoadState
        {
            PPV,
            Wind,
            Load
        }

        protected void Awake()
        {
            microgrid = microgrid ?? FindObjectOfType<Microgrid>();
        }

        private void Update()
        {
            bool state = false;

            switch (loadState)
            {
                case LoadState.PPV:
                //state = microgrid.input.pvPlant.loaded;
                break;

                case LoadState.Wind:
                //state = microgrid.input.windPlant.loaded;
                break;

                case LoadState.Load:
                //state = microgrid.input.loadLoaded;
                break;

                default:
                throw new InvalidOperationException();
            }

            if (state)
            {
                label.text = onTrueText;
                onTrueImage.gameObject.SetActive(true);
                onFalseImage.gameObject.SetActive(false);
            }
            else
            {
                label.text = onFalseText;
                onTrueImage.gameObject.SetActive(false);
                onFalseImage.gameObject.SetActive(true);
            }
        }
    }
}
