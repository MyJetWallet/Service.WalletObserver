using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Service.WalletObserver.Domain;
using Service.WalletObserver.Domain.Models;
using Service.WalletObserver.Grpc;
using Service.WalletObserver.Grpc.Models;

namespace Service.WalletObserver.Services
{
    public class InternalWalletObserver: IInternalWalletObserver
    {
        private readonly ILogger<InternalWalletObserver> _logger;
        private readonly InternalWalletStorage _internalWalletStorage;

        public InternalWalletObserver(ILogger<InternalWalletObserver> logger,
            InternalWalletStorage internalWalletStorage)
        {
            _logger = logger;
            _internalWalletStorage = internalWalletStorage;
        }
        
        public async Task<AddNewWalletResponse> UpsertWalletAsync(AddNewWalletRequest request)
        {
            _logger.LogInformation($"AddNewWalletAsync receive request: {JsonConvert.SerializeObject(request)}");
            try
            {
                await _internalWalletStorage.SaveWallet(new InternalWallet()
                {
                    Name = request.Name,
                    MinBalanceInUsd = request.MinBalanceInUsd
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
