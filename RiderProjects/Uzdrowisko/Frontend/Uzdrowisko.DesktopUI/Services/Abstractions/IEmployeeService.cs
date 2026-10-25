using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SharedStructures.Dtos.Employee;
using Uzdrowisko.DesktopUI.Models;

namespace Uzdrowisko.DesktopUI.Services.Abstractions;

public interface IEmployeeService
{
    Task<IReadOnlyList<EmployeeItem> > GetAllEmployeesAsync(CancellationToken ct);
    Task<EmployeeItem?> GetEmployeeItemAsync(int id, CancellationToken ct);
    Task<EmployeeItem?> CreateEmployeeItemAsync(RegisterEmployeeDto  dto, CancellationToken ct);
    Task<bool> UpdateEmployeeItemAsync(int id,UpdateEmployeeDto dto, CancellationToken ct);
    Task<bool> DeleteEmployeeItemAsync(int id, CancellationToken ct);
}