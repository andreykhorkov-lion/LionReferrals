using UnityEngine;
using UnityEngine.UI;

namespace LionReferrals
{
    public class SharePageView : MonoBehaviour
    {
        [SerializeField] private Button _closeBtn;
        [SerializeField] private Button _inviteBtn;
        [SerializeField] private GameObject _raycastBlocker;
        
        public Button InviteBtn => _inviteBtn;
        
        private void Awake()
        {
            _closeBtn.onClick.AddListener(Hide);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            _raycastBlocker.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _raycastBlocker.SetActive(false);
        }
    }
}