@echo off

if [%1]==[] goto usage

set path_from=c:\Temp\Images\%1

REM Delete old files from temp directories to rebuild them
del /f /q %path_from%\*.*
REM del /f /q %path_from%-fb\*.*

gg_img.py %1 %2

REM del /q %path_to%\*.*

goto end 

:usage
echo Usage: %0 ^<location_name^> [db date in YYYMMDD]
exit /B 1

:end

exit /B