CREATE TABLE [dbo].[Block]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY(0,1), 
    [Hash] VARCHAR(64) NOT NULL, 
    [Size] BIGINT NOT NULL, 
    [Version] INT NOT NULL, 
    [PreviousBlockHash] VARCHAR(64) NOT NULL, 
    [NextBlockHash] VARCHAR(64) NULL, 
    [Merkleroot] VARCHAR(64) NOT NULL, 
    [Time] BIGINT NOT NULL, 
    [Index] BIGINT NOT NULL, 
    [Nonce] VARCHAR(16) NOT NULL, 
    [NextConsensus] VARCHAR(34) NOT NULL, 
    [TxCount] INT NOT NULL, 
    [Sys_Fee] NUMERIC(20, 8) NOT NULL, 
    [Net_Fee] NUMERIC(20, 8) NOT NULL
)

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Auto increment ID',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Block',
    @level2type = N'COLUMN',
    @level2name = N'Id'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Block hash',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Block',
    @level2type = N'COLUMN',
    @level2name = N'Hash'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Block size in bytes',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Block',
    @level2type = N'COLUMN',
    @level2name = N'Size'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Version of the block which is 0 for now',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Block',
    @level2type = N'COLUMN',
    @level2name = N'Version'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Hash value of the previous block',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Block',
    @level2type = N'COLUMN',
    @level2name = N'PreviousBlockHash'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Hash value of the next block',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Block',
    @level2type = N'COLUMN',
    @level2name = N'NextBlockHash'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Root hash of a transaction list',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Block',
    @level2type = N'COLUMN',
    @level2name = N'Merkleroot'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Time stamp unix',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Block',
    @level2type = N'COLUMN',
    @level2name = N'Time'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Block height',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Block',
    @level2type = N'COLUMN',
    @level2name = N'Index'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Random number',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Block',
    @level2type = N'COLUMN',
    @level2name = N'Nonce'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Contract address of next miner',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Block',
    @level2type = N'COLUMN',
    @level2name = N'NextConsensus'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Count of Transactions',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Block',
    @level2type = N'COLUMN',
    @level2name = N'TxCount'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Summed Sys Fee of Block',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Block',
    @level2type = N'COLUMN',
    @level2name = 'Sys_Fee'
GO


CREATE UNIQUE INDEX [IX_Block_Id] ON [dbo].[Block] ([Id])

GO

EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Summed Net Fee of Block',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Block',
    @level2type = N'COLUMN',
    @level2name = N'Net_Fee'
GO

CREATE UNIQUE INDEX [IX_Block_Hash] ON [dbo].[Block] ([Hash])
