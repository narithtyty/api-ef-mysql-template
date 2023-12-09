using api_ef_mysql_template.Models.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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
                command.CommandTimeout = 10;
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
            command.CommandTimeout = 10;
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
    public static async Task<IEnumerable<IDictionary<string, object>>> ExecuteStoredProcedure(
    this AppDbContext context, string storedProcedureName, Dictionary<string, object> parameters = null)
    {
        var connection = context.Database.GetDbConnection();
        var command = connection.CreateCommand();
        command.CommandText = storedProcedureName;
        command.CommandType = CommandType.StoredProcedure;
        command.CommandTimeout = 10;
        if (parameters != null)
        {
            foreach (var parameter in parameters)
            {
                var dbParameter = command.CreateParameter();
                dbParameter.ParameterName = parameter.Key;
                dbParameter.Value = parameter.Value;
                command.Parameters.Add(dbParameter);
            }
        }

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
    public static async Task<ResponseResult<IDictionary<string, object>>> ExecuteQueryOrStoredProcedure(
    this AppDbContext context, string queryOrProcedure,
    Dictionary<string, object> parameters = null, PaginationOptions pagination = null)
    {
        var connection = context.Database.GetDbConnection();
        var command = connection.CreateCommand();
        command.CommandTimeout = 10;
        // Check if the provided query or procedure is a stored procedure
        bool isStoredProcedure = !queryOrProcedure.Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase);

        if (isStoredProcedure)
        {
            command.CommandText = queryOrProcedure;
            command.CommandType = CommandType.StoredProcedure;
        }
        else
        {
            command.CommandText = queryOrProcedure;
            command.CommandType = CommandType.Text;
        }

        if (parameters != null)
        {
            foreach (var parameter in parameters)
            {
                var dbParameter = command.CreateParameter();
                dbParameter.ParameterName = parameter.Key;
                dbParameter.Value = parameter.Value;
                command.Parameters.Add(dbParameter);
            }
        }

        // Count the total number of records if it's a direct query
        long totalDatas = isStoredProcedure ? 0 : await CountTotalDatasAsync(command);

        if (pagination != null)
        {
            int startRow = (pagination.PageNumber - 1) * pagination.PageSize;
            command.CommandText += $" LIMIT {startRow}, {pagination.PageSize}";
            //command.CommandText += $" OFFSET {startRow} ROWS FETCH NEXT {pagination.PageSize} ROWS ONLY"; // for sql server
        }

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

            // Calculate total pages based on the total count and page size
            int totalPages = (pagination != null ? (int)Math.Ceiling((double)totalDatas / pagination.PageSize) : 0);

            return new ResponseResult<IDictionary<string, object>>
            {
                PageNumber = pagination?.PageNumber ?? 1,
                Data = resultData,
                TotalPages = totalPages,
                TotalDatas = totalDatas
            };
        }
    }
    private static async Task<long> CountTotalDatasAsync(DbCommand command)
    {
        using (var countCommand = command.Connection.CreateCommand())
        {
            countCommand.CommandText = $"SELECT COUNT(*) FROM ({command.CommandText}) AS CountTable";
            return (long)await countCommand.ExecuteScalarAsync();
        }
    }
    public class PaginationOptions
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ResponseResult<T>
    {
        public int PageNumber { get; set; }
        public IEnumerable<T> Data { get; set; }
        public int TotalPages { get; set; }
        public long TotalDatas { get; set; }
    }
    public static void DisposeContext(this AppDbContext context)
    {
        context.Dispose();
    }
}
