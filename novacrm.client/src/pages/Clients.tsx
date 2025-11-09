import { type KeyboardEvent, useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import Header from "../layout/Header";
import ThemeProvider from "../providers/ThemeProvider";
import "../styles/dashboard/index.css";
import "../styles/clients/index.css";
import { authApi } from "../app/auth";

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

const formatCurrency = (value: number) =>
    new Intl.NumberFormat("ru-RU", { style: "currency", currency: "RUB", maximumFractionDigits: 0 }).format(value);

export default function Clients() {
    const navigate = useNavigate();
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
                phone: "+7 (926) 904-33-80",
                email: "leo.volk@example.com",
                segment: "Постоянные",
                status: "Лояльный",
                tags: ["Massage", "Gift"],
                lastVisit: "2 октября",
                lifetimeValue: 72000,
                totalVisits: 8,
                satisfaction: 4.8,
                city: "Красногорск",
                master: "Мия Т.",
                notes: "Любит вечерние сеансы, часто покупает подарочные сертификаты.",
                communications: [
                    { id: "c-10", type: "visit", channel: "Spa", summary: "Детокс-массаж", time: "2 окт, 20:00" },
                    { id: "c-11", type: "message", channel: "Email", summary: "Подбор ухода домой", time: "30 сен, 09:15" },
                ],
                tasks: [{ id: "t-07", title: "Отправить подборку масел", due: "Сегодня", completed: false }],
            },
        ],
        []
    );

    const segments = useMemo(() => {
        const unique = Array.from(new Set(clients.map((client) => client.segment)));
        return ["Все", ...unique];
    }, [clients]);

    const [segment, setSegment] = useState<string>("Все");
    const [search, setSearch] = useState<string>("");
    const [selectedId, setSelectedId] = useState<string | null>(clients[0]?.id ?? null);

    const filteredClients = useMemo(() => {
        const normalized = search.trim().toLowerCase();

        return clients.filter((client) => {
            const matchesSegment = segment === "Все" || client.segment === segment;
            const matchesSearch =
                normalized.length === 0 ||
                [client.name, client.phone, client.email].some((field) => field.toLowerCase().includes(normalized));

            return matchesSegment && matchesSearch;
        });
    }, [clients, search, segment]);

    useEffect(() => {
        if (!filteredClients.length) {
            if (selectedId !== null) {
                setSelectedId(null);
            }
            return;
        }

        const exists = selectedId ? filteredClients.some((client) => client.id === selectedId) : false;

        if (!exists) {
            setSelectedId(filteredClients[0].id);
        }
    }, [filteredClients, selectedId]);

    const selectedClient = filteredClients.find((client) => client.id === selectedId) ?? null;

    const summary = useMemo(() => {
        const total = clients.length;
        const returning = clients.filter((client) => client.totalVisits > 1).length;
        const averageLTV =
            total > 0 ? Math.round(clients.reduce((acc, client) => acc + client.lifetimeValue, 0) / total) : 0;
        const satisfaction = total > 0 ? (clients.reduce((acc, client) => acc + client.satisfaction, 0) / total).toFixed(1) : "0";

        return { total, returning, averageLTV, satisfaction };
    }, [clients]);

    const open = (label: string) => alert(label);

    const handleLogout = () => {
        authApi.logout();
        navigate("/auth", { replace: true });
    };

    return (
        <ThemeProvider>
            <Header breadcrumb="Клиенты" onLogout={handleLogout} />

            <main className="clients-page">
                <section className="clients-toolbar">
                    <div className="clients-heading">
                        <h1>Клиенты</h1>
                        <p>Все данные о гостях в одном месте: история посещений, задачи и активность.</p>
                    </div>
                    <button type="button" className="clients-add" onClick={() => open("Add client")}>Добавить клиента</button>
                </section>

                <section className="clients-metrics" aria-label="Сводка по клиентам">
                    <article className="clients-metric-card">
                        <span className="clients-metric-label">Всего</span>
                        <strong className="clients-metric-value">{summary.total}</strong>
                        <span className="clients-metric-hint">Клиентов в базе</span>
                    </article>
                    <article className="clients-metric-card">
                        <span className="clients-metric-label">Повторные</span>
                        <strong className="clients-metric-value">{summary.returning}</strong>
                        <span className="clients-metric-hint">Ходят чаще одного раза</span>
                    </article>
                    <article className="clients-metric-card">
                        <span className="clients-metric-label">Средний LTV</span>
                        <strong className="clients-metric-value">{formatCurrency(summary.averageLTV)}</strong>
                        <span className="clients-metric-hint">Средний доход с клиента</span>
                    </article>
                    <article className="clients-metric-card">
                        <span className="clients-metric-label">Удовлетворённость</span>
                        <strong className="clients-metric-value">{summary.satisfaction}</strong>
                        <span className="clients-metric-hint">Средняя оценка сервиса</span>
                    </article>
                </section>

                <section className="clients-content">
                    <section className="clients-table-panel" aria-label="Список клиентов">
                        <div className="clients-table-wrapper">
                            <table className="clients-table" aria-label="Список клиентов с фильтрами и поиском">
                                <thead>
                                    <tr className="clients-table-controls">
                                        <th colSpan={5}>
                                            <div className="clients-table-tools">
                                                <input
                                                    type="search"
                                                    className="clients-search"
                                                    placeholder="Поиск по имени, телефону или email"
                                                    value={search}
                                                    onChange={(event) => setSearch(event.target.value)}
                                                />
                                                <div className="clients-segments" role="tablist" aria-label="Сегменты клиентов">
                                                    {segments.map((item) => (
                                                        <button
                                                            key={item}
                                                            type="button"
                                                            role="tab"
                                                            aria-selected={segment === item}
                                                            className={`clients-segment${segment === item ? " is-active" : ""}`}
                                                            onClick={() => setSegment(item)}
                                                        >
                                                            {item}
                                                        </button>
                                                    ))}
                                                </div>
                                            </div>
                                        </th>
                                    </tr>
                                    <tr className="clients-table-head">
                                        <th scope="col">Клиент</th>
                                        <th scope="col">Сегмент</th>
                                        <th scope="col">Статус</th>
                                        <th scope="col">Последний визит</th>
                                        <th scope="col">Следующий визит</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {filteredClients.length > 0 ? (
                                        filteredClients.map((client) => {
                                            const isActive = selectedId === client.id;

                                            const handleKeyDown = (event: KeyboardEvent<HTMLTableRowElement>) => {
                                                if (event.key === "Enter" || event.key === " ") {
                                                    event.preventDefault();
                                                    setSelectedId(client.id);
                                                }
                                            };

                                            return (
                                                <tr
                                                    key={client.id}
                                                    className={`clients-table-row${isActive ? " is-active" : ""}`}
                                                    onClick={() => setSelectedId(client.id)}
                                                    tabIndex={0}
                                                    aria-selected={isActive}
                                                    onKeyDown={handleKeyDown}
                                                >
                                                    <td data-title="Клиент">
                                                        <div className="clients-table-primary">
                                                            <div className="clients-client-heading">
                                                                <span className="clients-client-name">{client.name}</span>
                                                            </div>
                                                            <div className="clients-client-meta">
                                                                <span>{client.phone}</span>
                                                                <span>{client.email}</span>
                                                            </div>
                                                            {client.tags.length > 0 && (
                                                                <div className="clients-client-tags">
                                                                    {client.tags.map((tag) => (
                                                                        <span key={tag}>{tag}</span>
                                                                    ))}
                                                                </div>
                                                            )}
                                                        </div>
                                                    </td>
                                                    <td data-title="Сегмент">{client.segment}</td>
                                                    <td data-title="Статус">
                                                        <span className={`clients-status clients-status--${client.status.toLowerCase()}`}>
                                                            {client.status}
                                                        </span>
                                                    </td>
                                                    <td data-title="Последний визит">{client.lastVisit}</td>
                                                    <td data-title="Следующий визит">{client.nextVisit ?? "—"}</td>
                                                </tr>
                                            );
                                        })
                                    ) : (
                                        <tr className="clients-table-empty">
                                            <td colSpan={5}>
                                                <div className="clients-empty" role="status">
                                                    <p>Ничего не найдено. Попробуйте другой запрос.</p>
                                                </div>
                                            </td>
                                        </tr>
                                    )}
                                </tbody>
                            </table>
                        </div>
                    </section>

                    <section className="clients-detail-panel" aria-live="polite">
                        {selectedClient ? (
                            <article className="clients-detail-card">
                                <header className="clients-detail-header">
                                    <div className="clients-detail-title">
                                        <h2>{selectedClient.name}</h2>
                                        <span className={`clients-status clients-status--${selectedClient.status.toLowerCase()}`}>
                                            {selectedClient.status}
                                        </span>
                                    </div>
                                    <p>{selectedClient.city} · Персональный мастер: {selectedClient.master}</p>
                                </header>

                                <div className="clients-detail-grid">
                                    <div>
                                        <span className="clients-detail-label">LTV</span>
                                        <strong>{formatCurrency(selectedClient.lifetimeValue)}</strong>
                                    </div>
                                    <div>
                                        <span className="clients-detail-label">Визитов</span>
                                        <strong>{selectedClient.totalVisits}</strong>
                                    </div>
                                    <div>
                                        <span className="clients-detail-label">Оценка</span>
                                        <strong>{selectedClient.satisfaction.toFixed(1)}</strong>
                                    </div>
                                </div>

                                <section className="clients-detail-section">
                                    <h3>Контакты</h3>
                                    <div className="clients-detail-contacts">
                                        <span>{selectedClient.phone}</span>
                                        <span>{selectedClient.email}</span>
                                    </div>
                                </section>

                                <section className="clients-detail-section">
                                    <h3>Активность</h3>
                                    <ul className="clients-timeline">
                                        {selectedClient.communications.map((item) => (
                                            <li key={item.id}>
                                                <span className="clients-timeline-time">{item.time}</span>
                                                <div>
                                                    <strong>{item.channel}</strong>
                                                    <p>{item.summary}</p>
                                                </div>
                                            </li>
                                        ))}
                                    </ul>
                                </section>

                                <section className="clients-detail-section">
                                    <h3>Задачи</h3>
                                    <ul className="clients-tasks">
                                        {selectedClient.tasks.map((task) => (
                                            <li key={task.id}>
                                                <input type="checkbox" checked={task.completed} readOnly />
                                                <div>
                                                    <span>{task.title}</span>
                                                    <small>{task.due}</small>
                                                </div>
                                            </li>
                                        ))}
                                    </ul>
                                </section>

                                <section className="clients-detail-section">
                                    <h3>Заметки</h3>
                                    <p className="clients-detail-notes">{selectedClient.notes}</p>
                                </section>
                            </article>
                        ) : (
                            <div className="clients-empty" role="status">
                                <p>Выберите клиента, чтобы увидеть детали.</p>
                            </div>
                        )}
                    </section>
                </section>
            </main>
        </ThemeProvider>
    );
}
