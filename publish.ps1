Remove-Item -Recurse artifacts -ErrorAction Ignore

dotnet pack .\Clypo\Clypo.csproj -c Release --include-symbols --include-source -o artifacts

dotnet publish .\Clypo.Console\Clypo.Console.csproj -c Release -r win-x64 -o artifacts\win
dotnet publish .\Clypo.Console\Clypo.Console.csproj -c Release -r linux-x64 -o artifacts\lin
dotnet publish .\Clypo.Console\Clypo.Console.csproj -c Release -r osx-x64 -o artifacts\osx
