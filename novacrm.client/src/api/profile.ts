import axios, { type AxiosError } from "axios";
import { api } from "../app/auth";

export interface Profile {
    userId: string;
    staffId: string;
    email: string;
    firstName: string;
    lastName: string;
    phone?: string | null;
    address?: string | null;
    notes?: string | null;
}

export type UpdateProfilePayload = {
    firstName: string;
    lastName: string;
    phone?: string | null;
    address?: string | null;
    notes?: string | null;
};

export async function getProfile(signal?: AbortSignal): Promise<Profile> {
    const { data } = await api.get<Profile>("/profile/me", { signal });
    return data;
}

export async function updateProfile(payload: UpdateProfilePayload): Promise<Profile> {
    const { data } = await api.put<Profile>("/profile", payload);
    return data;
}

export function isAxiosValidationError(
    error: unknown
): error is AxiosError<{ errors?: Record<string, string[]>; title?: string; detail?: string; message?: string; }> {
    return axios.isAxiosError(error) && typeof error.response?.data === "object" && error.response.data !== null;
}
