#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .

ARG BUILDVERSION
ENV BUILD_VERSION=$BUILDVERSION
RUN env

#RUN curl -L https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh  | sh
#ARG FEED_ACCESSTOKEN
#RUN echo $FEED_ACCESSTOKEN
#ENV VSS_NUGET_EXTERNAL_FEED_ENDPOINTS="{\"endpointCredentials\": [{\"endpoint\":\"https://pkgs.dev.azure.com/heptonlivestockllc/cattlecc/_packaging/cattle-country/nuget/v3/index.json\", \"username\":\"docker\", \"password\":\"${FEED_ACCESSTOKEN}\"}]}"

RUN dotnet restore WebApp.sln

RUN dotnet build -c Release -o /app/build WebApp.sln

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish WebApp.sln

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "WebApp.dll"]
