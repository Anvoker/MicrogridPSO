using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SSM.GraphDrawing
{
    public class RadarArray : MonoBehaviour
    {
        public RectTransform prototype;
        public RectTransform radarContainer;

        [NonSerialized]
        private List<RadarCell> radarCanvases = new List<RadarCell>();

        public void SetData(int[,] values)
        {
            var d1 = values.GetLength(0);
            var d2 = values.GetLength(1);
            EnsureCount(d1);

            for (int i = 0; i < d1; i++)
            {
                var row = Grid.MGHelper.GetRow(values, i);
                radarCanvases[i].radarCanvas.radar.Dimensions = d2;
                radarCanvases[i].radarCanvas.radar.SetLayer(0, row);
                radarCanvases[i].radarCanvas.isDirty = true;
            }
        }

        private void EnsureCount(int desiredCount)
        {
            if (desiredCount > radarCanvases.Count)
            {
                int delta = desiredCount - radarCanvases.Count;

                for (int i = 0; i < delta; i++)
                {
                    var radarRT = Instantiate(prototype, radarContainer);
                    var radarCanvas = radarRT.GetComponentInChildren<RadarCanvas>();
                    radarRT.gameObject.SetActive(true);
                    radarCanvas.isDirty = true;

                    radarCanvases.Add(new RadarCell()
                    {
                        panel = radarRT,
                        radarCanvas = radarCanvas
                    });
                }
            }
            else if (desiredCount < radarCanvases.Count)
            {
                int delta = radarCanvases.Count - desiredCount;

                for (int i = 0; i < delta; i++)
                {
                    var radarCell = radarCanvases[radarCanvases.Count - 1 - i];
                    Destroy(radarCell.panel);
                }

                radarCanvases.RemoveRange(desiredCount, delta);
            }
        }

        private void Awake()
        {
            prototype.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            prototype.gameObject.SetActive(false);
        }

        [System.Serializable]
        public struct RadarCell
        {
            public RectTransform panel;
            public RadarCanvas radarCanvas;
        }
    }
}
