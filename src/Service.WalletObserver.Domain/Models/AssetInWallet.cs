using System.Runtime.Serialization;

namespace Service.WalletObserver.Domain.Models
{
    [DataContract]
    public class AssetInWallet
    {
        [DataMember(Order = 1)] public string Asset { get; set; }
        [DataMember(Order = 2)] public decimal Volume { get; set; }
        [DataMember(Order = 3)] public decimal UsdVolume { get; set; }
    }
}