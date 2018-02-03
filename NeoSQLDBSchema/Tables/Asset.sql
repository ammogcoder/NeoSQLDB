CREATE TABLE [dbo].[Asset]
(
	[Id] BIGINT NOT NULL PRIMARY KEY  IDENTITY(0,1), 
    [Asset] VARCHAR(64) NOT NULL unique,
    [Type] VARCHAR(50) NOT NULL, 
    [Amount] NUMERIC(20, 8) NOT NULL, 
    [Precision] INT NOT NULL, 
    [Owner] VARCHAR(34) NOT NULL, 
    [Admin] VARCHAR(34) NOT NULL, 
    [Created] BIGINT NULL, 
    [Symbol] VARCHAR(255) NULL
)

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Asset Hash',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Asset',
    @level2type = N'COLUMN',
    @level2name = N'Asset'
GO
