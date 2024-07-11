FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy all the layers' csproj files into respective folders
COPY ["./WebAPI.Application/WebAPI.Application.csproj", "src/WebAPI.Application/"]
COPY ["./WebAPI.Domain/WebAPI.Domain.csproj", "src/WebAPI.Domain/"]
COPY ["./WebAPI.Infrastructure/WebAPI.Infrastructure.csproj", "src/WebAPI.Infrastructure/"]
COPY ["./WebAPI/WebAPI.csproj", "src/WebAPI/"]

# run restore over API project - this pulls restore over the dependent projects as well
RUN dotnet restore "src/WebAPI/WebAPI.csproj"

COPY . .

# run build over the API project
WORKDIR "/src/WebAPI/"
RUN dotnet build -c Release -o /app/build

# run publish over the API project
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
RUN ls -l
ENTRYPOINT [ "dotnet", "WebAPI.dll" ]