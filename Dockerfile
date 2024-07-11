FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# copy all the layers' csproj files into respective folders
COPY ["./SmartLockDoor/SmartLockDoor.csproj", "src/SmartLockDoor/"]

# run restore over API project - this pulls restore over the dependent projects as well
RUN dotnet restore "src/SmartLockDoor/SmartLockDoor.csproj"

COPY . .

# run build over the API project
WORKDIR "/src/SmartLockDoor/"
RUN dotnet build -c Release -o /app/build

# run publish over the API project
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS runtime
WORKDIR /app
EXPOSE 8011
COPY --from=publish /app/publish .
RUN ls -l
ENTRYPOINT [ "dotnet", "SmartLockDoor.dll" ]