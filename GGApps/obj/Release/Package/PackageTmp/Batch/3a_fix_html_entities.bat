@echo off

if [%1]==[] goto usage

gg_fix_html_entities.py %1 %2

goto end 

:usage
echo Usage: %0 ^<location_name^> [db date in YYYMMDD]
exit /B 1

:end

exit /B
