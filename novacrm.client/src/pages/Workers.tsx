import { useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import Header from "../layout/Header";
import ThemeProvider from "../providers/ThemeProvider";
import Widget from "../components/Widget";
import "../styles/dashboard/index.css";
import "../styles/dashboard/workers.css";
import { authApi } from "../app/auth";

type Status = "in-service" | "available" | "break" | "offline";

type Employee = {
    id: string;
    name: string;
    role: string;
    status: Status;
    avatar: string;
    rating: number;
    specialties: string[];
    nextAppointment: { client: string; service: string; time: string } | null;
    shift: string;
    email: string;
    phone: string;
    location: string;
    productivity: number;
    notes: string;
};

const EMPLOYEES: Employee[] = [
    {
        id: "anna-petrova",
        name: "Anna Petrova",
        role: "Lead Stylist",
        status: "in-service",
        avatar: "/team/anna.png",
        rating: 4.9,
        specialties: ["Balayage", "Cuts", "Event styling"],
        nextAppointment: { client: "Mira S.", service: "Bridal trial", time: "12:40" },
        shift: "09:00 – 18:00",
        email: "anna@novacrm.io",
        phone: "+7 (921) 555-18-42",
        location: "Studio 2",
        productivity: 88,
        notes: "Mentors junior stylists. Recently completed Keratin Pro certification.",
    },
    {
        id: "nikita-romanov",
        name: "Nikita Romanov",
        role: "Color Specialist",
        status: "available",
        avatar: "/team/nikita.png",
        rating: 4.7,
        specialties: ["Color correction", "Highlights"],
        nextAppointment: { client: "Alex K.", service: "Color refresh", time: "13:20" },
        shift: "10:00 – 19:00",
        email: "nikita@novacrm.io",
        phone: "+7 (911) 200-04-38",
        location: "Studio 3",
        productivity: 74,
        notes: "Prefers afternoon appointments. On track to complete balayage masterclass.",
    },
    {
        id: "sofia-ivanova",
        name: "Sofia Ivanova",
        role: "Nail Artist",
        status: "in-service",
        avatar: "/team/sofia.png",
        rating: 4.8,
        specialties: ["Gel art", "Spa treatments"],
        nextAppointment: { client: "Nika L.", service: "Signature manicure", time: "12:55" },
        shift: "08:00 – 16:00",
        email: "sofia@novacrm.io",
        phone: "+7 (903) 481-77-02",
        location: "Nail lounge",
        productivity: 92,
        notes: "Leads product ordering. Two VIP clients awaiting confirmations.",
    },
    {
        id: "daniil-zhukov",
        name: "Daniil Zhukov",
        role: "Massage Therapist",
        status: "break",
        avatar: "/team/daniil.png",
        rating: 4.6,
        specialties: ["Deep tissue", "Lymphatic", "Sports"],
        nextAppointment: { client: "Leon C.", service: "Sports massage", time: "14:15" },
        shift: "11:00 – 20:00",
        email: "daniil@novacrm.io",
        phone: "+7 (921) 111-67-39",
        location: "Wellness room",
        productivity: 68,
        notes: "Requested new oils for aromatherapy. Break scheduled until 12:30.",
    },
    {
        id: "alisa-kuznetsova",
        name: "Alisa Kuznetsova",
        role: "Brow & Lash Artist",
        status: "available",
        avatar: "/team/alisa.png",
        rating: 4.9,
        specialties: ["Lamination", "Architecting"],
        nextAppointment: null,
        shift: "09:00 – 17:00",
        email: "alisa@novacrm.io",
        phone: "+7 (931) 404-88-16",
        location: "Studio 1",
        productivity: 81,
        notes: "Open slot for premium lash lift. Pending follow-up with three leads.",
    },
    {
        id: "mia-ershova",
        name: "Mia Ershova",
        role: "Front Desk Manager",
        status: "offline",
        avatar: "/team/mia.png",
        rating: 4.5,
        specialties: ["Customer success", "Scheduling"],
        nextAppointment: null,
        shift: "Day off",
        email: "mia@novacrm.io",
        phone: "+7 (952) 210-34-55",
        location: "Reception",
        productivity: 0,
        notes: "Enjoys day off. Automatically rerouting calls to support desk.",
    },
];

const STATUS_LABELS: Record<Status, string> = {
    "in-service": "In service",
    available: "Available",
    break: "On break",
    offline: "Offline",
};

const STATUS_FILTERS: { id: "all" | Status; label: string }[] = [
    { id: "all", label: "All" },
    { id: "in-service", label: "In service" },
    { id: "available", label: "Available" },
    { id: "break", label: "Break" },
    { id: "offline", label: "Offline" },
];

export default function WorkersPage() {
    const navigate = useNavigate();
    const [search, setSearch] = useState("");
    const [statusFilter, setStatusFilter] = useState<(typeof STATUS_FILTERS)[number]["id"]>("all");
    const [roleFilter, setRoleFilter] = useState("all");

    const roles = useMemo(
        () => ["all", ...Array.from(new Set(EMPLOYEES.map(employee => employee.role)))],
        []
    );

    const filteredEmployees = useMemo(() => {
        return EMPLOYEES.filter(employee => {
            const matchesSearch = `${employee.name} ${employee.role} ${employee.specialties.join(" ")}`
                .toLowerCase()
                .includes(search.trim().toLowerCase());
            const matchesStatus = statusFilter === "all" || employee.status === statusFilter;
            const matchesRole = roleFilter === "all" || employee.role === roleFilter;
            return matchesSearch && matchesStatus && matchesRole;
        });
    }, [search, statusFilter, roleFilter]);

    const activeEmployees = EMPLOYEES.filter(employee => employee.status !== "offline").length;
    const occupiedEmployees = EMPLOYEES.filter(employee => employee.status === "in-service").length;
    const breaks = EMPLOYEES.filter(employee => employee.status === "break").length;
    const responseRate = Math.round(
        EMPLOYEES.filter(employee => employee.status === "available" || employee.status === "in-service").length /
        Math.max(1, EMPLOYEES.length) *
        100
    );

    const handleLogout = () => {
        authApi.logout();
        navigate("/auth", { replace: true });
    };

    return (
        <ThemeProvider>
            <Header
                breadcrumb="Team"
                onOpenAdmin={() => alert("Open admin")}
                onOpenSettings={() => alert("Open settings")}
                onOpenProfile={() => alert("Open profile")}
                onLogout={handleLogout}
            />
            <main className="fx-page workers-page">
                <section className="workers-hero">
                    <div className="hero-intro">
                        <h1>Team overview</h1>
                        <p>Track availability, load and wellbeing of every specialist in real time.</p>
                        <div className="hero-actions">
                            <button type="button" className="nx-btn primary" onClick={() => alert("Add worker")}>Add worker</button>
                            <button type="button" className="nx-btn ghost" onClick={() => alert("Generate schedule")}>Auto schedule</button>
                        </div>
                    </div>
                    <div className="hero-metrics">
                        <div className="metric-card accent">
                            <span className="metric-label">Active today</span>
                            <strong className="metric-value">{activeEmployees}</strong>
                            <span className="metric-sub">{EMPLOYEES.length} total staff</span>
                        </div>
                        <div className="metric-card">
                            <span className="metric-label">Booked right now</span>
                            <strong className="metric-value">{occupiedEmployees}</strong>
                            <span className="metric-sub">Appointments in progress</span>
                        </div>
                        <div className="metric-card">
                            <span className="metric-label">On break</span>
                            <strong className="metric-value">{breaks}</strong>
                            <span className="metric-sub">Scheduled pauses</span>
                        </div>
                        <div className="metric-card">
                            <span className="metric-label">Instant response rate</span>
                            <strong className="metric-value">{responseRate}%</strong>
                            <span className="metric-sub">Chats answered under 5 min</span>
                        </div>
                    </div>
                </section>

                <section className="workers-body">
                    <aside className="workers-sidebar">
                        <div className="sidebar-card">
                            <h3>Team health</h3>
                            <ul>
                                <li>
                                    <span>Client satisfaction</span>
                                    <span className="badge positive">4.8 ★</span>
                                </li>
                                <li>
                                    <span>Utilization</span>
                                    <span className="badge">72%</span>
                                </li>
                                <li>
                                    <span>Pending approvals</span>
                                    <span className="badge warning">6</span>
                                </li>
                                <li>
                                    <span>Upcoming birthdays</span>
                                    <span className="badge">2</span>
                                </li>
                            </ul>
                        </div>
                        <Widget title="Shift timeline" footer="Next 6 hours" minH={220}>
                            <ul className="timeline">
                                <li>
                                    <span className="time">12:30</span>
                                    <div>
                                        <strong>Sofia wraps VIP manicure</strong>
                                        <p>Prepare lounge for brow session</p>
                                    </div>
                                </li>
                                <li>
                                    <span className="time">13:15</span>
                                    <div>
                                        <strong>Nikita color refresh</strong>
                                        <p>Confirm toner delivery</p>
                                    </div>
                                </li>
                                <li>
                                    <span className="time">14:00</span>
                                    <div>
                                        <strong>Daniil sports massage</strong>
                                        <p>Room ready + oils preheated</p>
                                    </div>
                                </li>
                                <li>
                                    <span className="time">15:30</span>
                                    <div>
                                        <strong>Anna bridal trial</strong>
                                        <p>Send inspiration board follow-up</p>
                                    </div>
                                </li>
                            </ul>
                        </Widget>
                        <div className="sidebar-card">
                            <h3>Quick actions</h3>
                            <div className="quick-actions">
                                <button type="button" onClick={() => alert("Broadcast to team")}>Team broadcast</button>
                                <button type="button" onClick={() => alert("Export schedule")}>Export schedule</button>
                                <button type="button" onClick={() => alert("Open payroll")}>Payroll center</button>
                            </div>
                        </div>
                    </aside>

                    <div className="workers-content">
                        <div className="workers-toolbar">
                            <div className="search-group">
                                <label htmlFor="worker-search">Search</label>
                                <div className="search-input">
                                    <span aria-hidden>🔍</span>
                                    <input
                                        id="worker-search"
                                        type="search"
                                        placeholder="Name, role or specialty"
                                        value={search}
                                        onChange={event => setSearch(event.target.value)}
                                    />
                                </div>
                            </div>
                            <div className="toolbar-right">
                                <div className="filter-group" role="group" aria-label="Status filter">
                                    {STATUS_FILTERS.map(filter => (
                                        <button
                                            key={filter.id}
                                            type="button"
                                            className={`pill ${statusFilter === filter.id ? "is-active" : ""}`}
                                            onClick={() => setStatusFilter(filter.id)}
                                        >
                                            {filter.label}
                                        </button>
                                    ))}
                                </div>
                                <div className="select-group">
                                    <label htmlFor="role-filter">Role</label>
                                    <select
                                        id="role-filter"
                                        value={roleFilter}
                                        onChange={event => setRoleFilter(event.target.value)}
                                    >
                                        {roles.map(role => (
                                            <option key={role} value={role}>
                                                {role === "all" ? "All roles" : role}
                                            </option>
                                        ))}
                                    </select>
                                </div>
                            </div>
                        </div>

                        <div className="workers-grid" role="list">
                            {filteredEmployees.map(employee => (
                                <article key={employee.id} className="worker-card" role="listitem">
                                    <header className="worker-head">
                                        <div className="worker-avatar" aria-hidden>
                                            <img src={employee.avatar} alt="" onError={event => {
                                                (event.currentTarget as HTMLImageElement).style.display = "none";
                                            }} />
                                            <span className={`status-dot ${employee.status}`} aria-hidden />
                                        </div>
                                        <div>
                                            <h2>{employee.name}</h2>
                                            <p>{employee.role}</p>
                                        </div>
                                        <span className={`status-pill ${employee.status}`}>{STATUS_LABELS[employee.status]}</span>
                                    </header>

                                    <div className="worker-meta">
                                        <div>
                                            <span className="meta-label">Rating</span>
                                            <span className="meta-value">{employee.rating.toFixed(1)} ★</span>
                                        </div>
                                        <div>
                                            <span className="meta-label">Shift</span>
                                            <span className="meta-value">{employee.shift}</span>
                                        </div>
                                        <div>
                                            <span className="meta-label">Location</span>
                                            <span className="meta-value">{employee.location}</span>
                                        </div>
                                    </div>

                                    <div className="worker-specialties">
                                        {employee.specialties.map(skill => (
                                            <span key={skill}>{skill}</span>
                                        ))}
                                    </div>

                                    <div className="worker-body">
                                        <div className="worker-appointment">
                                            <h3>Next appointment</h3>
                                            {employee.nextAppointment ? (
                                                <p>
                                                    <strong>{employee.nextAppointment.time}</strong>
                                                    <span>{employee.nextAppointment.service}</span>
                                                    <span>{employee.nextAppointment.client}</span>
                                                </p>
                                            ) : (
                                                <p className="muted">No bookings — suggest leads</p>
                                            )}
                                        </div>
                                        <div className="worker-productivity">
                                            <h3>Productivity</h3>
                                            <div className="progress">
                                                <span style={{ width: `${employee.productivity}%` }} />
                                            </div>
                                            <p>{employee.productivity}% of daily target</p>
                                        </div>
                                    </div>

                                    <footer className="worker-footer">
                                        <div className="contact">
                                            <span>{employee.phone}</span>
                                            <span>{employee.email}</span>
                                        </div>
                                        <div className="actions">
                                            <button type="button" onClick={() => alert(`Open profile: ${employee.name}`)}>Profile</button>
                                            <button type="button" onClick={() => alert(`Assign task to ${employee.name}`)}>Assign task</button>
                                        </div>
                                    </footer>

                                    <p className="worker-notes">{employee.notes}</p>
                                </article>
                            ))}

                            {filteredEmployees.length === 0 && (
                                <div className="empty-state">
                                    <h2>No workers found</h2>
                                    <p>Try another search term or clear filters to see the full team list.</p>
                                    <button type="button" onClick={() => { setSearch(""); setRoleFilter("all"); setStatusFilter("all"); }}>
                                        Reset filters
                                    </button>
                                </div>
                            )}
                        </div>
                    </div>
                </section>
            </main>
        </ThemeProvider>
    );
}
