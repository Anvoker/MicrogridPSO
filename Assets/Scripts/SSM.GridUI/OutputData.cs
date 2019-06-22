using UnityEngine;
using SSM.Grid;
using TMPro;

namespace SSM.GridUI
{
    public class OutputData : MonoBehaviour
    {
        public Microgrid microgrid;
        public TMP_InputField energyExchangeIF;
        public TMP_InputField thermalCostIF;
        public TMP_InputField thermalEnergyIF;
        public TMP_InputField exchangeCostIF;
        public TMP_InputField totalCostIF;

        private float _e_sys_total = Mathf.Infinity;
        private float _e_thr_total = Mathf.Infinity;
        private float _c_sys_total = Mathf.Infinity;
        private float _c_thr_total = Mathf.Infinity;

        private void Awake()
        {
            microgrid = microgrid ?? FindObjectOfType<Microgrid>();
        }

        private void Update()
        {
            var r = microgrid.Result;

            if (_e_sys_total != r.e_sys_total)
            {
                energyExchangeIF.text = r.e_sys_total.ToString("F1");
                _e_sys_total = r.e_sys_total;
            }

            if (_e_thr_total != r.e_thr_total)
            {
                thermalEnergyIF.text = r.e_thr_total.ToString("F1");
                _e_thr_total = r.e_thr_total;
            }

            if (_c_thr_total != r.c_thr_total)
            {
                totalCostIF.text = (r.c_sys_total + r.c_thr_total).ToString("F1");
                thermalCostIF.text = r.c_thr_total.ToString("F1");
                _c_thr_total = r.c_thr_total;
            }

            if (_c_sys_total != r.c_sys_total)
            {
                totalCostIF.text = (r.c_sys_total + r.c_thr_total).ToString("F1");
                exchangeCostIF.text = r.c_sys_total.ToString("F1");
                _c_sys_total = r.c_sys_total;
            }
            
        }
    }
}
