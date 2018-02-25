CREATE PROCEDURE [dbo].[StoreBlock]
	@json nvarchar(max),
	@success bit OUTPUT
AS

BEGIN TRY
	SET NOCOUNT ON;
	--drop table #transaction
	select @success = 1

	--Store block json to temp table
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
	--temp table to get the PK(id) and hash from the inserted block
	declare @Temp table (
		Id bigint,
		[Hash] varchar(64)
	)
	--insert new block
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

	--update #block table with the identity ids from @Temp based on the hash
	update t1
	set
		t1.id_pk = t2.Id
	from #block t1 inner join
	@Temp t2 on t1.[Hash] = t2.[Hash]

	--drop table #script
	--insert block script from json into temp table
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
		--insert block script
		insert into [BlockScript]
			select 
				invocation,
				verification,
				id_pk
			from
				#script
	end

	--update NextBlockHash from previous block
	update t1
	set t1.NextBlockHash = t2.[hash]
	from #block t2 inner join 	[Block] t1 
	on t1.[Hash] = t2.previousblockhash

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
