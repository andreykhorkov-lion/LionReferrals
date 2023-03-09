using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LionReferrals
{
    public interface IReferrable
    {
        int TargetInvitesCount { get; }
        int InviteesCount { get; }
        IDictionary<string, RewardTypes[]> RewardCollection { get; }
        event Action<TaskCompletionSource<bool>> ReferralRewardAvailable;
        event Action ReferralRewardCollectionAcquired;
        void SetADID();
        Task<IDictionary<string, RewardTypes[]>> CheckRewards();
        Task NotifyAboutReward();
        Task ProcessRewards(IDictionary<string, RewardTypes[]> rewardCollections);
        Task OnLaunch();
    }
}