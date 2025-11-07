import { forwardRef } from "react";
import type { AppLauncherItemConfig } from "./types";
import styles from "./AppLauncher.module.css";

type Props = {
    item: AppLauncherItemConfig;
    onActivate: (item: AppLauncherItemConfig) => void;
    index: number;
    animationDelay: number;
    state: "opening" | "open" | "closing";
};

const AppLauncherItem = forwardRef<HTMLButtonElement, Props>(function AppLauncherItem({
    item,
    onActivate,
    index,
    animationDelay,
    state,
}, ref) {
    return (
        <button
            ref={ref}
            type="button"
            className={styles.itemButton}
            data-index={index}
            data-state={state}
            style={{ animationDelay: `${animationDelay}ms` }}
            onClick={() => onActivate(item)}
            role="gridcell"
        >
            <span className={styles.iconCircle} aria-hidden>
                {item.icon}
            </span>
            <span className={styles.label}>
                <span>{item.label}</span>
            </span>
        </button>
    );
});

export default AppLauncherItem;
