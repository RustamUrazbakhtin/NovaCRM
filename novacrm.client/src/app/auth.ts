import axios from "axios";

const baseURL = import.meta.env.VITE_API_URL ?? "/api";

export const api = axios.create({
    baseURL,
    headers: { "Content-Type": "application/json" },
    withCredentials: true,
});

const TOKEN_KEY = "token";

api.interceptors.request.use(config => {
    const token = localStorage.getItem(TOKEN_KEY);

    if (token) {
        config.headers = config.headers ?? {};
        config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
});

api.interceptors.response.use(
    response => response,
    error => {
        if (error.response?.status === 401) {
            setToken(undefined);
        }

        return Promise.reject(error);
    }
);

export function setToken(token?: string) {
    if (token) {
        localStorage.setItem(TOKEN_KEY, token);
    } else {
        localStorage.removeItem(TOKEN_KEY);
    }
}

const saved = localStorage.getItem(TOKEN_KEY);
if (saved) setToken(saved);

export interface RegistrationPayload {
    companyName: string;
    country: string;
    timezone: string;
    companyPhone: string;
    ownerEmail: string;
    ownerPassword: string;
    ownerPasswordRepeat: string;
    businessId?: string;
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
