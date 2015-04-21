@echo off

if [%1]==[] goto usage

cd /
cd GGAppContent
git add .
git commit -m "%1 updated."

goto success

:usage
@echo Usage: %0 ^<location_name^> 
exit /B 1

:success
echo Process Completed Successfully!
exit /B 
:end
exit /B 

