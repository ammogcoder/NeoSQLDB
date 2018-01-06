CREATE TABLE [dbo].[ErrorTable]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [ErrorLine] INT NULL, 
    [ErrorMessage] NVARCHAR(MAX) NULL, 
    [TxId] BIGINT NULL, 
    [json] NVARCHAR(MAX) NULL
)
