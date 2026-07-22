FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY EduApoyos.sln ./
COPY src/EduApoyos.Domain/EduApoyos.Domain.csproj src/EduApoyos.Domain/
COPY src/EduApoyos.Application/EduApoyos.Application.csproj src/EduApoyos.Application/
COPY src/EduApoyos.Infrastructure/EduApoyos.Infrastructure.csproj src/EduApoyos.Infrastructure/
COPY src/EduApoyos.Api/EduApoyos.Api.csproj src/EduApoyos.Api/
COPY tests/EduApoyos.Application.Tests/EduApoyos.Application.Tests.csproj tests/EduApoyos.Application.Tests/
RUN dotnet restore src/EduApoyos.Api/EduApoyos.Api.csproj

COPY . .
RUN dotnet publish src/EduApoyos.Api/EduApoyos.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "EduApoyos.Api.dll"]
