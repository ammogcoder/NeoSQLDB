
CREATE PROCEDURE [dbo].[StoreNEPTransfer]
	@json nvarchar(MAX),
	@success bit OUTPUT
AS

BEGIN TRY
	SET NOCOUNT ON;
	--drop table #transaction
	select @success = 1

	--drop table #neptransfer
	SELECT *, -1 'contract_pk', -1 'asset_pk', -1 'addrfromid', -1 'addrtoid', -1 'txid_pk' into #neptransfer FROM OPENJSON(@json)
	WITH (
		txid varchar(64),
		[time] bigint,
		[contract] varchar(64),
		blockid bigint,
		addressfrom varchar(34),
		addressto varchar(34),
		[value] numeric(20,8)
		)
	--update contract pk id and asset pk id in temp table
	update t1
	set	
		t1.contract_pk = t2.Id,
		t1.asset_pk = t3.Id,
		t1.txid_pk = t4.Id
	from #neptransfer t1
	left join [Contract] t2 on 
	t1.[contract] = t2.[Hash]
	left join [Asset] t3 on
	t1.[contract] = t3.Asset
	left join [Transaction] t4 on
	t1.[txid] = t4.TxId

	--store addressesfrom first if not available in Address table
	INSERT INTO [Address] 
		([Address], FirstTime, LastTime)
	SELECT distinct
		v.addressfrom, v.[time], v.[time]
	FROM #neptransfer v
	LEFT JOIN [Address] a on a.[Address] =  v.addressfrom
	WHERE a.[Address] is null --only insert if not in table yet
	and v.addressfrom <> '' --empty from address possible

	--store addressesto first if not available in Address table
	INSERT INTO [Address] 
		([Address], FirstTime, LastTime)
	SELECT distinct
		v.addressto, v.[time], v.[time]
	FROM #neptransfer v
	LEFT JOIN [Address] a on a.[Address] =  v.addressto
	WHERE a.[Address] is null --only insert if not in table yet

	--update addrfromid temp table
	update t1
	set	
		t1.addrfromid = t2.Id
	from #neptransfer t1
	left join [Address] t2 on 
	t1.addressfrom = t2.[Address]
	where t1.addressfrom <> ''

	--update addrtoid temp table
	update t1
	set	
		t1.addrtoid = t3.Id
	from #neptransfer t1
	left join [Address] t3 on
	t1.addressto = t3.[Address]

	--update addressfrom balance
	update t1
		set t1.[Balance] = [Balance] - t2.[ValueSum]
		from AddressBalance t1
		inner join (select v.addrfromid, v.asset_pk, SUM(v.[Value]) as ValueSum
					from #neptransfer v group by addrfromid, asset_pk) as t2
		on t2.addrfromid = t1.AddressId and t2.asset_pk = t1.AssetId

	--update addressto balance
	update t1
		set t1.[Balance] = [Balance] + t2.[ValueSum]
		from AddressBalance t1
		inner join (select v.addrtoid, v.asset_pk, SUM(v.[Value]) as ValueSum
					from #neptransfer v group by addrtoid, asset_pk) as t2
		on t2.addrtoid = t1.AddressId and t2.asset_pk = t1.AssetId


	--insert all addresses that are not yet in AddressBalance (first transaction for this asset to this address)
	insert into [AddressBalance]
		select nep.addrtoid,nep.asset_pk,sum(nep.[Value])
		from 
			#neptransfer nep
		left join
			[AddressBalance] ab
		on
			ab.AddressId = nep.addrtoid
			and ab.AssetId = nep.asset_pk
		where 
			ab.AddressId is null
		group by nep.addrtoid,nep.asset_pk
	

	--update addresstransaction (no duplicates) addressfrom
	insert into [AddressTransaction]
		select distinct
			v.addrfromid,
			v.txid_pk
		from
			#neptransfer v
		left join
			[AddressTransaction] adt
		on
			adt.AddressId = v.addrfromid
			and adt.TransactionId = v.txid_pk
		where
			adt.AddressId is null
			and v.addrfromid >= 0


	--update addresstransaction (no duplicates) addressto
	insert into [AddressTransaction]
		select distinct
			v.addrtoid,
			v.txid_pk
		from
			#neptransfer v
		left join
			[AddressTransaction] adt
		on
			adt.AddressId = v.addrtoid
			and adt.TransactionId = v.txid_pk
		where
			adt.AddressId is null

	--update last time	addrfrom	
	update t1
		set t1.LastTime = [Time]
		from #neptransfer t2 inner join
		[Address] t1 on t2.addrfromid = t1.Id

	--update last time	addrto	
	update t1
		set t1.LastTime = [Time]
		from #neptransfer t2 inner join
		[Address] t1 on t2.addrtoid = t1.Id


	--finally insert nep5 transfer
	insert into NepTransfer
		select
			nep.addressfrom,
			nep.addrfromid,
			nep.addressto,
			nep.addrtoid,
			nep.[Value],
			nep.[contract],
			nep.contract_pk,
			nep.txid,
			nep.txid_pk,
			nep.[time],
			nep.blockid,
			asset.Symbol,
			nep.asset_pk
		from
			#neptransfer nep
		inner join
			Asset asset
		on
			asset.Id = nep.asset_pk
			and asset.[Type] = 'NEP5'

	return @success;
	SET NOCOUNT OFF;
END TRY
BEGIN CATCH
	select @success = 0;
	--on error insert into ErrorTable and return success = false
	insert into ErrorTable
    SELECT ERROR_LINE() AS ErrorLine
     ,ERROR_MESSAGE() AS ErrorMessage
	 ,0 'Txid_pk',
	 @json 'json';
	 return @success;
	 SET NOCOUNT OFF;
END CATCH
RETURN 0