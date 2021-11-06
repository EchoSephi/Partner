CREATE TABLE dbo.BillionwattsPower (
	Guid uniqueidentifier NULL,
	Collector_Guid uniqueidentifier NULL,
	Sort int NULL,
	ACPower float(8) NULL DEFAULT (NULL),
	DCPower float(8) NULL DEFAULT (NULL),
	Sunshine float(8) NULL DEFAULT (NULL),
	Temperature float(8) NULL,
	STATUS nvarchar(50) NULL,
	UploadTime datetime NULL
);
GO
