CREATE PROCEDURE [dbo].[uspGetAddress]
	@address varchar(34)
AS
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
	select 
		adr.Id
		,adr.[Address]
		,adr.FirstTime
		,adr.LastTime
		,(select dbo.[ufnToNumberFormatVarchar](ab.Balance) 'Balance', case when asset.Symbol = '' then asset.[Asset] else asset.Symbol end 'Symbol', asset.[Asset] 'Asset' from AddressBalance ab left join Asset asset on asset.Id = ab.AssetId where ab.AddressId = adr.Id for json path) 'Balance'
	from 
		[Address] adr
	where 
		adr.[Address] = @address
	FOR JSON PATH
	RETURN 0
