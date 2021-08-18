using System.Runtime.Serialization;

namespace Service.WalletObserver.Grpc.Models
{
    [DataContract]
    public class AddNewWalletResponse
    {
        [DataMember(Order = 1)] public bool Success { get; set; }
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
    }
}