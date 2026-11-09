using Microsoft.Data.SqlClient;
using PetOffice.Core.Persistence;
using PetOffice.Core.Persistence.Dto;

namespace PetOffice.WpfApp.Services.Storage
{
    public class LocalDbWorldStorage : IWorldStorage
    {
        private const string DatabaseName =
            "PetOfficeDb";

        private const string MasterConnectionString =
            @"Server=(localdb)\MSSQLLocalDB;
              Database=master;
              Trusted_Connection=True;
              TrustServerCertificate=True;";

        private static string DatabaseConnectionString =>
            $@"Server=(localdb)\MSSQLLocalDB;
               Database={DatabaseName};
               Trusted_Connection=True;
               TrustServerCertificate=True;";

        public async Task SaveAsync(
            WorldDto world,
            string path,
            CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(world);

            await EnsureDatabaseAsync(token);

            await using SqlConnection connection =
                new(DatabaseConnectionString);

            await connection.OpenAsync(token);

            await using SqlTransaction transaction =
                (SqlTransaction)await connection
                    .BeginTransactionAsync(token);

            try
            {
                await ClearTablesAsync(
                    connection,
                    transaction,
                    token);

                foreach (PetDto pet in world.Pets)
                {
                    await InsertPetAsync(
                        connection,
                        transaction,
                        pet,
                        token);
                }

                await transaction.CommitAsync(token);
            }
            catch
            {
                await transaction.RollbackAsync(
                    CancellationToken.None);

                throw;
            }
        }

        public async Task<WorldDto> LoadAsync(
            string path,
            CancellationToken token = default)
        {
            await EnsureDatabaseAsync(token);

            WorldDto world = new();

            await using SqlConnection connection =
                new(DatabaseConnectionString);

            await connection.OpenAsync(token);

            const string sql = """
                SELECT
                    Id,
                    Type,
                    Name,
                    Energy,
                    TasksCompleted,
                    ReactionsCount,
                    RepositoryName
                FROM Pets
                ORDER BY Name;
                """;

            await using SqlCommand loadCommand =
                new(sql, connection);

            await using SqlDataReader reader =
                await loadCommand.ExecuteReaderAsync(token);

            while (await reader.ReadAsync(token))
            {
                world.Pets.Add(
                    new PetDto
                    {
                        Id = reader.GetGuid(0),
                        Type = reader.GetString(1),
                        Name = reader.GetString(2),
                        Energy = reader.GetInt32(3),
                        TasksCompleted = reader.GetInt32(4),
                        ReactionsCount = reader.GetInt32(5),
                        RepositoryName = reader.GetString(6)
                    });
            }

            return world;
        }

        private static async Task EnsureDatabaseAsync(
            CancellationToken token)
        {
            await using (
                SqlConnection masterConnection =
                    new(MasterConnectionString))
            {
                await masterConnection.OpenAsync(token);

                const string createDatabaseSql = """
                    IF DB_ID('PetOfficeDb') IS NULL
                    BEGIN
                        CREATE DATABASE PetOfficeDb;
                    END
                    """;

                await using SqlCommand createDatabaseCommand =
                    new(
                        createDatabaseSql,
                        masterConnection);

                await createDatabaseCommand
                    .ExecuteNonQueryAsync(token);
            }

            await using SqlConnection databaseConnection =
                new(DatabaseConnectionString);

            await databaseConnection.OpenAsync(token);

            const string createTableSql = """
                IF OBJECT_ID('Pets', 'U') IS NULL
                BEGIN
                    CREATE TABLE Pets
                    (
                        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                        Type NVARCHAR(100) NOT NULL,
                        Name NVARCHAR(100) NOT NULL,
                        Energy INT NOT NULL,
                        TasksCompleted INT NOT NULL,
                        ReactionsCount INT NOT NULL,
                        RepositoryName NVARCHAR(100) NOT NULL
                    );
                END
                """;

            await using SqlCommand createTableCommand =
                new(
                    createTableSql,
                    databaseConnection);

            await createTableCommand
                .ExecuteNonQueryAsync(token);
        }

        private static async Task ClearTablesAsync(
            SqlConnection connection,
            SqlTransaction transaction,
            CancellationToken token)
        {
            const string sql =
                "DELETE FROM Pets;";

            await using SqlCommand clearCommand =
                new(
                    sql,
                    connection,
                    transaction);

            await clearCommand
                .ExecuteNonQueryAsync(token);
        }

        private static async Task InsertPetAsync(
            SqlConnection connection,
            SqlTransaction transaction,
            PetDto pet,
            CancellationToken token)
        {
            const string sql = """
                INSERT INTO Pets
                (
                    Id,
                    Type,
                    Name,
                    Energy,
                    TasksCompleted,
                    ReactionsCount,
                    RepositoryName
                )
                VALUES
                (
                    @Id,
                    @Type,
                    @Name,
                    @Energy,
                    @TasksCompleted,
                    @ReactionsCount,
                    @RepositoryName
                );
                """;

            await using SqlCommand insertCommand =
                new(
                    sql,
                    connection,
                    transaction);

            insertCommand.Parameters.AddWithValue(
                "@Id",
                pet.Id);

            insertCommand.Parameters.AddWithValue(
                "@Type",
                pet.Type);

            insertCommand.Parameters.AddWithValue(
                "@Name",
                pet.Name);

            insertCommand.Parameters.AddWithValue(
                "@Energy",
                pet.Energy);

            insertCommand.Parameters.AddWithValue(
                "@TasksCompleted",
                pet.TasksCompleted);

            insertCommand.Parameters.AddWithValue(
                "@ReactionsCount",
                pet.ReactionsCount);

            insertCommand.Parameters.AddWithValue(
                "@RepositoryName",
                pet.RepositoryName);

            await insertCommand
                .ExecuteNonQueryAsync(token);
        }
    }
}