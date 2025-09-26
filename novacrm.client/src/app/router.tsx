import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import AuthPage from "../pages/AuthPage";
import Dashboard from "../pages/Dashboard";
import { isAuthenticated } from "./auth";

function Private({ children }: { children: JSX.Element }) {
    return isAuthenticated() ? children : <Navigate to="/auth" replace />;
}

export default function Router() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/auth" element={<AuthPage />} />
                <Route path="/" element={<Private><Dashboard /></Private>} />
                <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
        </BrowserRouter>
    );
}
