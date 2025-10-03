import Header from "../layout/Header";
import ThemeProvider from "../providers/ThemeProvider";
import Widget from "../components/Widget";
import MonthCalendar from "../components/MonthCalendar";
import 'bootstrap/dist/css/bootstrap.min.css';
import "../styles/dashboard/index.css"; // базовая тема, сетка и виджеты дашборда

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
    const today = new Date();
    const todayISO = today.toISOString().slice(0, 10);
    const tomorrowISO = new Date(today.getTime() + 86400000).toISOString().slice(0, 10);

    const events = [
        { date: todayISO, title: "Haircut — Anna", start: "12:00", end: "12:45", master: "Alsu" },
        { date: todayISO, title: "Nails — Kate", start: "15:30", end: "16:30", master: "Julia" },
        { date: todayISO, title: "Brows — Lina", start: "17:30", end: "18:15", master: "Mia" },
        { date: todayISO, title: "Balayage — Mia", start: "18:15", end: "19:30", master: "Aigul" },
        { date: tomorrowISO, title: "Coloring — Maria", start: "11:00", end: "12:00", master: "Alsu" },
        { date: tomorrowISO, title: "Massage — Leo", start: "16:00", end: "17:00", master: "Mia" },
    ];

    const open = (s: string) => alert(s);

    const salonOverview = [
        "Appointments: 18",
        "New Clients: 3",
        "No-shows: 1",
        "Walk-ins: 2",
    ];

    const upcomingHours = [
        "12:00 — Haircut / Anna",
        "12:45 — Manicure / Julia",
        "13:30 — Brows / Lina",
        "14:15 — Color fix / Sofia",
    ];

    const staffStatus = [
        "Olga — In service",
        "Kate — Break",
        "Maria — Available",
        "Daniel — Training",
    ];

    return (
        <ThemeProvider>
            <Header
                onOpenAdmin={() => open("Admin")}
                onOpenSettings={() => open("Settings")}
                onOpenProfile={() => open("Profile")}
                onLogout={() => open("Sign out")}
            />

            <main className="fx-page">
                <section className="fx-row fx-top">
                    <div className="fx-quarter">
                        <Widget
                            title="Today (Salon)"
                            footer="Overview"
                            minH={176}
                            onClick={() => open("Today overview")}
                        >
                            {renderLimitedList(salonOverview)}
                        </Widget>
                    </div>

                    <div className="fx-quarter">
                        <Widget
                            title="Next 2 hours"
                            footer="Upcoming"
                            minH={176}
                            onClick={() => open("Next 2 hours")}
                        >
                            {renderLimitedList(upcomingHours)}
                        </Widget>
                    </div>

                    <div className="fx-quarter">
                        <Widget
                            title="Revenue"
                            footer="This month"
                            minH={176}
                            onClick={() => open("Revenue")}
                        >
                            <div className="nx-number">$ 18,240</div>
                            <span className="nx-subtle">↑ 12% vs August</span>
                        </Widget>
                    </div>

                    <div className="fx-quarter">
                        <Widget
                            title="Staff"
                            footer="Status"
                            minH={176}
                            onClick={() => open("Staff status")}
                        >
                            {renderLimitedList(staffStatus)}
                        </Widget>
                    </div>
                </section>

                <section className="fx-row fx-main">
                    <div className="fx-left">
                        <Widget minH={420}>
                            <MonthCalendar title="Calendar" events={events} />
                        </Widget>
                    </div>

                    <div className="fx-right">
                        <Widget
                            title="Tasks"
                            footer="Today"
                            minH={140}
                            onClick={() => open("Tasks")}
                        >
                            <ul className="nx-todos">
                                <li><input type="checkbox" defaultChecked /> Order hair dye</li>
                                <li><input type="checkbox" /> Call supplier</li>
                                <li><input type="checkbox" /> IG promo post</li>
                            </ul>
                        </Widget>

                        <Widget
                            title="Inventory"
                            footer="Low stock"
                            minH={140}
                            onClick={() => open("Inventory")}
                        >
                            <ul className="nx-list">
                                <li>Shampoo #4 — 6 left</li>
                                <li>Nail base — 3 left</li>
                                <li>Serum — 5 left</li>
                            </ul>
                        </Widget>

                        <Widget
                            title="Reviews"
                            footer="This week"
                            minH={140}
                            onClick={() => open("Reviews")}
                        >
                            <div className="nx-number">4.8 ★</div>
                            <span className="nx-subtle">+32 new responses</span>
                        </Widget>
                    </div>
                </section>
            </main>
        </ThemeProvider>
    );
}
