using System.Runtime.Serialization;

namespace Service.WalletObserver.Grpc.Models
{
    [DataContract]
    public class AddNewWalletRequest
    {
        [DataMember(Order = 1)] public string Name { get; set; }
        [DataMember(Order = 2)] public decimal MinBalanceInUsd { get; set; }
    }
}