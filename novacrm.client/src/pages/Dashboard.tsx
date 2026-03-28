import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import Header from "../layout/Header";
import ThemeProvider from "../providers/ThemeProvider";
import Widget from "../components/Widget";
import MonthCalendar, { type CalendarEvent } from "../components/MonthCalendar";
import { authApi } from "../app/auth";
import { getDashboardOverview, type DashboardAnalyticsRing, type DashboardOverview } from "../api/dashboard";
import "bootstrap/dist/css/bootstrap.min.css";
import "../styles/dashboard/index.css";

const AUTO_REFRESH_MS = 45_000;

const EMPTY_OVERVIEW: DashboardOverview = {
    todaySummary: { appointmentsToday: 0, newClientsThisWeek: 0, noShows: 0, completedVisitsToday: 0 },
    upcomingSoon: [],
    analyticsSummary: { rings: [], isPlaceholder: true },
    accountingPreview: { revenueThisMonth: 0, payrollThisMonth: null, pendingAmount: null, formsConfiguredCount: null, statusNote: "Accounting data not configured yet." },
    staffSummary: { total: 0, inService: 0, onBreak: 0, available: 0, members: [] },
    calendarCounts: [],
    recentClients: [],
    clientSegments: [],
    reviewsSummary: { averageRating: 0, recentCount: 0, previousPeriodAverage: 0, trend: "flat" },
};

function AnalyticsRings({ rings, isPlaceholder }: { rings: DashboardAnalyticsRing[]; isPlaceholder: boolean }) {
    const normalizedRings = rings.length > 0 ? rings : [
        { key: "placeholder-1", label: "Appointments completed", valuePercent: 0, currentValue: 0, targetValue: 0, description: "No completed visits yet" },
        { key: "placeholder-2", label: "Returning clients", valuePercent: 0, currentValue: 0, targetValue: 0, description: "No returning client history yet" },
        { key: "placeholder-3", label: "Attendance health", valuePercent: 0, currentValue: 0, targetValue: 0, description: "No attendance data yet" },
    ];

    const ringColors = ["#ff4f8b", "#6f7dff", "#3bc9a1"];
    const baseRadius = 42;
    const stroke = 8;

    return (
        <div className="nx-analytics-wrap">
            <div className={`nx-rings ${isPlaceholder ? "is-placeholder" : ""}`}>
                <svg viewBox="0 0 120 120" role="img" aria-label="Analytics progress rings">
                    {normalizedRings.slice(0, 3).map((ring, index) => {
                        const radius = baseRadius - index * 11;
                        const circumference = 2 * Math.PI * radius;
                        const pct = Math.max(0, Math.min(100, ring.valuePercent));
                        const dashoffset = circumference - (pct / 100) * circumference;

                        return (
                            <g key={ring.key}>
                                <circle className="nx-ring-track" cx="60" cy="60" r={radius} strokeWidth={stroke} />
                                <circle
                                    className="nx-ring-progress"
                                    cx="60"
                                    cy="60"
                                    r={radius}
                                    strokeWidth={stroke}
                                    style={{
                                        stroke: ringColors[index] ?? "#8d95a6",
                                        strokeDasharray: circumference,
                                        strokeDashoffset: dashoffset,
                                    }}
                                />
                            </g>
                        );
                    })}
                </svg>
            </div>

            <ul className="nx-analytics-legend">
                {normalizedRings.slice(0, 3).map((ring, index) => (
                    <li key={ring.key}>
                        <span className="nx-dot" style={{ background: ringColors[index] ?? "#8d95a6" }} />
                        <div>
                            <strong>{ring.label} — {ring.valuePercent}%</strong>
                            <small>{ring.description}</small>
                        </div>
                    </li>
                ))}
            </ul>
        </div>
    );
}

export default function Dashboard() {
    const navigate = useNavigate();
    const [overview, setOverview] = useState<DashboardOverview>(EMPTY_OVERVIEW);
    const [isInitialLoading, setIsInitialLoading] = useState(true);
    const [isRefreshing, setIsRefreshing] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const mountedRef = useRef(true);

    const fetchOverview = useCallback(async (mode: "initial" | "refresh") => {
        if (mode === "refresh") setIsRefreshing(true);

        try {
            const nextOverview = await getDashboardOverview();
            if (!mountedRef.current) return;
            setOverview(nextOverview);
            setError(null);
        } catch (err) {
            if (!mountedRef.current) return;
            console.error("Failed to load dashboard overview", err);
            setError("Live updates are temporarily unavailable.");
            if (mode === "initial") setOverview(EMPTY_OVERVIEW);
        } finally {
            if (mountedRef.current) {
                if (mode === "initial") {
                    setIsInitialLoading(false);
                } else {
                    setIsRefreshing(false);
                }
            }
        }
    }, []);

    useEffect(() => {
        mountedRef.current = true;
        void fetchOverview("initial");

        const intervalId = window.setInterval(() => void fetchOverview("refresh"), AUTO_REFRESH_MS);
        const handleFocus = () => {
            if (document.visibilityState === "visible") void fetchOverview("refresh");
        };

        window.addEventListener("focus", handleFocus);
        document.addEventListener("visibilitychange", handleFocus);

        return () => {
            mountedRef.current = false;
            window.clearInterval(intervalId);
            window.removeEventListener("focus", handleFocus);
            document.removeEventListener("visibilitychange", handleFocus);
        };
    }, [fetchOverview]);

    const calendarEvents = useMemo<CalendarEvent[]>(() => (
        overview.calendarCounts.flatMap((entry) =>
            Array.from({ length: entry.count }).map((_, idx) => ({
                date: entry.date,
                title: `Appointment ${idx + 1}`,
                start: `${String(9 + (idx % 8)).padStart(2, "0")}:00`,
                end: `${String(10 + (idx % 8)).padStart(2, "0")}:00`,
            })))
    ), [overview.calendarCounts]);

    const handleLogout = () => {
        authApi.logout();
        navigate("/auth", { replace: true });
    };

    return (
        <ThemeProvider>
            <Header breadcrumb="Dashboard" onLogout={handleLogout} />

            <main className="fx-page">
                <section className="fx-row fx-top">
                    <div className="fx-quarter">
                        <Widget title="Today" footer="Operations" minH={132} onClick={() => navigate("/calendar")}>
                            {isInitialLoading ? <div className="nx-skeleton nx-skeleton-compact" /> : (
                                <>
                                    <ul className="nx-list">
                                        <li>Appointments: {overview.todaySummary.appointmentsToday}</li>
                                        <li>New clients (week): {overview.todaySummary.newClientsThisWeek}</li>
                                        <li>No-shows: {overview.todaySummary.noShows}</li>
                                        <li>Completed visits: {overview.todaySummary.completedVisitsToday}</li>
                                    </ul>
                                    <div className="nx-split-head">Next 2 hours</div>
                                    {overview.upcomingSoon.length === 0 ? (
                                        <span className="nx-subtle">No appointments in the next 2 hours.</span>
                                    ) : (
                                        <ul className="nx-list nx-list-clickable">
                                            {overview.upcomingSoon.slice(0, 3).map((item) => (
                                                <li key={item.id}>{item.startTime} — {item.clientName}</li>
                                            ))}
                                        </ul>
                                    )}
                                </>
                            )}
                        </Widget>
                    </div>

                    <div className="fx-quarter">
                        <Widget title="Analytics" footer="Progress rings" minH={132} onClick={() => navigate("/analytics")}>
                            {isInitialLoading ? <div className="nx-skeleton nx-skeleton-compact" /> : (
                                <AnalyticsRings rings={overview.analyticsSummary.rings} isPlaceholder={overview.analyticsSummary.isPlaceholder} />
                            )}
                        </Widget>
                    </div>

                    <div className="fx-quarter">
                        <Widget title="Accounting" footer="Bookkeeping" minH={132} onClick={() => navigate("/accounting")}>
                            {isInitialLoading ? <div className="nx-skeleton nx-skeleton-compact" /> : (
                                <>
                                    <ul className="nx-list">
                                        <li>Revenue this month: ${overview.accountingPreview.revenueThisMonth.toLocaleString()}</li>
                                        <li>Payroll: {overview.accountingPreview.payrollThisMonth == null ? "Not configured" : `$${overview.accountingPreview.payrollThisMonth.toLocaleString()}`}</li>
                                        <li>Pending amount: {overview.accountingPreview.pendingAmount == null ? "Unavailable" : `$${overview.accountingPreview.pendingAmount.toLocaleString()}`}</li>
                                        <li>Forms: {overview.accountingPreview.formsConfiguredCount == null ? "Not configured" : overview.accountingPreview.formsConfiguredCount}</li>
                                    </ul>
                                    <span className="nx-subtle">{overview.accountingPreview.statusNote}</span>
                                </>
                            )}
                        </Widget>
                    </div>

                    <div className="fx-quarter">
                        <Widget title="Staff" footer="Status" minH={132} onClick={() => navigate("/workers")}>
                            {isInitialLoading ? <div className="nx-skeleton nx-skeleton-compact" /> : (
                                <ul className="nx-list">
                                    <li>In service: {overview.staffSummary.inService}</li>
                                    <li>On break: {overview.staffSummary.onBreak}</li>
                                    <li>Available: {overview.staffSummary.available}</li>
                                </ul>
                            )}
                        </Widget>
                    </div>
                </section>

                <section className="fx-row fx-main">
                    <div className="fx-left">
                        <Widget minH={320}>
                            {isInitialLoading ? <div className="nx-skeleton nx-skeleton-calendar" /> : (
                                <MonthCalendar
                                    title="Calendar"
                                    events={calendarEvents}
                                    onAddEvent={() => navigate("/calendar")}
                                    onDaySelect={(date) => navigate(`/calendar?date=${date}`)}
                                />
                            )}
                        </Widget>
                    </div>

                    <aside className="fx-right">
                        <Widget title="Clients" footer="Overview" minH={220} onClick={() => navigate("/clients")}>
                            {isInitialLoading ? <div className="nx-skeleton" /> : (
                                <>
                                    {overview.recentClients.length === 0 ? (
                                        <div className="nx-empty-note">
                                            <strong>No clients yet.</strong>
                                            <span>Clients will appear here after you add them.</span>
                                            <span className="nx-subtle">Use the Clients page to create your first client.</span>
                                        </div>
                                    ) : (
                                        <>
                                            <div className="nx-split-head">Recent clients</div>
                                            <ul className="nx-list nx-list-clickable">
                                                {overview.recentClients.slice(0, 4).map((client) => (
                                                    <li key={client.id} onClick={(e) => { e.stopPropagation(); navigate("/clients"); }}>
                                                        <span>{client.name}</span>
                                                        <small className="nx-subtle">{client.createdAt}</small>
                                                    </li>
                                                ))}
                                            </ul>

                                            <div className="nx-split-head">Segments</div>
                                            {overview.clientSegments.length === 0 ? (
                                                <ul className="nx-list">
                                                    <li>Total clients: {overview.recentClients.length}</li>
                                                    <li>New this week: {overview.todaySummary.newClientsThisWeek}</li>
                                                </ul>
                                            ) : (
                                                <ul className="nx-list nx-list-clickable">
                                                    {overview.clientSegments.slice(0, 4).map((segment) => (
                                                        <li key={segment.id} onClick={(e) => { e.stopPropagation(); navigate("/clients"); }}>
                                                            <span>{segment.name}</span>
                                                            <strong>{segment.count}</strong>
                                                        </li>
                                                    ))}
                                                </ul>
                                            )}
                                        </>
                                    )}
                                    <button
                                        type="button"
                                        className="nx-inline-link"
                                        onClick={(e) => {
                                            e.stopPropagation();
                                            navigate("/clients");
                                        }}
                                    >
                                        View all clients
                                    </button>
                                </>
                            )}
                        </Widget>

                        <Widget title="Reviews" footer="This week" minH={132} onClick={() => navigate("/reviews")}>
                            {isInitialLoading ? <div className="nx-skeleton nx-skeleton-compact" /> : overview.reviewsSummary.recentCount === 0 ? (
                                <span className="nx-subtle">No recent reviews yet.</span>
                            ) : (
                                <>
                                    <div className="nx-number">{overview.reviewsSummary.averageRating.toFixed(1)} ★</div>
                                    <span className="nx-subtle">{overview.reviewsSummary.recentCount} recent reviews</span>
                                </>
                            )}
                        </Widget>
                    </aside>
                </section>

                {(error || isRefreshing) && (
                    <section className="nx-page-status" aria-live="polite">
                        {error ?? "Refreshing dashboard data…"}
                    </section>
                )}
            </main>
        </ThemeProvider>
    );
}
