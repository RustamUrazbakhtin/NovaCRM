import { useEffect, useId, useRef, useState } from "react";
import { useTheme } from "../providers/ThemeProvider";
import "./header.css";

type MenuItem = { label: string; onClick: () => void };

export default function Header({
    onOpenAdmin,
    onOpenSettings,
    onOpenProfile,
    onLogout,
}: {
    onOpenAdmin: () => void;
    onOpenSettings: () => void;
    onOpenProfile: () => void;
    onLogout: () => void;
}) {
    const { theme, setTheme, isDark } = useTheme();
    const [open, setOpen] = useState(false);
    const box = useRef<HTMLDivElement>(null);

    useEffect(() => {
        const onDoc = (e: MouseEvent) => {
            if (!box.current?.contains(e.target as Node)) setOpen(false);
        };
        document.addEventListener("click", onDoc);
        return () => document.removeEventListener("click", onDoc);
    }, []);

    const isSwitchOn = theme === "dark" || (theme === "system" && isDark);
    const themeStatusLabel =
        theme === "system"
            ? `System · ${isDark ? "Dark" : "Light"}`
            : isSwitchOn
                ? "Dark mode"
                : "Light mode";

    const handleThemeToggle = () => {
        if (theme === "system") {
            setTheme(isSwitchOn ? "light" : "dark");
            return;
        }

        setTheme(isSwitchOn ? "light" : "dark");
    };

    const items: MenuItem[] = [
        { label: "Profile", onClick: onOpenProfile },
        { label: "Settings", onClick: onOpenSettings },
        { label: "Admin Panel", onClick: onOpenAdmin },
        { label: "Sign out", onClick: onLogout },
    ];

    return (
        <header className="nx-header">
            <div className="nx-left">
                <div className="nx-logo">
                    <span className="nx-mark">◇</span>
                    <span>NovaCRM</span>
                </div>
                <div className="nx-divider" />
                <div className="nx-breadcrumb">Dashboard</div>
            </div>

            <div className="nx-right" ref={box}>
                <button className="nx-user" onClick={() => setOpen(v => !v)} aria-haspopup="menu" aria-expanded={open}>
                    <img src="/react.svg" alt="user avatar" />
                    <span className="nx-user-name">User</span>
                </button>

                {open && (
                    <div className="nx-menu" role="menu">
                        {items.map((it, i) => (
                            <button key={i} role="menuitem" onClick={() => { it.onClick(); setOpen(false); }}>
                                {it.label}
                            </button>
                        ))}
                        <div className="nx-theme-row">
                            <div className="nx-theme-info">
                                <span className="nx-theme-label">Theme</span>
                                <span className="nx-theme-status">{themeStatusLabel}</span>
                            </div>

                            <button
                                type="button"
                                className={`nx-switch ${isSwitchOn ? "is-on" : ""}`}
                                role="switch"
                                aria-checked={isSwitchOn}
                                aria-label={isSwitchOn ? "Switch to light theme" : "Switch to dark theme"}
                                onClick={handleThemeToggle}
                            >
                                <span className="nx-switch-thumb" />
                            </button>
                        </div>

                        <button
                            type="button"
                            className={`nx-theme-system ${theme === "system" ? "is-active" : ""}`}
                            aria-pressed={theme === "system"}
                            onClick={() => {
                                setTheme("system");
                                setOpen(false);
                            }}
                        >
                            Follow system settings
                        </button>
                    </div>
                )}
            </div>
        </header>
    );
}