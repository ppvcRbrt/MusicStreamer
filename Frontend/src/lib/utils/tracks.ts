export function getTrackFile(track: App.Track, preferredFormat: string): string {
    let formats: Record<string, string> = {};
    if(track) {
        for (const filePath of track.filePaths) {
            const extension = filePath.split('.').pop()?.toLowerCase();
            formats[extension ?? "Unknown"] = filePath;
        }
        let trackfile = formats[preferredFormat];
        if (!trackfile) {
            return formats[0];
        }
        return trackfile;
    }
    return '';
}