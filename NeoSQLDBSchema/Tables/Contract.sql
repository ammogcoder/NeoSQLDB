CREATE TABLE [dbo].[Contract]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY(0,1), 
	[Hash] VARCHAR(64) NOT NULL unique, 
    [Script] NVARCHAR(MAX) NOT NULL, 
    [ReturnType] VARCHAR(50) NOT NULL, 
    [Storage] BIT NOT NULL, 
    [Name] VARCHAR(255) NOT NULL, 
    [CodeVersion] VARCHAR(50) NOT NULL, 
    [Author] VARCHAR(50) NOT NULL, 
    [Email] VARCHAR(255) NOT NULL, 
    [Description] VARCHAR(255) NOT NULL, 
    [DynamicInvoke] BIT NULL, 
    [Version] INT NULL
)

GO

CREATE INDEX [IX_Contract_Hash] ON [dbo].[Contract] ([Hash])
