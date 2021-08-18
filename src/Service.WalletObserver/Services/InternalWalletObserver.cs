using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyNoSqlServer.Abstractions;
using Newtonsoft.Json;
using Service.WalletObserver.Domain.Models;
using Service.WalletObserver.Grpc;
using Service.WalletObserver.Grpc.Models;

namespace Service.WalletObserver.Services
{
    public class InternalWalletObserver: IInternalWalletObserver
    {
        private readonly ILogger<InternalWalletObserver> _logger;
        private readonly IMyNoSqlServerDataWriter<InternalWalletNoSql> _dataWriter;

        public InternalWalletObserver(ILogger<InternalWalletObserver> logger,
            IMyNoSqlServerDataWriter<InternalWalletNoSql> dataWriter)
        {
            _logger = logger;
            _dataWriter = dataWriter;
        }

        public async Task<AddNewWalletResponse> AddNewWalletAsync(AddNewWalletRequest request)
        {
            _logger.LogInformation($"AddNewWalletAsync receive request: {JsonConvert.SerializeObject(request)}");
            try
            {
                await _dataWriter.InsertOrReplaceAsync(InternalWalletNoSql.Create(new InternalWallet()
                {
                    Name = request.Name,
                    MinBalanceInUsd = request.MinBalanceInUsd
                }));
            } catch (Exception ex)
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

        public Task<GetWalletsResponse> GetWalletsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ChangeWalletMinBalanceResponse> ChangeWalletMinBalanceAsync(ChangeWalletMinBalanceRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<RemoveWalletResponse> RemoveWalletAsync(RemoveWalletRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
