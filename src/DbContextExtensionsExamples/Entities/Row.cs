using System;

namespace DbContextExtensionsExamples.Entities;

public class Row
{
    public int Id { get; set; }

    public int Column1 { get; set; }

    public string Column2 { get; set; }

    public DateTime Column3 { get; set; }

    public decimal Decimal { get; set; }

    public double Double { get; set; }

    public short Short { get; set; }

    public long Long { get; set; }

    public float Float { get; set; }
}
