import type { ReactNode } from "react";

export type AppLauncherItemConfig = {
    id: string;
    label: string;
    icon: ReactNode;
    href?: string;
    onSelect?: () => void;
};

export type AppLauncherTelemetryEvent =
    | { type: "open" }
    | { type: "close" }
    | { type: "item_click"; label: string };
