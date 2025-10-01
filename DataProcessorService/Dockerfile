FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./Shared ./Shared
COPY ./DataProcessorService ./DataProcessorService

WORKDIR /src/DataProcessorService
RUN dotnet restore
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/out .
COPY ./DataProcessorService/appsettings.json .

ENTRYPOINT ["dotnet", "DataProcessorService.dll"]
