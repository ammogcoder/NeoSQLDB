CREATE PROCEDURE [dbo].[uspGetTransaction]
	@txhash varchar(64)
AS
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
	SELECT [Id]
      ,[TxId]
      ,[Size]
      ,[Type]
      ,[TypeId]
      ,[Version]
      ,[Sys_Fee]
      ,[Net_Fee]
      ,[Nonce]
      ,[Time]
      ,[BlockId]
      ,[BlockHash]
      ,[Gas]
      ,[PublicKey]
      ,[Script]
  FROM [dbo].[Transaction]
  where TxId = @txhash
  FOR JSON PATH
RETURN 0
