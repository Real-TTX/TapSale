FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY TapSale.slnx dotnet-tools.json ./
COPY src/TapSale.Web/TapSale.Web.csproj src/TapSale.Web/
RUN dotnet restore src/TapSale.Web/TapSale.Web.csproj
COPY src/TapSale.Web/ src/TapSale.Web/
ARG BUILD_CONFIGURATION=Release
ARG VERSION=local-unknown
RUN dotnet publish src/TapSale.Web/TapSale.Web.csproj -c ${BUILD_CONFIGURATION} -o /app/publish --no-restore /p:Version=0.0.0 /p:AssemblyInformationalVersion=${VERSION}

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080 \
    DataPath=/app/data
EXPOSE 8080
COPY --from=build /app/publish .
HEALTHCHECK --interval=20s --timeout=3s --start-period=15s --retries=3 CMD bash -c 'exec 3<>/dev/tcp/127.0.0.1/8080' || exit 1
ENTRYPOINT ["dotnet", "TapSale.Web.dll"]
