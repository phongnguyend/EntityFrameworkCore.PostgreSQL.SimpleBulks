﻿using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.Database;

public class SingleKeyRow<TId>
{
    public TId Id { get; set; }

    public int Column1 { get; set; }

    public string Column2 { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime Column3 { get; set; }

    public Season? Season { get; set; }
}
