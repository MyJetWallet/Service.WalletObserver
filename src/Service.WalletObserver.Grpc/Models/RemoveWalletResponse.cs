using System.Runtime.Serialization;

namespace Service.WalletObserver.Grpc.Models
{
    [DataContract]
    public class RemoveWalletResponse
    {
        [DataMember(Order = 1)] public bool Success { get; set; }
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
    }
}