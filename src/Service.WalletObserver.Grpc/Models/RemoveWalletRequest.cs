using System.Runtime.Serialization;

namespace Service.WalletObserver.Grpc.Models
{
    [DataContract]
    public class RemoveWalletRequest
    {
        [DataMember(Order = 1)] public string Name { get; set; }
    }
}