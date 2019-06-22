namespace SSM.Grid
{
    [System.Serializable]
    public class MGOutput
    {
        public int[,] u_thr;

        public float[,] p_thr;
        public float[,] c_thr;

        public float[] p_sum;
        public float[] p_res;
        public float[] e_thr_sum;
        public float[] p_thr_sum;
        public float[] p_thr_max_sum;
        public float[] p_bat;
        public float[] p_sys;
        public float[] p_target;
        public float[] c_bat;
        public float[] c_thr_sum;
        public float[] c_sys;
        public float[] soc;
        public float[] convergence;

        public float e_sys_total;
        public float e_thr_total;
        public float c_sys_total;
        public float c_thr_total;
    }
}