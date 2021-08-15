SET QUOTED_IDENTIFIER OFF
GO
SET ANSI_NULLS ON
GO
Create Procedure SP_GetPartners 
as

	set nocount on
	select Account,Password,UrlAddress,Cases_Guid
	from Partners
	where deleted = 0;
GO