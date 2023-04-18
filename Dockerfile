FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /src
COPY . .
ENV DOTNET_NOLOGO=1
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
RUN dotnet publish -c Release -o bin

FROM mcr.microsoft.com/dotnet/runtime:7.0-alpine
WORKDIR /app
COPY --from=build /src/bin .
ENV DOTNET_NOLOGO=1
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
RUN apk upgrade --update-cache --available && apk add openssl icu-libs && rm -rf /var/cache/apk/*
ENTRYPOINT ["dotnet", "bin/Moonlight.dll"]