CREATE TABLE [dbo].[AddressTransaction]
(
	[AddressId] BIGINT NOT NULL, 
    [TransactionId] BIGINT NOT NULL, 
    PRIMARY KEY ([AddressId],[TransactionId])
)
