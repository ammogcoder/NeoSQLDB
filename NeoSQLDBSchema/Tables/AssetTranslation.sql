CREATE TABLE [dbo].[AssetTranslation]
(
	[Id] BIGINT NOT NULL, 
    [Language] VARCHAR(10) NOT NULL, 
    [IsDefault] BIT NOT NULL, 
    [Name] NVARCHAR(200) NOT NULL, 
    CONSTRAINT [FK_AssetTranslation_Asset] FOREIGN KEY ([Id]) REFERENCES [Asset]([Id])
)
