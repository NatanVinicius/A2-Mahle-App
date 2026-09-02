using A2MahleApp.Application.Features.Inspection.Services;
using InspectionEntity = A2MahleApp.Domain.Features.Inspection.Entities.Inspection;
using A2MahleApp.Infrastructure.Persistence;

using System.Data.Common;

using Microsoft.EntityFrameworkCore;

namespace A2MahleApp.Infrastructure.Features.Inspection.Repositories;

public sealed class InspectionRepository : IInspectionRepository
{
    private readonly IDbContextFactory<MahleDbContext> _contextFactory;
    private Task? _databaseInitialization;

    public InspectionRepository(IDbContextFactory<MahleDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task AddAsync(
        InspectionEntity inspection,
        CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseAsync(cancellationToken);

        await using MahleDbContext context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        context.Inspections.Add(inspection);
        await context.SaveChangesAsync(cancellationToken);
    }

    private Task EnsureDatabaseAsync(CancellationToken cancellationToken)
    {
        if (_databaseInitialization is null)
        {
            _databaseInitialization = InitializeDatabaseAsync(cancellationToken);
        }

        return _databaseInitialization;
    }

    private async Task InitializeDatabaseAsync(CancellationToken cancellationToken)
    {
        await using MahleDbContext context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        await context.Database.EnsureCreatedAsync(cancellationToken);

        await using var connection = context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var tableInfoCommand = connection.CreateCommand();
        tableInfoCommand.CommandText = "PRAGMA table_info('Inspections');";

        bool hasImageColumn = false;
        bool hasEvidencePathColumn = false;

        await using (DbDataReader reader = await tableInfoCommand.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                string columnName = reader.GetString(1);

                if (columnName.Equals("Image", StringComparison.OrdinalIgnoreCase))
                {
                    hasImageColumn = true;
                }

                if (columnName.Equals("EvidenceImagePath", StringComparison.OrdinalIgnoreCase))
                {
                    hasEvidencePathColumn = true;
                }
            }
        }

        if (!hasImageColumn && hasEvidencePathColumn)
        {
            return;
        }

        string evidenceSelect = hasEvidencePathColumn ? "EvidenceImagePath" : "NULL";

        await using DbTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            await ExecuteSqlAsync(connection, transaction,
                "ALTER TABLE Inspections RENAME TO Inspections_Legacy;", cancellationToken);

            await ExecuteSqlAsync(connection, transaction,
                "CREATE TABLE Inspections (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, DateTime TEXT NOT NULL, Status TEXT NOT NULL, CycleTimeMilliseconds INTEGER NOT NULL, EvidenceImagePath TEXT NULL);",
                cancellationToken);

            await ExecuteSqlAsync(connection, transaction,
                $"INSERT INTO Inspections (Id, DateTime, Status, CycleTimeMilliseconds, EvidenceImagePath) SELECT Id, DateTime, Status, CycleTimeMilliseconds, {evidenceSelect} FROM Inspections_Legacy;",
                cancellationToken);

            await ExecuteSqlAsync(connection, transaction,
                "DROP TABLE Inspections_Legacy;", cancellationToken);

            await ExecuteSqlAsync(connection, transaction,
                "CREATE INDEX IF NOT EXISTS IX_Inspections_DateTime ON Inspections(DateTime);",
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static async Task ExecuteSqlAsync(
        DbConnection connection,
        DbTransaction transaction,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
