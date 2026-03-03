# MusicStreamer
ASP.Net Backend with a Svelte frontend developed to stream your local library across any device.

## Development Plan
Below is a simple development roadmap/plan that will be updated as more features are thought of.

### .NET Backend (MusicStreamerBackend)
 - **Database Core Models & Migrations**
   - Tracks, Albums, Artists
   - DbContext (EFCore with PostgreSQL)
 - **Folder Scanning for media files & Database Upload of metadata (Generate Core Models)**
   - Scanning Service `(Method to retrieve files from a root folder & retrieve media type based on file extenstion prototyped)`
   - Scanning Tests
   - Scanning Controller
 - **ASP.Net Identity**
   - User Management Service
   - User Management Tests
   - Application JWT Auth
 - **Audio Streaming**
   - Streaming Service
   - Streaming Tests
   - Streaming Controller `(Initial endpoint tested and seems viable)`
   - Transcoding Service (Quality Selection)
   - Transcoding Tests
 - **Library Management**
   - Library Service
   - Library Tests
   - Library Controller
     - Search Track
     - Get Track ID
     - Get Artist
     - Get Album
 - **User Playlists Management**
     - User Playlists
     - User History (Skipped, Listened Total, Other Relevant Metadata)

### Svelte Frontend (MusicStreamerFronted)
 - **Auth Page**
 - **Library Page**
 - **Audio Player**
   - Background running
 - **Playlists Page**


 ## To Do
  - Sync play button with the current music player state
  - Album Screen for when clicking artists
  - Tracks Screen for when clicking on albums
  - When clicking track image in the music player, track page should be shown
  - Queue screen for current queue
  - Add to queue functionality on tracks
  - Search functionality on main screen
  - Persitence (local storage, will most likely just persist the current state)
  - Next/Previous track integrated for ios usage
  - Track metadata should include track number in album
  - Tracks should be ordered by their Album name + Track number
  - Transcoding step when finding files to opus 128 
  - Encoding selector for the user to pick (defaults to opus when on mobile data)
  - Currently playing track should be highlighted throughout the player
  