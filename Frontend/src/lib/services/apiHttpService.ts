class ApiHttpService {
    private readonly baseUrl:string;
    constructor() {
        this.baseUrl = `http://robs-laptop:5277`;
    }

    private async request<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
        const response = await fetch(`${this.baseUrl}${endpoint}`, {
            ...options,
            headers: {
                'Content-Type': 'application/json',
                ...options.headers,
            },
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        return response.json();
    }

    async get<T>(endpoint: string): Promise<T> {
        return this.request<T>(endpoint, { method: 'GET' });
    }

    async post<T>(endpoint: string, data: unknown): Promise<T> {
        return this.request<T>(endpoint, {
            method: 'POST',
            body: JSON.stringify(data),
        });
    }
    getMediaResourceUrl(filepath: string): string {
        return `${this.baseUrl}/music/stream?filePath=${encodeURIComponent(filepath)}`;
    }}
export const apiHttpService = new ApiHttpService();
