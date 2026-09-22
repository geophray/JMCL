@echo off
set package_root=..\..\
REM Find jmcl-proxybuilder.exe in the package folder (irrespective of version)
For /R %package_root% %%G IN (jmcl-proxybuilder.exe) do (
	IF EXIST "%%G" (set codegen_path=%%G
	goto :continue)
	)

:continue
@echo Using '%codegen_path%'
REM jmcl-proxybuilder [path] [connection-string]
"%codegen_path%" "/s:%cd%\.."

if errorlevel 1 (
echo Error Code=%errorlevel%
exit /b %errorlevel%
)

pause
