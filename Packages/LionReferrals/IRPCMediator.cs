using System.Threading.Tasks;

namespace LionReferrals
{
    public interface IRPCMediator
    {
        Task<bool> SendRPC(object payload, string rpcMethodId);
    }
}