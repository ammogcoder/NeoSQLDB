CREATE PROCEDURE [dbo].[ClearTables]
AS

ALTER TABLE [dbo].[AssetTranslation] DROP CONSTRAINT [FK_AssetTranslation_Asset]
ALTER TABLE [dbo].[Attribute] DROP CONSTRAINT [FK_Attribute_Transaction]
ALTER TABLE [dbo].[Claim] DROP CONSTRAINT [FK_Claim_Transaction]
ALTER TABLE [dbo].[TransactionScript] DROP CONSTRAINT [FK_TransactionScript_Transaction]
ALTER TABLE [dbo].[BlockScript] DROP CONSTRAINT [FK_BlockScript_Block]
ALTER TABLE [dbo].[TransactionInput] DROP CONSTRAINT [FK_TransactionInput_Transaction]
ALTER TABLE [dbo].[TransactionOutput] DROP CONSTRAINT [FK_TransactionOutput_Transaction]
ALTER TABLE [dbo].[ContractParameter] DROP CONSTRAINT [FK_ContractParameter_Contract]

Truncate Table AssetTranslation
Truncate Table Attribute
Truncate Table BlockScript
Truncate Table [Block]
Truncate Table Claim
Truncate Table TransactionScript
Truncate Table TransactionInput
Truncate Table TransactionOutput
Truncate Table Transactiontype
Truncate Table Asset
Truncate Table [Transaction]
Truncate Table [Address]
Truncate Table [AddressTransaction]
Truncate Table [AddressBalance]
Truncate Table BlockLog
Truncate Table ErrorTable
Truncate Table DebuggerTable
Truncate Table [ContractParameter]
Truncate Table [Contract]
Truncate Table NepTransfer

DBCC CHECKIDENT ('Attribute', RESEED, 0)
DBCC CHECKIDENT ('[Block]', RESEED, 0)
DBCC CHECKIDENT ('Claim', RESEED, 0)
DBCC CHECKIDENT ('TransactionScript', RESEED, 0)
DBCC CHECKIDENT ('BlockScript', RESEED, 0)
DBCC CHECKIDENT ('TransactionInput', RESEED, 0)
DBCC CHECKIDENT ('TransactionOutput', RESEED, 0)
DBCC CHECKIDENT ('Transactiontype', RESEED, 0)
DBCC CHECKIDENT ('Asset', RESEED, 0)
DBCC CHECKIDENT ('[Transaction]', RESEED, 0)
DBCC CHECKIDENT ('[Address]', RESEED, 0)
DBCC CHECKIDENT ('[Contract]', RESEED, 0)
DBCC CHECKIDENT ('[ContractParameter]', RESEED, 0)
DBCC CHECKIDENT ('[NepTransfer]', RESEED, 0)

ALTER TABLE [dbo].[AssetTranslation]  WITH CHECK ADD  CONSTRAINT [FK_AssetTranslation_Asset] FOREIGN KEY([Id])
REFERENCES [dbo].[Asset] ([Id])
ALTER TABLE [dbo].[AssetTranslation] CHECK CONSTRAINT [FK_AssetTranslation_Asset]

ALTER TABLE [dbo].[Attribute]  WITH CHECK ADD  CONSTRAINT [FK_Attribute_Transaction] FOREIGN KEY([TxId])
REFERENCES [dbo].[Transaction] ([Id])
ALTER TABLE [dbo].[Attribute] CHECK CONSTRAINT [FK_Attribute_Transaction]

ALTER TABLE [dbo].[Claim]  WITH CHECK ADD  CONSTRAINT [FK_Claim_Transaction] FOREIGN KEY([TxId])
REFERENCES [dbo].[Transaction] ([Id])
ALTER TABLE [dbo].[Claim] CHECK CONSTRAINT [FK_Claim_Transaction]

ALTER TABLE [dbo].[TransactionScript]  WITH CHECK ADD  CONSTRAINT [FK_TransactionScript_Transaction] FOREIGN KEY([TxId])
REFERENCES [dbo].[Transaction] ([Id])
ALTER TABLE [dbo].[TransactionScript] CHECK CONSTRAINT [FK_TransactionScript_Transaction]

ALTER TABLE [dbo].[BlockScript]  WITH CHECK ADD  CONSTRAINT [FK_BlockScript_Block] FOREIGN KEY([BlockId])
REFERENCES [dbo].[Block] ([Id])
ALTER TABLE [dbo].[BlockScript] CHECK CONSTRAINT [FK_BlockScript_Block]

ALTER TABLE [dbo].[TransactionInput]  WITH CHECK ADD  CONSTRAINT [FK_TransactionInput_Transaction] FOREIGN KEY([TxId])
REFERENCES [dbo].[Transaction] ([Id])
ALTER TABLE [dbo].[TransactionInput] CHECK CONSTRAINT [FK_TransactionInput_Transaction]

ALTER TABLE [dbo].[TransactionOutput]  WITH CHECK ADD  CONSTRAINT [FK_TransactionOutput_Transaction] FOREIGN KEY([TxId])
REFERENCES [dbo].[Transaction] ([Id])
ALTER TABLE [dbo].[TransactionOutput] CHECK CONSTRAINT [FK_TransactionOutput_Transaction]

ALTER TABLE [dbo].[ContractParameter]  WITH CHECK ADD  CONSTRAINT [FK_ContractParameter_Contract] FOREIGN KEY([ContractId])
REFERENCES [dbo].[Contract] ([Id])
ALTER TABLE [dbo].[ContractParameter] CHECK CONSTRAINT [FK_ContractParameter_Contract]

RETURN 0
