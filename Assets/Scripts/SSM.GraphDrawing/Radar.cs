using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SSM.GraphDrawing
{
    [Serializable]
    public class Radar
    {
        public int Dimensions
        {
            get => dimensions;
            set => SetDimensions(value);
        }

        public float Min
        {
            get
            {
                if (isDirtyMinMax)
                {
                    min = CalculateMin();
                    max = CalculateMax();
                    isDirtyMinMax = false;
                }

                return min;
            }
        }

        public float Max
        {
            get
            {
                if (isDirtyMinMax)
                {
                    min = CalculateMin();
                    max = CalculateMax();
                    isDirtyMinMax = false;
                }

                return max;
            }
        }

        [SerializeField] private int dimensions;
        [SerializeField] private List<Layer> layers = new List<Layer>();
        [SerializeField] private bool isDirtyMinMax = true;
        [SerializeField] private float min;
        [SerializeField] private float max;
        [SerializeField] private int layerCount = 1;

        public Radar()
        {
            layerCount = 1;
        }

        public Radar(int dimensions)
        {
            if (dimensions < 0)
            {
                throw new ArgumentException();
            }

            layerCount = 1;
            this.dimensions = dimensions;
            layers = new List<Layer>(dimensions);

            for (int i = 0; i < layerCount; i++)
            {
                layers.Add(new Layer(dimensions));
            }
        }

        public void SetLayer(int layerIndex, IList<int> values)
        {
            if (layerIndex < 0)
            {
                throw new ArgumentException();
            }

            if (values == null)
            {
                throw new ArgumentNullException();
            }

            if (values.Count != Dimensions)
            {
                throw new ArgumentException();
            }

            layers[layerIndex].Values.Clear();
            layers[layerIndex].Values.AddRange(values);
            isDirtyMinMax = true;
        }

        public List<Vector2> GetLayerCoords(int layerIndex, float min)
        {
            Layer layer = layers[layerIndex];
            var coords  = new List<Vector2>(Dimensions);
            var spread  = Max - min;
            var inc     = 360.0f / Dimensions;
            var angle   = 0.0f;

            for (int i = 0; i < Dimensions; i++, angle += inc)
            {
                var normValue = (layer.Values[i] - min) / spread;
                coords.Add(MathHelper.PolarToCartesian(angle, normValue));
            }

            return coords;
        }

        private void SetDimensions(int dimensions)
        {
            if (this.dimensions < 0)
            {
                throw new ArgumentException();
            }

            this.dimensions = dimensions;
            layers = new List<Layer>(layerCount);

            for (int i = 0; i < layerCount; i++)
            {
                layers.Add(new Layer(dimensions));
            }
        }

        private int CalculateMin() => layers.Count > 0 
            ? layers.Select(x => x.Values.Count > 0 ? x.Values.Min() : 0).Min()
            : 0;

        private int CalculateMax() => layers.Count > 0
            ? layers.Select(x => x.Values.Count > 0 ? x.Values.Max() : 0).Max()
            : 0;

        [Serializable]
        public class Layer
        {
            public List<int> Values;

            public Layer(int dimensions)
            {
                Values = new List<int>(dimensions);
            }
        }
    }
}
