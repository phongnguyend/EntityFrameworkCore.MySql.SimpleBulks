using System;

namespace EntityFrameworkCore.MySql.SimpleBulks;

public class TableInfor
{
    public string Schema { get; private set; }

    public string Name { get; private set; }

    public string SchemaQualifiedTableName { get; private set; }

    public TableInfor(string schema, string tableName, Func<string, string, string> schemaTranslator)
    {
        Schema = schema;
        Name = tableName;

        SchemaQualifiedTableName = string.IsNullOrEmpty(schema) ? $"`{tableName}`" : $"`{schemaTranslator(schema, tableName)}`";
    }

    public TableInfor(string tableName) : this(null, tableName, null)
    {
    }
}
