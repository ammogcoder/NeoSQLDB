CREATE TABLE [dbo].[AddressBalance]
(
	[AddressId] BIGINT NOT NULL,
    [AssetId] BIGINT NOT NULL, 
    [Balance] NUMERIC(20, 8) NOT NULL, 
    PRIMARY KEY ([AddressId],[AssetId])
)
