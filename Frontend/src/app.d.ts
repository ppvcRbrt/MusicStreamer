// See https://svelte.dev/docs/kit/types#app.d.ts
// for information about these interfaces
declare global {
	namespace App {
		// interface Error {}
		// interface Locals {}
		// interface PageData {}
		// interface PageState {}
		// interface Platform {}

		type ListeningEventType = import('$lib/utils/enums').ListeningEventType;
		type ContextType = import('$lib/utils/enums').ContextType;

		interface TranscodingStatus {
			lastError?: string;
			state: "Idle" | "Running"
			tracksFailed: number;
			tracksProcessed: number;
			tracksSkipped: number;
		}
		interface MetadataSyncStatus {
			albumsMatched: number;
			artistsProcessed: number;
			coversFetched: number;
			lastError?: string;
			state: "Idle" | "Running";
		}
		interface Track {
			id: number;
			trackNumber: number;
			title: string;
			duration: number;
			filePaths: string[];
			artist?: Artist;
			album?: Album;
		}
		interface Artist {
			id: number;
			name: string;
			image?: string;
			imageSmall?: string;
			imageLarge?: string;
		}
		interface Playlist {
			id: number;
			title?: string;
			image?: string;
			imageSmall?: string;
			imageLarge?: string;
			trackIds?: number[];
			tracks?: Track[];
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

		interface ListeningEvent {
			trackId: number;
			eventType: ListeningEventType;
			timestamp: Date;
			positionMs: number;
			durationMs: number;
			context: ContextType;
		}

		interface AddToPlaylistRequest {
			trackId: number;
			playlistId: number;
		}
	}
}

export {};
