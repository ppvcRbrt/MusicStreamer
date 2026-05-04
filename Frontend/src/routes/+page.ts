import type { PageLoad } from './$types';
import { apiHttpService } from '$lib/services/apiHttpService'
import { browser } from '$app/environment';
export const ssr = false;
export const  load: PageLoad = async ({ params }) => {
    let artists: App.Artist[] = [];
    let albums: App.Album[] = [];
    let tracks: App.Track[] = [];
    let playlists: App.Playlist[] = [];
    if(browser) {
        try {
            artists = await apiHttpService.get<App.Artist[]>('/music/artists')
            albums = await apiHttpService.get<App.Album[]>('/music/albums')
            tracks = albums.flatMap((album: App.Album) =>
                album.tracks.map(track => ({
                    ...track,
                    artist: album.artist,
                    album: album,
                    albumId: album.id
                })));
            playlists = await apiHttpService.get<App.Playlist[]>('/user/myPlaylists')
            playlists = playlists.map((playlist: App.Playlist) => {
                playlist.type = 'playlist';
                if (playlist.trackIds && playlist.tracks) {
                    playlist.tracks = playlist.tracks.map(track => ({
                        ...track,
                        trackNumber: playlist.trackIds!.indexOf(track.id) + 1
                    }));
                }
                return playlist;
            })
            console.log(playlists);
        }
        catch (error) {
            console.error('Error fetching music data:', error);
        }
    }

    return {
        artists: artists,
        albums: albums,
        tracks: tracks,
        playlists: playlists
    };
};
