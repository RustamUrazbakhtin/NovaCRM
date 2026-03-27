import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import Header from "../layout/Header";
import ThemeProvider from "../providers/ThemeProvider";
import Widget from "../components/Widget";
import MonthCalendar from "../components/MonthCalendar";
import { authApi } from "../app/auth";
import { getClientsOverview, type ClientOverview } from "../api/clients";
import 'bootstrap/dist/css/bootstrap.min.css';
import "../styles/dashboard/index.css";

const MAX_VISIBLE_ITEMS = 3;

const renderLimitedList = (items: string[]) => {
    const visible = items.slice(0, MAX_VISIBLE_ITEMS);
    const shouldClamp = items.length > MAX_VISIBLE_ITEMS;

    return (
        <ul className="nx-list" title={shouldClamp ? items.join("\n") : undefined}>
            {visible.map((item, index) => (
                <li key={index}>{item}</li>
            ))}
        </ul>
    );
};

export default function Dashboard() {
    const navigate = useNavigate();
    const [overview, setOverview] = useState<ClientOverview>({ totalClients: 0, returning: 0, averageLtv: 0, satisfaction: 0 });

    useEffect(() => {
        getClientsOverview().then(setOverview).catch((error) => {
            console.error("Failed to load dashboard overview", error);
            setOverview({ totalClients: 0, returning: 0, averageLtv: 0, satisfaction: 0 });
        });
    }, []);

    const salonOverview = useMemo(() => [
        `Total clients: ${overview.totalClients}`,
        `Returning clients: ${overview.returning}`,
        `Avg LTV: $${Math.round(overview.averageLtv)}`,
        `Satisfaction: ${overview.satisfaction.toFixed(1)}`,
    ], [overview]);

    const today = new Date();
    const todayISO = today.toISOString().slice(0, 10);
    const tomorrowISO = new Date(today.getTime() + 86400000).toISOString().slice(0, 10);

    const events = [
        { date: todayISO, title: "Haircut — Anna", start: "12:00", end: "12:45", master: "Alsu" },
        { date: todayISO, title: "Nails — Kate", start: "15:30", end: "16:30", master: "Julia" },
        { date: tomorrowISO, title: "Coloring — Maria", start: "11:00", end: "12:00", master: "Alsu" },
    ];

    const open = (s: string) => alert(s);

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
                        <Widget title="Today (Salon)" footer="Overview" minH={160} onClick={() => open("Today overview")}>
                            {renderLimitedList(salonOverview)}
                        </Widget>
                    </div>
                    <div className="fx-quarter">
                        <Widget title="Clients" footer="Core stats" minH={160} onClick={() => navigate("/clients")}>
                            {renderLimitedList([
                                `Total: ${overview.totalClients}`,
                                `Returning: ${overview.returning}`,
                                `Satisfaction: ${overview.satisfaction.toFixed(1)}`,
                            ])}
                        </Widget>
                    </div>
                    <div className="fx-quarter">
                        <Widget title="Revenue" footer="From client LTV" minH={160}>
                            <div className="nx-number">$ {Math.round(overview.averageLtv * Math.max(overview.totalClients, 1)).toLocaleString()}</div>
                            <span className="nx-subtle">Based on actual client records</span>
                        </Widget>
                    </div>
                    <div className="fx-quarter">
                        <Widget title="Staff" footer="Status" minH={160} href="/workers">
                            {renderLimitedList(["Use Workers page for details"])}
                        </Widget>
                    </div>
                </section>

                <section className="fx-row fx-main">
                    <div className="fx-left">
                        <Widget minH={360}>
                            <MonthCalendar title="Calendar" events={events} />
                        </Widget>
                    </div>
                </section>
            </main>
        </ThemeProvider>
    );
}
