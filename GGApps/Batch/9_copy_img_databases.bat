@echo off

if [%1]==[] goto usage
if [%2]==[] goto usage

ROBOCOPY  C:\temp\images\%1  C:\GGAppContent\%1\update\android\images /IS
ROBOCOPY  C:\temp\images\%1  C:\GGAppContent\%1\update\ios\images /IS


if exist "%~dp0dbfiles\android\GreekGuide_%1_EN_%2.db" (
    echo Creating zip file...  
    goto fileok1
)

:fileok1
if exist "%~dp0dbfiles\android\GreekGuide_%1_EL_%2.db" (
    goto fileok2
) 
 
echo File not found! Try again! 
goto end

:fileok2

if exist "%~dp0dbfiles\android\ContentEN.db"   del "%~dp0dbfiles\android\ContentEN.db" 
if exist "%~dp0dbfiles\android\ContentGR.db"  del "%~dp0dbfiles\android\ContentGR.db" 
if exist "%~dp0dbfiles\android\ContentRU.db"  del "%~dp0dbfiles\android\ContentRU.db" 


if exist "%~dp0dbfiles\android\GreekGuide_%1_EN_%2.db" (
 copy  "%~dp0dbfiles\android\GreekGuide_%1_EN_%2.db"   "%~dp0dbfiles\android\ContentEN.db"  
)

if exist "%~dp0dbfiles\android\GreekGuide_%1_EL_%2.db" (
 copy  "%~dp0dbfiles\android\GreekGuide_%1_EL_%2.db"   "%~dp0dbfiles\android\ContentGR.db"  
)

if exist "%~dp0dbfiles\android\GreekGuide_%1_RU_%2.db" (
 copy  "%~dp0dbfiles\android\GreekGuide_%1_RU_%2.db"   "%~dp0dbfiles\android\ContentRU.db"  
)



REM Files are created but check again.
REM if RUSSIAN : 
if exist  "%~dp0dbfiles\android\ContentRU.db" (
 if exist "%~dp0dbfiles\android\ContentEN.db"  (
  if exist "%~dp0dbfiles\android\ContentGR.db" (	
  if exist "%~dp0dbfiles\android\update.zip" del  "%~dp0dbfiles\android\update.zip"
  "C:\Program Files\7-Zip\7z.exe" a -tzip  "%~dp0dbfiles\android\update.zip" "%~dp0dbfiles\android\ContentEN.db" "%~dp0dbfiles\android\ContentGR.db" "%~dp0dbfiles\android\ContentRU.db"
  )
 )
)

REM Else..
if not exist  "%~dp0dbfiles\android\ContentRU.db" (
 if exist "%~dp0dbfiles\android\ContentEN.db"  (
   if exist "%~dp0dbfiles\android\ContentGR.db" (	
     if exist "%~dp0dbfiles\android\update.zip" del  "%~dp0dbfiles\android\update.zip"
     "C:\Program Files\7-Zip\7z.exe" a -tzip "%~dp0dbfiles\android\update.zip" "%~dp0dbfiles\android\ContentEN.db" "%~dp0dbfiles\android\ContentGR.db"
   )
 )
)


if exist "%~dp0dbfiles\android\update.zip" (

  if exist "%~dp0dbfiles\android\update.zip" ROBOCOPY  "%~dp0dbfiles\android" C:\GGAppContent\%1\update\android update.zip /IS

  ROBOCOPY  "%~dp0dbfiles\android" C:\GGAppContent\%1\update\android ContentEN.db /IS
  ROBOCOPY  "%~dp0dbfiles\android" C:\GGAppContent\%1\update\android ContentGR.db /IS
  ROBOCOPY  "%~dp0dbfiles\android" C:\GGAppContent\%1\update\android ContentRU.db /IS

  if exist  C:\GGAppContent\%1\update\android\update.zip goto a23
  goto end
  :a23
  goto successandroid
  
)








REM Do same for IOS
:successandroid

if exist "%~dp0dbfiles\ios\GreekGuide_%1_EN_%2.db" (
    echo Creating zip file...  
    goto fileok11
)

:fileok11
if exist "%~dp0dbfiles\ios\GreekGuide_%1_EL_%2.db" (
    goto fileok22
) 
 
echo File not found! Try again! 
goto end

:fileok22

if exist "%~dp0dbfiles\ios\ContentEN.db"   del "%~dp0dbfiles\ios\ContentEN.db" 
if exist "%~dp0dbfiles\ios\ContentGR.db"  del "%~dp0dbfiles\ios\ContentGR.db" 
if exist "%~dp0dbfiles\ios\ContentRU.db"  del "%~dp0dbfiles\ios\ContentRU.db" 


if exist "%~dp0dbfiles\ios\GreekGuide_%1_EN_%2.db" (
 copy  "%~dp0dbfiles\ios\GreekGuide_%1_EN_%2.db"   "%~dp0dbfiles\ios\ContentEN.db"  
)

if exist "%~dp0dbfiles\ios\GreekGuide_%1_EL_%2.db" (
 copy  "%~dp0dbfiles\ios\GreekGuide_%1_EL_%2.db"   "%~dp0dbfiles\ios\ContentGR.db"  
)

if exist "%~dp0dbfiles\ios\GreekGuide_%1_RU_%2.db" (
 copy  "%~dp0dbfiles\ios\GreekGuide_%1_RU_%2.db"   "%~dp0dbfiles\ios\ContentRU.db"  
)



REM Files are created but check again.
REM if RUSSIAN : 
if exist  "%~dp0dbfiles\ios\ContentRU.db" (
 if exist "%~dp0dbfiles\ios\ContentEN.db"  (
  if exist "%~dp0dbfiles\ios\ContentGR.db" (	
  if exist "%~dp0dbfiles\ios\update.zip" del  "%~dp0dbfiles\ios\update.zip"
  "C:\Program Files\7-Zip\7z.exe" a -tzip  "%~dp0dbfiles\ios\update.zip" "%~dp0dbfiles\ios\ContentEN.db" "%~dp0dbfiles\ios\ContentGR.db" "%~dp0dbfiles\ios\ContentRU.db"
  )
 )
)

REM Else..
if not exist  "%~dp0dbfiles\ios\ContentRU.db" (
 if exist "%~dp0dbfiles\ios\ContentEN.db"  (
   if exist "%~dp0dbfiles\ios\ContentGR.db" (	
     if exist "%~dp0dbfiles\ios\update.zip" del  "%~dp0dbfiles\ios\update.zip"
     "C:\Program Files\7-Zip\7z.exe" a -tzip "%~dp0dbfiles\ios\update.zip" "%~dp0dbfiles\ios\ContentEN.db" "%~dp0dbfiles\ios\ContentGR.db"
   )
 )
)


if exist "%~dp0dbfiles\ios\update.zip" (

  if exist "%~dp0dbfiles\ios\update.zip" ROBOCOPY  "%~dp0dbfiles\ios" C:\GGAppContent\%1\update\ios update.zip /IS

  ROBOCOPY  "%~dp0dbfiles\ios" C:\GGAppContent\%1\update\ios ContentEN.db /IS
  ROBOCOPY  "%~dp0dbfiles\ios" C:\GGAppContent\%1\update\ios ContentGR.db /IS
  ROBOCOPY  "%~dp0dbfiles\ios" C:\GGAppContent\%1\update\ios ContentRU.db /IS
  
  if exist  C:\GGAppContent\%1\update\ios\update.zip goto a2
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
