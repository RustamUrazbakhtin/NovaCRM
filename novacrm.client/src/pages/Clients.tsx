import axios from "axios";
import { useEffect, useMemo, useRef, useState } from "react";
import Header from "../layout/Header";
import ThemeProvider from "../providers/ThemeProvider";
import type { ClientDetails, ClientListItem, ClientOverview, ClientStatusTag } from "../api/clients";
import { createClient, getClientDetails, getClientsOverview, getClientStatusTags, searchClients } from "../api/clients";
import "../styles/dashboard/index.css";
import "../styles/clients/index.css";
import { authApi } from "../app/auth";
import { useNavigate } from "react-router-dom";

const formatCurrency = (value: number) =>
    new Intl.NumberFormat("en-US", { style: "currency", currency: "USD", maximumFractionDigits: 0 }).format(value);

const formatDate = (value?: string | null) => {
    if (!value) return "—";
    const date = new Date(value);
    return date.toLocaleDateString("en-US", { month: "short", day: "numeric", year: "numeric" });
};

const statusSlug = (value: string) => value.toLowerCase().replace(/[^a-z0-9]+/g, "-");

export default function Clients() {
    const navigate = useNavigate();
    const [overview, setOverview] = useState<ClientOverview | null>(null);
    const [clients, setClients] = useState<ClientListItem[]>([]);
    const [statusFilter, setStatusFilter] = useState<string | null>(null);
    const [statusTags, setStatusTags] = useState<ClientStatusTag[]>([]);
    const [search, setSearch] = useState("");
    const [loadingList, setLoadingList] = useState(false);
    const [loadingOverview, setLoadingOverview] = useState(false);
    const [loadingDetails, setLoadingDetails] = useState(false);
    const [selectedId, setSelectedId] = useState<string | null>(null);
    const [selectedClient, setSelectedClient] = useState<ClientDetails | null>(null);
    const [isAddOpen, setIsAddOpen] = useState(false);
    const [addForm, setAddForm] = useState({ firstName: "", lastName: "", phone: "", email: "", segment: "" });
    const [savingClient, setSavingClient] = useState(false);
    const abortControllerRef = useRef<AbortController | null>(null);

    const filters = useMemo(
        () => [
            { id: null as string | null, label: "All" },
            ...statusTags.map((tag) => ({ id: tag.id, label: tag.name })),
        ],
        [statusTags]
    );

    const loadOverview = async () => {
        setLoadingOverview(true);
        try {
            const data = await getClientsOverview();
            setOverview(data);
        } catch (error: any) {
            if (axios.isCancel?.(error) || error?.name === "CanceledError") return;
            console.error("Failed to load clients overview", error);
        } finally {
            setLoadingOverview(false);
        }
    };

    useEffect(() => {
        getClientStatusTags()
            .then(setStatusTags)
            .catch((error) => console.error("Failed to load status tags", error));
    }, []);

    const loadClients = async (query: string, statusTagId: string | null) => {
        abortControllerRef.current?.abort();
        const controller = new AbortController();
        abortControllerRef.current = controller;
        setLoadingList(true);
        try {
            const data = await searchClients({ search: query, statusTagId }, controller.signal);
            setClients(data);
            if (data.length === 0) {
                setSelectedId(null);
            }
        } catch (error: any) {
            if (axios.isCancel?.(error) || error?.name === "CanceledError") return;
            console.error("Failed to load clients", error);
        } finally {
            setLoadingList(false);
        }
    };

    const loadDetails = async (id: string) => {
        setLoadingDetails(true);
        setSelectedClient(null);
        try {
            const data = await getClientDetails(id);
            setSelectedClient(data);
        } catch (error: any) {
            if (axios.isCancel?.(error) || error?.name === "CanceledError") return;
            console.error("Failed to load client details", error);
        } finally {
            setLoadingDetails(false);
        }
    };

    useEffect(() => {
        void loadOverview();
    }, []);

    useEffect(() => {
        void loadClients(search, statusFilter);
    }, [search, statusFilter]);

    useEffect(() => () => abortControllerRef.current?.abort(), []);

    useEffect(() => {
        if (!selectedId) {
            setSelectedClient(null);
            return;
        }

        void loadDetails(selectedId);
    }, [selectedId]);

    useEffect(() => {
        const onKeyDown = (event: KeyboardEvent) => {
            if (event.key === "Escape") {
                setSelectedClient(null);
                setSelectedId(null);
                setIsAddOpen(false);
            }
        };

        document.addEventListener("keydown", onKeyDown);
        return () => document.removeEventListener("keydown", onKeyDown);
    }, []);

    const handleLogout = () => {
        authApi.logout();
        navigate("/auth", { replace: true });
    };

    const handleOpenAdd = () => {
        setAddForm({ firstName: "", lastName: "", phone: "", email: "", segment: "" });
        setIsAddOpen(true);
    };

    const handleCreate = async () => {
        if (!addForm.firstName.trim() || !addForm.lastName.trim() || !addForm.phone.trim()) {
            return;
        }

        setSavingClient(true);
        try {
            const created = await createClient({
                firstName: addForm.firstName,
                lastName: addForm.lastName,
                phone: addForm.phone,
                email: addForm.email || null,
                segment: addForm.segment || null,
            });
            setIsAddOpen(false);
            setSelectedId(created.id);
            await loadOverview();
            await loadClients(search, statusFilter);
        } finally {
            setSavingClient(false);
        }
    };

    const sortedClients = useMemo(() => clients, [clients]);

    return (
        <ThemeProvider>
            <Header breadcrumb="Clients" onLogout={handleLogout} />
            <main className="clients-page">
                <section className="clients-toolbar">
                    <div className="clients-heading">
                        <h1>Clients</h1>
                        <p>All your guests in one place: visits, value, and activity.</p>
                    </div>
                </section>

                <section className="clients-overview-card">
                    <header>
                        <h2>Clients overview</h2>
                        <span>Real-time metrics from your database</span>
                    </header>
                    <div className="clients-metrics-grid">
                        <article className="clients-metric-card">
                            <span className="clients-metric-label">Total clients</span>
                            <strong className="clients-metric-value">
                                {loadingOverview ? "—" : overview?.totalClients ?? 0}
                            </strong>
                            <span className="clients-metric-hint">People in your CRM</span>
                        </article>
                        <article className="clients-metric-card">
                            <span className="clients-metric-label">Returning</span>
                            <strong className="clients-metric-value">
                                {loadingOverview ? "—" : overview?.returningClients ?? 0}
                            </strong>
                            <span className="clients-metric-hint">Visited more than once</span>
                        </article>
                        <article className="clients-metric-card">
                            <span className="clients-metric-label">Average LTV</span>
                            <strong className="clients-metric-value">
                                {loadingOverview ? "—" : formatCurrency(overview?.averageLtv ?? 0)}
                            </strong>
                            <span className="clients-metric-hint">Average revenue per client</span>
                        </article>
                        <article className="clients-metric-card">
                            <span className="clients-metric-label">Satisfaction</span>
                            <strong className="clients-metric-value">
                                {loadingOverview ? "—" : overview?.satisfaction.toFixed(1) ?? "0"}
                            </strong>
                            <span className="clients-metric-hint">Average rating from reviews</span>
                        </article>
                    </div>
                </section>

                <section className="clients-widget">
                    <header className="clients-widget__header">
                        <div className="clients-widget__actions">
                            <button type="button" className="clients-add" onClick={handleOpenAdd}>
                                Add client
                            </button>
                            <input
                                type="search"
                                className="clients-search"
                                placeholder="Search by name, phone, or email"
                                value={search}
                                onChange={(event) => setSearch(event.target.value)}
                            />
                        </div>
                        <div className="clients-segments" role="tablist" aria-label="Client segments">
                            {filters.map((item) => (
                                <button
                                    key={item.id ?? "all"}
                                    type="button"
                                    role="tab"
                                    aria-selected={statusFilter === item.id}
                                    className={`clients-segment${statusFilter === item.id ? " is-active" : ""}`}
                                    onClick={() => setStatusFilter(item.id)}
                                >
                                    {item.label}
                                </button>
                            ))}
                        </div>
                    </header>

                    <div className="clients-table-card">
                        <div className="clients-table-scroll" role="region" aria-label="Clients list">
                            <table className="clients-table">
                                <thead>
                                    <tr>
                                        <th>Client</th>
                                        <th>Tags</th>
                                        <th>Last visit</th>
                                        <th>LTV</th>
                                        <th>Status</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {loadingList ? (
                                        <tr>
                                            <td colSpan={5} className="clients-table-empty">
                                                Loading clients…
                                            </td>
                                        </tr>
                                    ) : sortedClients.length === 0 ? (
                                        <tr>
                                            <td colSpan={5} className="clients-table-empty">
                                                No clients found for this query.
                                            </td>
                                        </tr>
                                    ) : (
                                        sortedClients.map((client) => (
                                            <tr
                                                key={client.id}
                                                className="clients-table-row"
                                                onClick={() => setSelectedId(client.id)}
                                                tabIndex={0}
                                                onKeyDown={(event) => {
                                                    if (event.key === "Enter" || event.key === " ") {
                                                        event.preventDefault();
                                                        setSelectedId(client.id);
                                                    }
                                                }}
                                            >
                                                <td>
                                                    <div className="clients-table-primary">
                                                        <div className="clients-client-heading">
                                                            <span className="clients-client-name">{client.name}</span>
                                                            {client.city && <small className="clients-client-city">{client.city}</small>}
                                                        </div>
                                                        <div className="clients-client-meta">
                                                            <span>{client.phone}</span>
                                                            {client.email && <span>{client.email}</span>}
                                                        </div>
                                                    </div>
                                                </td>
                                                <td>
                                                    <div className="clients-client-tags">
                                                        {client.tags.length ? client.tags.map((tag) => <span key={tag}>{tag}</span>) : "—"}
                                                    </div>
                                                </td>
                                                <td>{formatDate(client.lastVisitAt)}</td>
                                                <td>{formatCurrency(client.lifetimeValue)}</td>
                                                <td>
                                                    <span
                                                        className={`clients-status clients-status--${statusSlug(client.status || "")}`}
                                                        style={client.statusColor ? { backgroundColor: client.statusColor } : undefined}
                                                    >
                                                        {client.status || "—"}
                                                    </span>
                                                </td>
                                            </tr>
                                        ))
                                    )}
                                </tbody>
                            </table>
                        </div>
                    </div>
                </section>

                {selectedClient && (
                    <div className="clients-modal" role="dialog" aria-modal="true">
                        <div className="clients-modal__backdrop" onClick={() => setSelectedClient(null)} />
                        <article className="clients-modal__content">
                            <header className="clients-modal__header">
                                <div>
                                    <h2>{selectedClient.name}</h2>
                                    <p>
                                        {selectedClient.city ? `${selectedClient.city} · ` : ""}
                                        {selectedClient.master ? `Preferred master: ${selectedClient.master}` : ""}
                                    </p>
                                </div>
                                <div className="clients-modal__status">
                                    <span
                                        className={`clients-status clients-status--${statusSlug(selectedClient.status || "")}`}
                                        style={selectedClient.statusColor ? { backgroundColor: selectedClient.statusColor } : undefined}
                                    >
                                        {selectedClient.status || "—"}
                                    </span>
                                    {loadingDetails && <span className="clients-loading">Refreshing…</span>}
                                    <button type="button" className="clients-modal__close" onClick={() => setSelectedClient(null)}>
                                        ×
                                    </button>
                                </div>
                            </header>

                            <div className="clients-detail-grid">
                                <div>
                                    <span className="clients-detail-label">LTV</span>
                                    <strong>{formatCurrency(selectedClient.lifetimeValue)}</strong>
                                </div>
                                <div>
                                    <span className="clients-detail-label">Visits</span>
                                    <strong>{selectedClient.visits}</strong>
                                </div>
                                <div>
                                    <span className="clients-detail-label">Rating</span>
                                    <strong>{selectedClient.satisfaction.toFixed(1)}</strong>
                                </div>
                            </div>

                            <section className="clients-detail-section">
                                <h3>Contacts</h3>
                                <div className="clients-detail-contacts">
                                    <span>{selectedClient.phone}</span>
                                    {selectedClient.email && <span>{selectedClient.email}</span>}
                                </div>
                            </section>

                            <section className="clients-detail-section">
                                <h3>Recent activity</h3>
                                <ul className="clients-timeline">
                                    {selectedClient.recentActivity.length === 0 ? (
                                        <li>No recent activity found.</li>
                                    ) : (
                                        selectedClient.recentActivity.map((item) => (
                                            <li key={`${item.occurredAt}-${item.title}`}>
                                                <span className="clients-timeline-time">{formatDate(item.occurredAt)}</span>
                                                <div>
                                                    <strong>{item.title}</strong>
                                                    {item.description && <p>{item.description}</p>}
                                                </div>
                                            </li>
                                        ))
                                    )}
                                </ul>
                            </section>

                            <section className="clients-detail-section">
                                <h3>Tags</h3>
                                <div className="clients-client-tags">
                                    {selectedClient.tags.length ? selectedClient.tags.map((tag) => <span key={tag}>{tag}</span>) : "—"}
                                </div>
                            </section>

                            {selectedClient.notes && (
                                <section className="clients-detail-section">
                                    <h3>Notes</h3>
                                    <p className="clients-detail-notes">{selectedClient.notes}</p>
                                </section>
                            )}
                        </article>
                    </div>
                )}

                {isAddOpen && (
                    <div className="clients-modal" role="dialog" aria-modal="true">
                        <div className="clients-modal__backdrop" onClick={() => setIsAddOpen(false)} />
                        <article className="clients-modal__content">
                            <header className="clients-modal__header">
                                <div>
                                    <h2>Add client</h2>
                                    <p>Create a new client profile with minimal details.</p>
                                </div>
                                <button type="button" className="clients-modal__close" onClick={() => setIsAddOpen(false)}>
                                    ×
                                </button>
                            </header>
                            <div className="clients-form-grid">
                                <label>
                                    <span>First name</span>
                                    <input
                                        type="text"
                                        value={addForm.firstName}
                                        onChange={(e) => setAddForm((prev) => ({ ...prev, firstName: e.target.value }))}
                                        placeholder="Jane"
                                    />
                                </label>
                                <label>
                                    <span>Last name</span>
                                    <input
                                        type="text"
                                        value={addForm.lastName}
                                        onChange={(e) => setAddForm((prev) => ({ ...prev, lastName: e.target.value }))}
                                        placeholder="Doe"
                                    />
                                </label>
                                <label>
                                    <span>Phone</span>
                                    <input
                                        type="tel"
                                        value={addForm.phone}
                                        onChange={(e) => setAddForm((prev) => ({ ...prev, phone: e.target.value }))}
                                        placeholder="+1 (555) 123-4567"
                                    />
                                </label>
                                <label>
                                    <span>Email</span>
                                    <input
                                        type="email"
                                        value={addForm.email}
                                        onChange={(e) => setAddForm((prev) => ({ ...prev, email: e.target.value }))}
                                        placeholder="name@email.com"
                                    />
                                </label>
                                <label>
                                    <span>Segment</span>
                                    <input
                                        type="text"
                                        value={addForm.segment}
                                        onChange={(e) => setAddForm((prev) => ({ ...prev, segment: e.target.value }))}
                                        placeholder="VIP / Regular"
                                    />
                                </label>
                            </div>
                            <footer className="clients-modal__footer">
                                <button type="button" className="clients-secondary" onClick={() => setIsAddOpen(false)}>
                                    Cancel
                                </button>
                                <button type="button" className="clients-primary" onClick={() => void handleCreate()} disabled={savingClient}>
                                    {savingClient ? "Saving…" : "Create"}
                                </button>
                            </footer>
                        </article>
                    </div>
                )}
            </main>
        </ThemeProvider>
    );
}
