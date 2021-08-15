SET QUOTED_IDENTIFIER OFF
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE SP_KWH_Billionwatts @Collector_Guid uniqueidentifier , @UploadTime datetime , @totalPower float  
as 

-- 	declare @Collector_Guid uniqueidentifier = 'AD02195B-1319-4E68-AB06-117E4C3EEDAF'
-- 	declare @UploadTime datetime = '2021/04/11'
-- 	declare @totalPower float = 1414.898639500001
-- 	exec SP_KWH_Billionwatts @Collector_Guid , @UploadTime , @totalPower

	declare @d1 nvarchar(40)  = CONVERT(nvarchar(10), @UploadTime, 111) + ' 00:00:00'
	declare @d2 nvarchar(40) =  CONVERT(nvarchar(10), dateadd(day,1,@UploadTime), 111) + ' 00:00:00'

	declare @PanelCount1 int = 0 ;
	declare @PanelCount2 int = 0 ;

	declare @PanelPower1 float = 20000 ; -- 20A 最大功率 20000
	declare @PanelPower2 float = 70000 ; -- 70A 最大功率 70000

	declare @Kws float = 0;
	declare @Kw1 float = 0;
	declare @Kw2 float = 0;

	declare @TmpTable table (PanelCount int,SType nvarchar(30))
	
	set nocount on

	insert into @TmpTable
	select count(Sort) as PanelCount,SType
	from (
		select sum(ACPower) as power , B.Sort  ,A.SType
		from SolarCase.dbo.Inverter A with (nolock)
			join dbo.BillionwattsPower B  with (nolock)
				on A.Sort = B.Sort
		where B.Collector_Guid = @Collector_Guid
			and A.Collector_Guid = @Collector_Guid
			and B.UploadTime >= @d1
			and B.UploadTime <  @d2
			and A.deleted = 0 
		group by  B.Sort ,A.SType
	) X
	where X.power > 0.0
	group by SType

	select @PanelCount1 = PanelCount from @TmpTable where Stype = '20A'
	select @PanelCount2 = PanelCount from @TmpTable where Stype = '70A'

	if (@PanelCount1 > 0)
	begin
		set @Kw1 = (@PanelPower1 * @PanelCount1) / convert(float,1000)
	end

	if (@PanelCount2 > 0)
	begin
		set @Kw2 = (@PanelPower2 * @PanelCount2) / convert(float,1000)
	end

	set @Kws = (@totalPower) / (@Kw1 + @Kw2  )

	select @Kws as Kws , (@panelCount1 + @PanelCount2 ) as panelCount;
GO