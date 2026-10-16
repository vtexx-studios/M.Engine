@echo off
REM Komut satirindan veya hizli tetikleme icin köprü dosya
cd /d "%~dp0"
dotnet run --project "..\src\ProcessEngine.csproj" %*
