@echo off

:: This is the path to the HLSL compiler, in the Windows 10 SDK.
:: Change this if you have it installed somewhere else.
set "HLSL_COMPILER=C:\Program Files (x86)\Windows Kits\10\bin\x64\fxc.exe"

:: Make sure the compiler exists.
If NOT exist "%HLSL_COMPILER%" (
	echo "HLSL compiler not found. Edit this batch file to use the correct path."
	exit 1
)


:: Run the compiler on each shader file.
:: Use the "compileFX" function, defined below.
If NOT exist JC (
	echo "This batch file isn't running in the root of the repo."
	exit 2
)
pushd JC
call:compileFX "AudioDisplay" || exit 3
popd


exit :: Quit before control moves to the functions defined below.

:: Define the function that runs the HLSL compiler on a given fx file.
:compileFX
"%HLSL_COMPILER%" /T ps_2_0 /E main /Fo "%~1.ps" "%~1.fx"
echo _
echo _
GOTO:EOF