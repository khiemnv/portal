::export.bat <directory>
@echo off
if "%1"=="" (
  echo The syntax of the command is incorrect.
  echo   please specify output directory: export.bat ^<directory^>
  goto:eof
)
echo export to "%1" ...
set desdir="%1"
rmdir /s /q %desdir%
xcopy bin\*.* %desdir%\bin\ /y /q /S
xcopy *.rdlc %desdir% /y /q 

::zip
:zip
set path7z="C:\Program Files\7-Zip\7z.exe"
if not exist %path7z% goto:eof
call %path7z% a "%1.zip" "%1\"