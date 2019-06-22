using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SSM
{
    public class Program : MonoBehaviour
    {
        [SerializeField] private SSM.UI.IntroWindow introWindow;

        public void Exit()
        {
            Application.Quit();
        }

        protected void Start()
        {
            OpenIntro();
        }

        private void OpenIntro()
        {
            introWindow.Show();
        }
    }
}
