using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SharedStructures.Dtos.Transaction;
using Uzdrowisko.DesktopUI.Models;

namespace Uzdrowisko.DesktopUI.Services.Abstractions;

public interface ITransactionsService
{
    Task<IReadOnlyList<TransactionItem>> GetTransactionByPatientId(int patientId,CancellationToken  ct = default);
    Task<TransactionItem?> CreateTransaction(CreateTransactionDto createTransactionDto,CancellationToken ct = default);
}