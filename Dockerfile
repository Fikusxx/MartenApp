
# get base image from docker hub
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

# make folder for this app inside container
WORKDIR /app

# copy csproj into the root folder of a container
COPY *.csproj .

# Restore as distinct layers
RUN dotnet restore

# copy all files from this project into that /app folder
COPY . .

# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "MartenApp.dll"]