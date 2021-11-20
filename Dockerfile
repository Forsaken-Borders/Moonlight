FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -r linux-musl-x64 --self-contained

FROM alpine:latest
WORKDIR /app
COPY --from=build /src/bin/Release/net6.0/linux-musl-x64/publish/ .
RUN apk upgrade --update-cache --available && apk add openssl libstdc++ && rm -rf /var/cache/apk/*

ENTRYPOINT DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 /app/Moonlight