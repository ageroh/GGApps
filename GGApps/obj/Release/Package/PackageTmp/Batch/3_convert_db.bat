@echo off

if [%1]==[] goto usage

For /f "tokens=2-4 delims=/ " %%a in ('date /t') do (set mydate=%%c%%b%%a)

SQLiteConverter %1

goto end

:usage
@echo Usage: %0 ^<location_name^>
exit /B 1

:end

exit /B
