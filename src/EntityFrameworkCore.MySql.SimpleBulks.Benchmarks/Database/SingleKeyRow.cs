namespace EntityFrameworkCore.MySql.SimpleBulks.Benchmarks.Database;

public class SingleKeyRow<TId>
{
    public TId Id { get; set; }

    public int Column1 { get; set; }

    public string Column2 { get; set; }

    public DateTime Column3 { get; set; }
}
