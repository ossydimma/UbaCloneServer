IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241019094747_create'
)
BEGIN
    CREATE TABLE [Users] (
        [UserId] uniqueidentifier NOT NULL,
        [FullName] nvarchar(max) NOT NULL,
        [PasswordHash] varbinary(max) NOT NULL,
        [PasswordSalt] varbinary(max) NOT NULL,
        [PinHash] varbinary(max) NOT NULL,
        [PinSalt] varbinary(max) NOT NULL,
        [Contact] nvarchar(max) NOT NULL,
        [AccountNumber] int NOT NULL,
        [Balance] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([UserId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241019094747_create'
)
BEGIN
    CREATE TABLE [TransactionHistories] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Number] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Date] nvarchar(max) NOT NULL,
        [Time] nvarchar(max) NOT NULL,
        [Narrator] nvarchar(max) NOT NULL,
        [TypeOfTranscation] nvarchar(max) NOT NULL,
        [UbaCloneUserId] uniqueidentifier NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_TransactionHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TransactionHistories_Users_UbaCloneUserId] FOREIGN KEY ([UbaCloneUserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241019094747_create'
)
BEGIN
    CREATE INDEX [IX_TransactionHistories_UbaCloneUserId] ON [TransactionHistories] ([UbaCloneUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241019094747_create'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241019094747_create', N'8.0.8');
END;
GO

COMMIT;
GO

