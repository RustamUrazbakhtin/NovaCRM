import { useEffect, useMemo, useState } from "react";
import Header from "../layout/Header";
import ThemeProvider from "../providers/ThemeProvider";
import Widget from "../components/Widget";
import "../styles/dashboard/index.css";
import "../styles/clients/index.css";

type Communication = {
    id: string;
    type: "call" | "visit" | "message" | "note";
    channel: string;
    summary: string;
    time: string;
};

type ClientTask = {
    id: string;
    title: string;
    due: string;
    completed: boolean;
};

type ClientRecord = {
    id: string;
    name: string;
    phone: string;
    email: string;
    segment: "Active" | "New" | "VIP" | "Dormant" | "Lead";
    health: "Loyal" | "At Risk" | "Prospect" | "New";
    tags: string[];
    lastVisit: string;
    nextVisit?: string;
    lifetimeValue: number;
    upcoming: string;
    preferredMaster: string;
    totalVisits: number;
    satisfaction: number;
    notes: string;
    communications: Communication[];
    tasks: ClientTask[];
};

const clientsData: ClientRecord[] = [
    {
        id: "cl-01",
        name: "Анна Петрова",
        phone: "+7 (916) 555-23-45",
        email: "anna.pet@example.com",
        segment: "VIP",
        health: "Loyal",
        tags: ["Blond", "Premium", "Instagram"],
        lastVisit: "2023-09-28",
        nextVisit: "2023-10-12T14:00",
        lifetimeValue: 182000,
        upcoming: "Окрашивание balayage",
        preferredMaster: "Алсу Х.",
        totalVisits: 24,
        satisfaction: 4.9,
        notes: "Любит тёплые оттенки. Всегда бронирует дневные слоты, просит плейлист с lounge-музыкой.",
        communications: [
            { id: "c-01", type: "visit", channel: "Салон", summary: "Окрашивание balayage", time: "28 сен, 14:00" },
            { id: "c-02", type: "message", channel: "WhatsApp", summary: "Отправлено подтверждение визита", time: "27 сен, 09:12" },
            { id: "c-03", type: "note", channel: "Мастер Алсу", summary: "Рассмотреть уход K18", time: "27 сен, 15:40" },
        ],
        tasks: [
            { id: "t-01", title: "Подготовить набор premium-ухода", due: "До визита", completed: false },
            { id: "t-02", title: "Отправить опрос NPS", due: "Сегодня", completed: true },
        ],
    },
    {
        id: "cl-02",
        name: "Максим Орлов",
        phone: "+7 (903) 224-83-10",
        email: "max.orlov@example.com",
        segment: "Active",
        health: "Loyal",
        tags: ["Стрижка", "Барбер", "Apple Pay"],
        lastVisit: "2023-10-01",
        nextVisit: "2023-10-22T18:30",
        lifetimeValue: 96000,
        upcoming: "Стрижка + уход",
        preferredMaster: "Даниил С.",
        totalVisits: 14,
        satisfaction: 4.7,
        notes: "Оценивает скорость записи. Рекомендует друзьям, приносит рефералов.",
        communications: [
            { id: "c-04", type: "call", channel: "Телефон", summary: "Перенос времени визита", time: "2 окт, 11:05" },
            { id: "c-05", type: "visit", channel: "Барбершоп", summary: "Стрижка classic fade", time: "1 окт, 18:00" },
        ],
        tasks: [
            { id: "t-03", title: "Отправить спасибо за рекомендацию", due: "Завтра", completed: false },
        ],
    },
    {
        id: "cl-03",
        name: "Екатерина Мирная",
        phone: "+7 (905) 883-77-90",
        email: "katya.mir@example.com",
        segment: "New",
        health: "New",
        tags: ["Маникюр", "Педикюр"],
        lastVisit: "2023-09-30",
        lifetimeValue: 18000,
        upcoming: "Маникюр + покрытие",
        preferredMaster: "Юлия Н.",
        totalVisits: 1,
        satisfaction: 4.5,
        notes: "Новый клиент с рекламы в Instagram. Хочет попробовать spa-процедуры.",
        communications: [
            { id: "c-06", type: "message", channel: "Direct", summary: "Запрос на первый визит", time: "25 сен, 20:18" },
            { id: "c-07", type: "visit", channel: "Нейл-зал", summary: "Маникюр + покрытие", time: "30 сен, 13:30" },
        ],
        tasks: [
            { id: "t-04", title: "Назначить welcome-бонус", due: "Сегодня", completed: false },
        ],
    },
    {
        id: "cl-04",
        name: "Мария Ясина",
        phone: "+7 (921) 109-65-11",
        email: "maria.yas@example.com",
        segment: "Active",
        health: "At Risk",
        tags: ["Колоринг", "Кератин"],
        lastVisit: "2023-07-14",
        lifetimeValue: 112000,
        upcoming: "Нет записи",
        preferredMaster: "Алсу Х.",
        totalVisits: 11,
        satisfaction: 4.1,
        notes: "Нужно вернуть — пропустила две регулярные записи. Любит персонализированные предложения.",
        communications: [
            { id: "c-08", type: "message", channel: "SMS", summary: "Отправлено напоминание о визите", time: "15 сен, 10:00" },
            { id: "c-09", type: "note", channel: "Администратор", summary: "Отменилa из-за отпуска", time: "10 авг, 09:35" },
        ],
        tasks: [
            { id: "t-05", title: "Позвонить с персональным оффером", due: "Сегодня", completed: false },
            { id: "t-06", title: "Добавить в кампанию \"Возврат\"", due: "Завтра", completed: false },
        ],
    },
    {
        id: "cl-05",
        name: "Леонид Волков",
        phone: "+7 (911) 340-21-18",
        email: "leon.volk@example.com",
        segment: "Dormant",
        health: "At Risk",
        tags: ["Массаж", "Корпоратив"],
        lastVisit: "2023-05-18",
        lifetimeValue: 54000,
        upcoming: "Нет записи",
        preferredMaster: "Мия Р.",
        totalVisits: 6,
        satisfaction: 4.2,
        notes: "Клиент корпоративной программы. Любит утренние часы, ценит напоминания.",
        communications: [
            { id: "c-10", type: "call", channel: "Телефон", summary: "Проверка статуса корпоративного пакета", time: "1 июн, 12:40" },
            { id: "c-11", type: "note", channel: "Администратор", summary: "Отправлен счёт за продление", time: "25 май, 09:20" },
        ],
        tasks: [
            { id: "t-07", title: "Подготовить предложение на Q4", due: "На этой неделе", completed: false },
        ],
    },
    {
        id: "cl-06",
        name: "Софья Лайт",
        phone: "+7 (925) 707-04-80",
        email: "sofia.light@example.com",
        segment: "VIP",
        health: "Loyal",
        tags: ["Spa", "Wellness", "Family"],
        lastVisit: "2023-09-26",
        nextVisit: "2023-10-10T11:00",
        lifetimeValue: 204000,
        upcoming: "Spa-day + массаж",
        preferredMaster: "Мия Р.",
        totalVisits: 32,
        satisfaction: 5,
        notes: "Приводит дочь на семейные процедуры. Важен private-кабинет и здоровые снеки.",
        communications: [
            { id: "c-12", type: "visit", channel: "Spa-зона", summary: "Spa-day signature", time: "26 сен, 11:00" },
            { id: "c-13", type: "message", channel: "Email", summary: "Подтверждение семейного дня", time: "24 сен, 18:05" },
        ],
        tasks: [
            { id: "t-08", title: "Проверить наличие wellness-набора", due: "К визиту", completed: false },
        ],
    },
    {
        id: "cl-07",
        name: "Илья Соколов",
        phone: "+7 (931) 521-88-70",
        email: "ilya.sky@example.com",
        segment: "Lead",
        health: "Prospect",
        tags: ["Лазер", "Реклама"],
        lastVisit: "2023-09-20",
        lifetimeValue: 12000,
        upcoming: "Повторная консультация",
        preferredMaster: "Кира Л.",
        totalVisits: 1,
        satisfaction: 4.0,
        notes: "Интересуется курсом лазерной эпиляции. Нужен рассрочка-план.",
        communications: [
            { id: "c-14", type: "message", channel: "Telegram", summary: "Запрос стоимости", time: "18 сен, 21:10" },
            { id: "c-15", type: "visit", channel: "Косметология", summary: "Первая консультация", time: "20 сен, 17:00" },
        ],
        tasks: [
            { id: "t-09", title: "Отправить КП и варианты рассрочки", due: "Сегодня", completed: false },
        ],
    },
    {
        id: "cl-08",
        name: "Дарья Фокс",
        phone: "+7 (977) 650-17-42",
        email: "daria.fox@example.com",
        segment: "Active",
        health: "Loyal",
        tags: ["Брови", "Make-up"],
        lastVisit: "2023-09-29",
        upcoming: "Ламинирование бровей",
        preferredMaster: "Лина Б.",
        lifetimeValue: 76000,
        totalVisits: 16,
        satisfaction: 4.8,
        notes: "Заказывает макияж на мероприятия. Любит push-уведомления за день до визита.",
        communications: [
            { id: "c-16", type: "visit", channel: "Макияж", summary: "Make-up — fashion-съёмка", time: "29 сен, 19:00" },
            { id: "c-17", type: "message", channel: "Push", summary: "Напоминание за 24 часа", time: "28 сен, 19:05" },
        ],
        tasks: [
            { id: "t-10", title: "Подготовить подборку образов", due: "Завтра", completed: false },
        ],
    },
];

const segments = ["All clients", "Active", "New", "VIP", "Dormant", "Lead"] as const;
const statusFilters = ["Loyal", "At Risk", "Prospect", "New"] as const;

const palette = [
    "linear-gradient(135deg, #ffd2e9 0%, #c3d9ff 100%)",
    "linear-gradient(135deg, #ffe8cc 0%, #ffd3f4 100%)",
    "linear-gradient(135deg, #d7f7ff 0%, #e4d2ff 100%)",
    "linear-gradient(135deg, #fdf2ff 0%, #c2f1ff 100%)",
];

const currencyFormatter = new Intl.NumberFormat("ru-RU", {
    style: "currency",
    currency: "RUB",
    maximumFractionDigits: 0,
});

const dateFormatter = new Intl.DateTimeFormat("ru-RU", {
    day: "2-digit",
    month: "short",
    year: "numeric",
});

const timeFormatter = new Intl.DateTimeFormat("ru-RU", {
    hour: "2-digit",
    minute: "2-digit",
});

export default function ClientsPage() {
    const [searchTerm, setSearchTerm] = useState("");
    const [segment, setSegment] = useState<(typeof segments)[number]>("All clients");
    const [healthFilters, setHealthFilters] = useState<string[]>([]);
    const [selectedId, setSelectedId] = useState<string>(clientsData[0]?.id ?? "");

    const filteredClients = useMemo(() => {
        const normalized = searchTerm.trim().toLowerCase();
        return clientsData.filter(client => {
            if (segment !== "All clients" && client.segment !== segment) {
                return false;
            }

            if (healthFilters.length && !healthFilters.includes(client.health)) {
                return false;
            }

            if (!normalized) {
                return true;
            }

            return [client.name, client.phone, client.email, client.tags.join(" ")]
                .some(value => value.toLowerCase().includes(normalized));
        });
    }, [healthFilters, searchTerm, segment]);

    useEffect(() => {
        if (!filteredClients.length) {
            return;
        }

        if (!filteredClients.some(client => client.id === selectedId)) {
            setSelectedId(filteredClients[0].id);
        }
    }, [filteredClients, selectedId]);

    const selectedClient = useMemo(() => {
        return filteredClients.find(client => client.id === selectedId) ?? filteredClients[0] ?? null;
    }, [filteredClients, selectedId]);

    const totals = useMemo(() => {
        const active = clientsData.filter(client => client.segment === "Active").length;
        const vip = clientsData.filter(client => client.segment === "VIP").length;
        const newClients = clientsData.filter(client => client.segment === "New").length;
        const atRisk = clientsData.filter(client => client.health === "At Risk" || client.segment === "Dormant").length;

        const lifetime = clientsData.reduce((acc, client) => acc + client.lifetimeValue, 0);
        const averageLtv = lifetime / clientsData.length;

        return {
            active,
            vip,
            newClients,
            atRisk,
            averageLtv,
        };
    }, []);

    const automations = [
        {
            title: "Напоминание о визите",
            description: "Авто-сообщение за 24 часа до записи",
        },
        {
            title: "Возврат неактивных",
            description: "Запуск цепочки с персональной скидкой через 45 дней",
        },
        {
            title: "VIP care",
            description: "Персональный менеджер + подарочный набор к дню рождения",
        },
    ];

    const open = (action: string) => alert(action);

    return (
        <ThemeProvider>
            <Header
                breadcrumb="Clients"
                onOpenAdmin={() => open("Admin")}
                onOpenSettings={() => open("Settings")}
                onOpenProfile={() => open("Profile")}
                onLogout={() => open("Sign out")}
            />

            <main className="fx-page clients-page">
                <section className="clients-top">
                    <Widget title="Активные" footer={`${totals.active} / ${clientsData.length}`} minH={140}>
                        <div className="client-metric-number">{totals.active}</div>
                        <span className="client-metric-sub">Клиентов с регулярными визитами</span>
                    </Widget>
                    <Widget title="VIP" footer="Премиум сегмент" minH={140}>
                        <div className="client-metric-number">{totals.vip}</div>
                        <span className="client-metric-sub">Личный менеджер и подарки</span>
                    </Widget>
                    <Widget title="Новые" footer="за 30 дней" minH={140}>
                        <div className="client-metric-number">{totals.newClients}</div>
                        <span className="client-metric-sub">Welcome-кампания активна</span>
                    </Widget>
                    <Widget title="Риск ухода" footer="Контроль возврата" minH={140}>
                        <div className="client-metric-number warning">{totals.atRisk}</div>
                        <span className="client-metric-sub">Нужны персональные офферы</span>
                    </Widget>
                    <Widget title="Средний LTV" footer="по базе" minH={140}>
                        <div className="client-metric-number">{currencyFormatter.format(totals.averageLtv)}</div>
                        <span className="client-metric-sub">Растёт на 8% в год</span>
                    </Widget>
                </section>

                <section className="clients-content">
                    <div className="clients-left">
                        <Widget title="База клиентов" footer={`${filteredClients.length} из ${clientsData.length}`} minH={460}>
                            <div className="clients-controls">
                                <div className="clients-search">
                                    <input
                                        type="search"
                                        placeholder="Поиск по имени, тегу или контакту"
                                        value={searchTerm}
                                        onChange={event => setSearchTerm(event.target.value)}
                                    />
                                </div>
                                <div className="clients-segments" role="tablist" aria-label="Фильтр по сегментам">
                                    {segments.map(seg => (
                                        <button
                                            key={seg}
                                            type="button"
                                            role="tab"
                                            className={`segment-chip${segment === seg ? " is-active" : ""}`}
                                            aria-pressed={segment === seg}
                                            onClick={() => setSegment(seg)}
                                        >
                                            {seg === "All clients" ? "Все" : seg}
                                        </button>
                                    ))}
                                </div>
                                <div className="clients-status-filters" aria-label="Статусы">
                                    {statusFilters.map(status => {
                                        const isActive = healthFilters.includes(status);
                                        return (
                                            <button
                                                key={status}
                                                type="button"
                                                className={`status-chip status-${status.replace(/\s/g, "").toLowerCase()}${isActive ? " is-active" : ""}`}
                                                onClick={() => {
                                                    setHealthFilters(prev =>
                                                        isActive
                                                            ? prev.filter(item => item !== status)
                                                            : [...prev, status],
                                                    );
                                                }}
                                            >
                                                {status}
                                            </button>
                                        );
                                    })}
                                </div>
                                <div className="clients-actions">
                                    <button type="button" className="ghost" onClick={() => open("Export clients")}>Экспорт</button>
                                    <button type="button" className="primary" onClick={() => open("Add new client")}>+ Новый клиент</button>
                                </div>
                            </div>

                            <div className="clients-table-wrapper">
                                {filteredClients.length ? (
                                    <table className="clients-table">
                                        <thead>
                                            <tr>
                                                <th>Клиент</th>
                                                <th>Теги</th>
                                                <th>Последний визит</th>
                                                <th>Статус</th>
                                                <th>LTV</th>
                                                <th>Следующий шаг</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            {filteredClients.map((client, index) => {
                                                const isActive = selectedClient?.id === client.id;
                                                const gradient = palette[index % palette.length];
                                                const lastVisitLabel = dateFormatter.format(new Date(client.lastVisit));
                                                const nextVisitLabel = client.nextVisit
                                                    ? `${dateFormatter.format(new Date(client.nextVisit))} · ${timeFormatter.format(new Date(client.nextVisit))}`
                                                    : client.upcoming;
                                                return (
                                                    <tr
                                                        key={client.id}
                                                        className={isActive ? "is-active" : undefined}
                                                        onClick={() => setSelectedId(client.id)}
                                                    >
                                                        <td>
                                                            <div className="client-cell">
                                                                <span className="client-avatar" style={{ background: gradient }}>
                                                                    {client.name
                                                                        .split(" ")
                                                                        .map(part => part[0])
                                                                        .join("")}
                                                                </span>
                                                                <div className="client-ident">
                                                                    <strong>{client.name}</strong>
                                                                    <span>{client.phone}</span>
                                                                </div>
                                                            </div>
                                                        </td>
                                                        <td>
                                                            <div className="client-tags">
                                                                {client.tags.map(tag => (
                                                                    <span key={tag} className="client-tag">{tag}</span>
                                                                ))}
                                                            </div>
                                                        </td>
                                                        <td>{lastVisitLabel}</td>
                                                        <td>
                                                            <span className={`client-health health-${client.health.replace(/\s/g, "").toLowerCase()}`}>
                                                                {client.health}
                                                            </span>
                                                        </td>
                                                        <td>{currencyFormatter.format(client.lifetimeValue)}</td>
                                                        <td>
                                                            <div className="client-next">
                                                                <strong>{client.upcoming}</strong>
                                                                <span>{nextVisitLabel}</span>
                                                            </div>
                                                        </td>
                                                    </tr>
                                                );
                                            })}
                                        </tbody>
                                    </table>
                                ) : (
                                    <div className="clients-empty">
                                        <h4>Нет клиентов по текущим фильтрам</h4>
                                        <p>Сбросьте фильтры или добавьте нового клиента.</p>
                                    </div>
                                )}
                            </div>
                        </Widget>
                    </div>

                    <aside className="clients-right">
                        <Widget
                            title="Профиль"
                            footer={selectedClient ? selectedClient.segment : "Выберите клиента"}
                            minH={220}
                        >
                            {selectedClient ? (
                                <div className="client-profile">
                                    <div className="client-profile-head">
                                        <div className="client-profile-avatar" style={{ background: palette[0] }}>
                                            {selectedClient.name
                                                .split(" ")
                                                .map(part => part[0])
                                                .join("")}
                                        </div>
                                        <div>
                                            <h3>{selectedClient.name}</h3>
                                            <span className={`client-health health-${selectedClient.health.replace(/\s/g, "").toLowerCase()}`}>
                                                {selectedClient.health}
                                            </span>
                                        </div>
                                    </div>
                                    <dl className="client-profile-meta">
                                        <div>
                                            <dt>Телефон</dt>
                                            <dd>{selectedClient.phone}</dd>
                                        </div>
                                        <div>
                                            <dt>Email</dt>
                                            <dd>{selectedClient.email}</dd>
                                        </div>
                                        <div>
                                            <dt>Мастер</dt>
                                            <dd>{selectedClient.preferredMaster}</dd>
                                        </div>
                                        <div>
                                            <dt>Последний визит</dt>
                                            <dd>{dateFormatter.format(new Date(selectedClient.lastVisit))}</dd>
                                        </div>
                                    </dl>
                                    <div className="client-profile-stats">
                                        <div>
                                            <strong>{selectedClient.totalVisits}</strong>
                                            <span>визитов</span>
                                        </div>
                                        <div>
                                            <strong>{currencyFormatter.format(selectedClient.lifetimeValue)}</strong>
                                            <span>LTV</span>
                                        </div>
                                        <div>
                                            <strong>{selectedClient.satisfaction.toFixed(1)} ★</strong>
                                            <span>оценка</span>
                                        </div>
                                    </div>
                                    <p className="client-profile-notes">{selectedClient.notes}</p>
                                </div>
                            ) : (
                                <div className="clients-empty">
                                    <h4>Выберите клиента</h4>
                                    <p>Нажмите на строку в таблице слева.</p>
                                </div>
                            )}
                        </Widget>

                        <Widget title="Коммуникации" footer="Последние взаимодействия" minH={260}>
                            {selectedClient ? (
                                <ol className="client-timeline">
                                    {selectedClient.communications.map(item => (
                                        <li key={item.id}>
                                            <span className={`timeline-bullet type-${item.type}`} aria-hidden />
                                            <div>
                                                <div className="timeline-head">
                                                    <strong>{item.summary}</strong>
                                                    <span>{item.time}</span>
                                                </div>
                                                <span className="timeline-meta">{item.channel}</span>
                                            </div>
                                        </li>
                                    ))}
                                </ol>
                            ) : (
                                <div className="clients-empty">
                                    <p>Нет данных. Выберите клиента.</p>
                                </div>
                            )}
                        </Widget>

                        <Widget title="Задачи" footer="To-do для команды" minH={220}>
                            {selectedClient ? (
                                <ul className="client-tasks">
                                    {selectedClient.tasks.map(task => (
                                        <li key={task.id}>
                                            <label>
                                                <input type="checkbox" checked={task.completed} readOnly />
                                                <span>
                                                    {task.title}
                                                    <small>{task.due}</small>
                                                </span>
                                            </label>
                                        </li>
                                    ))}
                                </ul>
                            ) : (
                                <div className="clients-empty">
                                    <p>Нет задач по выбранному клиенту.</p>
                                </div>
                            )}
                        </Widget>

                        <Widget title="Автоматизации" footer="Работают в фоне" minH={200}>
                            <ul className="client-automations">
                                {automations.map(flow => (
                                    <li key={flow.title}>
                                        <div>
                                            <strong>{flow.title}</strong>
                                            <span>{flow.description}</span>
                                        </div>
                                        <button type="button" className="ghost" onClick={() => open(flow.title)}>Настроить</button>
                                    </li>
                                ))}
                            </ul>
                        </Widget>
                    </aside>
                </section>
            </main>
        </ThemeProvider>
    );
}
