using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Service.Balances.Grpc;
using Service.Balances.Grpc.Models;
using Service.IndexPrices.Client;
using Service.WalletObserver.Domain.Models;

namespace Service.WalletObserver.Services
{
    public class InternalWalletObserverMath
    {
        private readonly IWalletBalanceService _walletBalanceService;
        private readonly IIndexPricesClient _indexPricesClient;

        public InternalWalletObserverMath(IWalletBalanceService walletBalanceService,
            IIndexPricesClient indexPricesClient)
        {
            _walletBalanceService = walletBalanceService;
            _indexPricesClient = indexPricesClient;
        }

        public async Task<List<InternalWalletBalance>> GetInternalWalletBalanceCollection(string walletName)
        {
            var balancesDto = await _walletBalanceService.GetWalletBalancesAsync(new GetWalletBalancesRequest()
            {
                WalletId = walletName
            });
            var result = new List<InternalWalletBalance>();
            if (balancesDto?.Balances != null && balancesDto.Balances.Any())
            {
                balancesDto.Balances.ForEach(balance =>
                {
                    var volume = (decimal) balance.Balance;
                    var (indexPrice, usdVolume) =
                        _indexPricesClient.GetIndexPriceByAssetVolumeAsync(balance.AssetId, volume);
                    
                    result.Add(new InternalWalletBalance()
                    {
                        WalletName = walletName,
                        Asset = balance.AssetId,
                        Volume = volume,
                        UsdVolume = usdVolume
                    });
                });
            }
            return result;
        }
    }
}