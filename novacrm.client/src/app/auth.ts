import axios from "axios";

export const api = axios.create({
    baseURL: "/api",
    headers: { "Content-Type": "application/json" }
});

export function setToken(token?: string) {
    if (token) {
        localStorage.setItem("token", token);
        api.defaults.headers.common.Authorization = `Bearer ${token}`;
    } else {
        localStorage.removeItem("token");
        delete api.defaults.headers.common.Authorization;
    }
}

const saved = localStorage.getItem("token");
if (saved) setToken(saved);

export interface RegistrationPayload {
    companyName: string;
    industry?: string;
    country: string;
    timezone: string;
    businessEmail: string;
    companyPhone?: string;
    branchName?: string;
    branchAddress?: string;
    branchCity?: string;
    ownerFullName: string;
    ownerEmail: string;
    ownerPassword: string;
}

export const authApi = {
    async register(payload: RegistrationPayload) {
        await api.post("/auth/register", payload);
    },
    async login(email: string, password: string) {
        const { data } = await api.post("/auth/login", { email, password });
        setToken(data.token);
    },
    logout() { setToken(undefined); }
};

export function isAuthenticated() {
    return Boolean(localStorage.getItem("token"));
}
