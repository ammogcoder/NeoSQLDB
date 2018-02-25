
CREATE PROCEDURE [dbo].[StoreContractAsset]
	@json nvarchar(MAX),
	@success bit OUTPUT
AS

BEGIN TRY
	SET NOCOUNT ON;
	--drop table #transaction
	select @success = 1

	--drop table #contract
	SELECT -1 'contract_pk',-1 'asset_pk', * into #contract FROM OPENJSON(@json)
	WITH (
		[version] int,
		[hash] varchar(64),
		[script] nvarchar(max),
		[parameters] nvarchar(max) AS JSON,
		returntype varchar(50),
		[name] varchar(255),
		code_version varchar(50),
		author varchar(50),
		email varchar(255),
		[description] varchar(255),
		assetname varchar(255),
		assetdecimals int,
		assetSymbol varchar(10),
		assetSupply numeric(20,8),
		storage  bit,
		dynamic_invoke bit
		)

	declare @Temp table (
		Id bigint,
		[Hash] varchar(64)
		)

	--insert contract
	insert into [Contract]
	Output
		inserted.Id,
		inserted.[Hash]
	into @Temp
	select distinct
		[hash],
		[script],
		returntype,
		storage,
		[name],
		code_version,
		author,
		email,
		[description],
		dynamic_invoke,
		[version]
	from
		#contract

	--update #contract table with identity ids from @Temp
	update t1
	set	t1.contract_pk = t2.Id
	from #contract t1
	left join @Temp t2 on 
	t1.[hash] = t2.[Hash]




	--insert contract parameters into temp table from #contract
	--drop table #parameters
	SELECT 
		f.contract_pk 'contract_pk',
		result.[value] 'parameter'
	into #parameters
	FROM #contract f 
		CROSS APPLY OPENJSON(f.[parameters]) as result
	order by result.[key]

	--check if parameters needs to be inserted
	if (select count(contract_pk) from #parameters) > 0 begin
		--insert parameters into table
		insert into ContractParameter
			select distinct
				contract_pk,
				parameter
			from #parameters
	end

	declare @TempAsset table (
		Id bigint,
		Asset varchar(64)
		)

	insert into Asset
	Output
		inserted.Id,
		inserted.[Asset]
	into @TempAsset
	select distinct
		[hash],
		'NEP5',
		assetSupply,
		assetdecimals,
		'' 'Owner',
		'' 'Admin',
		0 'Created',
		assetSymbol
	from #contract

	--update #contract table with identity ids from @Temp
	update t1
	set	t1.asset_pk = t2.Id
	from #contract t1
	left join @TempAsset t2 on 
	t1.[hash] = t2.Asset

	insert into AssetTranslation
	select
		asset_pk,
		'en' 'lang',
		0,
		assetname
	from	
		#contract
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