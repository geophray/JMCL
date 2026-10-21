@echo off
set package_root=..\..\
set codegen_path=
REM Find proxybuilder.exe in the solution-local packages folder (packages.config), irrespective of version
For /R %package_root% %%G IN (proxybuilder.exe) do (
	IF EXIST "%%G" (set codegen_path=%%G
	goto :continue)
	)

:continue
REM PackageReference restores to the global NuGet cache instead, so fall back to it
set nuget_root=%NUGET_PACKAGES%
if not defined nuget_root set nuget_root=%USERPROFILE%\.nuget\packages
if not defined codegen_path if exist "%nuget_root%\jmcl.dataverse.proxybuilder" (
	For /R "%nuget_root%\jmcl.dataverse.proxybuilder" %%G IN (proxybuilder.exe) do (
		IF EXIST "%%G" set codegen_path=%%G
		)
	)

if not defined codegen_path (
echo Unable to find proxybuilder.exe. Restore the JMCL.Dataverse.ProxyBuilder package and try again.
exit /b 1
)

@echo Using '%codegen_path%'
REM proxybuilder.bat ["/c:<connection-string>"] [other proxybuilder args]
REM Any arguments are passed through to proxybuilder.exe. Quote /c: values, since cmd splits on semicolons.
"%codegen_path%" "/s:%cd%\.." %*

if errorlevel 1 (
echo Error Code=%errorlevel%
exit /b %errorlevel%
)

pause
