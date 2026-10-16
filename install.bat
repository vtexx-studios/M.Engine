@echo off
TITLE M.Engine Kurulum Sihirbazi
echo [*] M.Engine sistemi kuruluyor...

REM .mengine uzantisini M.Engine uygulamasi ile iliskilendir
assoc .mengine=MEngineFile
ftype MEngineFile="%~dp0bin\M.Engine.exe" "%%1"

echo [+] Kurulum tamamlandi! .mengine uzantisi artik M.Engine ile acilacak.
pause
