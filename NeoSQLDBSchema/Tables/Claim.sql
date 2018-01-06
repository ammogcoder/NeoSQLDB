CREATE TABLE [dbo].[Claim]
(
	[Id] BIGINT NOT NULL PRIMARY KEY  IDENTITY(0,1), 
    [Vout] INT NOT NULL, 
    [TxId] BIGINT NOT NULL, 
    [FromTxId] BIGINT NOT NULL, 
    CONSTRAINT [FK_Claim_Transaction] FOREIGN KEY ([TxId]) REFERENCES [Transaction]([Id])
)

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Stores Claims[] data of a Transaction',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Claim',
    @level2type = N'COLUMN',
    @level2name = N'Id'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Vout',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Claim',
    @level2type = N'COLUMN',
    @level2name = N'Vout'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'TxId',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Claim',
    @level2type = N'COLUMN',
    @level2name = N'TxId'
GO

CREATE INDEX [IX_Claim_TxId] ON [dbo].[Claim] ([TxId])

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'From TxId',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Claim',
    @level2type = N'COLUMN',
    @level2name = N'FromTxId'
GO

CREATE INDEX [IX_Claim_FromTxId] ON [dbo].[Claim] ([FromTxId])
