# BUILD
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /Libs
COPY ../Libs ./
WORKDIR /src
COPY ./ModularAssistentForDiscordServer ./
RUN dotnet restore
RUN dotnet publish -c Debug -o out

# RUNNER IMAGE
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
RUN apk add --no-cache icu-libs
WORKDIR /app
COPY --from=build /src/out .
WORKDIR /config 

ENTRYPOINT ["dotnet", "/app/ModularAssistentForDiscordServer.dll"]