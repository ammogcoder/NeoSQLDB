CREATE PROCEDURE [dbo].[uspGetBlockHash]
	@blockhash varchar(64)
AS
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
	SELECT [Id]
      ,[Hash]
      ,[Size]
      ,[Version]
      ,[PreviousBlockHash]
      ,[NextBlockHash]
      ,[Merkleroot]
      ,[Time]
      ,[Index]
      ,[Nonce]
      ,[NextConsensus]
      ,[TxCount]
      ,[Sys_Fee]
      ,[Net_Fee]
  FROM [dbo].[Block]
  where [Hash] = @blockhash
  FOR JSON PATH
RETURN 0
