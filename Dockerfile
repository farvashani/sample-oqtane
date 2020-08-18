FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster as build

WORKDIR .

COPY ["/oqtane.framework/Oqtane.Server/Oqtane.Server.csproj", "Oqtane.Server/"]
COPY ["/oqtane.framework/Oqtane.Client/Oqtane.Client.csproj", "Oqtane.Client/"]
COPY ["/oqtane.framework/Oqtane.Shared/Oqtane.Shared.csproj", "Oqtane.Shared/"]

RUN dotnet restore "Oqtane.Server/Oqtane.Server.csproj"

COPY /oqtane.framework .

RUN dotnet build "Oqtane.Server/Oqtane.Server.csproj" -c Release -o /work/build/

FROM build AS publish
RUN dotnet publish "Oqtane.Server/Oqtane.Server.csproj" -c Release -o /work/publish/

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim
WORKDIR /app

FROM build AS final

WORKDIR /app/

COPY --from=publish /work/publish/ /app/
ENTRYPOINT ["dotnet", "Oqtane.Server.dll"]