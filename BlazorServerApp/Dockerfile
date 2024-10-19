# Using an official .NET Core runtime as a base image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5029

# Use SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "BlazorServerApp.csproj"
RUN dotnet build "BlazorServerApp.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "BlazorServerApp.csproj" -c Release -o /app/publish

# Final stage: copy the publish folder to the base image and run
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN mkdir -p /app/Saved_Environments

ENTRYPOINT ["dotnet", "BlazorServerApp.dll"]