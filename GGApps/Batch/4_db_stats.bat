@echo off

if [%1]==[] goto usage
if [%2]==[] goto usage

REM sqlcmd -S localhost -d ContentDB_165_Lan_2_Cat_%1 -U sa -P 3lpid@31 -i db_stats.sql -s "/" -W -o %2
sqlcmd -S 10.0.64.32 -d ContentDB_165_Lan_2_Cat_%1 -U ContentAbility_User_165 -P 3E6EA993-5EBA-4648-BF18-83C38D3E26DC -i db_stats.sql -s "/" -W -o %2

goto end

:usage
@echo Usage: %0 ^<location_id^> ^<export_file^> 
exit /B 1

:end
exit /B 