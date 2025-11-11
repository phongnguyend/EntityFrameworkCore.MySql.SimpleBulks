using MySqlConnector;

namespace EntityFrameworkCore.MySql.SimpleBulks;

public class ParameterInfo
{
    public string Name { get; set; }

    public string Type { get; set; }

    public MySqlParameter Parameter { get; set; }

    public bool FromConverter { get; set; }

    public override string ToString()
    {
        if (FromConverter)
        {
            return $"{Name} (Type: {Type}), (FromConverter: {FromConverter})";
        }

        return $"{Name} (Type: {Type})";
    }
}
