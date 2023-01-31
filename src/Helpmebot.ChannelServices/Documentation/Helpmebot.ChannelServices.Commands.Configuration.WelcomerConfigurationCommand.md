This command configures the channel welcomer. Any user who has a hostmask on the list (but not on the ignore list) will be greeted with the message defined by the [response](https://helpmebot.org.uk/responses) `channelservices.welcomer.welcome` on their entry to the channel. It is strongly recommended to [set a local override](https://helpmebot.org.uk/commands#command-message) for this message in the channel.

If an override mode is configured, then a different message key might be used.

If there are no hostmasks in the list to be welcomed, then nobody will be welcomed.
