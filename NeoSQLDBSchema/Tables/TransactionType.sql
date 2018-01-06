CREATE TABLE [dbo].[TransactionType]
(
	[Id] INT NOT NULL PRIMARY KEY  IDENTITY(0,1), 
    [Name] VARCHAR(250) NOT NULL
)

GO

CREATE INDEX [IX_TransactionType_Name] ON [dbo].[TransactionType] ([Name])
