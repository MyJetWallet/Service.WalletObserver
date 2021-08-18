using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Service.WalletObserver.Grpc;
using Service.WalletObserver.Grpc.Models;

namespace Service.WalletObserver.Services
{
    public class InternalWalletObserver: IInternalWalletObserver
    {
        private readonly ILogger<InternalWalletObserver> _logger;

        public InternalWalletObserver(ILogger<InternalWalletObserver> logger)
        {
            _logger = logger;
        }

        public Task<AddNewWalletResponse> AddNewWalletAsync(AddNewWalletRequest request)
        {
            throw new NotImplementedException();
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
