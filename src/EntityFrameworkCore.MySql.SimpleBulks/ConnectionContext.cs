using MySqlConnector;

namespace EntityFrameworkCore.MySql.SimpleBulks;

public record struct ConnectionContext(MySqlConnection Connection, MySqlTransaction Transaction);