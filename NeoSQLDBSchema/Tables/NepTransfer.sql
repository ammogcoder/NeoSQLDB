CREATE TABLE [dbo].[NepTransfer]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY (0,1), 
    [AddressFrom] VARCHAR(34) NOT NULL, 
    [AddressFromId] BIGINT NOT NULL, 
    [AddressTo] VARCHAR(34) NOT NULL, 
    [AddressToId] BIGINT NOT NULL, 
    [Value] NUMERIC(20, 8) NOT NULL, 
    [Contract] VARCHAR(64) NOT NULL, 
    [ContractId] BIGINT NOT NULL, 
    [TxIdHash] VARCHAR(64) NOT NULL, 
    [TxId] BIGINT NOT NULL, 
    [Time] BIGINT NOT NULL, 
    [BlockId] BIGINT NOT NULL, 
    [AssetSymbol] VARCHAR(10) NOT NULL, 
    [AssetId] BIGINT NOT NULL
)

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'FK Asset Id',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'NepTransfer',
    @level2type = N'COLUMN',
    @level2name = N'AssetId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Asset Symbol',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'NepTransfer',
    @level2type = N'COLUMN',
    @level2name = N'AssetSymbol'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Block Id',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'NepTransfer',
    @level2type = N'COLUMN',
    @level2name = N'BlockId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Block Time',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'NepTransfer',
    @level2type = N'COLUMN',
    @level2name = N'Time'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'FK Tx Id',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'NepTransfer',
    @level2type = N'COLUMN',
    @level2name = N'TxId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Tx Hash',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'NepTransfer',
    @level2type = N'COLUMN',
    @level2name = N'TxIdHash'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'FK Contract Id',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'NepTransfer',
    @level2type = N'COLUMN',
    @level2name = N'ContractId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Contract Hash',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'NepTransfer',
    @level2type = N'COLUMN',
    @level2name = N'Contract'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Value',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'NepTransfer',
    @level2type = N'COLUMN',
    @level2name = N'Value'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'FK From Address Id',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'NepTransfer',
    @level2type = N'COLUMN',
    @level2name = N'AddressToId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'To Address',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'NepTransfer',
    @level2type = N'COLUMN',
    @level2name = N'AddressTo'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'FK From Address Id',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'NepTransfer',
    @level2type = N'COLUMN',
    @level2name = N'AddressFromId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'From Address',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'NepTransfer',
    @level2type = N'COLUMN',
    @level2name = N'AddressFrom'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'PK',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'NepTransfer',
    @level2type = N'COLUMN',
    @level2name = N'Id'
GO

CREATE INDEX [IX_NepTransfer_TxId] ON [dbo].[NepTransfer] ([TxId])

GO

CREATE INDEX [IX_NepTransfer_AddressFromId] ON [dbo].[NepTransfer] ([AddressFromId])

GO

CREATE INDEX [IX_NepTransfer_AddressToId] ON [dbo].[NepTransfer] ([AddressToId])
