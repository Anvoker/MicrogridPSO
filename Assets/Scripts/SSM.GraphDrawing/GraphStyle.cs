using System;

namespace SSM.GraphDrawing
{
    [Serializable]
    public class GraphStyle
    {
        public GraphValueType minValueType;
        public GraphValueType maxValueType;
        public GraphValueType majorUnitType;
        public GraphValueType minorUnitType;

        public float minValueFixed;
        public float maxValueFixed;
        public float majorUnitFixed;
        public float minorUnitFixed;
    }
}
