#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Dolittle.Scaffolding/Dolittle.Scaffolding.csproj", "."]
RUN dotnet restore "./Dolittle.Scaffolding.csproj"
COPY Dolittle.Scaffolding .
WORKDIR "/src/."
RUN dotnet build "Dolittle.Scaffolding.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Dolittle.Scaffolding.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Dolittle.Scaffolding.dll"]