using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DAPIs.Db;

[Keyless]
[Table("Heros")]
public class Hero
{
    public int Id { set; get; }

    public string Name { set; get; }
}