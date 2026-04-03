import { useEffect, useMemo, useState } from "react";
import Header from "../layout/Header";
import ThemeProvider from "../providers/ThemeProvider";
import { authApi } from "../app/auth";
import { useNavigate } from "react-router-dom";
import { createStaff, getStaffCatalog, getStaffList, type StaffCatalog, type StaffItem, updateStaff, type UpsertStaffPayload } from "../api/staff";
import "../styles/dashboard/index.css";
import "../styles/clients/index.css";

const FILTERS = [
  ["all", "All"], ["active", "Active"], ["available", "Available"], ["busy", "Busy"], ["on-leave", "On leave"], ["admin", "Admin"], ["specialist", "Specialist"], ["owner", "Owner"], ["manager", "Manager"], ["top-rated", "Top rated"], ["upcoming", "Has upcoming appointments"]
] as const;

const blankForm = (): UpsertStaffPayload => ({
  branchId: null,
  hasCrmAccess: false,
  userId: null,
  firstName: "",
  lastName: "",
  phone: "",
  email: "",
  notes: "",
  isActive: true,
  employmentStatus: "Available",
  roleIds: [],
  specializationIds: [],
  compensation: { compensationType: "Fixed", fixedSalary: 0, hourlyRate: null, commissionPercent: null, perServiceAmount: null, effectiveFrom: new Date().toISOString(), effectiveTo: null, notes: "" }
});

export default function StaffPage() {
  const navigate = useNavigate();
  const [data, setData] = useState<StaffItem[]>([]);
  const [search, setSearch] = useState("");
  const [filter, setFilter] = useState("all");
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [overview, setOverview] = useState({ totalStaff: 0, activeToday: 0, bookedToday: 0, availableToday: 0, avgRating: 0, payrollThisMonth: 0 });
  const [catalog, setCatalog] = useState<StaffCatalog>({ roles: [], specializations: [], branches: [], users: [] });
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState<StaffItem | null>(null);
  const [form, setForm] = useState<UpsertStaffPayload>(blankForm);

  const load = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const res = await getStaffList(search, filter);
      setData(res.items ?? []);
      setOverview(res.overview);
    } catch {
      setError("Could not load staff right now. Please retry.");
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => { void load(); }, [search, filter]);
  useEffect(() => {
    getStaffCatalog().then(setCatalog).catch(() => undefined);
  }, []);

  const logout = () => { authApi.logout(); navigate("/auth", { replace: true }); };

  const onSave = async () => {
    if (!form.firstName.trim() || !form.lastName.trim()) return;
    try {
      if (editing) await updateStaff(editing.id, form); else await createStaff(form);
      setOpen(false); setEditing(null); setForm(blankForm());
      await load();
    } catch {
      setError("Failed to save staff profile.");
    }
  };

  const openCreate = () => { setEditing(null); setForm(blankForm()); setOpen(true); };
  const openEdit = (item: StaffItem) => {
    setEditing(item);
    setForm({
      branchId: null,
      hasCrmAccess: item.hasCrmAccess,
      userId: item.userId,
      firstName: item.firstName,
      lastName: item.lastName,
      phone: item.phone,
      email: item.email,
      notes: "",
      isActive: item.isActive,
      employmentStatus: item.employmentStatus,
      roleIds: item.roles.map(r => r.id),
      specializationIds: item.specializations.map(s => s.id),
      compensation: item.currentCompensation ?? { compensationType: "Fixed", fixedSalary: 0, effectiveFrom: new Date().toISOString() }
    });
    setOpen(true);
  };

  const title = useMemo(() => `${overview.totalStaff} team members`, [overview.totalStaff]);

  return <ThemeProvider>
    <Header breadcrumb="Staff" onLogout={logout} />
    <main className="fx-page clients-page">
      <section className="clients-header"><h1>Staff</h1><p>Manage your team, roles, schedules, pay, performance, and CRM access in one place.</p></section>
      <section className="clients-stats-grid">
        <div className="client-stat-card"><span>Total Staff</span><strong>{overview.totalStaff}</strong></div>
        <div className="client-stat-card"><span>Active Today</span><strong>{overview.activeToday}</strong></div>
        <div className="client-stat-card"><span>Booked Today</span><strong>{overview.bookedToday}</strong></div>
        <div className="client-stat-card"><span>Available Today</span><strong>{overview.availableToday}</strong></div>
        <div className="client-stat-card"><span>Avg Rating</span><strong>{overview.avgRating.toFixed(1)}</strong></div>
        <div className="client-stat-card"><span>Payroll</span><strong>${Math.round(overview.payrollThisMonth).toLocaleString()}</strong></div>
      </section>
      <section className="clients-toolbar">
        <div className="workflow-filter-row">{FILTERS.map(([k, l]) => <button key={k} type="button" className={`workflow-chip ${filter === k ? "active" : ""}`} onClick={() => setFilter(k)}>{l}</button>)}</div>
        <div className="clients-controls-row">
          <input className="clients-search" placeholder="Search name, phone, email, role, specialization" value={search} onChange={e => setSearch(e.target.value)} />
          <button type="button" className="nx-btn primary" onClick={openCreate}>Add Staff</button>
        </div>
      </section>
      <section className="clients-shell">
        <div className="clients-list-panel"><h2>{title}</h2>
          {isLoading ? <div className="clients-empty"><p>Loading staff…</p></div> : error ? <div className="clients-empty"><p>{error}</p><button className="nx-btn ghost" onClick={() => void load()}>Retry</button></div> : data.length === 0 ? <div className="clients-empty"><p>No staff yet. Add your first team member to start scheduling and CRM access.</p><button type="button" className="nx-btn primary" onClick={openCreate}>Add first staff</button></div> :
            <table className="clients-table"><thead><tr><th>Name</th><th>Branch</th><th>Roles</th><th>Specializations</th><th>Schedule</th><th>Appointments</th><th>Compensation</th><th>Rating</th><th>Status</th><th /></tr></thead>
              <tbody>{data.map(item => <tr key={item.id}><td><strong>{item.firstName} {item.lastName}</strong><div>{item.phone} · {item.email}</div></td>
                <td>{item.branchName ?? "—"}</td>
                <td>{item.roles.map(r => <span key={r.id} className="status-pill">{r.name}</span>)}</td>
                <td>{item.specializations.slice(0, 3).map(s => <span key={s.id} className="status-pill">{s.name}</span>)}{item.specializations.length > 3 && <span className="status-pill">+{item.specializations.length - 3}</span>}</td>
                <td>{item.todaySchedule ?? "—"}</td><td>{item.appointmentsToday} today / {item.appointmentsWeek} week</td><td>{item.currentCompensation?.compensationType ?? "—"}</td><td>{item.ratingAverage.toFixed(1)} ({item.ratingCount})</td><td>{item.employmentStatus}{item.hasCrmAccess ? " · CRM" : ""}</td><td><button className="nx-btn ghost" onClick={() => openEdit(item)}>Edit</button></td></tr>)}</tbody></table>}
        </div>
      </section>
    </main>
    {open && <div className="clients-modal-overlay" onClick={() => setOpen(false)}><section className="clients-modal" onClick={e => e.stopPropagation()}>
      <h3>{editing ? "Edit staff" : "Add staff"}</h3>
      <div className="form-grid" style={{ display: "grid", gap: 12, gridTemplateColumns: "1fr 1fr" }}>
        <input placeholder="First name" value={form.firstName} onChange={e => setForm(f => ({ ...f, firstName: e.target.value }))} />
        <input placeholder="Last name" value={form.lastName} onChange={e => setForm(f => ({ ...f, lastName: e.target.value }))} />
        <input placeholder="Phone" value={form.phone ?? ""} onChange={e => setForm(f => ({ ...f, phone: e.target.value }))} />
        <input placeholder="Email" value={form.email ?? ""} onChange={e => setForm(f => ({ ...f, email: e.target.value }))} />
        <select value={form.employmentStatus} onChange={e => setForm(f => ({ ...f, employmentStatus: e.target.value }))}><option>Available</option><option>Busy</option><option>OnLeave</option><option>Active</option></select>
        <select value={form.branchId ?? ""} onChange={e => setForm(f => ({ ...f, branchId: e.target.value || null }))}><option value="">Branch</option>{catalog.branches.map(b => <option key={b.id} value={b.id}>{b.name}</option>)}</select>
      </div>
      <div style={{ marginTop: 10 }}><strong>Roles</strong><div className="workflow-filter-row">{catalog.roles.map(r => <button key={r.id} type="button" className={`workflow-chip ${form.roleIds.includes(r.id) ? "active" : ""}`} onClick={() => setForm(f => ({ ...f, roleIds: f.roleIds.includes(r.id) ? f.roleIds.filter(x => x !== r.id) : [...f.roleIds, r.id] }))}>{r.name}</button>)}</div></div>
      <div><strong>Specializations</strong><div className="workflow-filter-row">{catalog.specializations.map(s => <button key={s.id} type="button" className={`workflow-chip ${form.specializationIds.includes(s.id) ? "active" : ""}`} onClick={() => setForm(f => ({ ...f, specializationIds: f.specializationIds.includes(s.id) ? f.specializationIds.filter(x => x !== s.id) : [...f.specializationIds, s.id] }))}>{s.name}</button>)}</div></div>
      <div style={{ marginTop: 10 }}>
        <label style={{ display: "flex", gap: 8, alignItems: "center" }}><input type="checkbox" checked={form.hasCrmAccess} onChange={e => setForm(f => ({ ...f, hasCrmAccess: e.target.checked, userId: e.target.checked ? f.userId : null }))} />Allow CRM access</label>
        {form.hasCrmAccess && <select value={form.userId ?? ""} onChange={e => setForm(f => ({ ...f, userId: e.target.value || null }))}><option value="">Link user (optional)</option>{catalog.users.map(u => <option key={u.code} value={u.code}>{u.name}</option>)}</select>}
      </div>
      <div className="form-grid" style={{ display: "grid", gap: 12, gridTemplateColumns: "1fr 1fr", marginTop: 10 }}>
        <select value={form.compensation?.compensationType ?? "Fixed"} onChange={e => setForm(f => ({ ...f, compensation: { ...f.compensation, compensationType: e.target.value } }))}><option>Fixed</option><option>Hourly</option><option>Commission</option><option>Hybrid</option></select>
        <input type="number" placeholder="Fixed salary" value={form.compensation?.fixedSalary ?? ""} onChange={e => setForm(f => ({ ...f, compensation: { ...f.compensation, fixedSalary: Number(e.target.value) } }))} />
        <input type="number" placeholder="Hourly rate" value={form.compensation?.hourlyRate ?? ""} onChange={e => setForm(f => ({ ...f, compensation: { ...f.compensation, hourlyRate: Number(e.target.value) } }))} />
        <input type="number" placeholder="Commission %" value={form.compensation?.commissionPercent ?? ""} onChange={e => setForm(f => ({ ...f, compensation: { ...f.compensation, commissionPercent: Number(e.target.value) } }))} />
      </div>
      <textarea placeholder="Notes" value={form.notes ?? ""} onChange={e => setForm(f => ({ ...f, notes: e.target.value }))} style={{ width: "100%", minHeight: 90, marginTop: 10 }} />
      <div className="clients-modal-actions"><button className="nx-btn ghost" onClick={() => setOpen(false)}>Cancel</button><button className="nx-btn primary" onClick={() => void onSave()}>Save</button></div>
    </section></div>}
  </ThemeProvider>;
}
