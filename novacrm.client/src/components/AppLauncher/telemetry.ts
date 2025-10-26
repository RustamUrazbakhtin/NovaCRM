import type { AppLauncherTelemetryEvent } from "./types";

type Listener = (event: AppLauncherTelemetryEvent) => void;

const listeners = new Set<Listener>();

export function emitTelemetry(event: AppLauncherTelemetryEvent) {
    queueMicrotask(() => {
        listeners.forEach(listener => {
            try {
                listener(event);
            } catch (err) {
                console.error("AppLauncher telemetry listener failed", err);
            }
        });
        if (listeners.size === 0) {
            if (event.type === "item_click") {
                console.info(`[app-launcher] item_click`, event.label);
            } else {
                console.info(`[app-launcher] ${event.type}`);
            }
        }
    });
}

export function subscribeTelemetry(listener: Listener) {
    listeners.add(listener);
    return () => listeners.delete(listener);
}
