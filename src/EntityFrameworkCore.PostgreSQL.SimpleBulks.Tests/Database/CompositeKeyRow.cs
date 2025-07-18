﻿using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.Database;

public class CompositeKeyRow<TId1, TId2>
{
    public TId1 Id1 { get; set; }

    public TId2 Id2 { get; set; }

    public int Column1 { get; set; }

    public string Column2 { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime Column3 { get; set; }

    public Season? Season { get; set; }
}
