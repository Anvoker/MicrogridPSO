using System;

namespace SSM.Grid
{
    public class RunStatusEventArgs : EventArgs
    {
        public RunStatus Status { get; }

        public RunStatusEventArgs(RunStatus status)
        {
            this.Status = status;
        }
    }
}