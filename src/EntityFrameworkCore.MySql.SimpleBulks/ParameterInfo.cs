using MySqlConnector;

namespace EntityFrameworkCore.MySql.SimpleBulks;

public class ParameterInfo
{
    public string Name { get; set; }

    public string Type { get; set; }

    public MySqlParameter Parameter { get; set; }
}
