import { api } from "../app/auth";

export interface ClientOverview {
    totalClients: number;
    returning: number;
    averageLtv: number;
    satisfaction: number;
}

export interface ClientListItem {
    id: string;
    firstName: string;
    lastName: string;
    phone: string;
    email?: string | null;
    tags: ClientTag[];
    lastVisitAt?: string | null;
    lifetimeValue?: number | null;
    status?: string | null;
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
    segmentTagId?: string | null;
}

export interface ClientTag {
    id: string;
    name: string;
    color?: string | null;
}

export interface ClientFiltersResponse {
    clientTags: ClientTag[];
    statuses: ClientTag[];
    segments: ClientTag[];
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
    const { data } = await api.get<ClientOverview & { returningClients?: number }>("/clients/overview", { signal });
    return {
        totalClients: data?.totalClients ?? 0,
        returning: data?.returning ?? data?.returningClients ?? 0,
        averageLtv: data?.averageLtv ?? 0,
        satisfaction: data?.satisfaction ?? 0,
    };
}

export async function searchClients(params: SearchClientsRequest, signal?: AbortSignal): Promise<ClientListItem[]> {
    const { data } = await api.get<ClientListItem[]>("/clients", {
        signal,
    });
    return (data ?? []).map((client) => ({
        ...client,
        tags: client?.tags ?? [],
        lastVisitAt: client?.lastVisitAt ?? null,
        lifetimeValue: client?.lifetimeValue ?? null,
        status: client?.status ?? null,
    }));
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
    const { data } = await api.get<ClientTag[]>("/client-tags", { signal });
    return data;
}

export async function getClientFilters(signal?: AbortSignal): Promise<ClientFiltersResponse> {
    const { data } = await api.get<ClientFiltersResponse>("/filters", { signal });
    return {
        clientTags: data?.clientTags ?? [],
        statuses: data?.statuses ?? [],
        segments: data?.segments ?? [],
    };
}

export async function getClientStatusTags(signal?: AbortSignal): Promise<ClientTag[]> {
    const { data } = await api.get<ClientTag[]>("/clients/status-tags", { signal });
    return data;
}
