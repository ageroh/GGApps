@echo off

if [%1]==[] goto usage

if exist "C:\GGAppContent\%1\%1.zip" del  "C:\GGAppContent\%1\%1.zip"

REM Create a zip with whole update folder under appName
"C:\Program Files\7-Zip\7z.exe" a -tzip  "C:\GGAppContent\%1\%1.zip" "C:\GGAppContent\%1\update\"
 
if exist  "C:\GGAppContent\%1\%1.zip" goto success
else goto end

:usage
@echo Usage: %0 ^<location_name^> 
exit /B 1

:success
echo Process Completed Successfully!
exit /B 
:end
exit /B 

