SET QUOTED_IDENTIFIER OFF
GO
SET ANSI_NULLS ON
GO
ALTER PROCEDURE SP_GetCollectors (@CasesId uniqueidentifier)
as
-- 	execute SP_GetCollectors '7263EA04-DD42-41CC-8CB8-4411C1F31E77'

	set nocount on

	SELECT Sort , Guid , MacAddress , LastUploadTime
	FROM Collector
	where Cases_Guid = @CasesId
		and deleted = 0 
	order by Sort;
GO