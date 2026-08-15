@echo off
set "LIARA=%~dp0.tools\nodejs\liara.cmd"
if not exist "%LIARA%" (
  echo Liara CLI not found:
  echo   %LIARA%
  echo Install with:
  echo   "%~dp0.tools\nodejs\npm.cmd" install -g @liara/cli
  exit /b 1
)
"%LIARA%" %*
