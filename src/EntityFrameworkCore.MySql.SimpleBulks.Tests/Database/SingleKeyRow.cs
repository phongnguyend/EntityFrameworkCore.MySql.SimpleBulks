﻿namespace EntityFrameworkCore.MySql.SimpleBulks.Tests.Database;

public class SingleKeyRow<TId>
{
    public TId Id { get; set; }

    public int Column1 { get; set; }

    public string Column2 { get; set; }

    public DateTime Column3 { get; set; }

    public Season? Season { get; set; }

    public Guid? BulkId { get; set; }

    public int? BulkIndex { get; set; }
}
