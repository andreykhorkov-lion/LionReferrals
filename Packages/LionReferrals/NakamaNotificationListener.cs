using Nakama;

namespace LionReferrals
{
    public class NakamaNotificationListener : INotificationListener 
    {
        public enum NotificationCode
        {
            ReferralInstallation = 100,
        }
        
        private readonly ISocket _socket;
        private IApiNotification _notification;

        public NakamaNotificationListener(ISocket socket)
        {
            socket.ReceivedNotification += notification =>
            {
                // _notification = notification;
                // UnityMainThreadDispatcher.Instance().Enqueue(OnNakamaNotificationReceived);
            };
        }
    }
}