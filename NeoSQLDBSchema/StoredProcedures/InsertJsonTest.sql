CREATE PROCEDURE [dbo].[InsertJsonTest]
	@json nvarchar(max),
	@success bit OUTPUT
AS
	insert into DebuggerTable select @json
	select @success = 1
	return @success
RETURN 1
