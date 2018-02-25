CREATE TABLE [dbo].[Transaction]
(
	[Id] BIGINT NOT NULL PRIMARY KEY  IDENTITY(0,1), 
    [TxId] VARCHAR(64) NOT NULL, 
    [Size] BIGINT NOT NULL, 
    [Type] VARCHAR(50) NOT NULL, 
	[TypeId] INT NOT NULL, 
    [Version] INT NOT NULL, 
    [Sys_Fee] NUMERIC(20, 8) NOT NULL, 
    [Net_Fee] NUMERIC(20, 8) NOT NULL, 
    [Nonce] VARCHAR(16) NULL, 
    [Time] BIGINT NOT NULL, 
    [BlockId] BIGINT NOT NULL, 
    [BlockHash] VARCHAR(64) NOT NULL, 
    [Gas] NUMERIC(20, 8) NULL, 
    [PublicKey] VARCHAR(34) NULL, 
	[Script] NVARCHAR(MAX) NULL)
    --CONSTRAINT [FK_Transaction_Block] FOREIGN KEY ([BlockId]) REFERENCES [Block]([Id])    

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Transaction Id',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Transaction',
    @level2type = N'COLUMN',
    @level2name = N'TxId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Size in Bytes',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Transaction',
    @level2type = N'COLUMN',
    @level2name = N'Size'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Transaction Type',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Transaction',
    @level2type = N'COLUMN',
    @level2name = N'Type'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Version',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Transaction',
    @level2type = N'COLUMN',
    @level2name = N'Version'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Sys Fee',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Transaction',
    @level2type = N'COLUMN',
    @level2name = 'Sys_Fee'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Net Fee',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Transaction',
    @level2type = N'COLUMN',
    @level2name = N'Net_Fee'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Nonce',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Transaction',
    @level2type = N'COLUMN',
    @level2name = N'Nonce'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Time stamp unix from Block',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Transaction',
    @level2type = N'COLUMN',
    @level2name = N'Time'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Hash of Block',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Transaction',
    @level2type = N'COLUMN',
    @level2name = N'BlockHash'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Index of Block',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Transaction',
    @level2type = N'COLUMN',
    @level2name = 'BlockId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Costs required to run the smart contract',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Transaction',
    @level2type = N'COLUMN',
    @level2name = N'Gas'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Used for EnrollmentTransaction',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Transaction',
    @level2type = N'COLUMN',
    @level2name = N'PublicKey'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Transaction Type ID (db internal)',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Transaction',
    @level2type = N'COLUMN',
    @level2name = 'TypeId'
GO

CREATE UNIQUE INDEX [IX_Transaction_TxId] ON [dbo].[Transaction] ([TxId])

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Script of Invocation',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Transaction',
    @level2type = N'COLUMN',
    @level2name = N'Script'