@echo off

if [%1]==[] goto usage
if [%2]==[] goto usage

ROBOCOPY  C:\temp\images\%1  C:\GGAppContent\%1\update\android\images /IS
ROBOCOPY  C:\temp\images\%1  C:\GGAppContent\%1\update\ios\images /IS


if exist "%~dp0dbfiles\GreekGuide_%1_EN_%2.db" (
    echo Creating zip file...  
    goto fileok1
)

:fileok1
if exist "%~dp0dbfiles\GreekGuide_%1_EL_%2.db" (
    goto fileok2
) 
 
echo File not found! Try again! 
goto end

:fileok2

if exist "%~dp0dbfiles\ContentEN.db"   del "%~dp0dbfiles\ContentEN.db" 
if exist "%~dp0dbfiles\ContentGR.db"  del "%~dp0dbfiles\ContentGR.db" 
if exist "%~dp0dbfiles\ContentRU.db"  del "%~dp0dbfiles\ContentRU.db" 


if exist "%~dp0dbfiles\GreekGuide_%1_EN_%2.db" (
 copy  "%~dp0dbfiles\GreekGuide_%1_EN_%2.db"   "%~dp0dbfiles\ContentEN.db"  
)

if exist "%~dp0dbfiles\GreekGuide_%1_EL_%2.db" (
 copy  "%~dp0dbfiles\GreekGuide_%1_EL_%2.db"   "%~dp0dbfiles\ContentGR.db"  
)

if exist "%~dp0dbfiles\GreekGuide_%1_RU_%2.db" (
 copy  "%~dp0dbfiles\GreekGuide_%1_RU_%2.db"   "%~dp0dbfiles\ContentRU.db"  
)



REM Files are created but check again.
REM if RUSSIAN : 
if exist  "%~dp0dbfiles\ContentRU.db" (
 if exist "%~dp0dbfiles\ContentEN.db"  (
  if exist "%~dp0dbfiles\ContentGR.db" (	
  if exist "%~dp0dbfiles\update.zip" del  "%~dp0dbfiles\update.zip"
  "C:\Program Files\7-Zip\7z.exe" a -tzip  "%~dp0dbfiles\update.zip" "%~dp0dbfiles\ContentEN.db" "%~dp0dbfiles\ContentGR.db" "%~dp0dbfiles\ContentRU.db"
  )
 )
)

REM Else..
 if exist "%~dp0dbfiles\ContentEN.db"  (
   if exist "%~dp0dbfiles\ContentGR.db" (	
     if exist "%~dp0dbfiles\update.zip" del  "%~dp0dbfiles\update.zip"
     "C:\Program Files\7-Zip\7z.exe" a -tzip "%~dp0dbfiles\update.zip" "%~dp0dbfiles\ContentEN.db" "%~dp0dbfiles\ContentGR.db"
   )
 )


if exist "%~dp0dbfiles\update.zip" (

  if exist "%~dp0dbfiles\update.zip" ROBOCOPY  "%~dp0dbfiles" C:\GGAppContent\%1\update\ios update.zip /IS
  if exist "%~dp0dbfiles\update.zip" ROBOCOPY  "%~dp0dbfiles" C:\GGAppContent\%1\update\android update.zip /IS
  
  REM also copy the simple DB files
  ROBOCOPY  "%~dp0dbfiles" C:\GGAppContent\%1\update\ios ContentEN.db /IS
  ROBOCOPY  "%~dp0dbfiles" C:\GGAppContent\%1\update\ios ContentGR.db /IS
  ROBOCOPY  "%~dp0dbfiles" C:\GGAppContent\%1\update\ios ContentRU.db /IS

  ROBOCOPY  "%~dp0dbfiles" C:\GGAppContent\%1\update\android ContentEN.db /IS
  ROBOCOPY  "%~dp0dbfiles" C:\GGAppContent\%1\update\android ContentGR.db /IS
  ROBOCOPY  "%~dp0dbfiles" C:\GGAppContent\%1\update\android ContentRU.db /IS

  
  if exist  C:\GGAppContent\%1\update\ios\update.zip goto a1
  goto end
  :a1 
  if exist  C:\GGAppContent\%1\update\android\update.zip goto a2
  goto end
  :a2
  goto success
  
  
)


goto end

:usage
@echo Usage: %0 ^<location_name^> ^<db date in YYYMMDD^>
exit /B 1

:success
echo Process Completed Successfully!
exit /B 
:end
exit /B 

