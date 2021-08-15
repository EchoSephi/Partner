SET QUOTED_IDENTIFIER OFF
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE SP_GetCollectorDay @Collector_Guid uniqueidentifier , @UploadTime datetime , @lst datetime
as 

-- 	declare @Collector_Guid uniqueidentifier = 'AD02195B-1319-4E68-AB06-117E4C3EEDAF';
-- 	declare @UploadTime datetime = '2021-04-11';
-- 	exec SP_GetCollectorDay @Collector_Guid , @UploadTime;

	declare @y int = year(@UploadTime);
	declare @m int = month(@UploadTime);
	declare @d int = datepart(day, @UploadTime);

	declare @MonthtoDate float = 0.0;
	declare @YeartoDate float = 0.0;
	declare @Lifetime float = 0.0;
	declare @Today float = 0.0;
	declare @Yesterday float = 0.0;

	declare @IvtCnt int = 0;
	declare @CollectorDayCnt int  = 0;
	declare @Kws float = 0.0;
	declare @panelCount int = 0;
	declare @FailRate float = 0.0;

	set nocount on

	SELECT @MonthtoDate = (sum(AC_Power) / 60000.0) FROM dbo.PowerHour
	where Collector_Guid = @Collector_Guid 
		and Years = @y
		and Months = month(@UploadTime)
		and Dayss >= 1;

	SELECT @YeartoDate = (sum(AC_Power) / 60000.0)  FROM dbo.PowerHour
	where Collector_Guid = @Collector_Guid 
		and Years = @y
		and ( (Months >= 1 and Months <= (@m-1) and Dayss >=1 and Dayss <=31) 
			or ( Months = @m and Dayss >= 1 and Dayss <=11));

	SELECT @Lifetime = (sum(AC_Power) / 60000.0) FROM dbo.PowerHour
	where Collector_Guid = @Collector_Guid ;
	
	SELECT @Today = (sum(AC_Power) / 60000.0) FROM dbo.PowerHour
	where Collector_Guid = @Collector_Guid 
		and Years = @y
		and Months = @m
		and Dayss = @d ;

	declare @dt1 datetime = dateadd(day , -1 , @UploadTime );
	declare @y1 int = year(@dt1);
	declare @m1 int = month(@dt1);
	declare @d1 int = datepart(day, @dt1);
	
	SELECT @Yesterday = (sum(AC_Power) / 60000.0) FROM dbo.PowerHour
	where Collector_Guid = @Collector_Guid 
		and Years = @y1
		and Months = @m1
		and Dayss = @d1 ;
	
	declare @t1 table (
		Kws float ,
		panelCount int 
	);

	insert into @t1
	exec SolarString.dbo.SP_KWH_Billionwatts @Collector_Guid , @UploadTime , @Today
	
	select @Kws = Kws , @panelCount = panelCount from @t1;

	select @IvtCnt = count(Sort) 
	from Inverter 
	where Collector_Guid = @Collector_Guid 
		and deleted = 0;

	set @FailRate = (((@IvtCnt - @panelCount) / @IvtCnt) * 100);

	select @CollectorDayCnt = count(*) from CollectorDay 
	where Collector_Guid = @Collector_Guid
		and sDate = @UploadTime;

	if (@CollectorDayCnt = 0)
	begin
		insert into CollectorDay values 
		(newid() , @Collector_Guid , @UploadTime , @Today , @Yesterday , @YeartoDate , @MonthtoDate , @Kws , @panelCount , @FailRate);
	end
	else
	begin
		update CollectorDay
		set Today = @Today , YeartoDate = @YeartoDate , MonthtoDate = @MonthtoDate 
			, KWh = @Kws , KWhCnt = @panelCount , FailRate = @FailRate
		where Collector_Guid = @Collector_Guid
			and sDate = @UploadTime;
	end

	update Collector
	set TotalYield = @Lifetime , FailRate = convert(nchar(3),@FailRate) + '%' , LastUploadTime = @lst
	where Guid = @Collector_Guid
GO