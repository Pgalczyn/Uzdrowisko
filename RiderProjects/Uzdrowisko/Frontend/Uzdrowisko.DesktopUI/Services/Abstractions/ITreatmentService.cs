using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SharedStructures.Dtos.Treatment;
using Uzdrowisko.DesktopUI.Models;

namespace Uzdrowisko.Services.Abstractions;

public interface ITreatmentService
{
    Task<IReadOnlyList<TreatmentItem>> GetAllTreatmentsItemsAsync(CancellationToken ct = default);
    
    Task<TreatmentItem?> GetTreatmentItemAsync(int id, CancellationToken ct = default);
    
    Task<TreatmentItem?> CreateTreatmentItemAsync(CreateTreatmentDto dto, CancellationToken ct = default);
    
    Task<bool> UpdateTreatmentItemAsync(int id, UpdateTreatmentDto dto, CancellationToken ct = default);
    
    Task<bool> DeleteTreatmentItemAsync(int id, CancellationToken ct = default);
    
    // Pobranie zabiegow na dany dzien/tydzien/turnus
}