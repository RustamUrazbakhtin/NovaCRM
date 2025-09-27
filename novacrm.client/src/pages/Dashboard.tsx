import Header from "../layout/Header";
import ThemeProvider from "../providers/ThemeProvider";
import Widget from "../components/Widget";
import MonthCalendar from "../components/MonthCalendar";
import 'bootstrap/dist/css/bootstrap.min.css';
import "../styles/dashboard.css"; // можно оставить для внешнего вида виджетов (бордер, тени и т.п.)

export default function Dashboard() {
    const todayISO = new Date().toISOString().slice(0, 10);
    const tomorrowISO = new Date(Date.now() + 86400000).toISOString().slice(0, 10);

    const events = [
        { date: todayISO, title: "Haircut — Anna (12:00)" },
        { date: todayISO, title: "Nails — Kate (15:30)" },
        { date: tomorrowISO, title: "Coloring — Maria (11:00)" },
    ];

    const open = (s: string) => alert(s);

    return (
        <ThemeProvider>
            <Header
                onOpenAdmin={() => open("Admin")}
                onOpenSettings={() => open("Settings")}
                onOpenProfile={() => open("Profile")}
                onLogout={() => open("Sign out")}
            />

            {/* Контентная область: container-fluid на всю ширину, отступ под шапку */}
            <main className="fx-page container-fluid py-3 ">
                {/* TOP: четыре равных блока */}
                <div className="row g-3 mb-3">
                    <div className="col-sm-3">
                        <Widget title="Today (Salon)" footer="Overview">
                            <ul className="nx-list">
                                <li>Appointments: 18</li>
                                <li>New Clients: 3</li>
                                <li>No-shows: 1</li>
                            </ul>
                        </Widget>
                    </div>

                    <div className="col-sm-3 col-md-6 col-xl-3">
                        <Widget title="Next 2 hours" footer="Upcoming">
                            <ul className="nx-list" style={{ listStyle: "none", paddingLeft: 0 }}>
                                <li>12:00 — Haircut / Anna — 💇‍♀️</li>
                                <li>12:45 — Manicure / Julia — 💅</li>
                                <li>13:30 — Brows / Lina — 👁️</li>
                            </ul>
                        </Widget>
                    </div>

                    <div className="col-sm-3 col-md-6 col-xl-3">
                        <Widget title="Revenue" footer="This month">
                            <div className="nx-number">$ 18,240</div>
                        </Widget>
                    </div>

                    <div className="col-sm-3 col-md-6 col-xl-3">
                        <Widget title="Staff" footer="Status">
                            <ul className="nx-list">
                                <li>Olga — In service</li>
                                <li>Kate — Break</li>
                                <li>Maria — Available</li>
                            </ul>
                        </Widget>
                    </div>
                </div>

                {/* MAIN: слева календарь (3/4), справа стек (1/4) */}
                <div className="row g-3 max-w" style={{ minHeight: 'calc(100vh - 72px - 168px - 48px)' }}>
                    {/* LEFT (Calendar) */}
                    <div className="col-12 col-lg-9 d-flex">
                        <Widget title="Calendar" footer={new Date().toLocaleString(undefined, { month: "long", year: "numeric" })} className="w-100 d-flex flex-column">
                            {/* растягиваем календарь по высоте виджета */}
                            <div className="flex-grow-1 d-flex flex-column">
                                <MonthCalendar events={events} />
                            </div>
                        </Widget>
                    </div>
                    
                    {/* RIGHT (Stack) */}
                    <div className="col-12 col-lg-3 d-flex flex-column gap-3">
                        <Widget title="Tasks" footer="Today" className="flex-fill d-flex flex-column">
                            <ul className="nx-todos">
                                <li><input type="checkbox" defaultChecked /> Order hair dye</li>
                                <li><input type="checkbox" /> Call supplier</li>
                                <li><input type="checkbox" /> IG promo post</li>
                            </ul>
                        </Widget>

                        <Widget title="Inventory" footer="Low stock" className="flex-fill d-flex flex-column">
                            <ul className="nx-list">
                                <li>Shampoo #4 — 6 left</li>
                                <li>Nail base — 3 left</li>
                            </ul>
                        </Widget>

                        <Widget title="Reviews" footer="This week" className="flex-fill d-flex flex-column">
                            <div className="nx-number">4.8 ★</div>
                        </Widget>
                    </div>
                </div>
            </main>
        </ThemeProvider>
    );
}
