CREATE TABLE [dbo].[Address]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY(0,1), 
    [Address] VARCHAR(34) NOT NULL, 
    [FirstTime] BIGINT NOT NULL, 
    [LastTime] BIGINT NOT NULL, 
    [Description] VARCHAR(MAX) NULL
)

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'not used yet',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Address',
    @level2type = N'COLUMN',
    @level2name = N'Description'
GO

CREATE INDEX [IX_Address_Address] ON [dbo].[Address] ([Address])
