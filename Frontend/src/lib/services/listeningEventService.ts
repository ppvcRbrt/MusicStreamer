import {apiHttpService} from "$lib/services/apiHttpService";
import {type ContextType, ListeningEventType} from "$lib/utils/enums";

export async function logEvent(trackId: number, positionMs: number, durationMs: number, eventType: ListeningEventType, context: ContextType) {
    let eventData: App.ListeningEvent = {
        trackId,
        eventType: eventType,
        timestamp: new Date(),
        positionMs,
        durationMs,
        context
    };
    await apiHttpService.post('/user/listeningEvent', eventData);
}
