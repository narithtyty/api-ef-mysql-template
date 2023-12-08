using EntityFrameworkCore.MySQL.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

public static class AppDbContextExtensions
{
    public static int ExecuteSqlCommand(this AppDbContext context, string sql, Dictionary<string, object> parameters = null)
    {
        using (var connection = context.Database.GetDbConnection())
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;

                if (parameters != null)
                {
                    foreach (var kvp in parameters)
                    {
                        var parameter = command.CreateParameter();
                        parameter.ParameterName = kvp.Key;
                        parameter.Value = kvp.Value;
                        command.Parameters.Add(parameter);
                    }
                }

                int affectedRows = command.ExecuteNonQuery();

                return affectedRows;
            }
        }
    }

    public static IEnumerable<IDictionary<string, object>> SqlQuery(this AppDbContext context, string sql, Dictionary<string, object> parameters = null)
    {
        using (var command = context.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = sql;

            if (parameters != null)
            {
                foreach (var kvp in parameters)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = kvp.Key;
                    parameter.Value = kvp.Value;
                    command.Parameters.Add(parameter);
                }
            }

            command.CommandType = CommandType.Text;

            context.Database.OpenConnection();

            using (var result = command.ExecuteReader())
            {
                var resultData = new List<IDictionary<string, object>>();

                while (result.Read())
                {
                    var row = new Dictionary<string, object>();

                    for (int i = 0; i < result.FieldCount; i++)
                    {
                        row[result.GetName(i)] = result.GetValue(i);
                    }

                    resultData.Add(row);
                }

                return resultData;
            }
        }
    }

    public static async Task<IEnumerable<IDictionary<string, object>>> ExecuteStoredProcedure(this AppDbContext context, string storedProcedureName, params SqlParameter[] parameters)
    {
        var connection = context.Database.GetDbConnection();
        var command = connection.CreateCommand();
        command.CommandText = storedProcedureName;
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddRange(parameters);

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        using (var result = await command.ExecuteReaderAsync())
        {
            var resultData = new List<IDictionary<string, object>>();

            while (await result.ReadAsync())
            {
                var row = new Dictionary<string, object>();

                for (int i = 0; i < result.FieldCount; i++)
                {
                    row[result.GetName(i)] = result.GetValue(i);
                }

                resultData.Add(row);
            }

            return resultData;
        }
    }

    public static void DisposeContext(this AppDbContext context)
    {
        context.Dispose();
    }
}
