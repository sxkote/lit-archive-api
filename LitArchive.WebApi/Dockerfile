#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
 
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG PROJECT_PATH=Litskevichi/lit-archive/LitArchive.API
ARG PROJECT_NAME=LitArchive.WebApi
WORKDIR /src
COPY ["${PROJECT_PATH}/${PROJECT_NAME}/${PROJECT_NAME}.csproj", "${PROJECT_PATH}/${PROJECT_NAME}/"]
COPY "LBox/LIB/*.dll" "LBox/LIB/"
RUN dotnet restore "${PROJECT_PATH}/${PROJECT_NAME}/${PROJECT_NAME}.csproj"
COPY "${PROJECT_PATH}/" "${PROJECT_PATH}/"
WORKDIR "/src/${PROJECT_PATH}/${PROJECT_NAME}"
RUN dotnet build "${PROJECT_NAME}.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "${PROJECT_NAME}.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
ARG PROJECT_PATH=Litskevichi/lit-archive/LitArchive.API
ARG PROJECT_NAME=LitArchive.WebApi
ENV ASPNETCORE_ENVIRONMENT=Production
WORKDIR /app
RUN apt-get -y update && apt-get -y upgrade && apt-get install -y ffmpeg libgdiplus
COPY --from=publish /app/publish .
COPY "${PROJECT_PATH}/${PROJECT_NAME}/posters/" "posters/"
ENTRYPOINT ["dotnet", "${PROJECT_NAME}.dll"]