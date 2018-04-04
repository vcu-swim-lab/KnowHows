![KnowHows](https://raw.githubusercontent.com/vcu-swim-lab/KnowHows/master/docs/logo.png)
===================

KnowHows is a Slack app that allows users to search repositories for people who have knowledge on certain parts of code.

----------

##### Prequisities
* Visual Studio 2017 or Visual Studio Community
* .NET Core 2.0
* [srcML](http://www.srcml.org/)

##### Configuration
1. Navigate to `Website/`
2. Copy `appsettings.example.json` as `appsettings.json`
3. Update all the fields in the `AppSettings` section. `WEBSITE_BASE_URL` + `WEBSITE_PORT` + `GITHUB_APP_OAUTH_REDIRECT_URL` or `SLACK_APP_OAUTH_REDIRECT_URL` should be the callback URL that you want the user to be redirected to during the OAuth process.  Typically, this should look something like `http://localhost:53222/api/github/authenticate`

##### Building With Visual Studio
1. Open `KnowHows.sln` with Visual Studios and click build

##### Building From Command Line
1. In a terminal or command prompt, navigate to `Website/`
2. Run `dotnet build`
3. Alternatively, you can `dotnet publish` to prepare the website for deployment

##### Running
1. Before running the website, ensure that `appsettings.json` is present in the directory that you'll be running the website from (`Website/bin/Debug/netcoreapp2.0`, for example) and that `srcml` is available on your PATH.
2. From a terminal or command line, you may run `dotnet run` in order to start the website

