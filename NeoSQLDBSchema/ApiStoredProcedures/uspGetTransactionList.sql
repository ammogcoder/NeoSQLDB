CREATE PROCEDURE [dbo].[uspGetTransactionList]
	@amount int = 20,
	@page int = 1
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
	order by Id desc
	OFFSET (@page -1) * @amount ROWS
	FETCH NEXT @amount ROWS ONLY
	FOR JSON PATH 
RETURN 0
