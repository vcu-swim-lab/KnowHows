FROM microsoft/dotnet

WORKDIR /dotnetapp

COPY . .

WORKDIR /dotnetapp/Website
RUN dotnet publish -c Release -o /dotnetapp/Website/out
ENTRYPOINT dotnet /dotnetapp/Website/out/Website.dll
