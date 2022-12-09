FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build-env

ARG buildconfig

WORKDIR /usr/src/api

COPY . .

RUN dotnet restore && dotnet publish Samples.Players.sln -c ${buildconfig} -o publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS runtime

WORKDIR /opt/api

RUN apk add icu-libs

COPY --from=build-env /usr/src/api/publish /opt/api

ENV ASPNETCORE_URLS=http://+:8082
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0

EXPOSE 8082

ENTRYPOINT ["/opt/api/play"]
