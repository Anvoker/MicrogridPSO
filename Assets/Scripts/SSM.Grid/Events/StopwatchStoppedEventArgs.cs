using System;

namespace SSM.Grid
{
    public class StopwatchStoppedEventArgs : EventArgs
    {
        public StopwatchStoppedEventArgs(long miliseconds)
        {
            this.miliseconds = miliseconds;
        }

        public long miliseconds;
    }
}
