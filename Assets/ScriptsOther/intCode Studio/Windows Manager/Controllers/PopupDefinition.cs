using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WindowManager
{
    [Serializable]
    public class PopupDefinition
    {
        [SerializeField]
        public string titleText;

        [SerializeField]
        public string descriptionText;

        [SerializeField]
        public string confirmText;

        [SerializeField]
        public bool hasCancelButton = true;

        [SerializeField]
        public string cancelText;
    }
}