
CREATE PROCEDURE [dbo].[StoreTransactions]
	@json nvarchar(MAX),
	@success bit OUTPUT
AS

BEGIN TRY
	SET NOCOUNT ON;
	--drop table #transaction
	select @success = 1

	--insert transactions from json into temp table
	SELECT *, -1 'txid_pk' into #transaction FROM OPENJSON(@json)
	WITH (
		txid varchar(64),
		size bigint,
		type varchar(50),
		version int,
		sys_fee numeric(20,8),
		net_fee numeric(20,8),
		nonce varchar(16),
		time bigint,
		blockid bigint,
		blockhash varchar(64),
		gas numeric(20,9),
		publickey varchar(34),
		[script] nvarchar(max),
		vin nvarchar(max) AS JSON,
		vout nvarchar(max) AS JSON,
		scripts nvarchar(max) AS JSON,
		attributes nvarchar(max) AS JSON,
		asset nvarchar(max) AS JSON,
		claims nvarchar(max) AS JSON
		)

	--log the block and transcations
	insert into BlockLog 
	select getdate(), blockid, txid from #transaction

	--insert transaction type if not exist
	insert into TransactionType
		select distinct
			[type]
		from
			#transaction t1
		left join
			TransactionType t2
		on
			t2.Name = t1.[type]
		where
			t2.Id is null
	
	--temp table to get the inserted transcations PKs(Id)
	declare @Temp table (
		Id bigint,
		TxId varchar(64)
	)

	--insert transaction into transaction table
	--insert the identity ids into @Temp table
	insert into [Transaction]
	Output
		inserted.Id,
		inserted.TxId
	into @Temp
	select
		txid,
		size,
		t.[type],
		tt.Id,
		[version],
		sys_fee,
		net_fee,
		nonce,
		[time],
		blockid,
		blockhash,
		gas,
		publickey,
		t.[script]
	from
		#transaction t
	left join
		TransactionType tt
	on tt.[Name] = t.[type]

	--update #transaction table with the identity ids from @Temp
	update t1
	set	t1.txid_pk = t2.Id
	from #transaction t1
	left join @Temp t2 on 
	t1.txid = t2.TxId
	
	--get the time from the block
	declare @time int;
	select top 1 @time = [time] from #transaction

	--insert vin into temp table from #transaction
	--drop table #vin
	SELECT 
		f.txid 
		,right(result.txid,64) 'vin_txid'
		,result.vout
		,f.txid_pk 'txid_pk' 
		,-1 'vin_txid_pk' 
		,f.[time]
	into #vin
	FROM #transaction f 
		CROSS APPLY OPENJSON(f.vin)
		WITH (txid nvarchar(66) '$.txid'
				,vout int '$.vout'
				) as result

	--insert vout into temp table from #transaction
	--drop table #vout
	SELECT 
		f.txid 
		,result.n
		,right(result.asset,64) 'asset'
		,result.[address]
		,result.[value]
		,f.txid_pk 'txid_pk' 
		,f.[time]
	into #vout
	FROM #transaction f 
		CROSS APPLY OPENJSON(f.vout)
			WITH (n int '$.n',
			asset varchar(66) '$.asset',
			address varchar(34) '$.address',
			value numeric(20,8) '$.value'
				) as result

	--insert scripts into temp table from #transaction
	--drop table #script
	SELECT 
		f.txid 
		,result.invocation
		,result.verification
		,f.txid_pk 'txid_pk' 
	into #script
	FROM #transaction f 
		CROSS APPLY OPENJSON(f.scripts)
			WITH (
				invocation nvarchar(MAX) '$.invocation', 
				verification nvarchar(MAX)  '$.verification'
				) as result

	--insert attributes into temp table from #transaction
	--drop table #attribute
	SELECT 
		f.txid 
		,result.usage
		,result.[data]
		,f.txid_pk 'txid_pk' 
	into #attribute
	FROM #transaction f 
		CROSS APPLY OPENJSON(f.attributes)
			WITH (
				usage nvarchar(MAX) '$.usage', 
				[data] nvarchar(MAX)  '$.data'
				) as result

	--insert assets into temp table from #transaction
	--drop table #asset
	SELECT 
		f.txid 
		,result.[type]
		,result.amount
		,result.[precision]
		,result.[owner]
		,result.[admin]
		,result.[name]
		,f.txid_pk 'txid_pk' 
	into #asset
	FROM #transaction f 
		CROSS APPLY OPENJSON(f.asset)
			WITH (
				[type] VARCHAR(50) '$.type', 
				amount numeric(20,8) '$.amount',
				[precision] int '$.precision',
				[owner] varchar(34) '$.owner',
				[admin] varchar(34) '$.admin',
				[name] nvarchar(max) '$.name' as JSON 
				) as result

	--insert claims into temp table from #transaction
	--drop table #claim
	SELECT 
		f.txid 
		,right(result.txid,64) 'claim_txid'
		,result.vout
		,f.txid_pk 'txid_pk' 
		,-1 'claim_txid_pk'
	into #claim
	FROM #transaction f 
		CROSS APPLY OPENJSON(f.claims)
			WITH (
				txid varchar(66) '$.txid',
				vout int  '$.vout'
				) as result

	--check if asset needs to be inserted
	if (select count(txid_pk) from #asset) > 0 begin
		--insert asset into table
		insert into Asset
			select distinct
				txid,
				[type],
				amount,
				[precision],
				[owner],
				[admin],
				@time,
				''
			from #asset

		--insert asset transaction/name into table
		insert into AssetTranslation
		SELECT 
			asset.Id
			,result.lang
			,0
			,result.name
		FROM #asset f 
			CROSS APPLY OPENJSON(f.name)
				WITH (
					lang  nvarchar(10) '$.lang',
					[name] nvarchar(200) '$.name' 
					) as result
		left join
			[Asset] asset on asset.Asset = f.txid
	end

	--insert vout
	if (select count(txid_pk) from #vout) > 0 begin

		--store addresses first if not available in Address table
		INSERT INTO [Address] 
			([Address], FirstTime, LastTime)
		SELECT distinct
			v.[address], v.[time], v.[time]
		FROM #vout v
		LEFT JOIN [Address] a on a.[Address] =  v.[address]
		WHERE a.[Address] is null --only insert if not in table yet
		
		--temp table for getting IDs from insert
		declare @TempVout table (
			TxId bigint,
			AddressId bigint,
			AssetId bigint,
			[Value] numeric(20,8)
		)
		--insert into transaction output table
		insert into [TransactionOutput]
		Output
			inserted.TxId,
			inserted.AddressId,
			inserted.AssetId,
			inserted.[Value]
		into @TempVout
			select
				v.txid_pk,
				v.txid,
				v.n,
				case when a.Id is null then 0 else a.Id end 'Id',--for debug 
				v.asset 'asset',
				v.[address],
				ad.Id, 
				v.[value],
				null,
				null
			from 
				#vout v
			inner join
				[Asset] a
			on
				a.Asset = v.asset
			inner join
				[Address] ad
			on
				ad.[Address] = v.[address]

			--update last time		
			update t1
			set t1.LastTime = @time
			from @TempVout t2 inner join
			[Address] t1 on t2.AddressId = t1.Id

			--insert or update balance
			--update existing addresses first
			update t1
				set t1.[Balance] = [Balance] + t2.[ValueSum]
				from AddressBalance t1
				inner join (select v.AddressId, v.AssetId, SUM(v.[Value]) as ValueSum
							from @TempVout v group by AddressId, AssetId) as t2
				on t2.AddressId = t1.AddressId and t2.AssetId = t1.AssetId

			--insert all addresses that are not yet in AddressBalance (first transaction for this address)
			insert into [AddressBalance]
			select vout.AddressId,vout.AssetId,sum(vout.[Value])
			from 
				@TempVout vout
			left join
				[AddressBalance] ab
			on
				ab.AddressId = vout.AddressId
				and ab.AssetId = vout.AssetId
			where 
				ab.AddressId is null
			group by vout.AddressId,vout.AssetId

			--update AddressTransaction (no duplicate address/transaction combination is inserted)
			insert into [AddressTransaction]
			select distinct
				v.AddressId,
				v.TxId
			from
				@TempVout v
			left join
				[AddressTransaction] adt
			on
				adt.AddressId = v.AddressId
				and adt.TransactionId = v.TxId
			where
				adt.AddressId is null
	end

	--insert vin
	if (select count(txid_pk) from #vin) > 0 begin
		--get the respective VOUT primary key for this VIN and write into #vin temp table
		update t1
		set t1.vin_txid_pk = t2.Id
		from #vin t1 inner join [Transaction] t2
		on t2.TxId = t1.vin_txid

		--temp table for getting IDs while insert
		declare @TempVin table (
			TxId bigint,
			AddressId bigint,
			AssetId bigint,
			[Value] numeric(20,8)
		)

		--insert transaction inputs 
		insert into [TransactionInput]
		Output
			inserted.TxId,
			inserted.AddressId,
			inserted.AssetId,
			inserted.[Value]
		into @TempVin
			select
				v.txid_pk,
				v.txid,
				v.vout,
				case when a.Id is null then 1 else a.id end 'Id', --for debug 
				case when a.Asset is null then '1' else a.Asset end 'Asset',--for debug 
				case when vout.[Address] is null then 'debug' else vout.[Address] end 'Address', --for debug
				ad.Id, 
				case when vout.[Value] is null then 0 else vout.[Value] end 'Value', --for debug
				case when vout.TxId is null then 0 else vout.TxId end 'FromTxId', --for debug
				v.vin_txid 'txid' --FromTxIdhash
			from
				#vin v
			inner join 
				[TransactionOutput] vout
			on
				vout.TxId = v.vin_txid_pk
				and vout.N = v.vout
			inner join
				[Asset] a
			on
				a.Id = vout.AssetId
			inner join
				[Address] ad
			on
				ad.Id = vout.[AddressId]

			--update the VOUT link (ToTxId) for new VINs 
			update t1
				set	t1.ToTxId = t2.txid_pk,
					t1.ToTxIdHash = t2.txid
				from #vin t2 inner join
				TransactionOutput t1
				on t1.TxId = t2.vin_txid_pk
				and t1.N = t2.vout


			--update balance
			update t1
				set t1.[Balance] = [Balance] - t2.[ValueSum]
				from AddressBalance t1
				inner join (select v.AddressId, v.AssetId, SUM(v.[Value]) as ValueSum
							from @TempVin v group by AddressId, AssetId) as t2
				on t2.AddressId = t1.AddressId and t2.AssetId = t1.AssetId

			--update lasttime		
			update t1
			set t1.LastTime = @time
			from @TempVin t2 inner join
			[Address] t1 on t2.AddressId = t1.Id


			--update AddressTransaction
			insert into [AddressTransaction]
			select distinct
				v.AddressId,
				v.TxId
			from
				@TempVin v
			left join
				[AddressTransaction] adt
			on
				adt.AddressId = v.AddressId
				and adt.TransactionId = v.TxId
			where
				adt.AddressId is null
	end
	--check if claims needs to be inserted
	if (select count(txid_pk) from #claim) > 0 begin
		--get transaction pk id from claim tx
		update t1
			set t1.claim_txid_pk = t2.Id
			from #claim t1
			inner join [Transaction] t2
			on t1.claim_txid  = t2.TxId
		--insert claims
		insert into [Claim]
			select
				vout,
				txid_pk,
				claim_txid_pk
			from
				#claim
	end
	--check if script needs to be inserted
	if (select count(txid_pk) from #script) > 0 begin
		--insert transaction scripts
		insert into [TransactionScript]
			select 
				invocation,
				verification,
				txid_pk
			from
				#script
	end
	--check if attributes needs to be inserted
	if (select count(txid_pk) from #attribute) > 0 begin
		--insert attributes
		insert into [Attribute]
			select 
				usage,
				[data],
				txid_pk
			from
				#attribute
	end
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