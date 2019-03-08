FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /build
COPY *.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o out
FROM microsoft/dotnet:2.2-runtime AS runtime
WORKDIR /bot
COPY --from=build /build/out ./
ENTRYPOINT ["dotnet", "ReviewGrabberBot.dll"]