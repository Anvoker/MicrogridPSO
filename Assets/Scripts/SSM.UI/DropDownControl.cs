using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SSM.GridUI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Toggle))]
    public class DropDownControl : MonoBehaviour
    {
        public RectTransform menu;
        public RectTransform menuSurface;
        public Toggle toggle;

        private GameObject blockerGO;
        private EventTrigger menuSurfaceEventTrigger;
        private bool isPointerOverMenuSurface;
        private EventTrigger.Entry eventEnter;
        private EventTrigger.Entry eventExit;

        public void OnToggle()
        {
            if (toggle.isOn)
            {
                ToggleOn();
            }
            else
            {
                ToggleOff();
            }
        }

        public void ToggleOn()
        {
            menu.gameObject.SetActive(true);
            toggle.isOn = true;
            CreateBlocker();
        }

        public void ToggleOff()
        {
            menu.gameObject.SetActive(false);
            toggle.isOn = false;
            DestroyBlocker();
        }

        private void CreateBlocker()
        {
            if (blockerGO == null)
            {
                blockerGO = new GameObject();
                var rt = blockerGO.AddComponent<RectTransform>();
                var eventTrigger = blockerGO.AddComponent<EventTrigger>();
                var image = blockerGO.AddComponent<Image>();
                var canvas = GetComponentInParent<Canvas>();
                blockerGO.transform.SetParent(canvas.transform);
                rt.anchoredPosition3D = Vector3.zero;
                rt.localScale = Vector3.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.SetPivot(PivotPresets.MiddleCenter);
                rt.SetAnchor(AnchorPresets.StretchAll);
                image.color = Color.clear;

                var entryDown = new EventTrigger.Entry();
                entryDown.eventID = EventTriggerType.PointerDown;
                entryDown.callback.AddListener(
                    (eventData) => TryBlockerClose());
                eventTrigger.triggers.Add(entryDown);

                var entryClick = new EventTrigger.Entry();
                entryClick.eventID = EventTriggerType.PointerClick;
                entryClick.callback.AddListener(
                    (eventData) => TryBlockerClose());
                eventTrigger.triggers.Add(entryClick);
            }
        }

        private void TryBlockerClose()
        {
            if (!isPointerOverMenuSurface)
            {
                ToggleOff();
            }
        }

        private void DestroyBlocker()
        {
            if (blockerGO != null)
            {
                Destroy(blockerGO);
                blockerGO = null;
            }
        }

        private void OnEnable()
        {
            if (!Application.isPlaying) { return; }

            if (menuSurfaceEventTrigger == null)
            {
                menuSurfaceEventTrigger = menuSurface.gameObject.AddComponent<EventTrigger>();

                if (eventEnter == null)
                {
                    eventEnter = new EventTrigger.Entry();
                    eventEnter.eventID = EventTriggerType.PointerEnter;
                    eventEnter.callback.AddListener(
                        (eventData) => OnMenuSurfaceEnter());
                    menuSurfaceEventTrigger.triggers.Add(eventEnter);
                }

                if (eventExit == null)
                {
                    eventExit = new EventTrigger.Entry();
                    eventExit.eventID = EventTriggerType.PointerExit;
                    eventExit.callback.AddListener(
                        (eventData) => OnMenuSurfaceExit());
                    menuSurfaceEventTrigger.triggers.Add(eventExit);
                }
            }
            else
            {
                menuSurfaceEventTrigger.enabled = true;
            }
        }

        private void OnDisable()
        {
            if (!Application.isPlaying) { return; }

            if (menuSurfaceEventTrigger != null)
            {
                menuSurfaceEventTrigger.enabled = false;
            }
        }

        private void OnMenuSurfaceEnter()
        {
            isPointerOverMenuSurface = true;
        }

        private void OnMenuSurfaceExit()
        {
            isPointerOverMenuSurface = false;
        }

        private void Awake()
        {
            toggle = GetComponent<Toggle>();
        }

        private void Reset()
        {
            toggle = GetComponent<Toggle>();
        }
    }
}