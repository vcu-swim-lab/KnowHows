# HelpMeCode
Slack bot based off Noobot, original implementation courtesy of Noobot example (https://github.com/noobot/Noobot.Examples)

## Installation
##### Prequisities
* Visual Studio 2017 or Visual Studio Community

##### Building
1. Open `HelpMeCode.sln` with Visual Studios and click build.

##### Configuration
1. Navigate to `SlackBot/Configuration`
2. Copy `config.default.json` as `config.json`
3. Update the `slack:apiToken` key with your slack API token (obtained here: https://my.slack.com/services/new/bot)

##### Running
1. After having build and configured the bot, you can now run the bot by simply running the resulting executable.  To interact with the bot, you can either directly message it or in a channel of which you have invited it to to, directly invoke it by typing `@helpmecode help` into the channel