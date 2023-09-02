# IsoCON

IsoCON is a custom 3d virtual tabletop for RPGs that include tactical combat, such as ICON by Massif Press.

## This Release
This is the v0.5 beta release, the first release to include multiplayer support. Due to the complexity of adding networking, the feature in v0.4 that enabled token stat/status editing has been temporarily removed. Restoring it is the top priority for the next release!

## Multiplayer
Connections are made via P2P, which means no signups, no services, no costs! However, this comes at the cost of requiring a bit of networking know-how. If you are hosting a table online, you may need to enable port forwarding on port 7777. You'll need to find and share your global IP with your players. If you are hosting on a LAN, you only need to share your local IP address, which should be visible in the connection sidebar.

## Maps
Fully 3d maps can created, saved, and shared. Maps are saved to /maps in the data directory, which can be found or changed in the config sidebar.

## Custom Tokens
IsoCON does not include any default tokens. Custom tokens can be placed in /tokens in the data directory. To get started, consider the artwork from the ICON PDF, available at the ICON discord (https://discord.com/channels/426286410496999425/871618578489684018/1127608333902291034). IsoCON will handle copying your tokens to other clients automatically. Copied tokens can be found in the data directory under /remote-tokens.

## Feedback
Feel free to ask me questions or provide feedback in Discord (DM @delzhand or ask at https://discord.com/channels/426286410496999425/871618578489684018), or email (isocondev@gmail.com). Follow development on BlueSky (https://bsky.app/profile/delzhand.bsky.social).

## Copyright
IsoCON is open source. Licensed under Creative Commons CC BY-NC-SA.

ICON is copyright Tom Bloom and Massif Press. IsoCON is not affiliated with either and is not an official ICON product.
