FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app
 
# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app

# install System.Drawing native dependencies
RUN apt-get update \ 
    && apt-get install -y --no-install-recommends libgdiplus libc6-dev \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

RUN cd /usr/lib && ln -s libgdiplus.so gdiplus.dll

COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "FT_Management.dll"]