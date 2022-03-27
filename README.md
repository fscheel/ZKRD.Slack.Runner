# The ZKRD Slack bot

This is the slack bot used and developed by [ZKRD gGmbH](https://www.zkrd.de). It is used as a tool for training, prototyping and productivity. Its ultimate goal is to enhance the daily work the employees at ZKRD.

## Usage

The bot is currently not distributed in a packaged format, but you have multiple ways to compile and run the bot in production:

### Running

#### In Docker (recommended)

To run the bot in docker, run the following command in the root source directory to build the docker image:

```shell
$ docker build -t slack-bot .
```

You can then proceed to run the container. It is recommended to create a dedicated directory for the configuration file (see the chapter about configuration for more information). Use the following command (or an equivalent docker-compose file) to run the container:

```shell
$ docker run -d -v"$(pwd)/appsettings.json:/app/appsettings.json" slack-bot
```

#### Directly from source

To run the application directly from source, you need the .NET 6.0 SDK installed. Change into the `Zkrd.Slack.Runner` directory and run the application in release mode:

```shell
$ cd Zkrd.Slack.Runner && dotnet run -c Release
```

#### Compile and deploy

To run the compiled application from a different directory or on a different machine, you first need to compile the application on a machine with the .NET 6.0 SKD installed:

```shell
$ dotnet build -c Release
```

Then deploy the content of the directory `Zkrd.Slack.Runner/bin/Release/net6.0/` to a machine with the .NET 6.0 runtime installed. There you may run the bot:

```shell
$ ./Zkrd.Slack.Runner
```

### Configuration

The bot is configured via the usual .NET mechanisms. For production use, configuration via environment variables or an `appsettings.json` file is recommended.

*Attention! The `appsettings.json` file contains secret information like a Slack bot and app token. Secure the file appropriately for your environment!*

The `appsettings.json` file can be found in the following locations:
- for docker deployments: mount the file to `/app/appsettings.json`
- for running via source: `Zkrd.Slack.Runner/appsettings.json`
- for running via compile and deploy: directly in the deployed directory

#### SlackCore.AppToken

**Required**

A Slack App-Level token with the permission `connections:write`. You can create this on the "Basic Information" page of your created Slack app (https://api.slack.com/apps).

#### SlackCore.BotToken

**Required**

The bot user OAuth token of your app. You can find it on the "OAuth & Permissions" page of your app.

#### Proxy.Host

If your network requires a proxy to connect to web-pages, use this setting to configure the proxy host.

#### Proxy.Port

If your network requires a proxy to connect to web-pages, use this setting to configure the proxy port.

### Configuring the Slack app

To be able to connect to a Slack workspace, a Slack app must be created. The easiest way is to use the following manifest (replace any information in `<>`:

<details>
<summary>Expand to see the app manifest</summary>

```yaml
display_information:
  name: <name of the application>
features:
  bot_user:
    display_name: <the display name in Slack channels>
    always_online: false
oauth_config:
  scopes:
    user:
      - channels:history
      - im:history
    bot:
      - app_mentions:read
      - channels:history
      - channels:read
      - groups:read
      - im:history
      - im:read
      - mpim:read
      - chat:write
      - chat:write.public
settings:
  event_subscriptions:
    user_events:
      - message.app_home
      - message.channels
      - message.im
    bot_events:
      - app_mention
      - message.channels
      - message.im
  interactivity:
    is_enabled: true
  org_deploy_enabled: false
  socket_mode_enabled: true
  token_rotation_enabled: false
```

</details>

You will also need to create a App-Level Token on the "Basic information" page. The token should have the `connections:write` permission.

Don't forget to install the application to your workspace.

## Development

### Configuration for Development

It is recommended to use user secrets for environment specific settings like app token, bot token and proxy:

```shell
$ cd Zkrd.Slack.Runner
$ dotnet user-secrets init
$ dotnet user-secrets set SlackCore:BotToken "<your token>"
$ dotnet user-secrets set SlackCore:AppToken "<your token>"
$ dotner user-secrets set Proxy:Host "<host string>"
$ dotnet user-secrets set Proxy:Port "<port>"
```

*Attention! Despite its name user secrets are stored in plain text in the user directory.*

### Repository structure

The repository is structured into directories, each containing either a module for a particular purpose or test projects for these modules ending with `.Tests`. Each module has a README file containing further information about its purpose. A short summary of each module can be found here.

#### Zkrd.Slack.Runner

This is the main entrypoint of the application.

#### Zkrd.Slack.Core

The core module, which initializes the connection to the Slack servers and dispatches incoming messages to registered handlers.

#### Zkrd.Slack.FooBar

A demo module showcasing how to register a message handler and react to incoming messages.

#### Zkrd.Slack.WebApiDemo

A demo module showcasing how to register and implement API endpoints of the bot.

#### Zkrd.Slack.Help

A module to return the bot#s help information a user.

## Legal

### License

This application is licensed under the MIT license. See `LICENSE.md` for the full license text.