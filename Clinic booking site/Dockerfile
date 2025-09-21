ARG APP_UID=1000
ARG BUILD_CONFIGURATION=Release

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


ENV TZ=Africa/Cairo
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone


RUN apt-get update && apt-get install -y tzdata locales && apt-get clean
RUN locale-gen ar_EG.UTF-8
ENV LANG ar_EG.UTF-8
ENV LANGUAGE ar_EG:ar
ENV LC_ALL ar_EG.UTF-8



FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Clinic booking site/Clinic booking site.csproj", "Clinic booking site/"]
COPY ["Clinic.Repo/Clinic.Repo.csproj", "Clinic.Repo/"]
COPY ["Clinic.Domain/Clinic.Domain.csproj", "Clinic.Domain/"]
COPY ["Clinic.Service/Clinic.Service.csproj", "Clinic.Service/"]
COPY ["Service Abstraction/Service Abstraction.csproj", "Service Abstraction/"]
RUN dotnet restore "./Clinic booking site/Clinic booking site.csproj"
COPY . .
WORKDIR "/src/Clinic booking site"
RUN dotnet build "./Clinic booking site.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Clinic booking site.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Clinic booking site.dll"]