@echo off

if [%1]==[] goto usage
if [%2]==[] goto usage

sqlcmd -S 10.0.64.32 -d ContentDB_165_Lan_2_Cat_%2 -U ContentAbility_User_165 -P 3E6EA993-5EBA-4648-BF18-83C38D3E26DC -Q "set nocount on SELECT text FROM entity_text_vw quit" -W -h -1 -o entity_text.txt

set curr=%CD%
del ftpcmd.dat

echo cd /var/www/greekguide/%1/config>> ftpcmd.dat
echo lcd "%curr%">> ftpcmd.dat
echo option confirm off >> ftpcmd.dat
echo put  entity_text.txt>> ftpcmd.dat
echo exit>> ftpcmd.dat
winscp.com ggftp /script=ftpcmd.dat

echo " "

goto end

:usage
@echo Usage: %0 ^<location_name^> ^<location_id^>
exit /B 1

:end

exit /B