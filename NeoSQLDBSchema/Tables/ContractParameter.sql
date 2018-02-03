CREATE TABLE [dbo].[ContractParameter]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY (0,1), 
    [ContractId] BIGINT NOT NULL, 
    [Parameter] VARCHAR(50) NOT NULL, 
    CONSTRAINT [FK_ContractParameter_Contract] FOREIGN KEY ([ContractId]) REFERENCES [Contract]([Id])
)
