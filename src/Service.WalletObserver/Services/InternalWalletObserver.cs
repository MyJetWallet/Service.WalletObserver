using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Service.Balances.Grpc;
using Service.Balances.Grpc.Models;
using Service.IndexPrices.Client;
using Service.WalletObserver.Domain.Models;
using Service.WalletObserver.Grpc;
using Service.WalletObserver.Grpc.Models;

namespace Service.WalletObserver.Services
{
    public class InternalWalletObserver: IInternalWalletObserver
    {
        private readonly ILogger<InternalWalletObserver> _logger;
        private readonly InternalWalletStorage _internalWalletStorage;
        private readonly IWalletBalanceService _walletBalanceService;
        private readonly IIndexPricesClient _indexPricesClient;

        public InternalWalletObserver(ILogger<InternalWalletObserver> logger,
            InternalWalletStorage internalWalletStorage,
            IWalletBalanceService walletBalanceService,
            IIndexPricesClient indexPricesClient)
        {
            _logger = logger;
            _internalWalletStorage = internalWalletStorage;
            _walletBalanceService = walletBalanceService;
            _indexPricesClient = indexPricesClient;
        }
        
        // TODO: при каждом апдейте будут затираться минимумы по другим ассетам
        public async Task<AddNewWalletResponse> UpsertWalletAsync(AddNewWalletRequest request)
        {
            _logger.LogInformation($"AddNewWalletAsync receive request: {JsonConvert.SerializeObject(request)}");
            try
            {
                var wallet = new InternalWallet()
                {
                    Name = request.Name,
                    AssetInWalletCollection = await GetAssetInWalletCollection(request.Name)
                };
                var assetBalance = wallet.AssetInWalletCollection.FirstOrDefault(e => e.Asset == request.Asset);

                if (assetBalance == null)
                {
                    wallet.AssetInWalletCollection.Add(new AssetInWallet()
                    {
                        Asset = request.Asset,
                        Volume = 0,
                        UsdVolume = 0,
                        MinBalanceInUsd = request.MinBalanceInUsd
                    });
                }
                else
                {
                    assetBalance.MinBalanceInUsd = request.MinBalanceInUsd;
                }
                
                await _internalWalletStorage.SaveWallet(wallet);
            } 
            catch (Exception ex)
            {
                _logger.LogError($"AddNewWalletAsync throw exception: {JsonConvert.SerializeObject(ex)}");
                return new AddNewWalletResponse()
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
            return new AddNewWalletResponse()
            {
                Success = true
            };
        }

        private async Task<List<AssetInWallet>> GetAssetInWalletCollection(string walletName)
        {
            var balancesDto = await _walletBalanceService.GetWalletBalancesAsync(new GetWalletBalancesRequest()
            {
                WalletId = walletName
            });
            var result = new List<AssetInWallet>();
            if (balancesDto?.Balances != null && balancesDto.Balances.Any())
            {
                balancesDto.Balances.ForEach(balance =>
                {
                    var volume = (decimal) balance.Balance;
                    var (indexPrice, usdVolume) =
                        _indexPricesClient.GetIndexPriceByAssetVolumeAsync(balance.AssetId, volume);
                    
                    result.Add(new AssetInWallet()
                    {
                        Asset = balance.AssetId,
                        Volume = volume,
                        UsdVolume = usdVolume
                    });
                });
            }
            return result;
        }

        public async Task<GetWalletsResponse> GetWalletsAsync()
        {
            _logger.LogInformation($"GetWalletsAsync receive request.");

            var response = new GetWalletsResponse();
            try
            {
                response.WalletList = _internalWalletStorage.GetWallets();
                response.Success = true;
            } 
            catch (Exception ex)
            {
                _logger.LogError($"GetWalletsAsync throw exception: {JsonConvert.SerializeObject(ex)}");
                return new GetWalletsResponse()
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
            return response;
        }

        public async Task<RemoveWalletResponse> RemoveWalletAsync(RemoveWalletRequest request)
        {
            _logger.LogInformation($"RemoveWalletAsync receive request: {JsonConvert.SerializeObject(request)}");
            try
            {
                await _internalWalletStorage.RemoveWallet(request.Name);
            } 
            catch (Exception ex)
            {
                _logger.LogError($"RemoveWalletAsync throw exception: {JsonConvert.SerializeObject(ex)}");
                return new RemoveWalletResponse()
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
            return new RemoveWalletResponse()
            {
                Success = true
            };
        }
    }
}
