using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using SSM.Grid;
using UnityEngine.Events;

namespace SSM.GridUI
{
    public class MicrogridInputFields : MonoBehaviour
    {
        public Microgrid microgrid;
        public List<TMP_InputField> inputFields;
        public List<MicrogridVar> inputFieldsVars;

        private string floatFormat = "0.0000000";

        public void MicrogridToAllUI()
        {
            foreach (TMP_InputField i in inputFields)
            {
                MicrogridToInputField(i);
            }
        }

        public void InputFieldToMicrogrid(TMP_InputField i)
        {
            if (inputFields.Contains(i))
            {
                var varType = inputFieldsVars[inputFields.IndexOf(i)];
                float f = 0.0f;
                if (float.TryParse(i.text, out f))
                {
                    try
                    {
                        MGMisc.accessorFloats[varType].setter.Invoke(microgrid, f);
                    }
                    catch (KeyNotFoundException e)
                    {
                        throw new KeyNotFoundException(varType + " key doesn't exist in accessor dictionary.", e);
                    }
                }
            }
            MicrogridToInputField(i);
        }

        public void MicrogridToInputField(TMP_InputField i)
        {
            if (inputFields.Contains(i))
            {
                var varType = inputFieldsVars[inputFields.IndexOf(i)];
                try
                {
                    var value = MGMisc.accessorFloats[varType].getter.Invoke(microgrid);
                    i.text = value.ToString(floatFormat);
                }
                catch (KeyNotFoundException e)
                {
                    throw new KeyNotFoundException(varType + " key doesn't exist in accessor dictionary.", e);
                }
            }
        }

        private void OnEnable()
        {
            microgrid = microgrid ?? (Microgrid)FindObjectOfType(typeof(Microgrid));
            MicrogridToAllUI();
        }

        protected void Awake()
        {
            SubscribeChildrenInputFields();
        }

        [EasyButtons.Button]
        private void GetChildrenInputField()
        {
            inputFields = new List<TMP_InputField>(GetComponentsInChildren<TMP_InputField>());
        }

        private void SubscribeChildrenInputFields()
        {
            var inputFields = GetComponentsInChildren<TMP_InputField>();
            for (int i = 0; i < inputFields.Length; i++)
            {
                var inputField = inputFields[i];
                inputField.onEndEdit
                    .AddListener(new UnityAction<string>((s) => InputFieldToMicrogrid(inputField)));
            }
        }
    }
}
