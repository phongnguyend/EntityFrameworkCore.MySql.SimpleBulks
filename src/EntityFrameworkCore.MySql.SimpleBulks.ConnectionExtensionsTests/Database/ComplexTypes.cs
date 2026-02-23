using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFrameworkCore.MySql.SimpleBulks.ConnectionExtensionsTests.Database;

[ComplexType]
public class ComplexTypeAddress
{
    public string Street { get; set; }

    public ComplexTypeLocation Location { get; set; }
}

[ComplexType]
public class ComplexTypeLocation
{
    public decimal Lat { get; set; }

    public decimal Lng { get; set; }
}

public class JsonComplexTypeAddress
{
    public string Street { get; set; }

    public ComplexTypeLocation Location { get; set; }
}
