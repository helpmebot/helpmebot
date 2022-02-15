This command allows management of the bot's access control by managing which flag groups a user is a member of, as shown on the [Access control](/access) page.

##### Masks

This command requires a mask or an account name to apply the flag group change to. The bot will try to interpret what you mean, but it is recommended to either use a plain account name, or a full nick!user@host mask.

* If you wish to apply the change to a NickServ account, simply specify the account name - for example `!access grant global stwalkerster Superuser`
* If you wish to apply the change to a nickname, specify `nick!*@*` - for example `!access grant global nick!*@* Superuser`. Note that nickname grants are considered insecure.
* If you wish to apply the change to a hostname or cloak, specify `*!*@hostname.example.net`

##### Global vs Local

Use the `global` modifier to change the user's membership of a group across all channels. 

Local access is currently not supported.
