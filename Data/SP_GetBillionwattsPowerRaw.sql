SET QUOTED_IDENTIFIER OFF
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE SP_GetBillionwattsPowerRaw (@CollectorId uniqueidentifier,@date datetime)
as
-- 	execute SP_GetBillionwattsPowerRaw 'AD02195B-1319-4E68-AB06-117E4C3EEDAF','2021-03-31 13:17:00.000'


	set nocount on;

	SELECT UploadTime,Sort,ACPower 
	FROM dbo.BillionwattsPower
	where Collector_Guid = @CollectorId
	and UploadTime > @date
	order by UploadTime,Sort;
GO