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