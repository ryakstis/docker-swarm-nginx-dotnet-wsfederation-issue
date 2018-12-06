# Build
FROM microsoft/dotnet:2.2-sdk-alpine as dotnet
RUN apk update && apk add nodejs npm python make gcc g++
WORKDIR /source
COPY . .
RUN dotnet publish -c Release wsfed-issue.csproj

# Run
FROM microsoft/dotnet:2.2-aspnetcore-runtime-alpine
WORKDIR /app
COPY --from=dotnet /source/wwwroot wwwroot/
COPY --from=dotnet /source/bin/Release/netcoreapp2.2/publish/ .
COPY root.cer /usr/local/share/ca-certificates/root3.crt
COPY intermediate.cer /usr/local/share/ca-certificates/id-sw-38.crt
RUN update-ca-certificates

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
CMD ["dotnet", "wsfed-issue.dll"]
