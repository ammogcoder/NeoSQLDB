CREATE TABLE [dbo].[Attribute]
(
	[Id] BIGINT NOT NULL PRIMARY KEY  IDENTITY(0,1), 
    [Usage] NVARCHAR(MAX) NOT NULL, 
    [Data] NVARCHAR(MAX) NOT NULL, 
    [TxId] BIGINT NOT NULL, 
    CONSTRAINT [FK_Attribute_Transaction] FOREIGN KEY ([TxId]) REFERENCES [Transaction]([Id]) 
)

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Stores Attributes of Transaction',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Attribute',
    @level2type = N'COLUMN',
    @level2name = N'Id'
GO

CREATE INDEX [IX_Attribute_TxId] ON [dbo].[Attribute] ([TxId])
