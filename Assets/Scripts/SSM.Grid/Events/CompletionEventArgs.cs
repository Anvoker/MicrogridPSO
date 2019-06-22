using System;

namespace SSM.Grid
{
    public class CompletionEventArgs : EventArgs
    {
        public float Completion { get; }

        public CompletionEventArgs(float completion)
        {
            Completion = completion;
        }
    }
}