CREATE PROCEDURE [dbo].[uspGetAddressList]
	@amount int = 20,
	@page int = 1
AS
	select 
		adr.Id
		,adr.[Address]
		,adr.FirstTime
		,adr.LastTime
		,(select dbo.[ufnToNumberFormatVarchar](ab.Balance) 'Balance',case when asset.Symbol = '' then asset.[Asset] else asset.Symbol end 'Symbol', asset.[Asset] 'Asset' from AddressBalance ab left join Asset asset on asset.Id = ab.AssetId where ab.AddressId = adr.Id for json path) 'Balance'
	from 
		[Address] adr
	order by Id desc
	OFFSET (@page -1) * @amount ROWS
	FETCH NEXT @amount ROWS ONLY
	FOR JSON PATH 
RETURN 0
