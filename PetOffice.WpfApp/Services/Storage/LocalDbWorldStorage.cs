using Microsoft.Data.SqlClient;
using PetOffice.Core.Persistence;
using PetOffice.Core.Persistence.Dto;
using System.Data;
using System.IO;

namespace PetOffice.WpfApp.Services.Storage
{
    public class LocalDbWorldStorage : IWorldStorage
    {
        private const string DatabaseName = "PetOfficeDb";

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


        public async Task SaveAsync(WorldDto world, string path, CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(world);
            ArgumentNullException.ThrowIfNull(world.Pets);

            await EnsureDatabaseAsync(token);

            await using SqlConnection connection = new(DatabaseConnectionString);
            await connection.OpenAsync(token);

            await using SqlTransaction transaction = (SqlTransaction)await connection.BeginTransactionAsync(token);

            try
            {
                await ClearTablesAsync(connection, transaction, token);

                Dictionary<string, int> groupIds = new(StringComparer.Ordinal);
                IEnumerable<string> groupNames = world.Pets
                    .Select(pet => pet.GroupName)
                    .Distinct(StringComparer.Ordinal);

                foreach (string groupName in groupNames)
                {
                    int groupId = await InsertGroupAsync(connection, transaction, groupName, token);
                    groupIds[groupName] = groupId;
                }

                foreach (PetDto pet in world.Pets)
                {
                    if (!groupIds.TryGetValue(pet.GroupName, out int groupId))
                    {
                        throw new InvalidDataException($"Group '{pet.GroupName}' was not created.");
                    }

                    await InsertPetAsync(connection, transaction, pet, groupId, token);
                }

                await transaction.CommitAsync(token);
            }
            catch
            {
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
        }


        public async Task<WorldDto> LoadAsync(string path, CancellationToken token = default)
        {
            await EnsureDatabaseAsync(token);

            WorldDto world = new();

            await using SqlConnection connection = new(DatabaseConnectionString);
            await connection.OpenAsync(token);

            const string sql = """
                SELECT
                    p.Id, p.Type, p.Name, p.Energy, p.TasksCompleted, p.ReactionsCount, g.Name
                FROM Pets AS p
                INNER JOIN PetGroups AS g ON p.GroupId = g.Id
                ORDER BY p.Name;
                """;

            await using SqlCommand loadCommand = new(sql, connection);
            await using SqlDataReader reader = await loadCommand.ExecuteReaderAsync(token);

            while (await reader.ReadAsync(token))
            {
                string type = reader.GetString(1);
                string groupName = reader.GetString(6);

                world.Pets.Add(new PetDto
                {
                    Id = reader.GetGuid(0),
                    Type = type,
                    Name = reader.GetString(2),
                    Energy = reader.GetInt32(3),
                    TasksCompleted = reader.GetInt32(4),
                    ReactionsCount = reader.GetInt32(5),
                    GroupName = groupName,
                    RepositoryName = type
                });
            }

            return world;
        }


        private static async Task EnsureDatabaseAsync(CancellationToken token)
        {
            await EnsureDatabaseExistsAsync(token);

            await using SqlConnection connection = new(DatabaseConnectionString);
            await connection.OpenAsync(token);

            const string schemaSql = """
                IF OBJECT_ID('dbo.PetGroups', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.PetGroups
                    (
                        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PetGroups PRIMARY KEY,
                        Name NVARCHAR(100) NOT NULL CONSTRAINT UQ_PetGroups_Name UNIQUE
                    );
                END;

                IF OBJECT_ID('dbo.Pets', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.Pets
                    (
                        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Pets PRIMARY KEY,
                        Type NVARCHAR(100) NOT NULL,
                        Name NVARCHAR(100) NOT NULL,
                        Energy INT NOT NULL,
                        TasksCompleted INT NOT NULL,
                        ReactionsCount INT NOT NULL,
                        GroupId INT NOT NULL,
                        CONSTRAINT FK_Pets_PetGroups FOREIGN KEY (GroupId) REFERENCES dbo.PetGroups(Id)
                    );
                END;
                """;

            await using SqlCommand schemaCommand = new(schemaSql, connection);
            await schemaCommand.ExecuteNonQueryAsync(token);

            await MigrateOldSchemaAsync(connection, token);
            await EnsureForeignKeyAsync(connection, token);
        }


        private static async Task EnsureDatabaseExistsAsync(CancellationToken token)
        {
            await using SqlConnection masterConnection = new(MasterConnectionString);
            await masterConnection.OpenAsync(token);

            const string sql = """
                IF DB_ID('PetOfficeDb') IS NULL
                BEGIN
                    CREATE DATABASE PetOfficeDb;
                END;
                """;

            await using SqlCommand createDatabaseCommand = new(sql, masterConnection);
            await createDatabaseCommand.ExecuteNonQueryAsync(token);
        }


        private static async Task MigrateOldSchemaAsync(SqlConnection connection, CancellationToken token)
        {
            const string groupIdCheckSql = """
                SELECT COUNT(*) FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.Pets') AND name = 'GroupId';
                """;

            await using SqlCommand groupIdCheckCommand = new(groupIdCheckSql, connection);
            int groupIdExists = Convert.ToInt32(await groupIdCheckCommand.ExecuteScalarAsync(token));

            // New schema is already in use.
            if (groupIdExists > 0) return;

            const string repositoryCheckSql = """
                SELECT COUNT(*) FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.Pets') AND name = 'RepositoryName';
                """;

            await using SqlCommand repositoryCheckCommand = new(repositoryCheckSql, connection);
            int repositoryNameExists = Convert.ToInt32(await repositoryCheckCommand.ExecuteScalarAsync(token));

            if (repositoryNameExists == 0)
            {
                throw new InvalidDataException("The Pets table has an incompatible schema.");
            }

            // Copy old repository names into the new group table.
            const string insertGroupsSql = """
                INSERT INTO dbo.PetGroups (Name)
                SELECT DISTINCT p.RepositoryName
                FROM dbo.Pets AS p
                WHERE NOT EXISTS
                (
                    SELECT 1 FROM dbo.PetGroups AS g WHERE g.Name = p.RepositoryName
                );
                """;

            await using (SqlCommand insertGroupsCommand = new(insertGroupsSql, connection))
            {
                await insertGroupsCommand.ExecuteNonQueryAsync(token);
            }

            // Add the new column first.
            const string addGroupIdSql = """
                ALTER TABLE dbo.Pets ADD GroupId INT NULL;
                """;

            await using (SqlCommand addGroupIdCommand = new(addGroupIdSql, connection))
            {
                await addGroupIdCommand.ExecuteNonQueryAsync(token);
            }

            // This must be a separate command.
            const string updateGroupIdSql = """
                UPDATE p
                SET p.GroupId = g.Id
                FROM dbo.Pets AS p
                INNER JOIN dbo.PetGroups AS g ON g.Name = p.RepositoryName;
                """;

            await using (SqlCommand updateGroupIdCommand = new(updateGroupIdSql, connection))
            {
                await updateGroupIdCommand.ExecuteNonQueryAsync(token);
            }

            const string invalidRowsSql = """
                SELECT COUNT(*) FROM dbo.Pets WHERE GroupId IS NULL;
                """;

            await using SqlCommand invalidRowsCommand = new(invalidRowsSql, connection);
            int invalidRows = Convert.ToInt32(await invalidRowsCommand.ExecuteScalarAsync(token));

            if (invalidRows > 0)
            {
                throw new InvalidDataException("Some pets could not be assigned to a group.");
            }

            const string makeGroupIdRequiredSql = """
                ALTER TABLE dbo.Pets ALTER COLUMN GroupId INT NOT NULL;
                """;

            await using (SqlCommand makeGroupIdRequiredCommand = new(makeGroupIdRequiredSql, connection))
            {
                await makeGroupIdRequiredCommand.ExecuteNonQueryAsync(token);
            }

            const string removeOldColumnSql = """
                ALTER TABLE dbo.Pets DROP COLUMN RepositoryName;
                """;

            await using (SqlCommand removeOldColumnCommand = new(removeOldColumnSql, connection))
            {
                await removeOldColumnCommand.ExecuteNonQueryAsync(token);
            }
        }


        private static async Task EnsureForeignKeyAsync(SqlConnection connection, CancellationToken token)
        {
            const string sql = """
                IF OBJECT_ID('dbo.Pets', 'U') IS NOT NULL
                   AND COL_LENGTH('dbo.Pets', 'GroupId') IS NOT NULL
                   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Pets_PetGroups')
                BEGIN
                    ALTER TABLE dbo.Pets
                    ADD CONSTRAINT FK_Pets_PetGroups FOREIGN KEY (GroupId) REFERENCES dbo.PetGroups(Id);
                END;
                """;

            await using SqlCommand foreignKeyCommand = new(sql, connection);
            await foreignKeyCommand.ExecuteNonQueryAsync(token);
        }


        private static async Task ClearTablesAsync(SqlConnection connection, SqlTransaction transaction, CancellationToken token)
        {
            const string sql = """
                DELETE FROM dbo.Pets;
                DELETE FROM dbo.PetGroups;
                """;

            await using SqlCommand clearCommand = new(sql, connection, transaction);
            await clearCommand.ExecuteNonQueryAsync(token);
        }


        private static async Task<int> InsertGroupAsync(SqlConnection connection, SqlTransaction transaction, string groupName, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                throw new InvalidDataException("Pet group name cannot be empty.");
            }

            const string sql = """
                INSERT INTO dbo.PetGroups (Name)
                OUTPUT INSERTED.Id
                VALUES (@Name);
                """;

            await using SqlCommand insertGroupCommand = new(sql, connection, transaction);
            insertGroupCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = groupName;

            object? result = await insertGroupCommand.ExecuteScalarAsync(token);

            if (result is null)
            {
                throw new InvalidDataException($"Could not create group '{groupName}'.");
            }

            return Convert.ToInt32(result);
        }


        private static async Task InsertPetAsync(SqlConnection connection, SqlTransaction transaction, PetDto pet, int groupId, CancellationToken token)
        {
            const string sql = """
                INSERT INTO dbo.Pets (Id, Type, Name, Energy, TasksCompleted, ReactionsCount, GroupId)
                VALUES (@Id, @Type, @Name, @Energy, @TasksCompleted, @ReactionsCount, @GroupId);
                """;

            await using SqlCommand insertPetCommand = new(sql, connection, transaction);

            insertPetCommand.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = pet.Id;
            insertPetCommand.Parameters.Add("@Type", SqlDbType.NVarChar, 100).Value = pet.Type;
            insertPetCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = pet.Name;
            insertPetCommand.Parameters.Add("@Energy", SqlDbType.Int).Value = pet.Energy;
            insertPetCommand.Parameters.Add("@TasksCompleted", SqlDbType.Int).Value = pet.TasksCompleted;
            insertPetCommand.Parameters.Add("@ReactionsCount", SqlDbType.Int).Value = pet.ReactionsCount;
            insertPetCommand.Parameters.Add("@GroupId", SqlDbType.Int).Value = groupId;

            await insertPetCommand.ExecuteNonQueryAsync(token);
        }
    }
}