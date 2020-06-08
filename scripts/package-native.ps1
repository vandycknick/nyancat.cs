dotnet publish Nyancat/Nyancat.csproj -c Release `
    --output ".build/publish/win-x64" `
    --runtime win-x64 `
    --framework netcoreapp3.1 `
    /p:Mode=CoreRT-ReflectionFree
