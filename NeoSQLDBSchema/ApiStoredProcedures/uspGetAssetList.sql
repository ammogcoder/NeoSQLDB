CREATE PROCEDURE [dbo].[uspGetAssetList]
	@amount int = 50,
	@page int = 1
AS
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
	SELECT asset.[Id]
      ,asset.[Asset]
      ,asset.[Type]
      ,dbo.[ufnToNumberFormatVarchar](asset.[Amount]) 'Amount'
      ,asset.[Precision]
      ,asset.[Owner]
      ,asset.[Admin]
      ,asset.[Created]
      ,asset.[Symbol]
	  ,transl.[Language]
	  ,transl.[Name]
  FROM [dbo].[Asset] asset
  left join
  (	SELECT Id, [Language], [Name]
	FROM (
	   SELECT *, ROW_NUMBER() OVER (PARTITION BY [Id] ORDER BY CASE WHEN [Language] = 'en' THEN  1 ELSE 2 END) AS rn
	   FROM AssetTranslation) t
	WHERE t.rn = 1 ) transl
	on transl.Id = asset.Id
	order by asset.Id asc
	OFFSET (@page -1) * @amount ROWS
	FETCH NEXT @amount ROWS ONLY
	FOR JSON PATH 
RETURN 0
