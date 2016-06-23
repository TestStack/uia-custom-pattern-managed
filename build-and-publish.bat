choco install nuget.commandline gitlink -y
if errorlevel 1 goto somethingbad

choco install gitversion.portable -pre -y
if errorlevel 1 goto somethingbad

rmdir /s /q UIACoreInterop

call BuildUIACoreInterop.bat
if errorlevel 1 goto somethingbad

nuget restore UiaCustomPattersManaged.sln
if errorlevel 1 goto somethingbad

gitversion /l console
if errorlevel 1 goto somethingbad

for /f %%v in ('gitversion /showvariable NuGetVersion') do set GitVersion_NuGetVersion=%%v

msbuild UiaCustomPattersManaged.sln "/p:Configuration=Release;Platform=Any CPU"
if errorlevel 1 goto somethingbad

GitLink . -u https://github.com/TestStack/uia-custom-pattern-managed -c Release -include ManagedUiaCustomizationCore
if errorlevel 1 goto somethingbad

echo nuget pack UiaCustomPattersManaged.nuspec -version "%GitVersion_NuGetVersion%"
nuget pack UiaCustomPattersManaged.nuspec -version "%GitVersion_NuGetVersion%" -verbosity detailed -basepath .
if errorlevel 1 goto somethingbad

git tag v%GitVersion_NuGetVersion%
if errorlevel 1 goto somethingbad

git push --tags
if errorlevel 1 goto somethingbad

echo Finished successfully
echo(---------------------
echo Remember to publish created nupkg file and update Changes section in Readme.md
goto end

:somethingbad
echo Something Bad Happened.

:end

pause