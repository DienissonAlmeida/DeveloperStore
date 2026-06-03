FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY DeveloperStore.slnx ./
COPY src/DeveloperStore.Domain/DeveloperStore.Domain.csproj             src/DeveloperStore.Domain/
COPY src/DeveloperStore.Application/DeveloperStore.Application.csproj   src/DeveloperStore.Application/
COPY src/DeveloperStore.Infrastructure/DeveloperStore.Infrastructure.csproj src/DeveloperStore.Infrastructure/
COPY src/DeveloperStore.Api/DeveloperStore.Api.csproj                   src/DeveloperStore.Api/

RUN dotnet restore src/DeveloperStore.Api/DeveloperStore.Api.csproj

COPY . .

RUN dotnet publish src/DeveloperStore.Api/DeveloperStore.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN mkdir -p logs

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "DeveloperStore.Api.dll"]
