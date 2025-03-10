using EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.CustomSchema;
using System.ComponentModel.DataAnnotations.Schema;
using static EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.Database.Enums;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.Database;

[Table("SingleKeyRows", Schema = TestConstants.Schema)]
public class SingleKeyRow<TId>
{
    public TId Id { get; set; }

    public int Column1 { get; set; }

    public string Column2 { get; set; }

    public DateTime Column3 { get; set; }

    public Season? Season { get; set; }
}
