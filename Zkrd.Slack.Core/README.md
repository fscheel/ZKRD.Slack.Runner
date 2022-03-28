# ZKRD Slack bot core module

This module is the main module of the application. It is used to connect it to the Slack service and dispatch incoming
messages to other modules. Additionally, it provides configuration options that may be required by other modules (ie
proxy settings).

## Background services

The application uses two background services to be able to receive messages from Slack and handle them independently of
each other. To facilitate that, the receiving service pushes incoming messages via
a [Channel](https://devblogs.microsoft.com/dotnet/an-introduction-to-system-threading-channels/) to a dispatcher
service. The dispatcher service then dispatches the messages to the other services, which be of synchronous or
asynchronous nature.

While both background services could implement their functionality directly, it is currently very hard to unit test
them. This is why the functionality was pushed into separate services and all the background services do is calling
them.
