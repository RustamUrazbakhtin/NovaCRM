import axios from "axios";
import { useEffect, useMemo, useRef, useState } from "react";
import Header from "../layout/Header";
import ThemeProvider from "../providers/ThemeProvider";
import type {
    ClientDetails,
    ClientFilter,
    ClientListItem,
    ClientOverview,
    ClientTag,
} from "../api/clients";
import { createClient, getClientDetails, getClientFilters, getClientTags, getClientsOverview, searchClients } from "../api/clients";
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

const formatLastVisit = (value?: string | null) => {
    if (!value) return "No visits yet";
    return formatDate(value);
};

const statusSlug = (value: string) => value.toLowerCase().replace(/[^a-z0-9]+/g, "-");
const initialsFor = (firstName?: string, lastName?: string) =>
    `${firstName?.charAt(0) ?? ""}${lastName?.charAt(0) ?? ""}`.toUpperCase() || "CL";

export default function Clients() {
    const navigate = useNavigate();
    const [overview, setOverview] = useState<ClientOverview>({
        totalClients: 0,
        returning: 0,
        averageLtv: 0,
        satisfaction: 0,
    });
    const [clients, setClients] = useState<ClientListItem[]>([]);
    const [statusFilter, setStatusFilter] = useState<string>("All");
    const [statusFilters, setStatusFilters] = useState<ClientFilter[]>([{ key: "All", label: "All", color: null }]);
    const [search, setSearch] = useState("");
    const [loadingList, setLoadingList] = useState(false);
    const [loadingOverview, setLoadingOverview] = useState(false);
    const [loadingDetails, setLoadingDetails] = useState(false);
    const [clientsError, setClientsError] = useState<string | null>(null);
    const [selectedId, setSelectedId] = useState<string | null>(null);
    const [selectedClient, setSelectedClient] = useState<ClientDetails | null>(null);
    const [isAddOpen, setIsAddOpen] = useState(false);
    const [addForm, setAddForm] = useState({ firstName: "", lastName: "", phone: "", email: "", segmentTagId: "" });
    const [savingClient, setSavingClient] = useState(false);
    const [segmentTags, setSegmentTags] = useState<ClientTag[]>([]);
    const [loadingSegments, setLoadingSegments] = useState(false);
    const [segmentsError, setSegmentsError] = useState<string | null>(null);
    const abortControllerRef = useRef<AbortController | null>(null);
    const segmentAbortRef = useRef<AbortController | null>(null);
    const hasLoggedOverviewError = useRef(false);
    const hasLoggedClientsError = useRef(false);
    const hasLoggedFiltersError = useRef(false);

    const loadOverview = async () => {
        setLoadingOverview(true);
        try {
            const data = await getClientsOverview();
            setOverview(data);
        } catch (error: any) {
            if (axios.isCancel?.(error) || error?.name === "CanceledError") return;
            setOverview({ totalClients: 0, returning: 0, averageLtv: 0, satisfaction: 0 });
            if (!hasLoggedOverviewError.current) {
                console.error("Failed to load clients overview", error);
                hasLoggedOverviewError.current = true;
            }
        } finally {
            setLoadingOverview(false);
        }
    };

    useEffect(() => {
        const controller = new AbortController();

        getClientFilters(controller.signal)
            .then((data) => {
                const statuses = data?.statuses ?? [];
                const incoming = statuses.length > 0
                    ? [
                          { key: "All", label: "All", color: null },
                          ...statuses.map((status) => ({ key: status.id, label: status.name, color: status.color ?? null })),
                      ]
                    : [{ key: "All", label: "All", color: null }];

                setStatusFilters(incoming);
                setStatusFilter((current) =>
                    current && incoming.some((item) => item.key === current) ? current : "All"
                );
            })
            .catch((error) => {
                if (axios.isCancel?.(error) || error?.name === "CanceledError") return;
                setStatusFilters([{ key: "All", label: "All", color: null }]);
                setStatusFilter("All");
                if (!hasLoggedFiltersError.current) {
                    console.error("Failed to load client filters", error?.message ?? error);
                    hasLoggedFiltersError.current = true;
                }
            });

        return () => controller.abort();
    }, []);

    const loadClients = async (query: string, filterKey: string) => {
        abortControllerRef.current?.abort();
        const controller = new AbortController();
        abortControllerRef.current = controller;
        setLoadingList(true);
        setClientsError(null);
        try {
            const data = await searchClients({ search: query, filter: filterKey }, controller.signal);
            setClients(data);
            if (data.length === 0) {
                setSelectedId(null);
            }
        } catch (error: any) {
            if (axios.isCancel?.(error) || error?.name === "CanceledError") return;
            setClientsError("Failed to load clients. Please try again.");
            if (!hasLoggedClientsError.current) {
                console.error("Failed to load clients", error?.message ?? error);
                hasLoggedClientsError.current = true;
            }
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

    const loadSegmentTags = async (signal?: AbortSignal) => {
        setLoadingSegments(true);
        setSegmentsError(null);

        try {
            const data = await getClientTags(signal);
            setSegmentTags(data);
        } catch (error: any) {
            if (axios.isCancel?.(error) || error?.name === "CanceledError") return;
            setSegmentsError("Failed to load segments.");
            console.error("Failed to load segment tags", error?.message ?? error);
        } finally {
            setLoadingSegments(false);
        }
    };

    const handleRetrySegments = () => {
        segmentAbortRef.current?.abort();
        const controller = new AbortController();
        segmentAbortRef.current = controller;
        void loadSegmentTags(controller.signal);
    };

    useEffect(() => {
        if (!isAddOpen || segmentTags.length > 0) {
            return;
        }

        handleRetrySegments();

        return () => segmentAbortRef.current?.abort();
    }, [isAddOpen, segmentTags.length]);

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
        setAddForm({ firstName: "", lastName: "", phone: "", email: "", segmentTagId: "" });
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
                segmentTagId: addForm.segmentTagId || null,
            });
            setIsAddOpen(false);
            setSelectedId(created.id);
            await loadOverview();
            await loadClients(search, statusFilter);
        } catch (error: any) {
            console.error("Failed to create client", error?.message ?? error);
        } finally {
            setSavingClient(false);
        }
    };

    const sortedClients = useMemo(() => clients ?? [], [clients]);

    return (
        <ThemeProvider>
            <Header breadcrumb="Clients" onLogout={handleLogout} />
            <main className="clients-page">
                <section className="clients-toolbar">
                    <div className="clients-heading">
                        <h1>Clients</h1>
                        <p>Manage your clients, visits, and value in one place.</p>
                    </div>
                    <div className="clients-toolbar__actions">
                        <button type="button" className="clients-add" onClick={handleOpenAdd}>
                            <span className="clients-add__text">Add Client</span>
                            <span className="clients-add__icon">+</span>
                        </button>
                        <label className="clients-search-wrap" aria-label="Search clients">
                            <span className="clients-search-icon" aria-hidden="true">⌕</span>
                            <input
                                type="search"
                                className="clients-search"
                                placeholder="Search by name, phone, or email"
                                value={search}
                                onChange={(event) => setSearch(event.target.value)}
                            />
                        </label>
                    </div>
                </section>

                <section className="clients-widget">
                    <header className="clients-widget__header">
                        <div className="clients-segments" role="tablist" aria-label="Client segments">
                            {statusFilters.map((item) => (
                                <button
                                    key={item.key}
                                    type="button"
                                    role="tab"
                                    aria-selected={statusFilter === item.key}
                                    className={`clients-segment${statusFilter === item.key ? " is-active" : ""}`}
                                    onClick={() => setStatusFilter(item.key)}
                                    style={item.color ? { borderColor: item.color } : undefined}
                                >
                                    {item.label}
                                </button>
                            ))}
                        </div>
                        <div className="clients-summary-strip" aria-label="Clients summary">
                            <article>
                                <span>Total</span>
                                <strong>{loadingOverview ? "—" : overview?.totalClients ?? 0}</strong>
                            </article>
                            <article>
                                <span>Returning</span>
                                <strong>{loadingOverview ? "—" : overview?.returning ?? 0}</strong>
                            </article>
                            <article>
                                <span>Avg LTV</span>
                                <strong>{loadingOverview ? "—" : formatCurrency(overview?.averageLtv ?? 0)}</strong>
                            </article>
                            <article>
                                <span>Satisfaction</span>
                                <strong>{loadingOverview ? "—" : (overview?.satisfaction ?? 0).toFixed(1)}</strong>
                            </article>
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
                                    {clientsError ? (
                                        <tr>
                                            <td colSpan={5} className="clients-table-empty">
                                                <div className="clients-error">
                                                    <p>{clientsError}</p>
                                                    <button type="button" className="clients-primary" onClick={() => void loadClients(search, statusFilter)}>
                                                        Retry
                                                    </button>
                                                </div>
                                            </td>
                                        </tr>
                                    ) : loadingList ? (
                                        Array.from({ length: 6 }).map((_, index) => (
                                            <tr key={`loading-${index}`} className="clients-row-loading">
                                                <td colSpan={5}>
                                                    <div className="clients-skeleton-row" />
                                                </td>
                                            </tr>
                                        ))
                                    ) : sortedClients.length === 0 ? (
                                        <tr>
                                            <td colSpan={5} className="clients-table-empty">
                                                <div className="clients-empty">
                                                    <span className="clients-empty__icon" aria-hidden="true">◌</span>
                                                    <h3>No clients yet</h3>
                                                    <p>
                                                        {search
                                                            ? "No clients matched this search. Try another query or clear filters."
                                                            : "Start building your client base by adding your first client."}
                                                    </p>
                                                    {!search && (
                                                        <button type="button" className="clients-primary" onClick={handleOpenAdd}>
                                                            Add Client
                                                        </button>
                                                    )}
                                                </div>
                                            </td>
                                        </tr>
                                    ) : (
                                        sortedClients.map((client) => (
                                            <tr
                                                key={client.id}
                                                className={`clients-table-row${selectedId === client.id ? " is-selected" : ""}`}
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
                                                            <span className="clients-avatar">
                                                                {initialsFor(client.firstName, client.lastName)}
                                                            </span>
                                                            <span className="clients-client-name">
                                                                {client.firstName} {client.lastName}
                                                            </span>
                                                        </div>
                                                        <div className="clients-client-meta">
                                                            <span>{client.phone}</span>
                                                            {client.email && <span>{client.email}</span>}
                                                        </div>
                                                    </div>
                                                </td>
                                                <td>
                                                    <div className="clients-client-tags">
                                                        {(client.tags ?? []).length
                                                            ? (client.tags ?? []).map((tag) => (
                                                                  <span key={tag.id} style={tag.color ? { backgroundColor: tag.color, color: "var(--ink)" } : undefined}>
                                                                      {tag.name}
                                                                  </span>
                                                              ))
                                                            : "—"}
                                                    </div>
                                                </td>
                                                <td>{formatLastVisit(client.lastVisitAt)}</td>
                                                <td>{formatCurrency(client.lifetimeValue ?? 0)}</td>
                                                <td>
                                                    <span
                                                        className={`clients-status clients-status--${statusSlug(client.status || "")}`}
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
                                    {(selectedClient.recentActivity ?? []).length === 0 ? (
                                        <li>No recent activity found.</li>
                                    ) : (
                                        (selectedClient.recentActivity ?? []).map((item) => (
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
                                    {(selectedClient.tags ?? []).length
                                        ? (selectedClient.tags ?? []).map((tag) => <span key={tag}>{tag}</span>)
                                        : "—"}
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
                                    <select
                                        value={addForm.segmentTagId}
                                        disabled={loadingSegments}
                                        onChange={(e) => setAddForm((prev) => ({ ...prev, segmentTagId: e.target.value }))}
                                    >
                                        <option value="">
                                            {loadingSegments
                                                ? "Loading…"
                                                : segmentTags.length === 0
                                                  ? "No segments"
                                                  : "No segment"}
                                        </option>
                                        {segmentTags.map((tag) => (
                                            <option key={tag.id} value={tag.id}>
                                                {tag.name}
                                            </option>
                                        ))}
                                    </select>
                                    {loadingSegments && <small className="clients-field-hint">Loading segments…</small>}
                                    {segmentsError && !loadingSegments && (
                                        <small className="clients-field-hint">
                                            {segmentsError}{" "}
                                            <button type="button" className="clients-retry" onClick={handleRetrySegments}>
                                                Retry
                                            </button>
                                        </small>
                                    )}
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
