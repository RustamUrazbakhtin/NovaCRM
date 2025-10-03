import { useEffect, useMemo, useRef } from "react";
import { createPortal } from "react-dom";
import type { CalendarEvent } from "./MonthCalendar";

export default function ScheduleModal({
    open, onClose, dateISO, items, originRect
}: {
    open: boolean;
    onClose: () => void;
    dateISO: string;
    items: CalendarEvent[];
    originRect: DOMRect | null;
}) {
    const sheetRef = useRef<HTMLDivElement | null>(null);

    // Мастера и тайм-слоты
    const masters = useMemo(() => {
        const set = new Set(items.map(i => i.master));
        return Array.from(set);
    }, [items]);

    const times = useMemo(() => {
        const arr: string[] = [];
        for (let h = 9; h <= 20; h++) arr.push(`${String(h).padStart(2, "0")}:00`);
        return arr;
    }, []);

    useEffect(() => {
        if (!open || !sheetRef.current || !originRect) return;
        const el = sheetRef.current;

        // стартовые размеры = из ячейки
        el.style.left = `${originRect.left}px`;
        el.style.top = `${originRect.top}px`;
        el.style.width = `${originRect.width}px`;
        el.style.height = `${originRect.height}px`;
        el.style.transform = `translate(0,0) scale(1)`;
        el.style.opacity = "0";

        requestAnimationFrame(() => {
            // целевое положение/размер
            const vw = Math.min(920, window.innerWidth * 0.92);
            const vh = Math.min(700, window.innerHeight * 0.8);
            const tx = (window.innerWidth - vw) / 2 - originRect.left;
            const ty = (window.innerHeight - vh) / 2 - originRect.top;

            el.style.transition = "transform .28s cubic-bezier(.2,.8,.2,1), width .28s, height .28s, opacity .18s";
            el.style.transform = `translate(${tx}px,${ty}px)`;
            el.style.width = `${vw}px`;
            el.style.height = `${vh}px`;
            el.style.opacity = "1";
        });

        return () => {
            el.style.transition = "";
        };
    }, [open, originRect]);

    const closeAnimated = () => {
        if (!sheetRef.current || !originRect) return onClose();
        const el = sheetRef.current;

        const rect = el.getBoundingClientRect();
        const tx = originRect.left - rect.left;
        const ty = originRect.top - rect.top;

        el.style.transition = "transform .24s cubic-bezier(.2,.8,.2,1), width .24s, height .24s, opacity .16s";
        el.style.transform = `translate(${tx}px,${ty}px)`;
        el.style.width = `${originRect.width}px`;
        el.style.height = `${originRect.height}px`;
        el.style.opacity = "0";
        setTimeout(onClose, 180);
    };

    if (!open) return null;

    return createPortal(
        <div className="modal-backdrop" onClick={closeAnimated}>
            <div
                ref={sheetRef}
                className="modal-sheet"
                onClick={(e) => e.stopPropagation()}
            >
                <div className="modal-content">
                    <div className="modal-head">
                        <strong>
                            {new Date(dateISO).toLocaleDateString(undefined, { weekday: "long", day: "numeric", month: "long" })}
                        </strong>
                        <button onClick={closeAnimated}>Close</button>
                    </div>

                    <div className="modal-body">
                        {/* сетка времени × мастера */}
                        <div
                            className="sch"
                            style={{ ["--masters" as any]: masters.length }}
                        >
                            <div></div>
                            {masters.map(m => <div key={m} className="sch-head">{m}</div>)}

                            {times.map(t => (
                                <>
                                    <div key={`t-${t}`} className="sch-time">{t}</div>
                                    {masters.map(m => {
                                        const ev = items.find(x => x.time === t && x.master === m);
                                        return (
                                            <div key={`${t}-${m}`} className={`sch-slot ${ev ? "busy" : ""}`}>
                                                {ev ? ev.title : ""}
                                            </div>
                                        );
                                    })}
                                </>
                            ))}
                        </div>
                    </div>
                </div>
            </div>
        </div>,
        document.body
    );
}
