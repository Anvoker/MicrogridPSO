using UnityEngine;
using System.Collections;

namespace WindowManager
{
    public class UIWindowBackground : MonoBehaviour
    {
        [SerializeField]
        private Animator animator;

        [SerializeField]
        private string fadeInTriggerName = "fadeIn";

        [SerializeField]
        private string fadeOutTriggerName = "fadeOut";

        public void FadeIn()
        {
            animator.SetTrigger(fadeInTriggerName);
        }

        public void FadeOut()
        {
            animator.SetTrigger(fadeOutTriggerName);
        }
    }
}