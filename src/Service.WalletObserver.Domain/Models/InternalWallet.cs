using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.WalletObserver.Domain.Models
{
    [DataContract]
    public class InternalWallet
    {
        [DataMember(Order = 1)] public string Name { get; set; }
        [DataMember(Order = 2)] public List<AssetInWallet> AssetInWalletCollection { get; set; }
    }
}