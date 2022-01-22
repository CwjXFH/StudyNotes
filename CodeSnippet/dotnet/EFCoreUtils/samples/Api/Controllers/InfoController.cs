using Api.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class InfoController : ControllerBase
{
    private readonly InfoDbContext _dbContext;

    public InfoController(InfoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Query([FromQuery] string tag)
    {
        var dbResult = await _dbContext.Infos
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(e => e.Tag == tag, CancellationToken.None);
        return new JsonResult(dbResult);
    }
}