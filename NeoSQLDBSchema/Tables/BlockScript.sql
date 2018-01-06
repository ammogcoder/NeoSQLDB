CREATE TABLE [dbo].[BlockScript]
(
	[Id] BIGINT NOT NULL PRIMARY KEY  IDENTITY(0,1), 
    [Invocation] NVARCHAR(MAX) NOT NULL, 
    [Verification] NVARCHAR(MAX) NOT NULL, 
    [BlockId] BIGINT NOT NULL, 
    CONSTRAINT [FK_BlockScript_Block] FOREIGN KEY ([BlockId]) REFERENCES [Block]([Id])
)

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Stores Scripts of Block',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'BlockScript',
    @level2type = N'COLUMN',
    @level2name = N'Id'
GO

CREATE INDEX [IX_BlockScript_BlockId] ON [dbo].[BlockScript] ([BlockId])
