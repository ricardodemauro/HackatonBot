FROM mcr.microsoft.com/dotnet/core/aspnet:2.1-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.1-stretch AS build
WORKDIR /src
COPY ["HackatonEchoBot.csproj", "./"]
RUN dotnet restore "HackatonEchoBot.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "HackatonEchoBot.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "HackatonEchoBot.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "HackatonEchoBot.dll"]
