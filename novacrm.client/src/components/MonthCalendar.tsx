import { useMemo, useState } from "react";
import "../styles/calendar.css";

export type CalendarEvent = {
    /** ISO date string yyyy-mm-dd */
    date: string;
    /** Short title (e.g., "Haircut — Anna") */
    title: string;
    /** Optional start time in HH:mm */
    start?: string;
    /** Optional end time in HH:mm */
    end?: string;
    /** Legacy single time slot */
    time?: string;
    /** Assigned staff member */
    master?: string;
};

type CalendarView = "month" | "week" | "year";

type Props = {
    events?: CalendarEvent[];
};

const MAX_EVENTS_PER_DAY = 3;
const WEEK_START_HOUR = 8;
const WEEK_END_HOUR = 20;
const MIN_EVENT_DURATION_MIN = 45;
const WEEK_START_MINUTES = WEEK_START_HOUR * 60;
const WEEK_END_MINUTES = WEEK_END_HOUR * 60;
const WEEK_RANGE_MINUTES = WEEK_END_MINUTES - WEEK_START_MINUTES;

const clamp = (value: number, min: number, max: number) => Math.min(Math.max(value, min), max);

const extractTimeMinutes = (source?: string) => {
    if (!source) {
        return null;
    }

    const match = source.match(/(\d{1,2}):(\d{2})/);
    if (!match) {
        return null;
    }

    const hours = Number.parseInt(match[1] ?? "", 10);
    const minutes = Number.parseInt(match[2] ?? "", 10);

    if (Number.isNaN(hours) || Number.isNaN(minutes)) {
        return null;
    }

    return clamp(hours * 60 + minutes, 0, 24 * 60);
};

const formatMinutes = (minutes: number) => {
    const normalized = clamp(minutes, 0, 24 * 60);
    const hrs = Math.floor(normalized / 60);
    const mins = normalized % 60;
    return `${String(hrs).padStart(2, "0")}:${String(mins).padStart(2, "0")}`;
};

const startOfWeek = (date: Date) => {
    const d = new Date(date);
    const day = (d.getDay() + 6) % 7; // Monday = 0
    d.setDate(d.getDate() - day);
    d.setHours(0, 0, 0, 0);
    return d;
};

const normalizeCursor = (date: Date, view: CalendarView) => {
    const normalized = new Date(date);
    normalized.setHours(0, 0, 0, 0);

    if (view === "month") {
        normalized.setDate(1);
        return normalized;
    }

    if (view === "week") {
        return startOfWeek(normalized);
    }

    normalized.setMonth(0, 1);
    return normalized;
};

const shiftCursor = (cursor: Date, view: CalendarView, direction: number) => {
    const next = new Date(cursor);

    if (view === "month") {
        next.setMonth(next.getMonth() + direction);
    } else if (view === "week") {
        next.setDate(next.getDate() + direction * 7);
    } else {
        next.setFullYear(next.getFullYear() + direction);
    }

    return normalizeCursor(next, view);
};

const VIEW_OPTIONS: { key: CalendarView; label: string }[] = [
    { key: "month", label: "Month" },
    { key: "week", label: "Week" },
    { key: "year", label: "Year" },
];

export default function MonthCalendar({ events = [] }: Props) {
    const today = useMemo(() => {
        const now = new Date();
        now.setHours(0, 0, 0, 0);
        return now;
    }, []);
    const todayISO = today.toISOString().slice(0, 10);

    const [view, setView] = useState<CalendarView>("month");
    const [cursor, setCursor] = useState(() => normalizeCursor(today, "month"));

    const data = useMemo(() => {
        const eventsByDate = new Map<string, CalendarEvent[]>();
        const eventsByMonth = new Map<string, number>();

        for (const ev of events) {
            const dayBucket = eventsByDate.get(ev.date) ?? [];
            dayBucket.push(ev);
            eventsByDate.set(ev.date, dayBucket);

            const monthKey = ev.date.slice(0, 7);
            eventsByMonth.set(monthKey, (eventsByMonth.get(monthKey) ?? 0) + 1);
        }

        if (view === "month") {
            const firstDay = new Date(cursor.getFullYear(), cursor.getMonth(), 1);
            const start = startOfWeek(firstDay);
            const lastDay = new Date(cursor.getFullYear(), cursor.getMonth() + 1, 0);
            const offset = (firstDay.getDay() + 6) % 7;
            const totalCells = Math.ceil((offset + lastDay.getDate()) / 7) * 7;
            const cells = Array.from({ length: totalCells }, (_, index) => {
                const date = new Date(start);
                date.setDate(start.getDate() + index);
                const iso = date.toISOString().slice(0, 10);
                const bucket = eventsByDate.get(iso) ?? [];
                return {
                    iso,
                    day: date.getDate(),
                    isToday: iso === todayISO,
                    isCurrentMonth: date.getMonth() === cursor.getMonth(),
                    events: bucket,
                };
            });

            return {
                label: cursor.toLocaleString(undefined, {
                    month: "long",
                    year: "numeric",
                }),
                type: "month" as const,
                dayNames: ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"],
                cells,
            };
        }

        if (view === "week") {
            const weekStart = startOfWeek(cursor);
            const hours = Array.from(
                { length: WEEK_END_HOUR - WEEK_START_HOUR + 1 },
                (_, index) => {
                    const hour = WEEK_START_HOUR + index;
                    return {
                        minutes: hour * 60,
                        label: `${String(hour).padStart(2, "0")}:00`,
                    };
                },
            );

            const weekDays = Array.from({ length: 7 }, (_, index) => {
                const date = new Date(weekStart);
                date.setDate(weekStart.getDate() + index);
                const iso = date.toISOString().slice(0, 10);
                const bucket = eventsByDate.get(iso) ?? [];

                const enrichedEvents = bucket
                    .map((event) => {
                        const startCandidate =
                            extractTimeMinutes(event.start) ??
                            extractTimeMinutes(event.time) ??
                            extractTimeMinutes(event.title);
                        const rawStart = startCandidate ?? WEEK_START_MINUTES;
                        const startMinutes = clamp(rawStart, WEEK_START_MINUTES, WEEK_END_MINUTES - MIN_EVENT_DURATION_MIN);
                        const startWasClamped = rawStart !== startMinutes;

                        const endCandidate = extractTimeMinutes(event.end);
                        const rawEnd = endCandidate ?? (startCandidate ?? WEEK_START_MINUTES) + MIN_EVENT_DURATION_MIN;
                        const minEnd = startMinutes + MIN_EVENT_DURATION_MIN;
                        const endMinutes = clamp(Math.max(rawEnd, minEnd), minEnd, WEEK_END_MINUTES);
                        const endWasClamped = rawEnd !== endMinutes;

                        const startLabel =
                            startWasClamped
                                ? formatMinutes(startMinutes)
                                : event.start ?? event.time ?? formatMinutes(startMinutes);
                        const endLabel =
                            endWasClamped
                                ? formatMinutes(endMinutes)
                                : event.end ?? (event.start || event.time ? formatMinutes(endMinutes) : undefined);

                        return {
                            ...event,
                            startMinutes,
                            endMinutes,
                            startLabel,
                            endLabel: endLabel ?? formatMinutes(endMinutes),
                        };
                    })
                    .sort((a, b) => a.startMinutes - b.startMinutes);

                return {
                    iso,
                    label: date.toLocaleDateString(undefined, {
                        weekday: "short",
                    }),
                    display: date.toLocaleDateString(undefined, {
                        month: "short",
                        day: "numeric",
                    }),
                    isToday: iso === todayISO,
                    events: enrichedEvents,
                };
            });

            const weekEnd = new Date(weekStart);
            weekEnd.setDate(weekEnd.getDate() + 6);

            return {
                label: `${weekStart.toLocaleDateString(undefined, {
                    month: "short",
                    day: "numeric",
                })} – ${weekEnd.toLocaleDateString(undefined, {
                    month: "short",
                    day: "numeric",
                })}`,
                type: "week" as const,
                hours,
                days: weekDays,
            };
        }

        const year = cursor.getFullYear();
        const months = Array.from({ length: 12 }, (_, index) => {
            const monthDate = new Date(year, index, 1);
            const key = `${year}-${String(index + 1).padStart(2, "0")}`;
            return {
                key,
                label: monthDate.toLocaleString(undefined, {
                    month: "short",
                }),
                count: eventsByMonth.get(key) ?? 0,
                isCurrent: year === today.getFullYear() && index === today.getMonth(),
                monthIndex: index,
            };
        });

        return {
            label: year.toString(),
            type: "year" as const,
            months,
        };
    }, [cursor, events, today, todayISO, view]);

    const handlePrev = () => setCursor((prev) => shiftCursor(prev, view, -1));
    const handleNext = () => setCursor((prev) => shiftCursor(prev, view, 1));
    const handleViewChange = (next: CalendarView) => {
        setView(next);
        setCursor((prev) => normalizeCursor(prev, next));
    };
    const handleAdd = () => alert("Add new event");

    const getMonthEventLabel = (event: CalendarEvent) => {
        const start = event.start ?? event.time;
        return start ? `${start} · ${event.title}` : event.title;
    };

    return (
        <div className="mc">
            <div className="mc-toolbar">
                <div className="mc-nav">
                    <button
                        type="button"
                        className="mc-btn mc-btn-circle"
                        onClick={handlePrev}
                        aria-label="Previous period"
                    >
                        ‹
                    </button>
                    <button
                        type="button"
                        className="mc-btn mc-btn-circle"
                        onClick={handleNext}
                        aria-label="Next period"
                    >
                        ›
                    </button>
                </div>

                <div className="mc-title">{data.label}</div>

                <div className="mc-toolbar-end">
                    <div className="mc-toggle-group" role="tablist" aria-label="Calendar view">
                        {VIEW_OPTIONS.map((option) => (
                            <button
                                key={option.key}
                                type="button"
                                className={`mc-toggle ${view === option.key ? "is-active" : ""}`}
                                aria-pressed={view === option.key}
                                onClick={() => handleViewChange(option.key)}
                            >
                                {option.label}
                            </button>
                        ))}
                    </div>

                    <button
                        type="button"
                        className="mc-btn mc-btn-circle mc-btn-accent"
                        onClick={handleAdd}
                        aria-label="Add new event"
                    >
                        +
                    </button>
                </div>
            </div>

            {data.type === "month" && (
                <div className="mc-grid" role="grid">
                    {data.dayNames.map((day) => (
                        <div key={day} className="mc-dayname" role="columnheader">
                            {day}
                        </div>
                    ))}

                    {data.cells.map((cell) => {
                        const visibleEvents = cell.events.slice(0, MAX_EVENTS_PER_DAY);
                        const remaining = cell.events.length - visibleEvents.length;
                        return (
                            <div
                                key={cell.iso}
                                className={`mc-cell ${cell.isCurrentMonth ? "" : "is-outside"} ${
                                    cell.isToday ? "is-today" : ""
                                }`}
                                role="gridcell"
                            >
                                <div className="mc-date">{cell.day}</div>
                                <div className="mc-events">
                                    {visibleEvents.map((event, index) => (
                                        <span key={index} className="mc-pill" title={event.title}>
                                            {getMonthEventLabel(event)}
                                        </span>
                                    ))}
                                    {remaining > 0 && (
                                        <span className="mc-more" title={`${remaining} more event(s)`}>
                                            +{remaining}
                                        </span>
                                    )}
                                </div>
                            </div>
                        );
                    })}
                </div>
            )}

            {data.type === "week" && (
                <div className="mc-week" role="grid">
                    <div className="mc-week-times" aria-hidden="true">
                        <div className="mc-week-time-spacer" />
                        {data.hours.map((hour) => (
                            <div key={hour.minutes} className="mc-week-time">
                                {hour.label}
                            </div>
                        ))}
                    </div>

                    {data.days.map((day) => (
                        <div
                            key={day.iso}
                            className={`mc-week-day ${day.isToday ? "is-today" : ""}`}
                            role="gridcell"
                        >
                            <header className="mc-week-head">
                                <span className="mc-week-label">{day.label}</span>
                                <span className="mc-week-date">{day.display}</span>
                            </header>

                            <div className="mc-week-track">
                                <div className="mc-week-gridlines">
                                    {data.hours.slice(1).map((hour, index) => (
                                        <span
                                            key={hour.minutes}
                                            className="mc-week-gridline"
                                            style={{
                                                top: `${((index + 1) / (data.hours.length - 1)) * 100}%`,
                                            }}
                                        />
                                    ))}
                                </div>

                                {day.events.map((event, index) => {
                                    const offsetTop = ((event.startMinutes - WEEK_START_MINUTES) / WEEK_RANGE_MINUTES) * 100;
                                    const height = ((event.endMinutes - event.startMinutes) / WEEK_RANGE_MINUTES) * 100;

                                    return (
                                        <article
                                            key={index}
                                            className="mc-week-event"
                                            style={{
                                                top: `${offsetTop}%`,
                                                height: `${height}%`,
                                            }}
                                            title={`${event.startLabel} – ${event.endLabel}: ${event.title}`}
                                        >
                                            <span className="mc-week-event-time">
                                                {event.startLabel}
                                                {event.endLabel ? ` – ${event.endLabel}` : ""}
                                            </span>
                                            <span className="mc-week-event-title">{event.title}</span>
                                        </article>
                                    );
                                })}
                            </div>
                        </div>
                    ))}
                </div>
            )}

            {data.type === "year" && (
                <div className="mc-year" role="list">
                    {data.months.map((month) => (
                        <button
                            key={month.key}
                            type="button"
                            className={`mc-year-cell ${month.isCurrent ? "is-current" : ""}`}
                            onClick={() => {
                                setView("month");
                                setCursor(
                                    normalizeCursor(new Date(cursor.getFullYear(), month.monthIndex, 1), "month"),
                                );
                            }}
                        >
                            <span className="mc-year-label">{month.label}</span>
                            <span className="mc-year-count">{month.count} events</span>
                        </button>
                    ))}
                </div>
            )}
        </div>
    );
}
