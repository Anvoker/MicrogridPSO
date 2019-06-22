namespace SSM.GraphDrawing
{
    [System.Serializable]
    public struct ModifiedCoord
    {
        public int coordIndex;
        public float magnitude;

        public ModifiedCoord(int coordIndex, float magnitude)
        {
            this.coordIndex = coordIndex;
            this.magnitude = magnitude;
        }
    }
}