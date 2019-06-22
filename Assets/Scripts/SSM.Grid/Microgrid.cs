using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine.UI;
using System.Linq;

namespace SSM.Grid
{
    public class Microgrid : MonoBehaviour
    {
        public MicrogridAlgorithm calculator;
        public event CalculatedEventHandler OnCalculated;
        public event LoadedEventHandler OnLoaded;
        public delegate void CalculatedEventHandler(object sender, CalculatedEventArgs e);
        public delegate void LoadedEventHandler(object sender, LoadedEventArgs e);

        public class CalculatedEventArgs : EventArgs { }
        public class LoadedEventArgs : EventArgs { }

        public MGInput Input => input;
        public MGOutput Result => result;

        [SerializeField]
        private MGInput input;

        [SerializeField]
        private MGOutput result;
        private List<Action> pending = new List<Action>();

        Button[] interactableButtons;
        InputField[] interactableFields;
        Toggle[] interactableToggles;

        private bool CheckTimeBasedArrays()
        {
            return input.p_load?.Length    >= input.tCount
                && input.p_w?.Length       >= input.tCount
                && input.p_pv?.Length      >= input.tCount
                && input.price?.Length     >= input.tCount;
        }

        private bool CheckTHRBasedArrays()
        {
            return input.p_thr_max.Length     >= input.genCount
                && input.thr_c_a.Length       >= input.genCount
                && input.thr_c_b.Length       >= input.genCount
                && input.thr_c_c.Length       >= input.genCount
                && input.thr_min_dtime.Length >= input.genCount
                && input.thr_min_utime.Length >= input.genCount;
        }

        public void NotifyLoaded()
        {
            OnLoaded?.Invoke(this, new LoadedEventArgs());
        }

        public void Calculate()
        {
            CalculateAsync(false);
        }

        public void CalculateIgnoreDirty()
        {
            CalculateAsync(true);
        }

        public void QueueInvocation(Action fn)
        {
            lock (pending)
            {
                pending.Add(fn);
            }
        }

        private void CalculateAsync(bool ignoreDirty)
        {
            if (calculator != null
                && (ignoreDirty || !input.dirty)
                && CheckTimeBasedArrays()
                && CheckTHRBasedArrays())
            {
                OnStart();

#if !UNITY_WEBGL || (UNITY_WEBGL && UNITY_SSM_WEBGL_THREADING_CAPABLE)
                var t = calculator.Calculate(input);
                StartCoroutine(TaskCoroutine(t));
#else
                result = calculator.Calculate(input).Result;
                OnComplete();
#endif
            }
        }

#if !UNITY_WEBGL || (UNITY_WEBGL && UNITY_SSM_WEBGL_THREADING_CAPABLE)
        private IEnumerator TaskCoroutine(Task<MGOutput> t)
        {
            while (!t.IsCompleted && !t.IsCanceled && !t.IsFaulted)
            {
                yield return null;
            }

            result = t.Result;
            OnComplete();
        }
#endif

        private void OnComplete()
        {
            Input.dirty = false;
            OnCalculated?.Invoke(this, new CalculatedEventArgs());

            foreach (Button button in interactableButtons)
            {
                button.interactable = true;
            }

            foreach (InputField field in interactableFields)
            {
                field.interactable = true;
            }

            foreach (Toggle toggle in interactableToggles)
            {
                toggle.interactable = true;
            }
        }

        private void OnStart()
        {
            interactableButtons = FindObjectsOfType<Button>().
                Where(x => x.interactable == true).ToArray();
            interactableFields = FindObjectsOfType<InputField>().
                Where(x => x.interactable == true).ToArray();
            interactableToggles = FindObjectsOfType<Toggle>().
                Where(x => x.interactable == true).ToArray();

            foreach (Button button in interactableButtons)
            {
                button.interactable = false;
            }

            foreach (InputField field in interactableFields)
            {
                field.interactable = false;
            }

            foreach (Toggle toggle in interactableToggles)
            {
                toggle.interactable = false;
            }
        }

        private void OnCompleteWrapper()
        {
            QueueInvocation(OnComplete);
        }

        private void InvokePending()
        {
            lock (pending)
            {
                foreach (Action action in pending)
                {
                    action();
                }

                pending.Clear();
            }
        }

        private void Awake()
        {
            calculator = calculator ?? FindObjectOfType<MicrogridAlgorithm>();
            //input.BecameDirty += OnBecomingDirty;
            Application.backgroundLoadingPriority = ThreadPriority.BelowNormal;
        }

        private void OnBecomingDirty(object sender, EventArgs e)
        {
            Calculate();
        }

        private void FixedUpdate()
        {
            InvokePending();
        }
    }
}
