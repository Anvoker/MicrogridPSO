namespace SSM.Grid.Search.Exhaustive
{
    [System.Serializable]
    public class Options
    {
        public int stepCount = 8;
        public int taskCount = 8;

        public Options(int stepCount, int taskCount)
        {
            this.stepCount = stepCount;
            this.taskCount = taskCount;
        }
    }
}