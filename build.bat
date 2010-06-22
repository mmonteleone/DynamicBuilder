@echo off
if "%1" == "" goto Usage
goto Build

:Usage
echo usage: build [TARGET]
echo where: target = one of "test", "release", "clean"
goto End

:Build
%systemroot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe DynamicBuilder.msbuild /t:%1

:End