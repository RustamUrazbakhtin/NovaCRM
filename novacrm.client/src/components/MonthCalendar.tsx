import { useMemo } from "react";
import "../styles/calendar.css";

export type CalendarEvent = {
    /** ISO date string yyyy-mm-dd */
    date: string;
    /** Short title (e.g., "Haircut — Anna 12:00") */
    title: string;
    /** Optional staff member responsible for the event */
    master?: string;
    /** Optional HH:mm label for scheduling */
    time?: string;
};

type Props = {
    events?: CalendarEvent[];
};

/**
 * Compact month calendar for dashboard.
 * Renders a 7×N grid; in each day shows a small count of events.
 */
export default function MonthCalendar({ events = [] }: Props) {
    const today = new Date();
    const y = today.getFullYear();
    const m = today.getMonth();

    const data = useMemo(() => {
        const first = new Date(y, m, 1);
        const last = new Date(y, m + 1, 0);
        const startWeekDay = (first.getDay() + 6) % 7; // Mon=0
        const totalCells = Math.ceil((startWeekDay + last.getDate()) / 7) * 7;

        const cells: {
            day?: number;
            iso?: string;
            isToday?: boolean;
            count: number;
        }[] = [];

        // Pre-index events by date for O(1) lookups
        const map = new Map<string, number>();
        for (const ev of events) {
            map.set(ev.date, (map.get(ev.date) ?? 0) + 1);
        }

        for (let i = 0; i < totalCells; i++) {
            const dayNum = i - startWeekDay + 1;
            if (dayNum >= 1 && dayNum <= last.getDate()) {
                const dt = new Date(y, m, dayNum);
                const iso = dt.toISOString().slice(0, 10);
                const isToday = dt.toDateString() === today.toDateString();
                cells.push({
                    day: dayNum,
                    iso,
                    isToday,
                    count: map.get(iso) ?? 0,
                });
            } else {
                cells.push({ count: 0 });
            }
        }

        const monthTitle = today.toLocaleString(undefined, {
            month: "long",
            year: "numeric",
        });

        return { cells, monthTitle };
    }, [y, m, events]);

    const days = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

    return (
        <div className="mc">
            <div className="mc-head">
                <div className="mc-title">{data.monthTitle}</div>
            </div>

            <div className="mc-grid">
                {days.map((d) => (
                    <div key={d} className="mc-dayname">
                        {d}
                    </div>
                ))}

                {data.cells.map((c, i) => (
                    <div key={i} className={`mc-cell ${c.isToday ? "mc-today" : ""}`}>
                        <div className="mc-date">{c.day ?? ""}</div>

                        {/* Simple counter badge if there are events */}
                        {c.count > 0 && (
                            <div
                                style={{
                                    marginTop: "auto",
                                    alignSelf: "flex-end",
                                    fontSize: 12,
                                    padding: "2px 6px",
                                    borderRadius: 999,
                                    background:
                                        "linear-gradient(90deg,var(--accent),var(--accent-2))",
                                    color: "#fff",
                                    fontWeight: 700,
                                }}
                                title={`${c.count} event(s)`}
                            >
                                {c.count}
                            </div>
                        )}
                    </div>
                ))}
            </div>
        </div>
    );
}
