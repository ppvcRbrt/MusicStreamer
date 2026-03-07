// See https://svelte.dev/docs/kit/types#app.d.ts
// for information about these interfaces
declare global {
	namespace App {
		// interface Error {}
		// interface Locals {}
		// interface PageData {}
		// interface PageState {}
		// interface Platform {}
		interface Track {
			id: number;
			trackNumber: number;
			title: string;
			duration: number;
			filePath: string;
			artist?: Artist;
			album?: Album;
		}
		interface Artist {
			id: number;
			name: string;
			image?: string;
		}
		interface Playlist {
			id: number;
			title: string;
			image?: string;
			imageSmall?: string;
			imageLarge?: string;
			tracks: Track[];
			type: 'playlist';
		}
		interface Album {
			id: number;
			title: string;
			image?: string;
			imageSmall?: string;
			imageLarge?: string;
			artist: Artist;
			tracks: Track[];
			type: 'album';
		}

		interface TrackStoreResult {
			message: string;
			tracksAdded: number;
			artistsAdded: number;
			albumsAdded: number;
		}

		interface MusicBrainzSearchResult {
			created: string;
			count: number;
			offset: number
			artists: MusicBrainzArtist[];
		}

		interface MusicBrainzArtist {
			id: string;
			type: string;
			score: number;
			name: string;
			disambiguation?: string;
			musicBrainzUrl: string;
		}
	}
}

export {};
