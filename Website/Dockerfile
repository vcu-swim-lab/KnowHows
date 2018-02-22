FROM microsoft/dotnet

WORKDIR /dotnetapp

COPY . .
RUN dotnet publish -c Release -o out
ENTRYPOINT ["dotnet", "out/Website.dll"]
