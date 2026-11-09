using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SharedStructures.Dtos.Patient;
using Uzdrowisko.DesktopUI.Models;

namespace Uzdrowisko.DesktopUI.Services.Abstractions;

public interface IPatientService
{
    Task<IReadOnlyList<PatientItem>> GetAllPatientsAsync(CancellationToken ct);
    Task<PatientItem?> GetPatientItemAsync(int id, CancellationToken ct);
    Task<PatientItem?> CreatePatientItemAsync(CreatePatientDto dto, CancellationToken ct);
    Task<bool> UpdatePatientItemAsync(int id, UpdatePatientDto dto, CancellationToken ct);
    Task<bool> DeletePatientItemAsync(int id, CancellationToken ct);
}