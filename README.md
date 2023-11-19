Introduction
--------------------

RamaPlayer is a lightweight media player for Windows that is operated exclusively through the keyboard. The interface is simple -- the player occupies the entire app window and the status bar at the bottom shows current position (if playing).

One unique quality of RamaPlayer is the ability to auto switch to the next track in the current folder (determined by a simple alphabetic order) as the current track finishes. SO this is great to use with tracks which have to be played in a sequence without having to create playlist etc. At the end of the last track, it restarts with the first track!

Another distinguishing quality is the multiple keyboard commands to rewind/forward by a very small time distance to by a very large one (see below).

RamaPlayer can play anything what VLC Media Player  can -- nothing more and nothing less. It actually uses latter's ActiveX component for playback (did you expect another playback software from scratch? Sorry to disappoint you!)

To configure RamaPlayer as the default program for a media extension, simply right click on a file with that extension, choose 'Open With ....', brows to the RamaPlayer executable and finally check the box 'Always use the selected program to open this kind of file' and then hit Ok. From next time Windows will open such type of files in RamaPlayer as you doubleclick or press enter on them.

Keyboard Commands
--------------------

- q to quit (close application)
- o to open brows file dialog
- space or p to toggle between pause/play
- m to toggle mute
- f to toggle fullscreen
- up/down arrows to increase or decrease volume, respectively
- shift + up/down arrows to increase or decrease rate, respectively
- w to jump to the previous track in the current folder immediately
- e to jump to the next track in the current folder immediately
- left/right arrows to rewind/forward, respectively by 4 seconds
- alt + left/right arrows to rewind/forward, respectively by 15 seconds
- ctrl + left/right arrows to rewind/forward, respectively by 40 seconds
- ctrl + shift + left/right arrows to rewind/forward, respectively by a minute
- pageup/pagedown  to rewind/forward, respectively by 20 percent of the total duration of the track. So if track duration is 50 minutes one pagedown advances current position by 10 minutes. For a 20 minutes track, the same will be advanced by 4 minutes
- r to set/unset repeat (if set the current track is played in a loop until you quit or change)
- home to go to beginning of the track
- end to go to the end of the track
- shift + end to go to the end of the track minus 2.5 seconds
- g to open Goto Time dialog. Following formats are supported:
    - minutes - just enter the position in minutes (say 81)
    - h:m:s - standard hours:minutes:seconds time format
    - h:m - hours:minutes time format
    - h:m:s.fff - accurate (upto milliseconds) hours:minutes:seconds.milliseconds time format
    - m-s - minutes-seconds time format (using hyphen as a separator rather than colon)
    - m-s.fff - minutes-seconds.milliseconds time format
    
Chrome Extension
--------------------

Under the Chrome folder in the repository, there's a source for a Chrome extension  that makes any web video player on any webpage you open on Chrome as RamaPlayer. I.E., it adds keyboard shortcuts to operate the player. It currently works on the \<video\> elements.

Following keyboard shortcuts are supported:
(Note: most of these shortcuts which are related to manipulating the video element work only when the video element is in playing mode)
- alt+1 to switch to the Pause/Play button
- alt+2 to click on Skip Ads button (on Youtube player)
- ctrl + shift + end to set video's 'seek position' at 100% (useful to skip Youtube ads when Skip Ads button isn't available)
- alt + up arrow opens up a dialog to increase audio volume by specified times (2 = 100% * 2, for example). It only works once on a video instance which means you've to refresh the page to apply greater volume. Works amazingly to go far beyond 100% volume otherwise possible with volume controls. Necessary for certain videos which have low max volume.
- alt + 3 focus on Like button for Youtube videos.
- alt + shift + 3 focus and clicked Like button for Youtube videos.
- ctrl + left/right arrows to rewind/forward, respectively by 30 seconds
- ctrl + shift + left/right arrows to rewind/forward, respectively by 1.5 minutes (90 seconds)
- pagedown/pageup to rewind/forward, respectively by 20 percent of the total duration of the track. So if track duration is 50 minutes one pagedown advances current position by 10 minutes. For a 20 minutes track, the same will be advanced by 4 minutes
- alt + pagedown/pageup to rewind/forward, respectively by 5 minutes
- alt + g to open go to position dialog to navigate to a precise position (by minutes) in the current video

