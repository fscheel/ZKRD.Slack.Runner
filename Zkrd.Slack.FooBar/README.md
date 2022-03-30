# ZKRD Slack bot demo module for reacting to incoming messages

This module demonstrate, how to implement a module that reacts to incoming messages. To do this, the
interface `Zkrd.Slack.Core.IAsyncMessageHandler` is implemented and registered in the service collection. The message
handlers `HandleMessageAsync` method receives the message and should check, whether the message is handled by this
module (for example by trying to match a regex). If so, the required action is taken. Usually, the module also posts a
message back to confirm that the message was received and processed.  
