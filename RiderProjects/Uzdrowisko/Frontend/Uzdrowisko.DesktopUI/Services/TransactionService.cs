using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using SharedStructures.Dtos.Transaction;
using Uzdrowisko.DesktopUI.Models;
using Uzdrowisko.DesktopUI.Services.Abstractions;
using Uzdrowisko.DesktopUI.Services.Common;
using System;
using System.Linq;

namespace Uzdrowisko.DesktopUI.Services;

public class TransactionService : ITransactionsService
{
    private readonly HttpClient _httpClient;
    public TransactionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<IReadOnlyList<TransactionItem>> GetTransactionByPatientId(int patientId,CancellationToken ct = default)
    {
        _httpClient.EnsureAuthHeader();
        
        
        try
        {
            var dtos = await _httpClient.GetFromJsonAsync<List<TransactionResponseDto>>($"api/Transaction/{patientId}", ct);
            if (dtos == null) return Array.Empty<TransactionItem>();
            
            return dtos.Select(TransactionItem.FromDto).ToList();
        }
        catch (HttpRequestException)
        {
            return Array.Empty<TransactionItem>();
        }
    }

    public async Task<TransactionItem?> CreateTransaction(CreateTransactionDto createTransactionDto,CancellationToken ct = default)
    {
        _httpClient.EnsureAuthHeader();
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/Transation", createTransactionDto, ct);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var createdDto = await response.Content.ReadFromJsonAsync<TransactionResponseDto>(cancellationToken: ct);

            return createdDto is not null ? TransactionItem.FromDto(createdDto) : null;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException)
        {
            return null;
        }
        
    }
}