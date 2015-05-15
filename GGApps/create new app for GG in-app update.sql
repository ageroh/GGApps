
--> Create DB for new GG application

-- take new id and Name of new App e.g. rethymno, 411

-- execute below for ios + android
declare @appID int
	, @App_VersionNumber nvarchar(10)
	, @ConfiguratioNumber nvarchar(10)
	, @DBVersionNumber nvarchar(10)
	, @VersionJSON nvarchar(max)
	, @ConfigurationJSON nvarchar(max)
	, @mobileDevice nvarchar(50)
	, @RC int

set @appID = 411
set @App_VersionNumber = '1'
set @ConfiguratioNumber = '1'
set @DBVersionNumber = '1'
set @VersionJSON = ''
set @ConfigurationJSON = ''
set @mobileDevice = 'android'


EXECUTE @RC = [GG_Reporting].[dbo].[usp_Update_Bundle] 
   @appID
  ,@App_VersionNumber
  ,@ConfiguratioNumber
  ,@DBVersionNumber
  ,@VersionJSON
  ,@ConfigurationJSON
  ,@mobileDevice
GO

-- add 411 and Rethymno under C:\inetpub\wwwroot\GGApps\ExternalApp\SQLiteConverter.exe.config on 10.0.64.66

-- exclue 411 id under all checks in C:\inetpub\wwwroot\GGApps\SQLScripts\*.sql on 10.0.64.66

