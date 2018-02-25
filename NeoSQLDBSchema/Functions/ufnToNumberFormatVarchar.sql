CREATE FUNCTION [dbo].[ufnToNumberFormatVarchar]
(
	@value numeric(20,8)
)
RETURNS varchar(30)
AS
BEGIN
	RETURN case when @value < 1 then cast(FORMAT(@value,'0.############') as varchar(30)) else cast(FORMAT(@value,'###,###,###,###,###.############') as varchar(30)) end
END
