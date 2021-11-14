###### Available variables
You may use the following variables in your message:
* Numbered variables such as `{0}`, `{1}`, and `{2}` may be used to reflect the input passed to Helpmebot with the command.
* Numbered variables can also be used like `{1*}` to display everything from parameter `{1}` to the end of the input. Note that numbered variables start at 0 so `{1}` would be the second word.
* URL encoding can be added by specifying `:url` just inside the closing brace, or an approximation of MediaWiki title encoding can be added by specifying `:title` just inside the closing brace. For example, `{3*:title}`
* `{channel}` is the full name of the current channel.
* `{nickname}!{username}@{hostname}` are three variables that represent the host mask of the user calling the command.
  * `{nickname}` is the user's nick in the channel
  * `{username}` is also known as "ident" on other IRC servers.
  * `{hostname}` this will be the user's cloak if they have one, otherwise it will be the host ISP's name and maybe IPv4 address they are connected from.
