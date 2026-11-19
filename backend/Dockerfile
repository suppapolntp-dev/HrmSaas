FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY HrmSaas.sln ./
COPY src/HrmSaas.Domain/HrmSaas.Domain.csproj src/HrmSaas.Domain/
COPY src/HrmSaas.Application/HrmSaas.Application.csproj src/HrmSaas.Application/
COPY src/HrmSaas.Infrastructure/HrmSaas.Infrastructure.csproj src/HrmSaas.Infrastructure/
COPY src/HrmSaas.WebApi/HrmSaas.WebApi.csproj src/HrmSaas.WebApi/

RUN dotnet restore HrmSaas.sln

COPY src/ src/
RUN dotnet publish src/HrmSaas.WebApi/HrmSaas.WebApi.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "HrmSaas.WebApi.dll"]
