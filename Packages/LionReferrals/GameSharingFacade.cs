using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace LionReferrals
{
    public class GameSharingFacade : MonoBehaviour
    {
        [SerializeField] private SharePageView _pageView;
        [SerializeField] private Button _sharePageBtn;
        [SerializeField] private TrackerSettings _trackerSettings;

        private INotificationListener _notificationListener;

        public IReferrable Referrable { get; private set; }

        private void Awake()
        {
            _sharePageBtn.onClick.AddListener(Show);
            _pageView.InviteBtn.onClick.AddListener(() => StartCoroutine(InviteFriends()));

            var nakamaConnection = NakamaController.Instance.GetConnection();
            _notificationListener = new NakamaNotificationListener(nakamaConnection.Socket);
            Referrable = new AdjustReferrable(_notificationListener, nakamaConnection.Session, nakamaConnection.Client);
        }

        private void Start()
        {
            Hide();
            OnStart();
        }

        private async void OnStart()
        {
            Referrable.SetADID();
            await Task.Delay(2); // await for till referral reward record is created by Adjust real-time callback
            await Referrable.OnLaunch(); //at this point invitee is going to find his record by ADID and swap it with (md5(userId))
            var rewardCollections = await Referrable.CheckRewards(); //check rewards for self
            
            if (rewardCollections == null || rewardCollections.Count == 0)
            {
                return;
            }
            
            await Referrable.NotifyAboutReward();
            await Referrable.ProcessRewards(rewardCollections);
        }
        
        private string CreateReferralLink(string id)
        {
            return $"{_trackerSettings.URL}/{_trackerSettings.TrackingId}?label={Utils.CreateMD5(id)}";
        }

        private void Show()
        {
            _pageView.Show();
        }

        private void Hide()
        {
            _pageView.Hide();
        }

        private IEnumerator InviteFriends()
        {
            var link = CreateReferralLink(NakamaController.Instance.Account.User.Id);
            GUIUtility.systemCopyBuffer = link;
            Debug.Log($"link copied to buffer:\n{link}");
            var tex = new Texture2D(2, 2);
            yield return StartCoroutine(Utils.SetSharePic("sharePic.png", tex));
            ShareOutside(link, tex);
            Destroy(tex);
            Hide();
        }

        private static void ShareOutside(string referralLink, Texture2D tex)
        {
            new NativeShare().AddFile(tex)
                .SetSubject("Share Merge Life").SetText("Share Merge Life Custom Message\n").SetUrl(referralLink)
                .SetCallback((result, shareTarget) => Debug.Log($"Share result: {result}, selected app: {shareTarget}"))
                .SetTitle("Share Merge Life").Share();

            // Share on WhatsApp only, if installed (Android only)
            //if( NativeShare.TargetExists( "com.whatsapp" ) )
            //	new NativeShare().AddFile( filePath ).AddTarget( "com.whatsapp" ).Share();
        }
    }
}