import { useEffect, useId, useLayoutEffect, useRef, useState } from "react";
import { createPortal } from "react-dom";
import styles from "./AppLauncher.module.css";
import AppLauncherItem from "./AppLauncherItem";
import type { AppLauncherItemConfig } from "./types";
import { emitTelemetry } from "./telemetry";

type PanelState = "closed" | "opening" | "open" | "closing";

type Props = {
    isOpen: boolean;
    onClose: () => void;
    triggerId: string;
    triggerRef: React.RefObject<HTMLButtonElement | null>;
    items: AppLauncherItemConfig[];
};

const STAGGER = 30;
const ANIMATION_MS = 200;

export default function AppLauncherPanel({ isOpen, onClose, triggerId, triggerRef, items }: Props) {
    const labelId = useId();
    const descriptionId = useId();
    const overlayRef = useRef<HTMLDivElement>(null);
    const panelRef = useRef<HTMLDivElement>(null);
    const closeButtonRef = useRef<HTMLButtonElement>(null);
    const itemRefs = useRef<Array<HTMLButtonElement | null>>([]);
    const [state, setState] = useState<PanelState>("closed");
    const restoreFocusRef = useRef<HTMLElement | null>(null);

    useEffect(() => {
        if (isOpen) {
            if (state === "closed") {
                setState("opening");
                emitTelemetry({ type: "open" });
            }
        } else if (state === "open") {
            setState("closing");
            emitTelemetry({ type: "close" });
        } else if (state === "opening") {
            setState("closing");
            emitTelemetry({ type: "close" });
        }
    }, [isOpen, state]);

    useEffect(() => {
        if (state === "opening") {
            const frame = requestAnimationFrame(() => setState("open"));
            return () => cancelAnimationFrame(frame);
        }
        if (state === "closing") {
            const timeout = setTimeout(() => setState("closed"), ANIMATION_MS);
            return () => clearTimeout(timeout);
        }
        return undefined;
    }, [state]);

    const shouldRender = state !== "closed";
    const motionState = state === "opening" ? "opening" : state === "closing" ? "closing" : "open";

    useEffect(() => {
        if (!shouldRender) return undefined;

        const originalOverflow = document.body.style.overflow;
        const originalPadding = document.body.style.paddingRight;
        const scrollBarWidth = window.innerWidth - document.documentElement.clientWidth;
        document.body.style.overflow = "hidden";
        if (scrollBarWidth > 0) {
            document.body.style.paddingRight = `${scrollBarWidth}px`;
        }

        return () => {
            document.body.style.overflow = originalOverflow;
            document.body.style.paddingRight = originalPadding;
        };
    }, [shouldRender]);

    useLayoutEffect(() => {
        if (state === "opening") {
            restoreFocusRef.current = (document.activeElement as HTMLElement) || null;
        }
    }, [state]);

    useLayoutEffect(() => {
        if (state !== "open") return;

        const firstFocusable = itemRefs.current[0] || closeButtonRef.current;
        firstFocusable?.focus();
    }, [state]);

    useEffect(() => {
        if (!shouldRender) return undefined;

        const onKeyDown = (event: KeyboardEvent) => {
            if (event.key === "Escape") {
                event.preventDefault();
                onClose();
                return;
            }

            if (event.key === "Tab") {
                const focusable = getFocusable();
                if (focusable.length === 0) return;
                const first = focusable[0];
                const last = focusable[focusable.length - 1];
                const active = document.activeElement as HTMLElement | null;
                if (!event.shiftKey && active === last) {
                    event.preventDefault();
                    first.focus();
                } else if (event.shiftKey && active === first) {
                    event.preventDefault();
                    last.focus();
                }
            }
        };

        document.addEventListener("keydown", onKeyDown);
        return () => document.removeEventListener("keydown", onKeyDown);
    }, [shouldRender, onClose]);

    useEffect(() => {
        if (state === "closed") {
            const previous = restoreFocusRef.current || triggerRef.current;
            previous?.focus();
            restoreFocusRef.current = null;
        }
    }, [state, triggerRef]);

    const registerItemRef = (index: number, node: HTMLButtonElement | null) => {
        itemRefs.current[index] = node;
    };

    const getFocusable = () => {
        if (!panelRef.current) return [] as HTMLElement[];
        const nodes = panelRef.current.querySelectorAll<HTMLElement>(
            'button, [href], [tabindex]:not([tabindex="-1"])'
        );
        return Array.from(nodes).filter(el => !el.hasAttribute("disabled"));
    };

    const handleOverlayMouseDown = (event: React.MouseEvent<HTMLDivElement>) => {
        if (event.target === overlayRef.current) {
            onClose();
        }
    };

    const [gridColumns, setGridColumns] = useState(1);

    const computeColumns = () => {
        const width = panelRef.current?.clientWidth ?? window.innerWidth;
        if (width >= 680) return 4;
        if (width >= 480) return 3;
        if (width >= 375) return 2;
        return 1;
    };

    useEffect(() => {
        if (!shouldRender) return;

        const update = () => setGridColumns(computeColumns());
        update();
        window.addEventListener("resize", update);
        return () => window.removeEventListener("resize", update);
    }, [shouldRender]);

    const handleGridKeyDown = (event: React.KeyboardEvent<HTMLDivElement>) => {
        const activeIndex = itemRefs.current.findIndex(node => node === document.activeElement);
        if (activeIndex === -1) return;

        let nextIndex = activeIndex;
        switch (event.key) {
            case "ArrowRight":
                nextIndex = Math.min(items.length - 1, activeIndex + 1);
                break;
            case "ArrowLeft":
                nextIndex = Math.max(0, activeIndex - 1);
                break;
            case "ArrowDown":
                nextIndex = Math.min(items.length - 1, activeIndex + gridColumns);
                break;
            case "ArrowUp":
                nextIndex = Math.max(0, activeIndex - gridColumns);
                break;
            case "Home":
                nextIndex = 0;
                break;
            case "End":
                nextIndex = items.length - 1;
                break;
            case "Enter":
            case " ":
                event.preventDefault();
                itemRefs.current[activeIndex]?.click();
                return;
            default:
                return;
        }

        if (nextIndex !== activeIndex) {
            event.preventDefault();
            itemRefs.current[nextIndex]?.focus();
        }
    };

    const content = shouldRender ? (
        <div
            ref={overlayRef}
            className={styles.overlay}
            data-state={motionState}
            onMouseDown={handleOverlayMouseDown}
            aria-hidden={state === "closing"}
        >
            <div
                ref={panelRef}
                role="dialog"
                aria-modal="true"
                aria-labelledby={labelId}
                aria-describedby={descriptionId}
                id={triggerId ? `${triggerId}-panel` : undefined}
                tabIndex={-1}
                className={styles.panelShell}
                data-state={motionState}
            >
                <div className={styles.panelContent}>
                    <header className={styles.header}>
                        <div>
                            <h2 id={labelId} className={styles.title}>Apps</h2>
                            <p id={descriptionId} className={styles.subtitle}>
                                Jump to the tools you use most often.
                            </p>
                        </div>
                        <button
                            ref={closeButtonRef}
                            type="button"
                            className={styles.closeButton}
                            onClick={onClose}
                        >
                            Close
                        </button>
                    </header>
                    <div className={styles.grid} role="grid" onKeyDown={handleGridKeyDown}>
                        {items.map((item, index) => (
                            <AppLauncherItem
                                key={item.id}
                                ref={node => registerItemRef(index, node)}
                                item={item}
                                index={index}
                                state={motionState === "closing" ? "closing" : "opening"}
                                animationDelay={index * STAGGER}
                                onActivate={selected => {
                                    emitTelemetry({ type: "item_click", label: selected.label });
                                    onClose();
                                    selected.onSelect?.();
                                }}
                            />
                        ))}
                    </div>
                </div>
            </div>
        </div>
    ) : null;

    if (!shouldRender) return null;
    return createPortal(content, document.body);
}
