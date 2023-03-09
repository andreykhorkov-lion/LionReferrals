using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nakama;
using Newtonsoft.Json;
using UnityEngine;

namespace LionReferrals
{
    public class LionData
    {
        [JsonProperty(PropertyName = "inviteeToInviterData")] 
        public IDictionary<string, InviteeData> InviteeToInviterData;
        
        [JsonProperty(PropertyName = "inviterToRewardCollections")] 
        public IDictionary<string, IDictionary<string, RewardTypes[]>> InviterToRewardCollections;
    }
        
    public class InviteeData
    {
        [JsonProperty(PropertyName = "attempts")] public int Attempts;
        [JsonProperty(PropertyName = "inviterToken")] public string InviterToken;
    }

    public enum RewardTypes //do not change order
    {
        Installation,
        PlayerProgress
    }

    public class ClaimRewardPayload
    {
        [JsonProperty(PropertyName = "inviterToken")] private string InviterToken { get; }
        [JsonProperty(PropertyName = "inviteeToken")] private string InviteeToken { get; }

        public ClaimRewardPayload(string inviterToken, string inviteeToken)
        {
            InviterToken = inviterToken;
            InviteeToken = inviteeToken;
        }
    }
    
    public class AdjustReferrable : IReferrable
    {
        private readonly ISession _session;
        private readonly IClient _client;
        private readonly IRPCMediator _rpcMediator;
        private readonly TrackerSettings _trackerSettings;

        private int _inviteesCount;

        public event Action<TaskCompletionSource<bool>> ReferralRewardAvailable = delegate {  };
        public event Action ReferralRewardCollectionAcquired = delegate {  };
        
        public IDictionary<string, RewardTypes[]> RewardCollection { get; private set; }
        int IReferrable.InviteesCount => _inviteesCount;
        int IReferrable.TargetInvitesCount => 3; // todo: take from backend

        public AdjustReferrable(INotificationListener notificationListener, ISession session, IClient client)
        {
            _session = session;
            _client = client;
            _rpcMediator = new NakamaRPCMediator(_client, _session);
        }

        void IReferrable.SetADID()
        {
            Utils.SetAdvertiserId();
        }

        async Task IReferrable.OnLaunch()
        {
            await _rpcMediator.SendRPC(new { adid = Utils.ADID }, "onLaunch");
        }

        async Task<IDictionary<string, RewardTypes[]>> IReferrable.CheckRewards()
        {
            if (_session == null || _client == null)
            {
                return null;
            }
            
            var apiList = await _client.ListStorageObjectsAsync(_session, "lionData");
            
            if (apiList == null || !apiList.Objects.Any())
            {
                return null;
            }
            
            var rewardData = apiList.Objects.FirstOrDefault()?.Value;

            if (rewardData == null)
            {
                return null;
            }
            
            var map = JsonConvert.DeserializeObject<IDictionary<string, LionData>>(rewardData);
            
            if (map == null || !map.TryGetValue("referralData", out var referralData))
            {
                return null;
            }

            var token = Utils.CreateMD5(_session.UserId);

            if (!referralData.InviterToRewardCollections.TryGetValue(token, out var rewardCollection))
            {
                return null;
            }

            var toDelete = new HashSet<string>();
            
            foreach (var kvp in rewardCollection)
            {
                if (kvp.Value == null || kvp.Value.Length == 0)
                {
                    toDelete.Add(kvp.Key);
                }
            }

            foreach (var key in toDelete)
            {
               rewardCollection.Remove(key);
            }

            RewardCollection = rewardCollection;
            _inviteesCount = rewardCollection.Count;

            return rewardCollection.Count == 0 ? null : rewardCollection;
        }

        async Task IReferrable.NotifyAboutReward()
        {
            var tcs = new TaskCompletionSource<bool>();
            ReferralRewardAvailable.Invoke(tcs);
            await tcs.Task;
        }

        async Task IReferrable.ProcessRewards(IDictionary<string, RewardTypes[]> rewardCollections)
        {
            foreach (var kvp in rewardCollections)
            {
                var inviterToken = Utils.CreateMD5(_session.UserId);
                var inviteeToken = kvp.Key;
                var payload = new ClaimRewardPayload(inviterToken, inviteeToken);
                var isSuccess = await _rpcMediator.SendRPC(payload, "claimReferralRewards");
                
                if (isSuccess)
                {
                    AcquireRewards(kvp.Value);
                }
            }
        }

        private void AcquireRewards(IEnumerable<RewardTypes> rewardTypes)
        {
            foreach (var rewardType in rewardTypes)
            {
                Debug.Log($"claiming reward for: {rewardType}");
            }
        }
    }
}