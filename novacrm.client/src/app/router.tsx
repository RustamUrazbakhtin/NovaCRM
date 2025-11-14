import type { ReactElement } from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import AuthPage from "../pages/AuthPage";
import Dashboard from "../pages/Dashboard";
import ClientsPage from "../pages/Clients";
import WorkersPage from "../pages/Workers";
import CalendarPage from "../pages/Calendar";
import StaffPage from "../pages/Staff";
import InventoryPage from "../pages/Inventory";
import AnalyticsPage from "../pages/Analytics";
import TasksPage from "../pages/Tasks";
import ReviewsPage from "../pages/Reviews";
import SettingsPage from "../pages/Settings";
import SettingsCompanyPage from "../pages/SettingsCompany";
import SettingsBillingPage from "../pages/SettingsBilling";
import ProfilePage from "../pages/ProfilePage";
import { isAuthenticated } from "./auth";

function Private({ children }: { children: ReactElement }) {
    return isAuthenticated() ? children : <Navigate to="/auth" replace />;
}

export default function Router() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/auth" element={<AuthPage />} />
                <Route path="/" element={<Private><Dashboard /></Private>} />
                <Route path="/clients" element={<Private><ClientsPage /></Private>} />
                <Route path="/calendar" element={<Private><CalendarPage /></Private>} />
                <Route path="/staff" element={<Private><StaffPage /></Private>} />
                <Route path="/workers" element={<Private><WorkersPage /></Private>} />
                <Route path="/inventory" element={<Private><InventoryPage /></Private>} />
                <Route path="/analytics" element={<Private><AnalyticsPage /></Private>} />
                <Route path="/tasks" element={<Private><TasksPage /></Private>} />
                <Route path="/reviews" element={<Private><ReviewsPage /></Private>} />
                <Route path="/profile" element={<Private><ProfilePage /></Private>} />
                <Route path="/settings" element={<Private><SettingsPage /></Private>} />
                <Route path="/settings/profile" element={<Private><Navigate to="/profile" replace /></Private>} />
                <Route path="/settings/company" element={<Private><SettingsCompanyPage /></Private>} />
                <Route path="/settings/billing" element={<Private><SettingsBillingPage /></Private>} />
                <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
        </BrowserRouter>
    );
}
