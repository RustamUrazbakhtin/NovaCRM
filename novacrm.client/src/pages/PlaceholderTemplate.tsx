import type { ReactNode } from "react";
import { useNavigate } from "react-router-dom";
import Header from "../layout/Header";
import ThemeProvider from "../providers/ThemeProvider";
import { authApi } from "../app/auth";
import "../styles/dashboard/index.css";
import "../styles/placeholder.css";

type Props = {
    breadcrumb: string;
    title: string;
    description: string;
    children?: ReactNode;
};

export default function PlaceholderTemplate({ breadcrumb, title, description, children }: Props) {
    const navigate = useNavigate();

    const handleLogout = () => {
        authApi.logout();
        navigate("/auth", { replace: true });
    };

    return (
        <ThemeProvider>
            <Header breadcrumb={breadcrumb} onLogout={handleLogout} />
            <main className="fx-page fx-placeholder">
                <section className="fx-placeholder-panel">
                    <h1>{title}</h1>
                    <p>{description}</p>
                    {children}
                </section>
            </main>
        </ThemeProvider>
    );
}
