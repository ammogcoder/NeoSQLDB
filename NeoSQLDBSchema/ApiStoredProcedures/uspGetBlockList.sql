CREATE PROCEDURE [dbo].[uspGetBlockList]
	@amount int = 20,
	@page int = 1
AS
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
	SELECT [Id]
		  ,[Hash]
		  ,[Size]
		  ,[Version]
		  ,isnull([PreviousBlockHash],'') 'PreviousBlockHash'
		  ,isnull([NextBlockHash],'') 'NextBlockHash'
		  ,[Merkleroot]
		  ,[Time]
		  ,[Index]
		  ,[Nonce]
		  ,[NextConsensus]
		  ,[TxCount]
		  ,[Sys_Fee]
		  ,[Net_Fee]
	FROM [dbo].[Block]
	order by Id desc
	OFFSET (@page -1) * @amount ROWS
	FETCH NEXT @amount ROWS ONLY
	FOR JSON PATH 
RETURN 0
