﻿using System;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks;

public class BulkOptions
{
    public int BatchSize { get; set; }

    public int Timeout { get; set; }

    public Action<string> LogTo { get; set; }

    public BulkOptions()
    {
        Timeout = 30;
    }
}
