using System.Runtime.Serialization;

namespace Service.WalletObserver.Grpc.Models
{
    [DataContract]
    public class AddNewWalletRequest
    {
        [DataMember(Order = 1)] public string BrokerId { get; set; }
        [DataMember(Order = 2)] public string WalletName { get; set; }
        [DataMember(Order = 3)] public string AccountId { get; set; }
        [DataMember(Order = 4)] public string WalletId { get; set; }
        [DataMember(Order = 5)] public string Asset { get; set; }
        [DataMember(Order = 6)] public decimal MinBalanceInUsd { get; set; }
    }
}