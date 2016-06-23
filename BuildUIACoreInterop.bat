@echo off

set WindowsSdkDir=c:\Program Files (x86)\Windows Kits\8.1\
set netfxtools=c:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\x64\
set ResultDir=.\UIACoreInterop\Build\
set framework=c:\Windows\Microsoft.NET\Framework64\v4.0.30319\
set VS=%VS140COMNTOOLS%

echo Settings VS variables: %VS%vsvars32.bat
call %VS%vsvars32.bat

mkdir %ResultDir%

echo Running midl: %WindowsSdkDir%include\um\UIAutomationCore.idl -^> %ResultDir%UIAutomationCore.tlb
midl /nologo /char signed /out %ResultDir% /tlb UIAutomationCore.tlb /h UIAutomationCore_h.h "%WindowsSdkDir%include\um\UIAutomationCore.idl"
if errorlevel 1 goto somethingbad

echo Importing TLB: %ResultDir%UIAutomationCore.tlb -^> %ResultDir%Raw\Interop.UIAutomationCore.dll
tlbimp /machine:Agnostic /silent /out:%ResultDir%Raw\Interop.UIAutomationCore.dll /namespace:Interop.UIAutomationCore %ResultDir%UIAutomationCore.tlb
if errorlevel 1 goto somethingbad

echo Disassembling interop DLL: %ResultDir%Raw\Interop.UIAutomationCore.dll -^> %ResultDir%UIAutomationCore.il
ildasm %ResultDir%Raw\Interop.UIAutomationCore.dll /out=%ResultDir%UIAutomationCore.il /nobar
if errorlevel 1 goto somethingbad

echo Building CustomizeUiaInterop project
MSBuild.exe .\CustomizeUiaInterop\CustomizeUiaInterop.csproj /t:Build /p:Configuration=Debug;SolutionDir=.
if errorlevel 1 goto somethingbad

echo Customizing UIA IL: %ResultDir%UIAutomationCore.il -^> %ResultDir%Custom.UIAutomationCore.il
.\CustomizeUiaInterop\bin\Debug\CustomizeUiaInterop %ResultDir%UIAutomationCore.il %ResultDir%Custom.UIAutomationCore.il
if errorlevel 1 goto somethingbad

echo Assembling IL: %ResultDir%Custom.UIAutomationCore.il -^> %ResultDir%Interop.UIAutomationCore.dll 
ilasm /dll /output=%ResultDir%Interop.UIAutomationCore.dll %ResultDir%Custom.UIAutomationCore.il
if errorlevel 1 goto somethingbad

echo Finished successfully
goto end

:somethingbad
echo Something Bad Happened.
exit /B 1

:end
exit /B 0