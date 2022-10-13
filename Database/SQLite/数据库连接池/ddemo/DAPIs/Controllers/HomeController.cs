using DAPIs.Db;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DAPIs.Controllers;

[ApiController]
[Route("[controller]")]
public class HomeController : ControllerBase
{
    private readonly HeroDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public HomeController(HeroDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var hero = await _dbContext.Heroes.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        var response = new { Data = hero, Successed = hero != null };
        return Ok(response);
    }

    [HttpGet("dapper/{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var dbConn = _configuration["Sqlite:ConnectionString"];
        using var conn = new SqliteConnection(dbConn);
        var hero = await conn.QueryAsync<Hero>("SELECT * FROM Heros WHERE Id = @id;", new { id });
        var response = new { Data = hero, Successed = hero != null };
        return Ok(response);
    }
}