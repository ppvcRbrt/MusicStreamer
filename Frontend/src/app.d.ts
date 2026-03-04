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
			tracks: Track[];
			type: 'playlist';
		}
		interface Album {
			id: number;
			title: string;
			image?: string;
			artist: Artist;
			tracks: Track[];
			type: 'album';
		}
	}
}

export {};
