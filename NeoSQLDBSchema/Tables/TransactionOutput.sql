CREATE TABLE [dbo].[TransactionOutput]
(
	[Id] BIGINT NOT NULL PRIMARY KEY  IDENTITY(0,1), 
    [TxId] BIGINT NOT NULL, 
	[TxIdHash] VARCHAR(64) NOT NULL, 
    [N] INT NOT NULL, 
    [AssetId] BIGINT NOT NULL, 
    [Asset] VARCHAR(64) NOT NULL, 
    [Address] VARCHAR(34) NOT NULL, 
    [AddressId] BIGINT NOT NULL, 
    [Value] NUMERIC(20, 8) NOT NULL, 
    [ToTxId] BIGINT NULL, 
    [ToTxIdHash] VARCHAR(64) NULL, 
    CONSTRAINT [FK_TransactionOutput_Transaction] FOREIGN KEY ([TxId]) REFERENCES [Transaction]([Id])
)

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'FK',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'TransactionOutput',
    @level2type = N'COLUMN',
    @level2name = N'TxId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Position for consuming input',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'TransactionOutput',
    @level2type = N'COLUMN',
    @level2name = 'N'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'FK id of asset',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'TransactionOutput',
    @level2type = N'COLUMN',
    @level2name = N'AssetId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Hash of Asset',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'TransactionOutput',
    @level2type = N'COLUMN',
    @level2name = N'Asset'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'FK',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'TransactionOutput',
    @level2type = N'COLUMN',
    @level2name = N'AddressId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Public Key',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'TransactionOutput',
    @level2type = N'COLUMN',
    @level2name = N'Address'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Value of Transaction',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'TransactionOutput',
    @level2type = N'COLUMN',
    @level2name = N'Value'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'To TXID fk ',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'TransactionOutput',
    @level2type = N'COLUMN',
    @level2name = N'ToTxId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'To TXID hash',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'TransactionOutput',
    @level2type = N'COLUMN',
    @level2name = N'ToTxIdHash'
GO

CREATE INDEX [IX_TransactionOutput_TxId] ON [dbo].[TransactionOutput] ([TxId])

GO

CREATE INDEX [IX_TransactionOutput_AssetId] ON [dbo].[TransactionOutput] ([AssetId])

GO

CREATE INDEX [IX_TransactionOutput_AddressId] ON [dbo].[TransactionOutput] ([AddressId])
