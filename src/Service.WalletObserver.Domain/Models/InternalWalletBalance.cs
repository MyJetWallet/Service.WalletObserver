using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.WalletObserver.Domain.Models
{
    [DataContract]
    public class InternalWalletBalance
    {
        [DataMember(Order = 1)] public string WalletName { get; set; }
        [DataMember(Order = 2)] public string Asset { get; set; }
        [DataMember(Order = 3)] public decimal Volume { get; set; }
        [DataMember(Order = 4)] public decimal UsdVolume { get; set; }
        [DataMember(Order = 5)] public decimal MinBalanceInUsd { get; set; }
    }
}