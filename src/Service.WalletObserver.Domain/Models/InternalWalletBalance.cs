using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.WalletObserver.Domain.Models
{
    [DataContract]
    public class InternalWalletBalance
    {
        [DataMember(Order = 1)] public string BrokerId { get; set; }
        [DataMember(Order = 2)] public string WalletName { get; set; }
        [DataMember(Order = 3)] public string AccountId { get; set; }
        [DataMember(Order = 4)] public string WalletId { get; set; }
        [DataMember(Order = 5)] public string Asset { get; set; }
        [DataMember(Order = 6)] public decimal Volume { get; set; }
        [DataMember(Order = 7)] public decimal UsdVolume { get; set; }
        [DataMember(Order = 8)] public decimal MinBalanceInUsd { get; set; }
        [DataMember(Order = 9)] public InternalWalletTypes WalletTypes { get; set; }

        public InternalWalletBalance GetCopy()
        {
            return new InternalWalletBalance()
            {
                BrokerId = BrokerId,
                WalletName = WalletName,
                AccountId = AccountId,
                WalletId = WalletId,
                Asset = Asset,
                Volume = Volume,
                UsdVolume = UsdVolume,
                MinBalanceInUsd = MinBalanceInUsd
            };
        }
    }
}