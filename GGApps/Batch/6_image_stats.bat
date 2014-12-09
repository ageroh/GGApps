@echo off

if [%1]==[] goto usage
if [%2]==[] goto usage
if [%3]==[] goto usage

set path_from=c:\Temp\Images\%1

echo *	> %3
echo *** images in db	>> %3
echo *					>> %3
sqlcmd -S localhost -d ContentDB_165_Lan_1_Cat_%2 -U ContentAbility_User_165 -P 3E6EA993-5EBA-4648-BF18-83C38D3E26DC -i db_number_of_images.sql -h -1 	>> %3
echo *	>> %3
echo *** fb images in local dir (%path_from%-fb) 	>> %3
echo *	>> %3
dir /b %path_from%-fb | wc -l | sed "s/^ */* /"  >> %3
echo *	>> %3
echo *** images in local dir (%path_from%) 	>> %3
echo *	>> %3
dir /b %path_from% | wc -l | sed "s/^ */* /"	>> %3
echo *	>> %3


goto end

:usage
@echo Usage: %0 ^<location_name^> ^<location_id^> ^<OutFilePath^>
exit /B 1

:end

exit /B