using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SSM.GraphDrawing;
using System.Linq;
using SSM.Grid;

namespace SSM.GridUI
{
    public class CHPGraphCanvasManager : MonoBehaviour
    {
        [HideInInspector]
        public Microgrid microgrid;
        public MicrogridVar mvar;
        public RectTransform prototype;
        public RectTransform rtParent;
        public List<GraphItem> graphItems = new List<GraphItem>();
        public ContentSizeFitter fitter;
        public int canvasLimitScroll = 10;
        public bool syncAxisLimitsToMax;

        public void AddItems(int count)
        {
            for (int i = 0; i < count; i++)
            {
                AddItem();
            }
        }

        public void RemItems(int count)
        {
            for (int i = 0; i < count; i++)
            {
                RemoveCanvas();
            }
        }

        public void AddItem()
        {
            RectTransform rt = Instantiate(prototype);
            var newItem = rt.GetComponent<GraphItem>();
            rt.SetParent(rtParent, false);
            rt.gameObject.SetActive(true);
            graphItems.Add(newItem);

            var setupData = new GraphSubscriber.MicrogridVarAndIndex
            {
                index = graphItems.Count - 1,
                mvar = this.mvar
            };

            newItem.gSubscriber.loadOnSetup
                = new List<GraphSubscriber.MicrogridVarAndIndex>()
            {
                setupData
            };

            CheckLimit();
            FixLabel();
            SetGlobalAxisLimits();
        }

        public void RemoveCanvas()
        {
            if (graphItems.Count <= 0)
            {
                return;
            }

            var itemToRemove = graphItems[graphItems.Count - 1];
            graphItems.Remove(itemToRemove);
            Destroy(itemToRemove.gameObject);

            CheckLimit();
            FixLabel();
            SetGlobalAxisLimits();
        }

        private void SetGlobalAxisLimits()
        {
            if (syncAxisLimitsToMax)
            {
                float maxY = Mathf.NegativeInfinity;

                for (int i = 0; i < graphItems.Count; i++)
                {
                    for (int j = 0; j < graphItems[i].gCanvas.view.graphs.Count; j++)
                    {
                        var rawCoords = graphItems[i].gCanvas.view.graphs[0].RawCoords;
                        for (int k = 0; k < rawCoords.Count; k++)
                        {
                            if (rawCoords[k].y > maxY)
                            {
                                maxY = rawCoords[k].y;
                            }
                        }
                    }
                }

                foreach (GraphItem item in graphItems)
                {
                    item.gCanvas.view.style.maxAxis = new Vector2(0.0f, maxY);
                    item.gCanvas.view.style.maxAxisYOverride = true;
                }
            }
        }

        private void FixLabel()
        {
            int i = 0;
            foreach (GraphItem item in graphItems)
            {
                item.labelNumber.text = "#" + i.ToString("G");
                i++;
            }
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
            if (chpCount > graphItems.Count)
            {
                int newCanvasesNeeded = chpCount - graphItems.Count;
                AddItems(newCanvasesNeeded);
            }
            else if (chpCount < graphItems.Count)
            {
                int canvasesToRemove = graphItems.Count - chpCount;
                RemItems(canvasesToRemove);
            }

            Invoke(nameof(SetGlobalAxisLimits), 0.1f);
        }

        private void CheckLimit()
        {
            if (graphItems.Count > canvasLimitScroll)
            {
                fitter.enabled = true;
                rtParent.SetAnchor(AnchorPresets.HorStretchTop);
                rtParent.SetPivot(PivotPresets.TopLeft);
            }
            else
            {
                fitter.enabled = false;
                rtParent.SetAnchor(AnchorPresets.StretchAll);
                rtParent.SetPivot(PivotPresets.MiddleCenter);
                rtParent.sizeDelta = Vector2.zero;
            }
        }

        private void OnDestroy()
        {
            microgrid.OnCalculated -= CheckCHPCount;
        }
    }
}
