#Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["trading-platform.csproj", "./"]
RUN dotnet restore "./trading-platform.csproj"

COPY . .
RUN dotnet build "trading-platform.csproj" -c Release -o /app/build
RUN dotnet publish "trading-platform.csproj" -c Release -o /app/publish

#Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5001
EXPOSE 5001
COPY --from=build /app/publish .
COPY Data/sp500.csv ./Data/sp500.csv
ENTRYPOINT ["dotnet", "trading-platform.dll"]