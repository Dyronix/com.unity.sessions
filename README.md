# Unity Sessions

## summary
This class uses a unique player ID to bind a player to a session. Once that player connects to a host, the host
associates the current ClientID to the player's unique ID. If the player disconnects and reconnects to the same
host, the session is preserved.

## remarks
Using a client-generated player ID and sending it directly could be problematic, as a malicious user could
intercept it and reuse it to impersonate the original user. We are currently investigating this to offer a
solution that handles security better.

## Reference
https://github.com/Unity-Technologies/com.unity.multiplayer.samples.coop