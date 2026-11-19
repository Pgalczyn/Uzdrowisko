using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedStructures.Dtos.Transaction;
using Uzdrowisko.Infrastructure.DataBase.Context;
using Uzdrowisko.Infrastructure.DataBase.Entities;

namespace Uzdrowisko.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class TransactionController : ControllerBase
{
    private readonly UzdrowiskoDbContext _dbContext;
    
    public TransactionController(UzdrowiskoDbContext dbContext)
        {
        _dbContext = dbContext;
        }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<List<TransactionResponseDto>>> GetTransactionsByPatientId(int id)
    {
        var transactions = await _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.PatientId == id )
            .Select(t => new TransactionResponseDto
            {
                Id = t.Id,
                TreatmentId = t.TreatmentId,
                TreatmentName = t.Treatment.Name,
                PatientId = t.PatientId,
                Price = t.Price,
                Date = t.Date,
                StartOfTreatment = t.StartOfTreatment,
                EndOfTreatment = t.EndOfTreatment,
                IsPaid = t.IsPaid,
                IsActive = t.IsActive,
            })
            .ToListAsync();
        
        return Ok(transactions);
    }

    [HttpPost]
    public async Task<ActionResult<TransactionResponseDto>> CreateTransaction(
        [FromBody] CreateTransactionDto createTransactionDto)
    {   
        var patient  = await _dbContext.Patients.FindAsync(createTransactionDto.PatientId);
        if (patient is null)
        {
            return NotFound($"Pacjent o Id {createTransactionDto.PatientId} nie istnieje");
        }

        var newTransaction = new Transactions
        {
            PatientId = createTransactionDto.PatientId,
            TreatmentId = createTransactionDto.TreatmentId,
            Price = createTransactionDto.Price,
            Date = createTransactionDto.Date,
            StartOfTreatment = createTransactionDto.StartOfTreatment,
            EndOfTreatment = createTransactionDto.EndOfTreatment,
            IsPaid = createTransactionDto.IsPaid,
        };
        _dbContext.Transactions.Add(newTransaction);
        await _dbContext.SaveChangesAsync();

        var responseDto = new TransactionResponseDto
        {
            Id = newTransaction.Id,
            PatientId = newTransaction.PatientId,
            TreatmentId = newTransaction.TreatmentId,
            TreatmentName = newTransaction.Treatment.Name,
            Price = newTransaction.Price,
            Date = newTransaction.Date,
            StartOfTreatment = newTransaction.StartOfTreatment,
            EndOfTreatment = newTransaction.EndOfTreatment,
            IsPaid = newTransaction.IsPaid,
            IsActive = newTransaction.IsActive,
        };
        
        return Ok(responseDto);
    }
    
}