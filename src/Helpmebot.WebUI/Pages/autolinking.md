---
title: Automatic link parsing
path: docs/autolinking
navigationTitle: Autolinking
---
Helpmebot can be configured to automatically detect MediaWiki-style wikilinks in a channel, and transform them into a 
standard URL. The effect of this is similar to someone running the `!link` command after every message containing a wikilink:

```
<@stwalkerster> This is a [[test]] message.
   <+Helpmebot> https://en.wikipedia.org/wiki/test
<@stwalkerster> But this message doesn't contain any links
```

Automatic parsing of links follows the same logic as the `!link` command - interwiki prefixes are parsed out and template syntax is transformed into a the Template namespace.

```
<@stwalkerster> But have you considered the {{cite web}} template?
   <+Helpmebot> https://en.wikipedia.org/wiki/Template:cite_web
```
```
<@stwalkerster> maybe the french article? [[fr:Test]]
   <+Helpmebot> https://fr.wikipedia.org/wiki/Test
```

Multiple links in one message are grouped into one line:

```
<@stwalkerster> some vegetables: [[carrot]] [[potato]] [[parsnip]] [[mushroom]]
   <+Helpmebot> https://en.wikipedia.org/wiki/carrot, https://en.wikipedia.org/wiki/potato, https://en.wikipedia.org/wiki/parsnip, https://en.wikipedia.org/wiki/mushroom
```

### Configuration

To configure autolinking in a channel, use the `!autolink` command in that channel.

**Enable:** `!autolink enable`

**Disable:** `!autolink disable`
