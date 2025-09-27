import { createContext, useContext, useEffect, useMemo, useState } from "react";

type Theme = "light" | "dark" | "system";
type Ctx = { theme: Theme; setTheme: (t: Theme) => void; isDark: boolean };

const ThemeCtx = createContext<Ctx | null>(null);
export const useTheme = () => useContext(ThemeCtx)!;

export default function ThemeProvider({ children }: { children: React.ReactNode }) {
    const [theme, setTheme] = useState<Theme>(() => (localStorage.getItem("theme") as Theme) || "system");

    useEffect(() => {
        const root = document.documentElement;
        const sysDark = window.matchMedia("(prefers-color-scheme: dark)").matches;
        const dark = theme === "dark" || (theme === "system" && sysDark);
        root.classList.toggle("dark", dark);
        localStorage.setItem("theme", theme);
    }, [theme]);

    const value = useMemo(() => {
        const sysDark = window.matchMedia("(prefers-color-scheme: dark)").matches;
        const isDark = theme === "dark" || (theme === "system" && sysDark);
        return { theme, setTheme, isDark };
    }, [theme]);

    return <ThemeCtx.Provider value={value}>{children}</ThemeCtx.Provider>;
}