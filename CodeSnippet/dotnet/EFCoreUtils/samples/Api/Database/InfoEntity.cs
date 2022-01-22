using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Database;

[Table("Info")]
public class InfoEntity
{
    [Key]
    public int Id { set; get; }

    [Required]
    [Column(TypeName = "NVARCHAR(20)")]
    public string Tag { set; get; }
}
