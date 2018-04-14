![KnowHows](https://raw.githubusercontent.com/vcu-swim-lab/KnowHows/master/docs/logo.png)

KnowHows is a Slack application that tracks your GitHub repositories and allows users to search for other developers who have knowledge about certain parts of code, such as an API or software concept.

[![Add to Slack](https://platform.slack-edge.com/img/add_to_slack.png)](https://slack.com/oauth/authorize?client_id=183604701555.341310646448&scope=commands)

## Installation
### Prequisities
* Visual Studio 2017 or Visual Studio Community
* .NET Core 2.0
* [srcML](http://www.srcml.org/)
* [Apache Solr](https://lucene.apache.org/solr/)

### Configuration
1. Navigate to `Website`.
2. Create GitHub and Slack apps to generate the necessary OAuth tokens for each.
3. Copy `appsettings.example.json` as `appsettings.json`.
4. Update all the fields in the `AppSettings` section. The callback URLs for GitHub and Slack will be `{WEBSITE_BASE_URL}/api/github/authenticate` and `{WEBSITE_BASE_URL}/api/slack/authenticate` respectively.
5. Setup Solr according to [Apache's directions](https://lucene.apache.org/solr/guide/7_0/installing-solr.html) using the `schema.xml` and `solrconfig.xml` provided in the docs. 
6. Naming conventions for your created core *MUST* match that in `appsettings.json`. This project communicates through an authenticated user. Setting this up is left to the installer, for simple open connections configure the connection in `SolrManager.cs` to be just your connection string.
7. Make sure your Slack app has a slash command configured that makes requests to `{WEBSITE_BASE_URL}/api/slack/processmessage`.

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

- `/knowhows to [query]` - Performs a natural language search on a concept, such as `write to a file`. This produces a ranked list of tracked users indicating files that they have changed.
- `/knowhows search [query]` - Performs a literal search on a code term, such as an API name like `FileWriter`. This produces a ranked list of tracked users indicating files that they have changed.
- `/knowhows track [repository_name | *]` - Tracks and indexes one or all (*) of your repositories. When no repository is specified, a list of your untracked repositories is returned.
- `/knowhows untrack [repository_name | *]` - Untracks and unindexes one or all (*) of your repositories. When no repository is specified, a list of your currently tracked repositories is returned.
- `/knowhows help` - Prints a brief description of each of these commands.

## Credits
KnowHows was developed [Robbie Uvanni](https://github.com/seefo), [Ben Leach](https://github.com/broem), and [Alex Aplin](https://github.com/AlexAplin) under the guidance of [Kostadin Damevski](https://egr.vcu.edu/directory/kostadindamevski/) for the [VCU Capstone Design Expo 2018](https://egr.vcu.edu/capstone/).

## License
[MIT](./LICENSE)
