using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.MySql.SimpleBulks.ConnectionExtensionsTests.Database;

[Owned]
public class OwnedTypeAddress
{
    public string Street { get; set; }

    public OwnedTypeLocation Location { get; set; }
}

[Owned]
public class OwnedTypeLocation
{
    public decimal Lat { get; set; }

    public decimal Lng { get; set; }
}

public class JsonOwnedTypeAddress
{
    public string Street { get; set; }

    public OwnedTypeLocation Location { get; set; }
}