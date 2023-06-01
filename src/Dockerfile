FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app
ARG TARGETARCH

COPY *.csproj ./
RUN dotnet restore -a $TARGETARCH

COPY . ./
RUN dotnet publish -a $TARGETARCH -c Release --no-restore -o out

FROM mcr.microsoft.com/dotnet/runtime:7.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "RedditImageBot.dll"]