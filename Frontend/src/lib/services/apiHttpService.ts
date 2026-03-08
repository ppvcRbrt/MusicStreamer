import {setServerError} from "../../errorState.svelte";
class ApiHttpService {
    private readonly baseUrl:string;
    constructor() {
        this.baseUrl = `http://robs-laptop:5277`;
    }

    private async request<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
        let response: Response;
        try {
            response = await fetch(`${this.baseUrl}${endpoint}`, {
                ...options,
                headers: {
                    'Content-Type': 'application/json',
                    ...options.headers,
                },
            });
        } catch {
            setServerError('Unable to connect to the server');
            throw new Error('Network error: failed to fetch');
        }

        if (response.status >= 500) {
            setServerError(`Server error: ${response.status}`);
            throw new Error(`Server error: ${response.status}`);
        }

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const text = await response.text();
        if (!text) return undefined as T;

        try {
            return JSON.parse(text) as T;
        } catch {
            return text as unknown as T;
        }
    }


    async get<T>(endpoint: string): Promise<T> {
        return this.request<T>(endpoint, { method: 'GET' });
    }

    async post<T>(endpoint: string, data: unknown): Promise<T> {
        return await this.request<T>(endpoint, {
            method: 'POST',
            body: JSON.stringify(data),
        });
    }
    async imageExists(path: string): Promise<boolean> {
        return new Promise((resolve) => {
            const img = new Image();
            img.onload = () => resolve(true);
            img.onerror = () => resolve(false);
            img.src = path;
        });
    }
    getBaseUrl(): string {
        return this.baseUrl;
    }
    getMediaResourceUrl(filepath: string): string {
        return `${this.baseUrl}/music/stream?filePath=${encodeURIComponent(filepath)}`;
    }
}

export const apiHttpService = new ApiHttpService();
