using System;

namespace Service.WalletObserver.Domain.Models;

[Flags]
public enum InternalWalletTypes
{
    Default = 0,
    Converter = 1 << 0,
    Bonus = 1 << 1,
    HighYield = 1 << 2,

}