using Ptl.Contracts.Dtos;

namespace Ptl.Core.Interfaces
{
    public interface ITransactionSource
    {
        PickTransactionDto? GetNextTransaction();
        IEnumerable<PickTransactionDto> GetActiveTransactions();
        bool UpdateTransaction(string txId, int status);
        bool ProcessPicked(string txDetailId, int qty);

        bool MarkDetailUnavailable(string txDetailId);
    }
}
