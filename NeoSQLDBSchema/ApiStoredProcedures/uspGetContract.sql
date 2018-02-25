CREATE PROCEDURE [dbo].[uspGetContract]
	@contracthash varchar(64)
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
  where c.[Hash] = @contracthash
  FOR JSON PATH
RETURN 0
