import { api } from "../app/auth";

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
    status: string;
    statusColor?: string | null;
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
    status: string;
    statusColor?: string | null;
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

export interface ClientTag {
    id: string;
    name: string;
    color?: string | null;
}

export type ClientFilterKey = "All" | string;

export interface ClientFilter {
    key: ClientFilterKey;
    label: string;
    color?: string | null;
}

export interface SearchClientsRequest {
    search?: string;
    filter?: ClientFilterKey | null;
}

export async function getClientsOverview(signal?: AbortSignal): Promise<ClientOverview> {
    const { data } = await api.get<ClientOverview>("/clients/overview", { signal });
    return data;
}

export async function searchClients(params: SearchClientsRequest, signal?: AbortSignal): Promise<ClientListItem[]> {
    const { search, filter } = params;
    const filterParam = filter && filter !== "All" ? filter : undefined;
    const { data } = await api.get<ClientListItem[]>("/clients", {
        params: {
            search: search?.trim() || undefined,
            filter: filterParam || undefined,
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

export async function getClientTags(signal?: AbortSignal): Promise<ClientTag[]> {
    const { data } = await api.get<ClientTag[]>("/clients/tags", { signal });
    return data;
}

export async function getClientFilters(signal?: AbortSignal): Promise<ClientFilter[]> {
    const { data } = await api.get<ClientFilter[]>("/clients/filters", { signal });
    return data;
}
