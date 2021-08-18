using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Service.Balances.Grpc;
using Service.Balances.Grpc.Models;
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

        public InternalWalletObserver(ILogger<InternalWalletObserver> logger,
            InternalWalletStorage internalWalletStorage,
            IWalletBalanceService walletBalanceService)
        {
            _logger = logger;
            _internalWalletStorage = internalWalletStorage;
            _walletBalanceService = walletBalanceService;
        }
        
        public async Task<AddNewWalletResponse> UpsertWalletAsync(AddNewWalletRequest request)
        {
            _logger.LogInformation($"AddNewWalletAsync receive request: {JsonConvert.SerializeObject(request)}");
            try
            {
                await _internalWalletStorage.SaveWallet(new InternalWallet()
                {
                    Name = request.Name,
                    MinBalanceInUsd = request.MinBalanceInUsd,
                    AssetInWalletCollection = await GetAssetInWalletCollection(request.Name)
                });
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
                    result.Add(new AssetInWallet()
                    {
                        Asset = balance.AssetId,
                        Volume = (decimal) balance.Balance,
                        UsdVolume = 0 // todo : calculate usd volume
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
