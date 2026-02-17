// See https://svelte.dev/docs/kit/types#app.d.ts
// for information about these interfaces
declare global {
	namespace App {
		// interface Error {}
		// interface Locals {}
		// interface PageData {}
		// interface PageState {}
		// interface Platform {}
		interface Song {
			id: number;
			title: string;
			duration: number;
			artist: Artist;
			album: Album;
		}
		interface Artist {
			id: number;
			name: string;
			image?: string;
		}
		interface Playlist {
			id: number;
			name: string;
			image?: string;
			songs: Song[];
			type: 'playlist';
		}
		interface Album {
			id: number;
			name: string;
			image?: string;
			artist: Artist;
			songs: Song[];
			type: 'album';
		}
	}
}

export {};
