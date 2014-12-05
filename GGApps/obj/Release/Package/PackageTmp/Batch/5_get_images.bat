@echo off

if [%1]==[] goto usage

set path_from=c:\Temp\Images\%1
set path_to= M:\GreekGuide\Images\%1

del /q %path_from%\*.*
del /q %path_from%-fb\*.*

gg_img.py %1 %2

del /q %path_to%\*.*

xcopy %path_from%\*.* %path_to%

goto end 

:usage
echo Usage: %0 ^<location_name^> [db date in YYYMMDD]
exit /B 1

:end

exit /B