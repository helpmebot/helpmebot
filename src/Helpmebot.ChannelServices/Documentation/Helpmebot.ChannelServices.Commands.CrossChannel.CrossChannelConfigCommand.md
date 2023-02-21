This command configures any cross-channel operations which the bot supports.

Currently, this is used for:
* Notification commands (such as `!helper` in #wikipedia-en-help) notifying a backend channel of it's usage
* Welcomer notifications about ban exemption setting
* `!forcewelcome` cross-channel usage

###### Setting a notification message

The notification message specified using `!crosschannel notifymessage` is the message sent to the backend channel when the command is triggered.

The message is not stored as a usual response message, but is the actual message to be sent. This message accepts three parameters:
* `{0}`: the nickname of the person triggering the command
* `{1}`: the channel the command was triggered in
* `{2}`: the message given by the user, if any.

Both the notification message and the keyword to be used to trigger the message must be set before notifications can be enabled in the channel.

