![KnowHows](https://raw.githubusercontent.com/vcu-swim-lab/KnowHows/master/docs/logo.png)

KnowHows is a Slack app that allows users to search repositories for people who have knowledge on certain parts of code.

## Installation
### Prequisities
* Visual Studio 2017 or Visual Studio Community
* .NET Core 2.0
* [srcML](http://www.srcml.org/)
* [Apache Solr](https://lucene.apache.org/solr/)

### Configuration
1. Navigate to `Website`.
3. Create GitHub and Slack apps to receive the necessary OAuth tokens for each.
2. Copy `appsettings.example.json` as `appsettings.json`.
3. Update all the fields in the `AppSettings` section. `WEBSITE_BASE_URL` + `WEBSITE_PORT` + `GITHUB_APP_OAUTH_REDIRECT_URL` or `SLACK_APP_OAUTH_REDIRECT_URL` should be the callback URL that you want the user to be redirected to during the OAuth process. Typically, this should look something like `http://example.com:53222/api/github/authenticate`.

### Building
#### Building With Visual Studio
1. Open `KnowHows.sln` with Visual Studio and click `Build`.

#### Building From Command Line
1. Navigate to `Website`.
2. Run `dotnet build`. Alternatively, you can run `dotnet publish` to prepare the website for deployment.

### Running
1. Before running the website, ensure that `appsettings.json` is present in the directory that you'll be running the website from and that `srcml` is available on your PATH.
2. From the command line, run `dotnet run` in order to start the website.

## Usage
From a Slack channel with KnowHows installed, the app can be invoked with the slash command `/knowhows`. When you invoke the command for the first time, the app will prompt for access to your GitHub account. The available commands are:

- `/knowhows to <query>` - Performs a natural language search for a concept, such as `write to a file`. This produces a ranked list of tracked users indicating files that they have changed.
- `/knowhows search <query>` - Performs a search for an explicit query, such as an API name like `FileWriter`. This produces a ranked list of tracked users indicating files that they have changed.
- `/knowhows track <repository_name>` - Tracks and indexes one of your repositories. When no repository is specified, a list of your untracked repositories is returned.
- `/knowhows untrack <repository_name>` - Untracks and unindexes one of your repositories. When no repository is specified, a list of your currently tracked repositories is returned.
- `/knowhows help` - Prints a brief description of each of these commands.

## Credits
KnowHows was developed [Robbie Uvanni](https://github.com/seefo), [Ben Leach](https://github.com/broem), and [Alex Aplin](https://github.com/AlexAplin) under the guidance of [Kostadin Damevski](https://egr.vcu.edu/directory/kostadindamevski/) for the [VCU Capstone Design Expo 2018](https://egr.vcu.edu/capstone/).

## License
[MIT](./LICENSE)
