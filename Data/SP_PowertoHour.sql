SET QUOTED_IDENTIFIER OFF
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE SP_PowertoHour (@Collector_Guid uniqueidentifier , @d1 datetime , @d2 datetime)
as

-- 	DECLARE @Collector_Guid uniqueidentifier = 'AD02195B-1319-4E68-AB06-117E4C3EEDAF';
-- 	DECLARE @d1 datetime = '2021-03-31 13:15:00.000' ;
-- 	DECLARE @d2 datetime = '2021-03-31 13:27:00.000';
-- 	execute SP_PowertoHour @Collector_Guid , @d1 , @d2 ;
	
	DECLARE @TmpTable TABLE (
		Sort int,
		Years int,
		Months int,
		Dayss int,
		Hourss int,
		AC_Power float,
		DC_Power float
	);

	set nocount on

	insert into @TmpTable 
	SELECT Sort , 
			year(UploadTime) as Years , 
			month(UploadTime) as Months , 
			datepart(day,UploadTime) as Dayss, 
			datepart(hour,UploadTime) as Hourss , 
			sum(ACPower) as AC_Power , 
			sum(DCPower) as DC_Power 
	from BillionwattsPower
	where Collector_Guid = @Collector_Guid
	and UploadTime >= @d1
	and UploadTime <= @d2 
	group by Sort , year(UploadTime) , month(UploadTime) , datepart(day,UploadTime) , datepart(hour,UploadTime);
	
	DECLARE spidCursor CURSOR
	LOCAL FAST_FORWARD
	FOR
		SELECT Sort,Years,Months,Dayss,Hourss,AC_Power,DC_Power
		from @TmpTable
		order by Sort,Years,Months,Dayss,Hourss;
	OPEN spidCursor

	DECLARE @Sort int;
	DECLARE @Years int;
	DECLARE @Months int;
	DECLARE @Dayss int;
	DECLARE @Hourss int;
	DECLARE @AC_Power float;
	DECLARE @DC_Power float;

	FETCH NEXT FROM spidCursor INTO @Sort,@Years,@Months,@Dayss,@Hourss,@AC_Power,@DC_Power
	WHILE (@@FETCH_STATUS = 0)
		BEGIN
			declare @cnt int = 0;
			select @cnt = count(*) from SolarCase.dbo.PowerHour
			where Collector_Guid = @Collector_Guid
				and Sort = @Sort
				and Years = @Years
				and Months = @Months
				and Dayss = @Dayss
				and Hourss = @Hourss;
			if (@cnt = 0)
			begin
				insert into SolarCase.dbo.PowerHour values
				(newid(), @Collector_Guid,@Sort,@Years,@Months,@Dayss,@Hourss,@AC_Power,@DC_Power);
			end
			else
			begin
				update SolarCase.dbo.PowerHour
				set AC_Power = @AC_Power , DC_Power = @DC_Power
				where Collector_Guid = @Collector_Guid
				and Sort = @Sort
				and Years = @Years
				and Months = @Months
				and Dayss = @Dayss
				and Hourss = @Hourss;
			end
	
		FETCH NEXT FROM spidCursor INTO @Sort,@Years,@Months,@Dayss,@Hourss,@AC_Power,@DC_Power
		End
	Close spidCursor
	DEALLOCATE spidCursor
GO