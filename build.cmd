@echo off
rem ############################################
rem # FAKE build bootstrapper
rem #
rem # Boot-straps build/build.fsx FAKE script
rem # See Required Configuration section below.
rem ############################################

if '%1' == '/?' goto PRINT_USAGE
if '%1' == '-?' goto PRINT_USAGE
if '%1' == '?' goto PRINT_USAGE
if '%1' == '/help' goto PRINT_USAGE
if '%1' == '--help' goto PRINT_USAGE
if '%1' == '-help' goto PRINT_USAGE
set TARGET=%1

rem ### Build Script Required Configuration ###
set "PROJECT_NAME=GeorgeCloney"
rem Set Flags to 1/0 for required testing tools
set REQUIRES_NUNIT=1

rem ### Build Script Optional Configuration ###
set REPOSITORY_ROOT=%~dp0
set BUILD_DIR=%REPOSITORY_ROOT%build/
set BUILD_PACKAGES_DIR=%BUILD_DIR%packages
set NUGET_PATH=%BUILD_PACKAGES_DIR%\nuget.exe
set NUGET_URL=https://nuget.org/nuget.exe
set NUGET_SOURCE=https://www.nuget.org/api/v2/
set FAKE_PATH=%BUILD_PACKAGES_DIR%\FAKE\tools\Fake.exe
set FAKE_BUILD_FILE=%BUILD_DIR%build.fsx
set FAKE_VERSION=4.16.0
set NUNIT_VERSION=2.6.4

rem Version from environment
if '%VERSION%'=='' set VERSION=%VERSION_NUMBER%
if '%VERSION%'=='' set VERSION=%APPVEYOR_BUILD_VERSION%
if '%VERSION%'=='' set VERSION=%BUILD_NUMBER%
if '%VERSION%'=='' set VERSION=0.0.0.0

echo.
echo ##########################################################
echo # %PROJECT_NAME% Build Script (%~nx0)
echo #   Repository Directory:     %REPOSITORY_ROOT%
echo #   Build Packages Directory: %BUILD_PACKAGES_DIR%
echo #   NuGet Path:               %NUGET_PATH%
echo #   Fake Path:                %FAKE_PATH%
echo #   Fake Build File:          %FAKE_BUILD_FILE%
echo #   Build Target:             %TARGET%
echo #   Build Version:            %VERSION%
echo #
echo.

:PREREQUISITES
if not exist "%BUILD_DIR%" (
	echo "./build" directory is missing.
	goto FAILED
)
if not exist "%BUILD_PACKAGES_DIR%" mkdir "%BUILD_PACKAGES_DIR%"

:NUGET
if not exist "%NUGET_PATH%" (
	echo Downloading latest NuGet executable...
	powershell.exe -ExecutionPolicy Bypass -NoProfile -NoLogo -Noninteractive ^
		-Command "Invoke-WebRequest '%NUGET_URL%' -OutFile '%NUGET_PATH%'"
	if %ERRORLEVEL% EQU 9009 (
		echo PowerShell is not in the PATH
		goto FAILED
	) else ( echo Successfully downloaded 'NuGet.exe'. )
) else ( echo 'NuGet.exe' file present )
echo.

:FAKE
if not exist "%BUILD_PACKAGES_DIR%/FAKE/" (
	echo Installing FAKE, version %FAKE_VERSION%...
	"%NUGET_PATH%" Install "FAKE" -Version "%FAKE_VERSION%" -ExcludeVersion -OutputDirectory "%BUILD_PACKAGES_DIR%" -Source "%NUGET_SOURCE%"
) else ( echo 'FAKE' directory present )
echo.

:NUNIT
if not '%REQUIRES_NUNIT%'=='1' goto RUN
if not exist "%BUILD_PACKAGES_DIR%/NUnit.Runners.Net4/" (
	echo Installing NUnit, version %NUNIT_VERSION%...
	"%NUGET_PATH%" Install "NUnit.Runners.Net4" -Version "%NUNIT_VERSION%" -ExcludeVersion -OutputDirectory "%BUILD_PACKAGES_DIR%" -Source "%NUGET_SOURCE%"
) else ( echo 'NUnit.Runners.Net4' directory present )
echo.

:RUN
"%FAKE_PATH%" "%FAKE_BUILD_FILE%" %TARGET% project=%PROJECT_NAME% version=%VERSION%

if %ERRORLEVEL% NEQ 0 (
	echo Failure within FAKE script.
	goto FAILED
)

:DONE
exit /B 0

:FAILED
echo Failed with Error Code: "%ERRORLEVEL%"
echo See %REPOSITORY_ROOT%docs\README.md for prerequisites and more information. 
exit /B 1

:PRINT_USAGE
echo.
echo Correct usage: 
echo    %0
echo.
goto FAILED
