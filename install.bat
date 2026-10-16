@echo off
TITLE M.Engine Kurulum Sihirbazi
echo [*] M.Engine sistemi kuruluyor...

REM Projeyi derle (Release modunda binary olusturur)
dotnet build -c Release

REM .mengine uzantisini M.Engine uygulamasi ile iliskilendir
assoc .mengine=MEngineFile
ftype MEngineFile="%~dp5bin\M.Engine.exe" "%1"
:: Not: Derleme sonrasi exe bin/Release/net8.0/ altinda olusur. 
:: Alternatif olarak doğrudan dotnet run tetikleyecek bir ftype da tanimlayabiliriz:

ftype MEngineFile=cmd.exe /c "cd /d %~dp0bin && mengine.cmd ""%%1"""

echo [+] Kurulum tamamlandi! .mengine uzantisi artik M.Engine ile tetiklenecek.
pause
