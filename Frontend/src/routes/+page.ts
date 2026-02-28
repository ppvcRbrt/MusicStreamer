import type { PageLoad } from './$types';
import { apiHttpService } from '$lib/services/apiHttpService'
import { browser } from '$app/environment';
export const ssr = false;
export const  load: PageLoad = async ({ params }) => {
    let artists: App.Artist[] = [];
    let albums: App.Album[] = [];
    let tracks: App.Track[] = [];
    if(browser) {
        artists = await apiHttpService.get<App.Artist[]>('/music/artists')
        albums = await apiHttpService.get<App.Album[]>('/music/albums')
        tracks = albums.flatMap((album: App.Album) =>
            album.tracks.map(track => ({
                ...track,
                artist: album.artist,
                album: album,
                albumId: album.id
            })));
    }

    return {
        artists: artists,
        albums: albums,
        tracks: tracks,
    };
};
