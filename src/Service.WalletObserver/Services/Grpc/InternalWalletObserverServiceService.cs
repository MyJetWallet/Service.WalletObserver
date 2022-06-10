using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Service.WalletObserver.Domain.Models;
using Service.WalletObserver.Grpc;
using Service.WalletObserver.Grpc.Models;

namespace Service.WalletObserver.Services.Grpc
{
    public class InternalWalletObserverServiceService : IInternalWalletObserverService
    {
        private readonly ILogger<InternalWalletObserverServiceService> _logger;
        private readonly InternalWalletStorage _internalWalletStorage;

        public InternalWalletObserverServiceService(
            ILogger<InternalWalletObserverServiceService> logger,
            InternalWalletStorage internalWalletStorage
        )
        {
            _logger = logger;
            _internalWalletStorage = internalWalletStorage;
        }

        public async Task<AddNewWalletResponse> UpsertWalletAsync(AddNewWalletRequest request)
        {
            try
            {
                _logger.LogInformation("AddNewWalletAsync receive request: {@Request}", request);

                var snapshot = await _internalWalletStorage.GetWalletsAsync();
                var balance = snapshot.FirstOrDefault(e => e.WalletId == request.WalletId && e.Asset == request.Asset);

                if (balance != null)
                {
                    balance.MinBalanceInUsd = request.MinBalanceInUsd;
                }
                else
                {
                    balance = new InternalWalletBalance
                    {
                        Asset = request.Asset,
                        WalletName = request.WalletName,
                        MinBalanceInUsd = request.MinBalanceInUsd,
                        AccountId = request.AccountId,
                        WalletId = request.WalletId,
                        BrokerId = request.BrokerId
                    };
                }

                await _internalWalletStorage.UpsertBalanceAsync(balance);

                return new AddNewWalletResponse
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddNewWalletAsync throw exception: {@ExMessage}", ex.Message);
                return new AddNewWalletResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<GetWalletsResponse> GetWalletsAsync()
        {
            var response = new GetWalletsResponse();

            try
            {
                _logger.LogInformation("GetWalletsAsync receive request");

                response.WalletList = await _internalWalletStorage.GetWalletsAsync();
                response.Success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetWalletsAsync throw exception: {@ExMess}", ex.Message);

                return new GetWalletsResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }

            return response;
        }

        public async Task<RemoveWalletResponse> RemoveWalletAsync(RemoveWalletRequest request)
        {
            try
            {
                _logger.LogInformation("RemoveWalletAsync receive request: {@Request}", request);

                await _internalWalletStorage.RemoveBalancesAsync(request.Name);
                return new RemoveWalletResponse
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RemoveWalletAsync throw exception: {@ExMess}", ex.Message);
                return new RemoveWalletResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}