using UnityEngine;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SSM.Grid
{
    [System.Serializable]
    public class MGInput
    {
        public bool dirty;
        public event BecameDirtyEventHandler BecameDirty;
        public delegate void BecameDirtyEventHandler(object sender, BecameDirtyEventArgs e);

        // tIncrementSize * tIncrementCount should be equal to 1440
        // TODO: algorithm doesn't actually account for increment size.
        // It only works properly for tIncrementSize = 1.0f.
        public float tIncrementSize = 1.0f;
        public int tCount = 1440;
        public int genCount = 1;

        public bool canBuy;
        public bool canSell;

        //State Fixed
        public float[] price;
        public float[] p_load;
        public float[] p_w;
        public float[] p_pv;
        public float[] p_thr_max;
        public float[] thr_c_a;
        public float[] thr_c_b;
        public float[] thr_c_c;
        public float[] thr_min_utime;
        public float[] thr_min_dtime;
        public int[] u_thr_init;
        public float p_bat_max;
        public float e_bat_max;
        public float soc_ini;
        public float soc_min;
        public float soc_max;
        public int c_bat_init;

        private bool suspendEvent;

        public void SuspendEvent()
        {
            suspendEvent = true;
            dirty = true;
        }

        public void ResumeEvent()
        {
            suspendEvent = false;
            if (dirty) { BecameDirty?.Invoke(this, new BecameDirtyEventArgs()); }
        }
    }

    public class BecameDirtyEventArgs : EventArgs { }
}