import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import Header from "../layout/Header";
import ThemeProvider from "../providers/ThemeProvider";
import Widget from "../components/Widget";
import MonthCalendar, { type CalendarEvent } from "../components/MonthCalendar";
import { authApi } from "../app/auth";
import { getDashboardOverview, type DashboardOverview } from "../api/dashboard";
import "bootstrap/dist/css/bootstrap.min.css";
import "../styles/dashboard/index.css";

const AUTO_REFRESH_MS = 45_000;

const EMPTY_OVERVIEW: DashboardOverview = {
    todaySummary: { appointmentsToday: 0, newClientsThisWeek: 0, noShows: 0, completedVisitsToday: 0 },
    upcomingAppointments: [],
    revenueSummary: { currentMonthRevenue: 0, previousMonthRevenue: 0, growthPercent: 0, trend: "flat" },
    staffSummary: { total: 0, inService: 0, onBreak: 0, available: 0, members: [] },
    calendarCounts: [],
    recentClients: [],
    clientSegments: [],
    reviewsSummary: { averageRating: 0, recentCount: 0, previousPeriodAverage: 0, trend: "flat" },
};

export default function Dashboard() {
    const navigate = useNavigate();
    const [overview, setOverview] = useState<DashboardOverview>(EMPTY_OVERVIEW);
    const [isInitialLoading, setIsInitialLoading] = useState(true);
    const [isRefreshing, setIsRefreshing] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const mountedRef = useRef(true);

    const fetchOverview = useCallback(async (mode: "initial" | "refresh") => {
        if (mode === "refresh") {
            setIsRefreshing(true);
        }

        try {
            const nextOverview = await getDashboardOverview();
            if (!mountedRef.current) return;
            setOverview(nextOverview);
            setError(null);
        } catch (err) {
            if (!mountedRef.current) return;
            console.error("Failed to load dashboard overview", err);
            setError("Live updates are temporarily unavailable.");

            if (mode === "initial") {
                setOverview(EMPTY_OVERVIEW);
            }
        } finally {
            if (!mountedRef.current) return;
            if (mode === "initial") {
                setIsInitialLoading(false);
            } else {
                setIsRefreshing(false);
            }
        }
    }, []);

    useEffect(() => {
        mountedRef.current = true;
        void fetchOverview("initial");

        const intervalId = window.setInterval(() => {
            void fetchOverview("refresh");
        }, AUTO_REFRESH_MS);

        const handleFocus = () => {
            void fetchOverview("refresh");
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

    const trendClass = `nx-trend nx-trend-${overview.revenueSummary.trend}`;

    return (
        <ThemeProvider>
            <Header breadcrumb="Dashboard" onLogout={handleLogout} />

            <main className="fx-page">
                <section className="fx-row fx-top">
                    <div className="fx-quarter">
                        <Widget title="Today (Salon)" footer="Overview" minH={132} onClick={() => navigate("/calendar")}>
                            {isInitialLoading ? <div className="nx-skeleton nx-skeleton-compact" /> : (
                                <ul className="nx-list nx-list-clickable">
                                    <li>Appointments: {overview.todaySummary.appointmentsToday}</li>
                                    <li>New clients (week): {overview.todaySummary.newClientsThisWeek}</li>
                                    <li>No-shows: {overview.todaySummary.noShows}</li>
                                    <li>Completed visits: {overview.todaySummary.completedVisitsToday}</li>
                                </ul>
                            )}
                            {!isInitialLoading && overview.todaySummary.appointmentsToday === 0 && <span className="nx-subtle">No appointments scheduled for today.</span>}
                        </Widget>
                    </div>

                    <div className="fx-quarter">
                        <Widget title="Next 2 hours" footer="Upcoming" minH={132} onClick={() => navigate("/calendar")}>
                            {isInitialLoading ? <div className="nx-skeleton nx-skeleton-compact" /> : overview.upcomingAppointments.length === 0 ? (
                                <span className="nx-subtle">No upcoming appointments in the next 2 hours.</span>
                            ) : (
                                <ul className="nx-list nx-list-clickable">
                                    {overview.upcomingAppointments.slice(0, 4).map((item) => (
                                        <li key={item.id}>{item.startTime} — {item.clientName}</li>
                                    ))}
                                </ul>
                            )}
                        </Widget>
                    </div>

                    <div className="fx-quarter">
                        <Widget title="Revenue" footer="This month" minH={132} onClick={() => navigate("/analytics")}>
                            {isInitialLoading ? <div className="nx-skeleton nx-skeleton-compact" /> : (
                                <>
                                    <div className="nx-number">${overview.revenueSummary.currentMonthRevenue.toLocaleString()}</div>
                                    <span className={trendClass}>{overview.revenueSummary.growthPercent.toFixed(1)}% vs previous month</span>
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
