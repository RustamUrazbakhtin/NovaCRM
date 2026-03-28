import { api } from "../app/auth";

export interface ServiceOption {
    id: string;
    name: string;
    category?: string | null;
    durationMinutes: number;
    price: number;
}

export async function getServices(signal?: AbortSignal): Promise<ServiceOption[]> {
    const { data } = await api.get<ServiceOption[]>("/services", { signal });
    return data ?? [];
}
