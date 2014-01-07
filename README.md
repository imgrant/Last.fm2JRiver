Last.fm2JRiver
==============

Scripts for use in JRiver Media Center to sync data from Last.fm (requires Scripting plugin for JRMC)

Based on an existing script and DLL contributed to the JRiver support forum for updating play counts in JRiver Media Center (JRMC) from Last.fm (http://yabb.jriver.com/interact/index.php?topic=73169.0).

Requires the .NET Scripting plugin for JRMC (http://accessories.jriver.com/mediacenter/accessories.php) and XML-RPC.NET library (supplied).
More details about the Scripting plugin: http://yabb.jriver.com/interact/index.php?topic=38693.0

At the moment, I couldn't work out how to compile the LastFm.APIMethods DLL and have it work properly, so each of the scripts currently includes the full source for this namespace directly.


Individual script descriptions
==============================

LastfmLastPlayed2JRiver - matches scrobbles on Last.fm to tracks in JRMC and updates their last played date accordingly.

LastfmLoved2JRiver - creates a playlist in JRMC of tracks that have been loved on Last.fm (if they exist in the JRMC library).

LastfmPlayCount2JRiver - matches tracks in a users' Last.fm library to those in JRMC and updates their play count to match Last.fm.
