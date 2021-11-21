---
title: Silent mode
path: docs/silent-mode
navigationTitle: Silent mode
disabled: true
---
Sometimes it is desirable to have Helpmebot present in a channel, but to not allow the bot to respond to 
commands in that channel. There are many reasons why this might be desired, but commonly it is due to a conflict
between Helpmebot's commands and another bot's commands.

Helpmebot can thus be configured to not respond to commands in the usual way. Helpmebot will *only* respond to
commands when addressed directly, either by prefixing the bot's name on all commands, or directing the message
to Helpmebot.

All of these will cause the bot to respond to a command normally invoked as `!ping`:

```
!helpmebot ping
Helpmebot: ping
Helpmebot, ping
Helpmebot: !ping
Helpmebot, !ping
```

### Configuration

To configure the bot to be silent, use the `!silence` command in that channel.

**Enable:** `!silence enable`

**Disable:** `!silence disable`
