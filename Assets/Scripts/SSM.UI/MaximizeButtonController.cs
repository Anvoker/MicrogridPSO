using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SSM.GridUI
{
    public class MaximizeButtonController : MonoBehaviour
    {
        public FlexibleGridController flexibleGridController;
        public List<Button> buttons;
        public List<GameObject> graphs;
        public Button activeButton;

        public void Maximize(Button button)
        {
            if (activeButton == button)
            {
                foreach (Button b in buttons)
                {
                    int i = buttons.IndexOf(b);
                    graphs[i].SetActive(true);
                }
                flexibleGridController.columns = 2;
                flexibleGridController.rows = 2;
                activeButton = null;
            }
            else if (buttons.Contains(button))
            {
                activeButton = button;
                foreach (Button b in buttons)
                {
                    if (b == button)
                    {
                        int i = buttons.IndexOf(b);
                        graphs[i].SetActive(true);
                    }
                    else
                    {
                        int i = buttons.IndexOf(b);
                        graphs[i].SetActive(false);
                    }
                }
                flexibleGridController.columns = 1;
                flexibleGridController.rows = 1;
            }
        }

        private void Awake()
        {
            flexibleGridController = FindObjectOfType<FlexibleGridController>();
        }
    }
}
