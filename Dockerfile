FROM mcr.microsoft.com/dotnet/core/sdk:3.1.201 AS dev

WORKDIR /app

COPY ./*.sln ./
COPY ./Nyancat/Nyancat.csproj ./Nyancat/Nyancat.csproj

RUN dotnet restore

COPY . .

RUN dotnet build

ENTRYPOINT ["dotnet", ".build/bin/Nyancat/Debug/netcoreapp3.1/nyancat.dll"]
