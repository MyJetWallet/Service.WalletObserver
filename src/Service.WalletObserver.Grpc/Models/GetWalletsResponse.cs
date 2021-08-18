using System.Collections.Generic;
using System.Runtime.Serialization;
using Service.WalletObserver.Domain.Models;

namespace Service.WalletObserver.Grpc.Models
{
    [DataContract]
    public class GetWalletsResponse
    {
        [DataMember(Order = 1)] public bool Success { get; set; }
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
        [DataMember(Order = 3)] public List<InternalWallet> WalletList { get; set; }
    }
}