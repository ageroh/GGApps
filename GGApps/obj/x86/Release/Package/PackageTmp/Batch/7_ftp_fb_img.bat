@echo off

if [%1]==[] goto usage

del ftpcmd.dat

echo cd /var/www/greekguide/%1/fb-assets>> ftpcmd.dat
echo binary>> ftpcmd.dat
echo lcd C:\Temp\Images\%1-fb>> ftpcmd.dat
echo synchronize remote>> ftpcmd.dat
echo exit>> ftpcmd.dat

winscp.com ggftp /script=ftpcmd.dat

echo " "

goto end

:usage
@echo Usage: %0 ^<location_name^>
exit /B 1

:end

exit /B