This command manages the bot's responses in the database, [as shown at the Responses page](/responses).

##### Alternates and Lines

Messages are defined under a specific key, and are split twice - firstly into a set of message "alternates", then into a set of "lines". 
When a message is requested, one alternate will be chosen at random, then all lines in that alternate will be sent.

On the [Responses](/responses) page, alternates are numbered to make it easier to determine which alternate is which.

For the majority of commands, there is a single line in a single alternate, so the `alternate` and `line` parameters will both be `1`. For example, to modify a message such as `brain.command.learn` to the value "I remember!", the following command can be used:
```
!message set global brain.command.learn 1 1 I remember!
```

To add a new line or alternate, just specify a number which is larger than the current set - for example, this will add a second alternative to the aforementioned message:
```
!message set global brain.command.learn 2 1 Saved to database
```

Removing lines or alternates is similar - just specify the alternate and line to remove. If the removed line is the last line in an alternate, the entire alternate will be removed. If the removed alternate is the last alternate in the message, the entire message will revert to the default value.
```
!message remove global brain.command.learn 2 1
```

##### Global vs Local

Use the `global` modifier to change the message across all channels. 

Use the `local` modifier to change the message just for the current channel.

Please note that there is no differentiation between private messages - a `local` change done in private message to the bot will change the message for all private messages for any user. This is for technical reasons - the bot will store the change with it's current nickname as the "channel".
