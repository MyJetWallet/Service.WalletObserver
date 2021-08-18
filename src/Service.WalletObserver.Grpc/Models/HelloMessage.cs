using System.Runtime.Serialization;
using Service.WalletObserver.Domain.Models;

namespace Service.WalletObserver.Grpc.Models
{
    [DataContract]
    public class HelloMessage : IHelloMessage
    {
        [DataMember(Order = 1)]
        public string Message { get; set; }
    }
}