﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.Database;

public class ConfigurationEntry
{
    public Guid Id { get; set; }

    [Timestamp]
    public uint RowVersion { get; set; }

    public DateTimeOffset CreatedDateTime { get; set; }

    public DateTimeOffset? UpdatedDateTime { get; set; }

    public string Key { get; set; }

    public string Value { get; set; }

    public string Description { get; set; }

    public bool IsSensitive { get; set; }

    [NotMapped]
    public string TestNotMapped { get; set; }
}
