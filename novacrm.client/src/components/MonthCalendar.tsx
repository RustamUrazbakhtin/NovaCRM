import { useEffect, useMemo, useState } from "react";
import type { CSSProperties, FormEvent, MouseEvent as ReactMouseEvent } from "react";
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

const DEFAULT_STAFF = ["Alsu", "Mia", "Julia", "Aigul"];
const FALLBACK_MASTER = "Unassigned";

type Props = {
    events?: CalendarEvent[];
    title?: string;
};

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

type DaySchedule = {
    iso: string;
    label: string;
    shortLabel: string;
    staff: string[];
    hours: { minutes: number; label: string }[];
    events: (CalendarEvent & {
        master: string;
        startMinutes: number;
        endMinutes: number;
        startLabel: string;
        endLabel: string;
    })[];
};

export default function MonthCalendar({ events = [], title = "Calendar" }: Props) {
    const today = useMemo(() => {
        const now = new Date();
        now.setHours(0, 0, 0, 0);
        return now;
    }, []);
    const todayISO = today.toISOString().slice(0, 10);

    const [view, setView] = useState<CalendarView>("month");
    const [cursor, setCursor] = useState(() => normalizeCursor(today, "month"));
    const [selectedDate, setSelectedDate] = useState<string | null>(null);

    const eventsByDate = useMemo(() => {
        const map = new Map<string, CalendarEvent[]>();
        for (const ev of events) {
            const bucket = map.get(ev.date) ?? [];
            bucket.push(ev);
            map.set(ev.date, bucket);
        }
        return map;
    }, [events]);

    const eventsByMonth = useMemo(() => {
        const monthMap = new Map<string, number>();
        for (const ev of events) {
            const monthKey = ev.date.slice(0, 7);
            monthMap.set(monthKey, (monthMap.get(monthKey) ?? 0) + 1);
        }
        return monthMap;
    }, [events]);

    const data = useMemo(() => {
        if (view === "month") {
            const firstDay = new Date(cursor.getFullYear(), cursor.getMonth(), 1);
            const start = startOfWeek(firstDay);
            const lastDay = new Date(cursor.getFullYear(), cursor.getMonth() + 1, 0);
            const offset = (firstDay.getDay() + 6) % 7;
            const weeks = Math.max(5, Math.ceil((offset + lastDay.getDate()) / 7));
            const totalCells = weeks * 7;
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

            const collectedStaff = new Set<string>();
            const weekDays = Array.from({ length: 7 }, (_, index) => {
                const date = new Date(weekStart);
                date.setDate(weekStart.getDate() + index);
                const iso = date.toISOString().slice(0, 10);
                const bucket = eventsByDate.get(iso) ?? [];

                const enrichedEvents = bucket
                    .map((event) => {
                        if (event.master) {
                            collectedStaff.add(event.master);
                        }
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

            const staff = [...DEFAULT_STAFF, ...Array.from(collectedStaff)].filter(
                (name, index, source) => source.indexOf(name) === index,
            );

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
                staff,
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
    }, [cursor, eventsByDate, eventsByMonth, today, todayISO, view]);

    const handlePrev = () => setCursor((prev) => shiftCursor(prev, view, -1));
    const handleNext = () => setCursor((prev) => shiftCursor(prev, view, 1));
    const handleViewChange = (next: CalendarView) => {
        setView(next);
        setCursor((prev) => normalizeCursor(prev, next));
    };
    const handleAdd = () => setSelectedDate(todayISO);
    const handleDayClick = (iso: string) => {
        setSelectedDate(iso);
    };

    const closeModal = () => setSelectedDate(null);

    useEffect(() => {
        if (!selectedDate) return;
        const onKey = (event: KeyboardEvent) => {
            if (event.key === "Escape") {
                event.preventDefault();
                closeModal();
            }
        };
        const previousOverflow = document.body.style.overflow;
        document.body.style.overflow = "hidden";
        document.addEventListener("keydown", onKey);
        return () => {
            document.body.style.overflow = previousOverflow;
            document.removeEventListener("keydown", onKey);
        };
    }, [selectedDate]);

    const selectedSchedule: DaySchedule | null = useMemo(() => {
        if (!selectedDate) {
            return null;
        }

        const date = new Date(selectedDate);
        if (Number.isNaN(date.getTime())) {
            return null;
        }

        const hours = Array.from({ length: WEEK_END_HOUR - WEEK_START_HOUR + 1 }, (_, index) => {
            const hour = WEEK_START_HOUR + index;
            return {
                minutes: hour * 60,
                label: `${String(hour).padStart(2, "0")}:00`,
            };
        });

        const dayEvents = (eventsByDate.get(selectedDate) ?? []).map((event) => {
            const startCandidate =
                extractTimeMinutes(event.start) ??
                extractTimeMinutes(event.time) ??
                extractTimeMinutes(event.title);
            const rawStart = startCandidate ?? WEEK_START_MINUTES;
            const startMinutes = clamp(
                rawStart,
                WEEK_START_MINUTES,
                WEEK_END_MINUTES - MIN_EVENT_DURATION_MIN,
            );
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
                master: event.master ?? FALLBACK_MASTER,
                startMinutes,
                endMinutes,
                startLabel,
                endLabel: endLabel ?? formatMinutes(endMinutes),
            };
        });

        const staffOrder = [...DEFAULT_STAFF];
        for (const ev of dayEvents) {
            if (ev.master && !staffOrder.includes(ev.master)) {
                staffOrder.push(ev.master);
            }
        }

        if (dayEvents.some((ev) => !ev.master || ev.master === FALLBACK_MASTER)) {
            if (!staffOrder.includes(FALLBACK_MASTER)) {
                staffOrder.push(FALLBACK_MASTER);
            }
        }

        return {
            iso: selectedDate,
            label: date.toLocaleDateString(undefined, {
                weekday: "long",
                month: "long",
                day: "numeric",
                year: "numeric",
            }),
            shortLabel: date.toLocaleDateString(undefined, {
                month: "short",
                day: "numeric",
            }),
            staff: staffOrder,
            hours,
            events: dayEvents,
        };
    }, [eventsByDate, selectedDate]);

    return (
        <div className="mc">
            <div className="mc-toolbar">
                <div className="mc-toolbar-left">
                    {title && <span className="mc-caption">{title}</span>}
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
                <div className="mc-month" role="grid" aria-label={`${data.label} calendar`}>
                    <div className="mc-month-head" role="row">
                        {data.dayNames.map((day) => (
                            <div key={day} className="mc-dayname" role="columnheader">
                                {day}
                            </div>
                        ))}
                    </div>

                    <div className="mc-month-body" role="rowgroup">
                        {data.cells.map((cell) => {
                            const count = cell.events.length;
                            const dayLabel = new Date(cell.iso).toLocaleDateString(undefined, {
                                weekday: "long",
                                month: "long",
                                day: "numeric",
                                year: "numeric",
                            });
                            const countLabel = count === 1 ? "1 task" : `${count} tasks`;
                            return (
                                <button
                                    key={cell.iso}
                                    type="button"
                                    className={`mc-cell-btn ${cell.isCurrentMonth ? "" : "is-outside"} ${
                                        cell.isToday ? "is-today" : ""
                                    }`}
                                    onClick={() => handleDayClick(cell.iso)}
                                    aria-label={`${dayLabel}. ${countLabel}`}
                                >
                                    <span className="mc-date" aria-hidden="true">
                                        {cell.day}
                                    </span>
                                    <span className="mc-count" aria-hidden="true">
                                        {count}
                                        <small>{count === 1 ? "task" : "tasks"}</small>
                                    </span>
                                </button>
                            );
                        })}
                    </div>
                </div>
            )}

            {data.type === "week" && (
                <>
                    <div className="mc-week-staff" role="list">
                        {data.staff.map((member) => (
                            <span key={member} className="mc-week-staff-pill" role="listitem">
                                {member}
                            </span>
                        ))}
                    </div>

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
                                        const masterHint = event.master ? ` (${event.master})` : "";

                                        return (
                                            <article
                                                key={index}
                                                className="mc-week-event"
                                                style={{
                                                    top: `${offsetTop}%`,
                                                    height: `${height}%`,
                                                }}
                                                title={`${event.startLabel} – ${event.endLabel}: ${event.title}${masterHint}`}
                                            >
                                                <span className="mc-week-event-time">
                                                    {event.startLabel}
                                                    {event.endLabel ? ` – ${event.endLabel}` : ""}
                                                </span>
                                                {event.master && (
                                                    <span className="mc-week-event-master">{event.master}</span>
                                                )}
                                                <span className="mc-week-event-title">{event.title}</span>
                                            </article>
                                        );
                                    })}
                                </div>
                            </div>
                        ))}
                    </div>
                </>
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

            {selectedSchedule && (
                <CalendarScheduleModal schedule={selectedSchedule} onClose={closeModal} />
            )}
        </div>
    );
}

function CalendarScheduleModal({
    schedule,
    onClose,
}: {
    schedule: DaySchedule;
    onClose: () => void;
}) {
    const { label, shortLabel, staff, hours, events } = schedule;
    const totalTasks = events.length;

    const columns = staff.map((member) => ({
        member,
        events: events.filter((event) => event.master === member),
    }));

    const gridStyle = useMemo(
        () => ({
            "--mc-modal-columns": staff.length,
        }) as CSSProperties,
        [staff.length],
    );

    const [formOpen, setFormOpen] = useState(false);

    const handleClose = () => {
        setFormOpen(false);
        onClose();
    };

    const handleOverlayClick = (event: ReactMouseEvent<HTMLDivElement>) => {
        if (event.target === event.currentTarget) {
            handleClose();
        }
    };

    const handleCreate = () => setFormOpen((prev) => !prev);

    const handleFormSubmit = (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        const formData = new FormData(event.currentTarget);
        const master = (formData.get("master") as string | null) ?? FALLBACK_MASTER;
        const time = (formData.get("time") as string | null) ?? "";
        const title = (formData.get("title") as string | null) ?? "";
        const notes = (formData.get("notes") as string | null) ?? "";
        alert(`Task saved for ${master}${time ? ` at ${time}` : ""}: ${title || "(untitled)"}${
            notes ? `\nNotes: ${notes}` : ""
        }`);
        event.currentTarget.reset();
        setFormOpen(false);
    };

    return (
        <div
            className="mc-overlay"
            role="dialog"
            aria-modal="true"
            aria-label={`${label} schedule`}
            onMouseDown={handleOverlayClick}
        >
            <div className="mc-sheet" role="document">
                <header className="mc-sheet-head">
                    <div className="mc-sheet-meta">
                        <span className="mc-sheet-chip">{shortLabel}</span>
                        <h3>{label}</h3>
                        <p>{totalTasks === 1 ? "1 task" : `${totalTasks} tasks`}</p>
                    </div>

                    <div className="mc-sheet-actions">
                        <button type="button" className="mc-btn mc-btn-ghost" onClick={handleClose}>
                            Close
                        </button>
                        <button
                            type="button"
                            className="mc-btn mc-btn-accent"
                            onClick={handleCreate}
                            aria-expanded={formOpen}
                        >
                            New task
                        </button>
                    </div>
                </header>

                <div
                    className="mc-modal-grid"
                    style={gridStyle}
                >
                    <div className="mc-modal-hours" aria-hidden="true">
                        <div className="mc-modal-hours-spacer" />
                        {hours.map((hour) => (
                            <span key={hour.minutes}>{hour.label}</span>
                        ))}
                    </div>

                    {columns.map((column) => (
                        <section key={column.member} className="mc-modal-column">
                            <header className="mc-modal-column-head">{column.member}</header>
                            <div className="mc-modal-track">
                                <div className="mc-modal-gridlines" aria-hidden="true">
                                    {hours.slice(1).map((hour, index) => (
                                        <span
                                            key={hour.minutes}
                                            style={{
                                                top: `${((index + 1) / (hours.length - 1)) * 100}%`,
                                            }}
                                        />
                                    ))}
                                </div>

                                {column.events.map((event, index) => {
                                    const offsetTop =
                                        ((event.startMinutes - WEEK_START_MINUTES) / WEEK_RANGE_MINUTES) * 100;
                                    const height =
                                        ((event.endMinutes - event.startMinutes) / WEEK_RANGE_MINUTES) * 100;

                                    return (
                                        <article
                                            key={`${event.title}-${index}`}
                                            className="mc-modal-event"
                                            style={{
                                                top: `${offsetTop}%`,
                                                height: `${height}%`,
                                            }}
                                        >
                                            <span className="mc-modal-event-time">
                                                {event.startLabel}
                                                {event.endLabel ? ` – ${event.endLabel}` : ""}
                                            </span>
                                            <span className="mc-modal-event-title">{event.title}</span>
                                        </article>
                                    );
                                })}

                                {column.events.length === 0 && (
                                    <div className="mc-modal-empty">No scheduled tasks yet</div>
                                )}
                            </div>
                        </section>
                    ))}
                </div>

                {formOpen && (
                    <form className="mc-modal-form" onSubmit={handleFormSubmit}>
                        <div className="mc-modal-form-row">
                            <label className="mc-modal-field">
                                <span>Master</span>
                                <select name="master" defaultValue={staff[0] ?? FALLBACK_MASTER}>
                                    {staff.map((member) => (
                                        <option key={member} value={member}>
                                            {member}
                                        </option>
                                    ))}
                                </select>
                            </label>
                            <label className="mc-modal-field">
                                <span>Time</span>
                                <input type="time" name="time" defaultValue="12:00" />
                            </label>
                            <label className="mc-modal-field">
                                <span>Task</span>
                                <input type="text" name="title" placeholder="Service name" required autoFocus />
                            </label>
                        </div>

                        <div className="mc-modal-form-row">
                            <label className="mc-modal-field mc-modal-field--full">
                                <span>Notes</span>
                                <textarea name="notes" rows={3} placeholder="Optional comment" />
                            </label>
                        </div>

                        <div className="mc-modal-form-actions">
                            <button type="button" className="mc-btn mc-btn-ghost" onClick={() => setFormOpen(false)}>
                                Cancel
                            </button>
                            <button type="submit" className="mc-btn mc-btn-accent">
                                Save task
                            </button>
                        </div>
                    </form>
                )}
            </div>
        </div>
    );
}
