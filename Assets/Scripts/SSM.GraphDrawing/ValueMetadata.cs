namespace SSM.GraphDrawing
{
    public struct ValueMetadata
    {
        public string name;
        public string unit;

        public ValueMetadata(string name, string unit)
        {
            this.name = name;
            this.unit = unit;
        }

        public ValueMetadata(string[] s)
        {
            if (s == null)
            {
                s = new string[2] { "", "" };
            }

            this.name = s[0];
            this.unit = s[1];
        }
    }
}
