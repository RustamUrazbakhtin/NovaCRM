import { useEffect, useMemo, useRef, useState, type ReactNode } from "react";
import { useTheme } from "../providers/ThemeProvider";
import { AppLauncher, type AppLauncherItemConfig } from "../components/AppLauncher";
import "./header.css";

type MenuItem = { label: string; onClick: () => void };

export default function Header({
    breadcrumb = "Dashboard",
    onOpenAdmin,
    onOpenSettings,
    onOpenProfile,
    onLogout,
}: {
    breadcrumb?: string;
    onOpenAdmin: () => void;
    onOpenSettings: () => void;
    onOpenProfile: () => void;
    onLogout: () => void;
}) {
    const { theme, setTheme, isDark } = useTheme();
    const [open, setOpen] = useState(false);
    const box = useRef<HTMLDivElement>(null);
    const icon = (paths: ReactNode) => (
        <svg viewBox="0 0 24 24" width="32" height="32" role="img" aria-hidden>
            <g
                fill="none"
                stroke="currentColor"
                strokeWidth={1.6}
                strokeLinecap="round"
                strokeLinejoin="round"
            >
                {paths}
            </g>
        </svg>
    );

    const placeholder = (label: string) => () => alert(`${label} is coming soon`);

    const launcherItems: AppLauncherItemConfig[] = useMemo(
        () => [
            {
                id: "calendar",
                label: "Calendar",
                icon: icon(
                    <>
                        <rect x="4" y="5" width="16" height="14" rx="3" />
                        <line x1="4" y1="9" x2="20" y2="9" />
                        <line x1="8" y1="3" x2="8" y2="7" />
                        <line x1="16" y1="3" x2="16" y2="7" />
                    </>
                ),
                href: "/",
            },
            {
                id: "clients",
                label: "Clients",
                icon: icon(
                    <>
                        <circle cx="8" cy="9" r="3" />
                        <circle cx="16" cy="9" r="3" />
                        <path d="M3.5 18.5c.8-2.5 2.8-4.5 4.5-4.5s3.7 2 4.5 4.5" />
                        <path d="M12.5 18.5c.8-2.5 2.8-4.5 4.5-4.5s3.7 2 4.5 4.5" />
                    </>
                ),
                href: "/clients",
            },
            {
                id: "staff",
                label: "Staff",
                icon: icon(
                    <>
                        <path d="M12 12a4 4 0 1 0-4-4 4 4 0 0 0 4 4Z" />
                        <path d="M5 21a7 7 0 0 1 14 0" />
                    </>
                ),
                href: "/workers",
            },
            {
                id: "inventory",
                label: "Inventory",
                icon: icon(
                    <>
                        <rect x="4" y="4" width="16" height="16" rx="2.5" />
                        <path d="M4 10h16" />
                        <path d="M10 4v6" />
                    </>
                ),
                onSelect: placeholder("Inventory"),
            },
            {
                id: "analytics",
                label: "Analytics",
                icon: icon(
                    <>
                        <path d="M4 20h16" />
                        <rect x="5.5" y="10" width="3" height="6.5" rx="1.2" />
                        <rect x="10.5" y="7" width="3" height="9.5" rx="1.2" />
                        <rect x="15.5" y="12" width="3" height="4.5" rx="1.2" />
                    </>
                ),
                onSelect: placeholder("Analytics"),
            },
            {
                id: "tasks",
                label: "Tasks",
                icon: icon(
                    <>
                        <path d="M8 4h11a1 1 0 0 1 1 1v11" />
                        <path d="M16 20H5a1 1 0 0 1-1-1V8" />
                        <path d="m9 11 2 2 4-4" />
                    </>
                ),
                onSelect: placeholder("Tasks"),
            },
            {
                id: "reviews",
                label: "Reviews",
                icon: icon(
                    <>
                        <path d="M6 4h12a2 2 0 0 1 2 2v9a2 2 0 0 1-2 2h-6l-4 3v-3H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2Z" />
                        <path d="M9 9h6" />
                        <path d="M9 12h4" />
                    </>
                ),
                onSelect: placeholder("Reviews"),
            },
            {
                id: "settings",
                label: "Settings",
                icon: icon(
                    <>
                        <circle cx="12" cy="12" r="3" />
                        <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09a1.65 1.65 0 0 0-1-1.51 1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09a1.65 1.65 0 0 0 1.51-1 1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1Z" />
                    </>
                ),
                onSelect: onOpenSettings,
            },
            {
                id: "logout",
                label: "Log out",
                icon: icon(
                    <>
                        <path d="M15 3h-6a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h6" />
                        <path d="m10 12 4-4" />
                        <path d="m10 12 4 4" />
                        <path d="M14 12H3" />
                    </>
                ),
                onSelect: onLogout,
            },
        ],
        [onOpenSettings, onLogout],
    );

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
                    <span className="nx-mark">
                        <img src="/NovaCRM_Logo.png" alt="NovaCRM" height={28} />
                    </span>
                    <span className="brand__name">NovaCRM</span>
                </div>
                <div className="nx-divider" />
                <div className="nx-breadcrumb">{breadcrumb}</div>
            </div>

            <div className="nx-right" ref={box}>
                <AppLauncher
                    items={launcherItems}
                    onOpenChange={next => {
                        if (next) setOpen(false);
                    }}
                />
                <button className="nx-user" onClick={() => setOpen(v => !v)} aria-haspopup="menu" aria-expanded={open}>
                    <img src="/user.svg" alt="user avatar" />
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