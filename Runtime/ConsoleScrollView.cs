using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DeveloperConsole
{
    [RequireComponent(typeof(ScrollRect))]
    public class SmartScroll : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect;

        private void Awake()
        {
            if (!scrollRect)
                scrollRect = GetComponent<ScrollRect>();
        }

        public void OnNewContent()
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
