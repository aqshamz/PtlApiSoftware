using Ptl.Contracts.Dtos;

public interface ITransactionCommandRepository
{
    void InsertTransaction(PickTransactionDto tx);
}
