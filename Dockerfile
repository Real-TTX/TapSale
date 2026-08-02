FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY TabSale.slnx dotnet-tools.json ./
COPY src/TabSale.Web/TabSale.Web.csproj src/TabSale.Web/
RUN dotnet restore src/TabSale.Web/TabSale.Web.csproj
COPY src/TabSale.Web/ src/TabSale.Web/
ARG BUILD_CONFIGURATION=Release
ARG VERSION=local-unknown
RUN dotnet publish src/TabSale.Web/TabSale.Web.csproj -c ${BUILD_CONFIGURATION} -o /app/publish --no-restore /p:Version=0.0.0 /p:AssemblyInformationalVersion=${VERSION}

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080 \
    DataPath=/app/data
EXPOSE 8080
COPY --from=build /app/publish .
HEALTHCHECK --interval=20s --timeout=3s --start-period=15s --retries=3 CMD bash -c 'exec 3<>/dev/tcp/127.0.0.1/8080' || exit 1
ENTRYPOINT ["dotnet", "TabSale.Web.dll"]
