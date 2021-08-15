CREATE TABLE dbo.Partners (
	Guid uniqueidentifier NULL,
	Name nvarchar(100) NULL,
	Account nvarchar(100) NULL,
	Password nvarchar(100) NULL,
	UrlAddress nvarchar(100) NULL,
	deleted int NULL,
	Cases_Guid uniqueidentifier NULL DEFAULT (NULL)
);
GO