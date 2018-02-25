CREATE PROCEDURE [dbo].[uspGetBlockId]
	@blockid bigint
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
  where Id = @blockid
  FOR JSON PATH
RETURN 0
