using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SSM.GraphDrawing;
using SSM.Grid;

namespace SSM.GridUI
{
    public class EDCanvasManager : MonoBehaviour
    {
        [HideInInspector]
        public Microgrid microgrid;
        public RectTransform prototype;
        public RectTransform rtParent;
        public List<RectTransform> canvases
            = new List<RectTransform>();
        public List<GraphSubscriber> graphSubscribers
            = new List<GraphSubscriber>();

        public void AddCanvases(int count)
        {
            for (int i = 0; i < count; i++)
            {
                AddCanvas();
            }
        }

        public void RemoveCanvases(int count)
        {
            for (int i = 0; i < count; i++)
            {
                RemoveCanvas();
            }
        }

        public void AddCanvas()
        {
            RectTransform newCanvas = Instantiate(prototype);
            newCanvas.transform.SetParent(rtParent, false);
            newCanvas.gameObject.SetActive(true);
            canvases.Add(newCanvas);

            var g = newCanvas.GetComponentInChildren<GraphSubscriber>();
            graphSubscribers.Add(g);
            var setupData = new GraphSubscriber.MicrogridVarAndIndex
            {
                index = canvases.Count - 1,
                mvar = MicrogridVar.PCHP
            };

            g.loadOnSetup = new List<GraphSubscriber.MicrogridVarAndIndex>()
            {
                setupData
            };
        }

        public void RemoveCanvas()
        {
            if (canvases.Count <= 0)
            {
                return;
            }

            var rt = canvases[canvases.Count - 1];
            graphSubscribers.Remove(
                rt.GetComponentInChildren<GraphSubscriber>());
            canvases.Remove(rt);
            Destroy(rt.gameObject);
        }

        private void Awake()
        {
            microgrid = microgrid ?? FindObjectOfType<Microgrid>();
            microgrid.OnCalculated += CheckCHPCount;
        }

        private void Start()
        {
            CheckCHPCount(null, null);
        }

        private void CheckCHPCount(object sender, Microgrid.CalculatedEventArgs e)
        {
            int chpCount = microgrid.Input.genCount;
            if (chpCount > canvases.Count)
            {
                int newCanvasesNeeded = chpCount - canvases.Count;
                AddCanvases(newCanvasesNeeded);
            }
            else if (chpCount < canvases.Count)
            {
                int canvasesToRemove = canvases.Count - chpCount;
                RemoveCanvases(canvasesToRemove);
            }
        }

        private void OnDestroy()
        {
            microgrid.OnCalculated -= CheckCHPCount;
        }
    }
}
