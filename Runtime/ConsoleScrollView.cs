using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DeveloperConsole
{
    [RequireComponent(typeof(ScrollRect))]
    public class SmartScroll : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private float scrollSpeed = 5f;
        [SerializeField] private float bottomThreshold = 1f; // pixels

        private bool m_UserScrolling = false;   // True if the user is actively scrolling
        private bool m_StickToBottom = true;    // True if we should auto-scroll
        private bool m_IgnoreCallback = false;  // Prevent recursion when changing scroll programmatically

        private void Awake()
        {
            if (!scrollRect) scrollRect = GetComponent<ScrollRect>();
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }

        /// <summary>
        /// Call this whenever new content is added
        /// </summary>
        public void OnNewContent()
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }

        private void Update()
        {
            AutoScrollToBottom();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            m_UserScrolling = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // m_UserScrolling = false;
            // m_StickToBottom = IsContentSmallerThanViewport() || IsAtBottom();
        }

        private void OnScrollValueChanged(Vector2 pos)
        {
            // if (m_IgnoreCallback) return;
            //
            // // User scrolled manually (mouse wheel, touch, drag)
            // if (!m_UserScrolling)
            // {
            //     m_UserScrolling = true;
            // }
            //
            // m_StickToBottom = IsContentSmallerThanViewport() || IsAtBottom();
        }

        private void AutoScrollToBottom()
        {
            // scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
