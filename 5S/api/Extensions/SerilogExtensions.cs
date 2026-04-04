using Serilog;
using Serilog.Sinks.MSSqlServer;
using System.Collections.ObjectModel;
using System.Data;

namespace api.Extensions
{
    public static class SerilogExtensions
    {
        public static void AddSerilogLogging(this WebApplicationBuilder builder)
        {
            var columnOptions = new ColumnOptions();

            // Remove defaults
            columnOptions.Store.Remove(StandardColumn.Properties);
            columnOptions.Store.Remove(StandardColumn.MessageTemplate);

            columnOptions.TimeStamp.ColumnName = "TimeStamp";

            columnOptions.AdditionalColumns = new Collection<SqlColumn>
            {
                new SqlColumn("UserId", SqlDbType.Int),
                new SqlColumn("CompanyId", SqlDbType.Int),
                new SqlColumn("RequestPath", SqlDbType.NVarChar, dataLength: 500),
                new SqlColumn("ActionName", SqlDbType.NVarChar, dataLength: 200)
            };

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.MSSqlServer(
                    connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
                    sinkOptions: new MSSqlServerSinkOptions
                    {
                        TableName = "AppLogs",
                        AutoCreateSqlTable = true
                    },
                    columnOptions: columnOptions
                )
                .Enrich.FromLogContext()
                .CreateLogger();

            builder.Host.UseSerilog();
        }
    }
}
