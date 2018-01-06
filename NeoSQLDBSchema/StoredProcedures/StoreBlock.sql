CREATE PROCEDURE [dbo].[StoreBlock]
	@json nvarchar(max),
	@success bit OUTPUT
AS

BEGIN TRY
	SET NOCOUNT ON;
	--drop table #transaction
	select @success = 1

	SELECT *, -1 'id_pk' into #block FROM OPENJSON(@json)
	WITH (
		[hash] varchar(64),
		size bigint,
		[version] int,
		previousblockhash varchar(64),
		nextblockhash varchar(64),
		merkleroot varchar(64),
		[time] bigint,
		[index] bigint,
		nonce varchar(16),
		nextconsensus varchar(34),
		txcount int,
		sys_fee numeric(20,8),
		net_fee numeric(20,8),
		script nvarchar(max) AS JSON
		)
	declare @Temp table (
		Id bigint,
		[Hash] varchar(64)
	)
	insert into [Block]
	Output
		inserted.Id,
		inserted.[Hash]	
	into @Temp
	select 
		[hash],
		size,
		[version],
		previousblockhash,
		nextblockhash,
		merkleroot,
		[time],
		[index],
		nonce,
		nextconsensus,
		txcount,
		sys_fee,
		net_fee
	from #block

	--update #transaction table with the identity ids from @Temp
	/*update 
		#block
	set
		id_pk = t1.Id
	from @Temp t1 left join
	#block t2 on
	t1.[Hash] = t2.[Hash]*/

	update t1
	set
		t1.id_pk = t2.Id
	from #block t1 inner join
	@Temp t2 on t1.[Hash] = t2.[Hash]

	--drop table #script
	SELECT 
		result.invocation
		,result.verification
		,f.id_pk
	into #script
	FROM #block f 
		CROSS APPLY OPENJSON(f.script)
			WITH (
				invocation nvarchar(MAX) '$.invocation', 
				verification nvarchar(MAX)  '$.verification'
				) as result

	--check if script needs to be inserted
	if (select count(id_pk) from #script) > 0 begin
		insert into [BlockScript]
			select 
				invocation,
				verification,
				id_pk
			from
				#script
	end

	--update NextBlockHash
	update t1
	set t1.NextBlockHash = t2.[hash]
	from #block t2 inner join 	[Block] t1 
	on t1.[Hash] = t2.previousblockhash

	return @success;
END TRY
BEGIN CATCH
	select @success = 0;
	insert into ErrorTable
    SELECT ERROR_LINE() AS ErrorLine
     ,ERROR_MESSAGE() AS ErrorMessage
	 ,0 'Txid_pk',
	 @json 'json';
	 return @success;
END CATCH
RETURN 0
