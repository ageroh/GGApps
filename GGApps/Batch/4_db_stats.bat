@echo off

if [%1]==[] goto usage

sqlcmd -S localhost -d ContentDB_165_Lan_2_Cat_%1 -U ContentAbility_User_165 -P 3E6EA993-5EBA-4648-BF18-83C38D3E26DC -i db_stats.sql -s "/" -W

goto end

:usage
@echo Usage: %0 ^<location_id^>
exit /B 1

:end
exit /B 