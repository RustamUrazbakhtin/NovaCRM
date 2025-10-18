import { useEffect, useMemo, useState } from "react";
import Header from "../layout/Header";
import ThemeProvider from "../providers/ThemeProvider";
import "../styles/dashboard/index.css";
import "../styles/clients/index.css";

type CommunicationType = "visit" | "call" | "message" | "note";

type Communication = {
    id: string;
    type: CommunicationType;
    channel: string;
    summary: string;
    time: string;
};

type Task = {
    id: string;
    title: string;
    due: string;
    completed: boolean;
};

type Client = {
    id: string;
    name: string;
    avatar: string;
    phone: string;
    email: string;
    segment: string;
    status: "Лояльный" | "Новый" | "Риск" | "VIP";
    tags: string[];
    lastVisit: string;
    nextVisit?: string;
    lifetimeValue: number;
    totalVisits: number;
    satisfaction: number;
    city: string;
    master: string;
    notes: string;
    communications: Communication[];
    tasks: Task[];
};

type SummaryMetric = {
    id: string;
    label: string;
    value: string;
    delta: string;
    positive?: boolean;
};

const formatCurrency = (value: number) =>
    new Intl.NumberFormat("ru-RU", { style: "currency", currency: "RUB", maximumFractionDigits: 0 }).format(value);

export default function Clients() {
    const clients = useMemo<Client[]>(
        () => [
            {
                id: "cl-01",
                name: "Анна Петрова",
                avatar: "/avatars/client-01.png",
                phone: "+7 (916) 555-23-45",
                email: "anna.pet@example.com",
                segment: "VIP",
                status: "VIP",
                tags: ["Balayage", "Premium", "Lounge"],
                lastVisit: "28 сентября",
                nextVisit: "12 октября · 14:00",
                lifetimeValue: 182000,
                totalVisits: 24,
                satisfaction: 4.9,
                city: "Москва",
                master: "Алсу Х.",
                notes: "Любит тёплые оттенки, просит плейлист с lounge-музыкой. Всегда бронирует дневные слоты.",
                communications: [
                    { id: "c-01", type: "visit", channel: "Салон", summary: "Окрашивание balayage", time: "28 сен, 14:00" },
                    { id: "c-02", type: "message", channel: "WhatsApp", summary: "Подтверждение визита", time: "27 сен, 09:12" },
                    { id: "c-03", type: "note", channel: "Мастер", summary: "Рекомендовать уход K18", time: "27 сен, 15:40" },
                ],
                tasks: [
                    { id: "t-01", title: "Подготовить набор premium-ухода", due: "До визита", completed: false },
                    { id: "t-02", title: "Отправить опрос NPS", due: "Сегодня", completed: true },
                ],
            },
            {
                id: "cl-02",
                name: "Максим Орлов",
                avatar: "/avatars/client-02.png",
                phone: "+7 (903) 224-83-10",
                email: "max.orlov@example.com",
                segment: "Постоянные",
                status: "Лояльный",
                tags: ["Барбер", "Apple Pay"],
                lastVisit: "1 октября",
                nextVisit: "22 октября · 18:30",
                lifetimeValue: 96000,
                totalVisits: 14,
                satisfaction: 4.7,
                city: "Москва",
                master: "Даниил С.",
                notes: "Оценивает скорость записи, регулярно рекомендует друзьям.",
                communications: [
                    { id: "c-04", type: "call", channel: "Телефон", summary: "Перенос визита", time: "2 окт, 11:05" },
                    { id: "c-05", type: "visit", channel: "Барбершоп", summary: "Стрижка classic fade", time: "1 окт, 18:00" },
                ],
                tasks: [{ id: "t-03", title: "Отправить спасибо за рекомендацию", due: "Завтра", completed: false }],
            },
            {
                id: "cl-03",
                name: "Екатерина Мирная",
                avatar: "/avatars/client-03.png",
                phone: "+7 (905) 883-77-90",
                email: "katya.mir@example.com",
                segment: "Новые",
                status: "Новый",
                tags: ["Маникюр", "Spa"],
                lastVisit: "30 сентября",
                nextVisit: "18 октября · 12:00",
                lifetimeValue: 18000,
                totalVisits: 1,
                satisfaction: 4.5,
                city: "Химки",
                master: "Юлия Н.",
                notes: "Пришла с Instagram, интересуется spa-программами.",
                communications: [
                    { id: "c-06", type: "message", channel: "Direct", summary: "Запрос на первый визит", time: "25 сен, 20:18" },
                    { id: "c-07", type: "visit", channel: "Нейл-зал", summary: "Маникюр + покрытие", time: "30 сен, 13:30" },
                ],
                tasks: [{ id: "t-04", title: "Назначить welcome-бонус", due: "Сегодня", completed: false }],
            },
            {
                id: "cl-04",
                name: "Мария Ясина",
                avatar: "/avatars/client-04.png",
                phone: "+7 (921) 109-65-11",
                email: "maria.yas@example.com",
                segment: "Риск",
                status: "Риск",
                tags: ["Колоринг", "Кератин"],
                lastVisit: "14 июля",
                lifetimeValue: 112000,
                totalVisits: 11,
                satisfaction: 4.1,
                city: "Москва",
                master: "Алсу Х.",
                notes: "Пропустила две записи. Нужен персональный оффер.",
                communications: [
                    { id: "c-08", type: "message", channel: "SMS", summary: "Напоминание о визите", time: "15 сен, 10:00" },
                    { id: "c-09", type: "note", channel: "Администратор", summary: "Отменилa из-за отпуска", time: "10 авг, 09:35" },
                ],
                tasks: [
                    { id: "t-05", title: "Позвонить с персональным предложением", due: "Сегодня", completed: false },
                    { id: "t-06", title: "Добавить в кампанию \"Возврат\"", due: "Завтра", completed: false },
                ],
            },
            {
                id: "cl-05",
                name: "Леонид Волков",
                avatar: "/avatars/client-05.png",
                phone: "+7 (911) 340-21-18",
                email: "leon.volk@example.com",
                segment: "Корпоративные",
                status: "Лояльный",
                tags: ["Массаж", "Корп"],
                lastVisit: "18 мая",
                lifetimeValue: 54000,
                totalVisits: 6,
                satisfaction: 4.2,
                city: "Санкт-Петербург",
                master: "Мия Р.",
                notes: "Клиент корпоративной программы. Любит утренние часы.",
                communications: [
                    { id: "c-10", type: "call", channel: "Телефон", summary: "Статус корпоративного пакета", time: "1 июн, 12:40" },
                    { id: "c-11", type: "note", channel: "Администратор", summary: "Отправлен счёт за продление", time: "25 май, 09:20" },
                ],
                tasks: [{ id: "t-07", title: "Запросить обратную связь", due: "На этой неделе", completed: false }],
            },
        ],
        [],
    );

    const segments = useMemo(() => ["Все", "VIP", "Постоянные", "Новые", "Риск", "Корпоративные"], []);

    const summaryMetrics = useMemo<SummaryMetric[]>(
        () => [
            { id: "mrr", label: "LTV клиентов", value: "₽1.2M", delta: "+12%", positive: true },
            { id: "visits", label: "Записи в октябре", value: "86", delta: "+8%", positive: true },
            { id: "retention", label: "Удержание", value: "78%", delta: "-3%" },
            { id: "nps", label: "NPS", value: "64", delta: "+5", positive: true },
        ],
        [],
    );

    const [activeSegment, setActiveSegment] = useState<string>(segments[0]);
    const [query, setQuery] = useState("");

    const filtered = useMemo(() => {
        const normalized = query.trim().toLowerCase();
        return clients.filter(client => {
            const matchesSegment = activeSegment === "Все" || client.segment === activeSegment;
            if (!matchesSegment) return false;
            if (!normalized) return true;
            return [client.name, client.phone, client.email, client.tags.join(" ")]
                .join(" ")
                .toLowerCase()
                .includes(normalized);
        });
    }, [activeSegment, clients, query]);

    const [selectedId, setSelectedId] = useState<string>(clients[0]?.id ?? "");

    useEffect(() => {
        if (!filtered.length) {
            setSelectedId("");
            return;
        }

        if (!filtered.some(client => client.id === selectedId)) {
            setSelectedId(filtered[0].id);
        }
    }, [filtered, selectedId]);

    const selectedClient = filtered.find(client => client.id === selectedId) ?? filtered[0] ?? null;

    return (
        <ThemeProvider>
            <div className="clients-screen">
                <Header
                    breadcrumb="Клиенты"
                    onOpenAdmin={() => undefined}
                    onOpenProfile={() => undefined}
                    onOpenSettings={() => undefined}
                    onLogout={() => undefined}
                />

                <main className="clients-shell">
                    <section className="clients-hero">
                        <div className="hero-copy">
                            <h1>Клиенты</h1>
                            <p>Все данные клиентов, активности и задачи в одном месте. Управляйте качеством сервиса и удержанием.</p>
                        </div>
                        <div className="hero-metrics">
                            {summaryMetrics.map(metric => (
                                <article key={metric.id} className="metric-card">
                                    <span className="metric-label">{metric.label}</span>
                                    <span className="metric-value">{metric.value}</span>
                                    <span className={`metric-delta ${metric.positive ? "is-positive" : "is-negative"}`}>{metric.delta}</span>
                                </article>
                            ))}
                        </div>
                    </section>

                    <section className="clients-toolbar">
                        <div className="segment-chips" role="tablist" aria-label="Сегменты клиентов">
                            {segments.map(segment => (
                                <button
                                    key={segment}
                                    type="button"
                                    className={`segment-chip ${segment === activeSegment ? "is-active" : ""}`}
                                    onClick={() => setActiveSegment(segment)}
                                    role="tab"
                                    aria-selected={segment === activeSegment}
                                >
                                    {segment}
                                </button>
                            ))}
                        </div>

                        <div className="toolbar-actions">
                            <label className="search-field">
                                <span className="icon" aria-hidden="true">🔍</span>
                                <input
                                    type="search"
                                    placeholder="Поиск по имени, телефону или тегам"
                                    value={query}
                                    onChange={event => setQuery(event.target.value)}
                                />
                            </label>
                            <button type="button" className="toolbar-btn ghost">Фильтры</button>
                            <button type="button" className="toolbar-btn primary">Добавить клиента</button>
                        </div>
                    </section>

                    <section className="clients-content">
                        <div className="clients-list">
                            <header className="list-head">
                                <span>Клиент</span>
                                <span>Последний визит</span>
                                <span>Статус</span>
                                <span>Город</span>
                            </header>
                            <div className="list-scroll" role="list">
                                {filtered.length ? (
                                    filtered.map(client => {
                                        const isActive = client.id === selectedClient?.id;
                                        const statusClass = `status-${client.status.toLowerCase().replace(/\s+/g, "-")}`;
                                        return (
                                            <button
                                                key={client.id}
                                                type="button"
                                                className={`client-row ${isActive ? "is-active" : ""}`}
                                                onClick={() => setSelectedId(client.id)}
                                                role="listitem"
                                            >
                                                <div className="client-main">
                                                    <div className="client-avatar" aria-hidden="true">
                                                        <span>{client.name[0]}</span>
                                                    </div>
                                                    <div>
                                                        <p className="client-name">{client.name}</p>
                                                        <p className="client-tags">{client.tags.join(" • ")}</p>
                                                    </div>
                                                </div>
                                                <span className="client-last">{client.lastVisit}</span>
                                                <span className={`client-status ${statusClass}`}>{client.status}</span>
                                                <span className="client-city">{client.city}</span>
                                            </button>
                                        );
                                    })
                                ) : (
                                    <div className="empty-state">
                                        <h3>Нет клиентов в сегменте</h3>
                                        <p>Попробуйте изменить фильтры или добавить нового клиента.</p>
                                    </div>
                                )}
                            </div>
                        </div>

                        {selectedClient && (
                            <div className="client-details">
                                <section className="details-header">
                                    <div className="details-id">
                                        <div className="details-avatar" aria-hidden="true">
                                            <span>{selectedClient.name[0]}</span>
                                        </div>
                                        <div>
                                            <h2>{selectedClient.name}</h2>
                                            <p>{selectedClient.phone}</p>
                                            <p>{selectedClient.email}</p>
                                        </div>
                                    </div>
                                    <div className="details-chips">
                                        <span className={`status-pill status-${selectedClient.status
                                            .toLowerCase()
                                            .replace(/\s+/g, "-")}`}
                                        >
                                            {selectedClient.status}
                                        </span>
                                        <span className="status-pill secondary">{selectedClient.segment}</span>
                                    </div>
                                </section>

                                <section className="details-grid">
                                    <article className="detail-card">
                                        <h3>Следующий визит</h3>
                                        <p className="detail-value">{selectedClient.nextVisit ?? "—"}</p>
                                        <p className="detail-sub">Мастер: {selectedClient.master}</p>
                                    </article>
                                    <article className="detail-card">
                                        <h3>LTV</h3>
                                        <p className="detail-value">{formatCurrency(selectedClient.lifetimeValue)}</p>
                                        <p className="detail-sub">Всего визитов: {selectedClient.totalVisits}</p>
                                    </article>
                                    <article className="detail-card">
                                        <h3>Удовлетворённость</h3>
                                        <p className="detail-value">{selectedClient.satisfaction.toFixed(1)}</p>
                                        <p className="detail-sub">Последний опрос NPS 2 недели назад</p>
                                    </article>
                                    <article className="detail-card notes">
                                        <h3>Заметки</h3>
                                        <p>{selectedClient.notes}</p>
                                    </article>
                                </section>

                                <section className="details-split">
                                    <article className="timeline">
                                        <header>
                                            <h3>Активность</h3>
                                            <span>{selectedClient.communications.length} события</span>
                                        </header>
                                        <ol>
                                            {selectedClient.communications.map(item => (
                                                <li key={item.id} className={`timeline-item type-${item.type}`}>
                                                    <div className="timeline-marker" aria-hidden="true" />
                                                    <div className="timeline-content">
                                                        <span className="timeline-time">{item.time}</span>
                                                        <p className="timeline-summary">{item.summary}</p>
                                                        <span className="timeline-channel">{item.channel}</span>
                                                    </div>
                                                </li>
                                            ))}
                                        </ol>
                                    </article>

                                    <article className="tasks">
                                        <header>
                                            <h3>Задачи</h3>
                                            <button type="button" className="link-btn">
                                                Новая задача
                                            </button>
                                        </header>
                                        <ul>
                                            {selectedClient.tasks.map(task => (
                                                <li key={task.id} className={task.completed ? "is-done" : ""}>
                                                    <div className="task-check" aria-hidden="true">{task.completed ? "✓" : ""}</div>
                                                    <div>
                                                        <p>{task.title}</p>
                                                        <span>{task.due}</span>
                                                    </div>
                                                </li>
                                            ))}
                                        </ul>
                                    </article>
                                </section>
                            </div>
                        )}
                    </section>
                </main>
            </div>
        </ThemeProvider>
    );
}
