import { api } from "../app/auth";

export type ClientFilter = "All" | "Vip" | "Regular" | "New" | "AtRisk";

export interface ClientOverview {
    totalClients: number;
    returningClients: number;
    averageLtv: number;
    satisfaction: number;
}

export interface ClientListItem {
    id: string;
    name: string;
    phone: string;
    email?: string | null;
    status: ClientFilter;
    lifetimeValue: number;
    lastVisitAt?: string | null;
    satisfaction: number;
    visits: number;
    tags: string[];
    city?: string | null;
}

export interface ClientActivityDto {
    occurredAt: string;
    title: string;
    description?: string | null;
}

export interface ClientDetails {
    id: string;
    name: string;
    phone: string;
    email?: string | null;
    city?: string | null;
    master?: string | null;
    status: ClientFilter;
    lifetimeValue: number;
    visits: number;
    satisfaction: number;
    tags: string[];
    recentActivity: ClientActivityDto[];
    notes?: string | null;
}

export interface CreateClientPayload {
    firstName: string;
    lastName: string;
    phone: string;
    email?: string | null;
    segment?: string | null;
}

export async function getClientsOverview(signal?: AbortSignal): Promise<ClientOverview> {
    const { data } = await api.get<ClientOverview>("/clients/overview", { signal });
    return data;
}

export async function searchClients(
    params: { search?: string; filter?: ClientFilter },
    signal?: AbortSignal
): Promise<ClientListItem[]> {
    const { search, filter } = params;
    const { data } = await api.get<ClientListItem[]>("/clients", {
        params: {
            search: search?.trim() || undefined,
            filter,
        },
        signal,
    });
    return data;
}

export async function getClientDetails(id: string, signal?: AbortSignal): Promise<ClientDetails> {
    const { data } = await api.get<ClientDetails>(`/clients/${id}`, { signal });
    return data;
}

export async function createClient(payload: CreateClientPayload): Promise<ClientDetails> {
    const { data } = await api.post<ClientDetails>("/clients", payload);
    return data;
}
