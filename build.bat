C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild src\DynamicBuilder\DynamicBuilder.csproj /p:Configuration=Release
src\.nuget\NuGet.exe pack src\DynamicBuilder\DynamicBuilder.csproj -Prop Configuration=Release

PAUSE