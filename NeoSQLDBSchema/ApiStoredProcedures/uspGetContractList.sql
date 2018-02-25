CREATE PROCEDURE [dbo].[uspGetContractList]
	@amount int = 50,
	@page int = 1
AS
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
	SELECT c.[Id]
      ,c.[Hash]
      ,c.[Script]
      ,c.[ReturnType]
      ,c.[Storage]
      ,c.[Name]
      ,c.[CodeVersion]
      ,c.[Author]
      ,c.[Email]
      ,c.[Description]
      ,c.[DynamicInvoke]
      ,c.[Version]
	  ,(select dbo.ufnToRawJsonArray((select Parameter from ContractParameter where ContractId = c.Id for json path), 'Parameter')) 'Parameter'
  FROM [dbo].[Contract] c
  order by c.Id asc
	OFFSET (@page -1) * @amount ROWS
	FETCH NEXT @amount ROWS ONLY
	FOR JSON PATH 
RETURN 0
