FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ContaCerta.sln ./
COPY src/ContaCerta.Domain/ContaCerta.Domain.csproj src/ContaCerta.Domain/
COPY src/ContaCerta.Application/ContaCerta.Application.csproj src/ContaCerta.Application/
COPY src/ContaCerta.Infrastructure/ContaCerta.Infrastructure.csproj src/ContaCerta.Infrastructure/
COPY src/ContaCerta.Api/ContaCerta.Api.csproj src/ContaCerta.Api/
RUN dotnet restore src/ContaCerta.Api/ContaCerta.Api.csproj

COPY src/ ./src/
RUN dotnet publish src/ContaCerta.Api/ContaCerta.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Usuario nao root: a imagem base ja vem com o usuario "app" criado para isso.
USER app

COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ContaCerta.Api.dll"]
