import { useMemo, useState } from "react";
import "../styles/calendar.css";

/** === Types === */
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

/** === Constants === */
const WEEK_START_HOUR = 8;
const WEEK_END_HOUR = 20;
const MIN_EVENT_DURATION_MIN = 45;
const WEEK_START_MINUTES = WEEK_START_HOUR * 60;
const WEEK_END_MINUTES = WEEK_END_HOUR * 60;
const WEEK_RANGE_MINUTES = WEEK_END_MINUTES - WEEK_START_MINUTES;

/** === Utils === */
const clamp = (value: number, min: number, max: number) =>
    Math.min(Math.max(value, min), max);

const extractTimeMinutes = (source?: string) => {
    if (!source) return null;
    const match = source.match(/(\d{1,2}):(\d{2})/);
    if (!match) return null;

    const hours = Number.parseInt(match[1] ?? "", 10);
    const minutes = Number.parseInt(match[2] ?? "", 10);
    if (Number.isNaN(hours) || Number.isNaN(minutes)) return null;

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
    if (view === "month") next.setMonth(next.getMonth() + direction);
    else if (view === "week") next.setDate(next.getDate() + direction * 7);
    else next.setFullYear(next.getFullYear() + direction);
    return normalizeCursor(next, view);
};

/** === Color helpers (по мастеру) === */
function colorFromString(seed: string) {
    let h = 0 >>> 0;
    for (let i = 0; i < seed.length; i++) h = (h * 31 + seed.charCodeAt(i)) >>> 0;
    const hue = h % 360; // 0..359
    return `hsl(${hue} 90% 55%)`;
}

function getDominantStaffColorByMaster(events: CalendarEvent[]): string | undefined {
    const counts = new Map<string, number>();
    for (const ev of events) {
        if (!ev.master) continue;
        counts.set(ev.master, (counts.get(ev.master) ?? 0) + 1);
    }
    const top = [...counts.entries()].sort((a, b) => b[1] - a[1])[0];
    return top ? colorFromString(top[0]) : undefined;
}

/** === View options === */
const VIEW_OPTIONS: { key: CalendarView; label: string }[] = [
    { key: "month", label: "Month" },
    { key: "week", label: "Week" },
];

/** === Component === */
export default function MonthCalendar({
    events = [],
    title = "Calendar",
}: {
    events?: CalendarEvent[];
    title?: string;
}) {
    const today = useMemo(() => {
        const now = new Date();
        now.setHours(0, 0, 0, 0);
        return now;
    }, []);
    const todayISO = today.toISOString().slice(0, 10);

    const [view, setView] = useState<CalendarView>("month");
    const [cursor, setCursor] = useState(() => normalizeCursor(today, "month"));

    /** === Simple popover state (для бейджа) === */
    type PopState = { rect: DOMRect; dateLabel: string; events: CalendarEvent[] } | null;
    const [popover, setPopover] = useState<PopState>(null);

    function openDayPopover(anchorEl: HTMLElement, dateLabel: string, evs: CalendarEvent[]) {
        const rect = anchorEl.getBoundingClientRect();
        setPopover({ rect, dateLabel, events: evs });
    }
    function closeDayPopover() {
        setPopover(null);
    }

    /** === Data model for current view === */
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
            // фиксированная высота — всегда 6 строк
            const weeks = Math.max(6, Math.ceil((offset + lastDay.getDate()) / 7));
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
                label: cursor.toLocaleString(undefined, { month: "long", year: "numeric" }),
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
                    return { minutes: hour * 60, label: `${String(hour).padStart(2, "0")}:00` };
                }
            );

            const collectedStaff = new Set<string>();
            const weekDays = Array.from({ length: 7 }, (_, index) => {
                const date = new Date(weekStart);
                date.setDate(weekStart.getDate() + index);
                const iso = date.toISOString().slice(0, 10);
                const bucket = eventsByDate.get(iso) ?? [];

                const enrichedEvents = bucket
                    .map((event) => {
                        if (event.master) collectedStaff.add(event.master);
                        const startCandidate =
                            extractTimeMinutes(event.start) ??
                            extractTimeMinutes(event.time) ??
                            extractTimeMinutes(event.title);
                        const rawStart = startCandidate ?? WEEK_START_MINUTES;
                        const startMinutes = clamp(
                            rawStart,
                            WEEK_START_MINUTES,
                            WEEK_END_MINUTES - MIN_EVENT_DURATION_MIN
                        );
                        const startWasClamped = rawStart !== startMinutes;

                        const endCandidate = extractTimeMinutes(event.end);
                        const rawEnd =
                            endCandidate ?? (startCandidate ?? WEEK_START_MINUTES) + MIN_EVENT_DURATION_MIN;
                        const minEnd = startMinutes + MIN_EVENT_DURATION_MIN;
                        const endMinutes = clamp(Math.max(rawEnd, minEnd), minEnd, WEEK_END_MINUTES);
                        const endWasClamped = rawEnd !== endMinutes;

                        const startLabel = startWasClamped
                            ? formatMinutes(startMinutes)
                            : event.start ?? event.time ?? formatMinutes(startMinutes);
                        const endLabel = endWasClamped
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
                    label: date.toLocaleDateString(undefined, { weekday: "short" }),
                    display: date.toLocaleDateString(undefined, { month: "short", day: "numeric" }),
                    isToday: iso === todayISO,
                    events: enrichedEvents,
                };
            });

            const weekEnd = new Date(weekStart);
            weekEnd.setDate(weekStart.getDate() + 6);

            const staff = [...DEFAULT_STAFF, ...Array.from(collectedStaff)].filter(
                (name, index, source) => source.indexOf(name) === index
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

        // year
        const year = cursor.getFullYear();
        const months = Array.from({ length: 12 }, (_, index) => {
            const monthDate = new Date(year, index, 1);
            const key = `${year}-${String(index + 1).padStart(2, "0")}`;
            return {
                key,
                label: monthDate.toLocaleString(undefined, { month: "short" }),
                count: eventsByMonth.get(key) ?? 0,
                isCurrent: year === today.getFullYear() && index === today.getMonth(),
                monthIndex: index,
            };
        });

        return { label: year.toString(), type: "year" as const, months };
    }, [cursor, events, today, todayISO, view]);

    /** === Handlers === */
    const handlePrev = () => setCursor((prev) => shiftCursor(prev, view, -1));
    const handleNext = () => setCursor((prev) => shiftCursor(prev, view, 1));
    const handleToday = () => {
        setCursor(() => normalizeCursor(new Date(), view));
    };
    const handleViewChange = (next: CalendarView) => {
        setView(next);
        setCursor((prev) => normalizeCursor(prev, next));
    };
    const handleAdd = () => alert("Add new event");

    return (
        <div className="mc">
            <div className="mc-toolbar">
                <div className="mc-toolbar-left">
                    {title && <span className="mc-caption">{title}</span>}
                    <div className="mc-nav">
                        <button type="button" className="mc-btn mc-btn-circle" onClick={handlePrev} aria-label="Previous period">‹</button>
                        <button type="button" className="mc-btn mc-btn-circle" onClick={handleNext} aria-label="Next period">›</button>
                    </div>
                </div>

                <div className="mc-title">{data.label}</div>

                <div className="mc-toolbar-end">
                    <button
                        type="button"
                        className="mc-btn mc-today"
                        onClick={handleToday}
                        aria-label="Go to today">
                        Today
                    </button>
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
                        aria-label="Add new event">+</button>
                </div>
            </div>

            {data.type === "month" && (
                <div className="mc-month" role="grid" aria-label={`${data.label} calendar`}>
                    <div className="mc-month-head" role="row">
                        {data.dayNames.map((day) => (
                            <div key={day} className="mc-dayname" role="columnheader">{day}</div>
                        ))}
                    </div>

                    <div className="mc-month-body" role="rowgroup">
                        {data.cells.map((cell) => {
                            const dayLabel = new Date(cell.iso).toLocaleDateString(undefined, {
                                weekday: "long", month: "long", day: "numeric", year: "numeric",
                            });

                            return (
                                <div
                                    key={cell.iso}
                                    className={`mc-cell-btn ${cell.isCurrentMonth ? "" : "is-outside"} ${cell.isToday ? "is-today" : ""}`}
                                    role="gridcell"
                                >
                                    <div className="mc-date" aria-label={dayLabel}>{cell.day}</div>

                                    {/* Бейдж "N events" в центре ячейки с окраской */}
                                    <div className="mc-events">
                                        {(() => {
                                            const list = cell.isCurrentMonth ? cell.events ?? [] : [];
                                            const total = list.length;
                                            if (total <= 0) return null;

                                            const busy = total >= 10 ? "is-high" : total >= 5 ? "is-mid" : "is-low";
                                            const domStaff = getDominantStaffColorByMaster(list);

                                            const dateLabel = new Date(cell.iso).toLocaleDateString(undefined, {
                                                weekday: "short", month: "short", day: "numeric",
                                            });

                                            return (
                                                <button
                                                    type="button"
                                                    className={`mc-count-label ${busy} ${domStaff ? "use-staff" : ""}`}
                                                    style={domStaff ? ({ ["--mc-staff" as any]: domStaff }) : undefined}
                                                    onClick={(e) => {
                                                        const anchor =
                                                            (e.currentTarget.closest(".mc-cell-btn") as HTMLElement) ??
                                                            e.currentTarget;
                                                        openDayPopover(anchor, dateLabel, list);
                                                    }}
                                                    aria-label={`${total} events on ${dateLabel}`}
                                                    title={`${total} events on ${dateLabel}`}
                                                >
                                                    <b className="num">{total > 99 ? "99+" : total}</b>
                                                    <span className="txt">events</span>
                                                </button>
                                            );
                                        })()}
                                    </div>
                                </div>
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
                                <div key={hour.minutes} className="mc-week-time">{hour.label}</div>
                            ))}
                        </div>

                        {data.days.map((day) => (
                            <div key={day.iso} className={`mc-week-day ${day.isToday ? "is-today" : ""}`} role="gridcell">
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
                                                style={{ top: `${((index + 1) / (data.hours.length - 1)) * 100}%` }}
                                            />
                                        ))}
                                    </div>

                                    {day.events.map((event, index) => {
                                        const offsetTop =
                                            ((event.startMinutes - WEEK_START_MINUTES) / WEEK_RANGE_MINUTES) * 100;
                                        const height =
                                            ((event.endMinutes - event.startMinutes) / WEEK_RANGE_MINUTES) * 100;
                                        const masterHint = event.master ? ` (${event.master})` : "";

                                        return (
                                            <article
                                                key={index}
                                                className="mc-week-event"
                                                style={{ top: `${offsetTop}%`, height: `${height}%` }}
                                                title={`${event.startLabel} – ${event.endLabel}: ${event.title}${masterHint}`}
                                            >
                                                <span className="mc-week-event-time">
                                                    {event.startLabel}
                                                    {event.endLabel ? ` – ${event.endLabel}` : ""}
                                                </span>
                                                {event.master && <span className="mc-week-event-master">{event.master}</span>}
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
                                    normalizeCursor(new Date(cursor.getFullYear(), month.monthIndex, 1), "month")
                                );
                            }}
                        >
                            <span className="mc-year-label">{month.label}</span>
                            <span className="mc-year-count">{month.count} events</span>
                        </button>
                    ))}
                </div>
            )}

            {/* Простой поповер (если хочешь — вынесу в отдельный файл) */}
            {popover && (
                <div className="mc-popover-backdrop" onClick={closeDayPopover}>
                    <div
                        className="mc-popover"
                        style={{
                            left: popover.rect.left,
                            top: popover.rect.bottom + 10,
                        }}
                        onClick={(e) => e.stopPropagation()}
                    >
                        <div className="mc-popover-head">
                            <div className="mc-popover-title">{popover.dateLabel}</div>
                            <button className="mc-btn-ghost" onClick={closeDayPopover}>Close</button>
                        </div>
                        <div className="mc-popover-body">
                            {popover.events.map((ev, i) => (
                                <div key={i} className="mc-poprow">
                                    <span className="mc-pop-time">{ev.start ?? ev.time ?? "—"}</span>
                                    <span className="mc-pop-title">
                                        {ev.master ? `${ev.master} · ` : ""}{ev.title}
                                    </span>
                                </div>
                            ))}
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}
