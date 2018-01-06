CREATE TABLE [dbo].[TransactionInput]
(
	[Id] BIGINT NOT NULL PRIMARY KEY  IDENTITY(0,1), 
    [TxId] BIGINT NOT NULL, 
	[TxIdHash] VARCHAR(64) NOT NULL, 
    [Vout] INT NOT NULL, 
	[AssetId] BIGINT NOT NULL, 
    [Asset] VARCHAR(64) NOT NULL, 
    [Address] VARCHAR(34) NOT NULL, 
    [AddressId] BIGINT NOT NULL, 
    [Value] NUMERIC(20, 8) NOT NULL, 
    [FromTxId] BIGINT NOT NULL, 
    [FromTxIdHash] VARCHAR(64) NOT NULL, 
    CONSTRAINT [FK_TransactionInput_Transaction] FOREIGN KEY ([TxId]) REFERENCES [Transaction]([Id])
)

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Position of Vout',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'TransactionInput',
    @level2type = N'COLUMN',
    @level2name = N'Vout'
GO

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'FK',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'TransactionInput',
    @level2type = N'COLUMN',
    @level2name = N'TxId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'From TXID fk ',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'TransactionInput',
    @level2type = N'COLUMN',
    @level2name = N'FromTxId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'From TXID hash',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'TransactionInput',
    @level2type = N'COLUMN',
    @level2name = N'FromTxIdHash'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'TxId Hash',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'TransactionInput',
    @level2type = N'COLUMN',
    @level2name = N'TxIdHash'
GO

CREATE INDEX [IX_TransactionInput_TxId] ON [dbo].[TransactionInput] ([TxId])

GO

CREATE INDEX [IX_TransactionInput_AssetId] ON [dbo].[TransactionInput] ([AssetId])

GO

CREATE INDEX [IX_TransactionInput_AddressId] ON [dbo].[TransactionInput] ([AddressId])
