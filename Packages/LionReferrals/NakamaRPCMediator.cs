using System;
using System.Threading.Tasks;
using Nakama;
using Newtonsoft.Json;
using UnityEngine;

namespace LionReferrals
{
    public class NakamaRPCMediator : IRPCMediator
    {
        private readonly IClient _client;
        private readonly ISession _session;

        public NakamaRPCMediator(IClient client, ISession session)
        {
            _client = client;
            _session = session;
        }
        
        async Task<bool> IRPCMediator.SendRPC(object payload, string rpcMethodId)
        {
            try
            {
                var json = JsonConvert.SerializeObject(payload);
                var apiRpc = await _client.RpcAsync(_session, rpcMethodId, json);
                return apiRpc != null;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }   
        }
    }
}