using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedStructures.Dtos.Room;
using Uzdrowisko.Infrastructure.DataBase.Context;
using Uzdrowisko.Infrastructure.DataBase.Entities;

namespace Uzdrowisko.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class RoomController : ControllerBase
{
    private readonly UzdrowiskoDbContext _dbContext;

    public RoomController(UzdrowiskoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    [Authorize(Roles = "Sekretarka,Admin")]
    public async Task<ActionResult<RoomResponseDto>> Create([FromBody] CreateRoomDto dtoRoom)
    {
        var roomExists = await _dbContext.Rooms.AnyAsync(r => r.Number == dtoRoom.Number);
        if (roomExists)
        {
            return Conflict($"Pokój o numerze {dtoRoom.Number} już istnieje");
        }

        var newRoom = new Room()
        {
            Number = dtoRoom.Number,
            Name = dtoRoom.Name,
            Location = dtoRoom.Location,
        };
        
        _dbContext.Rooms.Add(newRoom);
        await _dbContext.SaveChangesAsync();

        var response = new RoomResponseDto
        {
            Id = newRoom.Id,
            Name = newRoom.Name,
            Location = newRoom.Location,
        };
        return CreatedAtAction(nameof(GetRoomById), new { id = newRoom.Id }, response);


    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoomResponseDto>> GetRoomById(int id)
    {
        var room = await _dbContext.Rooms
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new RoomResponseDto
            {
                Id = id,
                Name = r.Name,
                Location = r.Location,
            })
            .FirstOrDefaultAsync();
        
        if  (room == null)
        {
            return NotFound($"Nie znaleziono pokoju o id {room.Id}");
        }
        
        return Ok(room);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchRoom(int id, [FromBody] UpdateRoomDto dtoRoom)
    {
        var room = await _dbContext.Rooms.FirstOrDefaultAsync(r => r.Number == dtoRoom.Number);
        if (room == null)
        {
            return Conflict($"Pokój o numerze {dtoRoom.Number} już istnieje");
        }

        if (dtoRoom.Number != null)
        {
            room.Number = dtoRoom.Number;
        }

        if (dtoRoom.Name != null)
        {
            room.Name = dtoRoom.Name;
        }

        if (dtoRoom.Location != null)
        {
            room.Location =  dtoRoom.Location;
        }

        if (dtoRoom.Active != null)
        {
            room.Active =  dtoRoom.Active.Value;
        }
        
        await  _dbContext.SaveChangesAsync();
        return NoContent();
    }
    

}