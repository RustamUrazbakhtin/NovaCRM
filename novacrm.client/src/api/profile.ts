import axios, { type AxiosError } from "axios";
import { api } from "../app/auth";

export interface UserProfile {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
    phone?: string;
    roleId?: string;
    roleName?: string;
    companyId?: string;
    companyName?: string;
    timezone?: string;
    locale?: string;
    address?: string;
    notes?: string;
    avatarUrl?: string | null;
    updatedAt: string;
}

export type UpdateProfilePayload = {
    firstName: string;
    lastName: string;
    email: string;
    phone?: string | null;
    roleId?: string | null;
    timezone?: string | null;
    locale?: string | null;
    address?: string | null;
    notes?: string | null;
};

export interface ProfileRole {
    id: string;
    name: string;
}

export async function getProfile(signal?: AbortSignal): Promise<UserProfile> {
    const { data } = await api.get<UserProfile>("/profile/me", { signal });
    return data;
}

export async function updateProfile(payload: UpdateProfilePayload): Promise<UserProfile> {
    const { data } = await api.put<UserProfile>("/profile/me", payload);
    return data;
}

export async function getProfileRoles(signal?: AbortSignal): Promise<ProfileRole[]> {
    const { data } = await api.get<ProfileRole[]>("/profile/roles", { signal });
    return data;
}

export async function uploadAvatar(file: File): Promise<UserProfile> {
    const formData = new FormData();
    formData.append("file", file);

    const { data } = await api.post<UserProfile>("/profile/me/avatar", formData, {
        headers: { "Content-Type": "multipart/form-data" },
    });

    return data;
}

export async function deleteAvatar(): Promise<UserProfile> {
    const { data } = await api.delete<UserProfile>("/profile/me/avatar");
    return data;
}

export function isAxiosValidationError(
    error: unknown
): error is AxiosError<{ errors?: Record<string, string[]>; title?: string; detail?: string; message?: string; }> {
    return axios.isAxiosError(error) && typeof error.response?.data === "object" && error.response.data !== null;
}
