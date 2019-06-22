using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SSM
{
    [RequireComponent(typeof(RectTransform))]
    public class ToolTip : MonoBehaviour, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public RectTransform rt;
        public float onPointerEnterTime = 0.3f;
        public float iconOffset = 28.0f;
        public TMPro.TMP_Text header;
        public TMPro.TMP_Text body;
        private Coroutine showingCoroutine;

        public void OnPointerEnter(PointerEventData eventData)
        {
            showingCoroutine = StartCoroutine(Show());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (showingCoroutine != null)
            {
                StopCoroutine(showingCoroutine);
            }
            rt.gameObject.SetActive(false);
        }

        private void Awake()
        {
            rt.gameObject.SetActive(false);
        }

        private IEnumerator Show()
        {
            yield return new WaitForSecondsRealtime(onPointerEnterTime);
            rt.gameObject.SetActive(true);

            MoveTooltipToFitScreen();
            MoveTooltipToFitScreen();
        }

        private void MoveTooltipToFitScreen()
        {
            var cameraCorners = new Vector2[4];
            var rectCorners = new Vector3[4];

            cameraCorners[0] = Camera.main.pixelRect.min;
            cameraCorners[1] = new Vector3(Camera.main.pixelRect.yMax,
                Camera.main.pixelRect.xMin);
            cameraCorners[2] = Camera.main.pixelRect.max;
            cameraCorners[3] = new Vector3(Camera.main.pixelRect.yMin,
                Camera.main.pixelRect.xMax);

            rt.GetWorldCorners(rectCorners);

            for (int i = 0; i < 4; i++)
            {
                rectCorners[i] = Camera.main.WorldToScreenPoint(rectCorners[i]);
            }

            Vector2 rtMin = rectCorners[0];
            Vector2 rtMax = rectCorners[2];
            Vector2 camMin = cameraCorners[0];
            Vector2 camMax = cameraCorners[2];

            float anchorMinX = rt.anchorMin.x;
            float anchorMinY = rt.anchorMin.y;
            float anchorMaxX = rt.anchorMax.x;
            float anchorMaxY = rt.anchorMax.y;
            float pivotX = rt.pivot.x;
            float pivotY = rt.pivot.y;
            float x = 0.0f;
            float y = 0.0f;

            if (rtMax.x > camMax.x)
            {
                anchorMinX = 1.0f;
                anchorMaxX = 1.0f;
                pivotX = 1.0f;
            }
            else if (rtMin.x < camMin.x)
            {
                anchorMinX = 0.0f;
                anchorMaxX = 0.0f;
                pivotX = 0.0f;
            }

            if (rtMax.y > camMax.y)
            {
                anchorMinY = 1.0f;
                anchorMaxY = 1.0f;
                pivotY = 1.0f;
            }
            else if (rtMin.y < camMin.y)
            {
                anchorMinY = 0.0f;
                anchorMaxY = 0.0f;
                pivotY = 0.0f;
            }

            if (pivotX == 0.0f)
            {
                x = iconOffset;
            }
            else if (pivotX == 1.0f)
            {
                x = -iconOffset;
            }

            rt.anchorMin = new Vector2(anchorMinX, anchorMinY);
            rt.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
            rt.pivot = new Vector2(pivotX, pivotY);
            rt.anchoredPosition = new Vector2(x, y);
            rt.ForceUpdateRectTransforms();
            Canvas.ForceUpdateCanvases();
        }

        public static Rect RectTransformToScreenSpace(RectTransform transform)
        {
            Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
            Rect rect = new Rect(transform.position.x, Screen.height - transform.position.y, size.x, size.y);
            rect.x -= (transform.pivot.x * size.x);
            rect.y -= ((1.0f - transform.pivot.y) * size.y);
            return rect;
        }
    }
}