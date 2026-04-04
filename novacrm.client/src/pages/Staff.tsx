import { useEffect, useMemo, useState } from "react";
import Header from "../layout/Header";
import ThemeProvider from "../providers/ThemeProvider";
import { authApi } from "../app/auth";
import { useNavigate } from "react-router-dom";
import { createStaff, getStaffCatalog, getStaffList, type StaffCatalog, type StaffItem, updateStaff, type UpsertStaffPayload } from "../api/staff";
import "../styles/dashboard/index.css";
import "../styles/clients/index.css";
import "../styles/staff/index.css";

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
  employmentStatus: "Active",
  roleIds: [],
  specializationIds: [],
  compensation: { compensationType: "Fixed", fixedSalary: 0, hourlyRate: null, commissionPercent: null, perServiceAmount: null, effectiveFrom: new Date().toISOString(), effectiveTo: null, notes: "" }
});

const initials = (firstName: string, lastName: string) => `${firstName[0] ?? ""}${lastName[0] ?? ""}`.toUpperCase();

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
      setError("We couldn’t load staff right now. Please retry.");
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
      const nextComp = form.compensation
        ? {
          ...form.compensation,
          fixedSalary: form.compensation.compensationType === "Fixed" ? form.compensation.fixedSalary ?? null : null,
          hourlyRate: form.compensation.compensationType === "Hourly" ? form.compensation.hourlyRate ?? null : null,
          commissionPercent: form.compensation.compensationType === "Commission" ? form.compensation.commissionPercent ?? null : null,
          perServiceAmount: null,
        }
        : null;
      const payload = { ...form, compensation: nextComp };

      if (editing) await updateStaff(editing.id, payload); else await createStaff(payload);
      setOpen(false); setEditing(null); setForm(blankForm());
      await load();
    } catch {
      setError("Failed to save staff profile. Please try again.");
    }
  };

  const openCreate = () => { setEditing(null); setForm(blankForm()); setOpen(true); };
  const openEdit = (item: StaffItem) => {
    setEditing(item);
    setForm({
      branchId: item.branchId ?? null,
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
      compensation: item.currentCompensation
        ? {
          ...item.currentCompensation,
          compensationType: ["Fixed", "Hourly", "Commission"].includes(item.currentCompensation.compensationType) ? item.currentCompensation.compensationType : "Fixed"
        }
        : { compensationType: "Fixed", fixedSalary: 0, effectiveFrom: new Date().toISOString() }
    });
    setOpen(true);
  };

  const staffTitle = useMemo(() => `${overview.totalStaff} team members`, [overview.totalStaff]);
  const shouldShowLocationField = catalog.branches.length > 1;
  const singleLocation = catalog.branches.length === 1 ? catalog.branches[0] : null;

  useEffect(() => {
    if (!open || editing || !singleLocation) return;
    setForm((f) => ({ ...f, branchId: f.branchId ?? singleLocation.id }));
  }, [open, editing, singleLocation]);

  return <ThemeProvider>
    <Header breadcrumb="Staff" onLogout={logout} />

    <main className="fx-page clients-page staff-page">
      <section className="clients-header">
        <h1>Staff</h1>
        <p>Manage your team, roles, schedules, pay, performance, and CRM access in one place.</p>
      </section>

      <section className="staff-kpi-grid" aria-label="Staff KPI cards">
        <article className="staff-kpi-card"><span>Total Staff</span><strong>{overview.totalStaff}</strong></article>
        <article className="staff-kpi-card"><span>Active Today</span><strong>{overview.activeToday}</strong></article>
        <article className="staff-kpi-card"><span>Booked Today</span><strong>{overview.bookedToday}</strong></article>
        <article className="staff-kpi-card"><span>Available Today</span><strong>{overview.availableToday}</strong></article>
        <article className="staff-kpi-card"><span>Avg Rating</span><strong>{overview.avgRating.toFixed(1)}</strong></article>
        <article className="staff-kpi-card"><span>Payroll</span><strong>${Math.round(overview.payrollThisMonth).toLocaleString()}</strong></article>
      </section>

      <section className="clients-widget staff-toolbar-shell">
        <div className="clients-workflow-filters">
          {FILTERS.map(([k, l]) => <button key={k} type="button" className={`clients-segment ${filter === k ? "is-active" : ""}`} onClick={() => setFilter(k)}>{l}</button>)}
        </div>
        <div className="clients-toolbar__actions staff-toolbar-actions">
          <div className="clients-search-wrap">
            <span className="clients-search-icon">⌕</span>
            <input className="clients-search" placeholder="Search name, phone, email, role, specialization" value={search} onChange={e => setSearch(e.target.value)} />
          </div>
          <button type="button" className="clients-add" onClick={openCreate}><span className="clients-add__text">Add Staff</span><span className="clients-add__icon">＋</span></button>
        </div>
      </section>

      <section className="clients-widget">
        <div className="clients-widget__header">
          <div className="clients-heading"><h2>{staffTitle}</h2></div>
        </div>

        {isLoading ? <div className="staff-state"><div className="clients-skeleton-row" /><p>Loading team data…</p></div> : null}
        {!isLoading && error ? <div className="clients-error"><span>{error}</span><button type="button" className="clients-retry" onClick={() => void load()}>Retry</button></div> : null}
        {!isLoading && !error && data.length === 0 ? <div className="clients-empty staff-state"><div className="clients-empty__icon">✦</div><h3>Your team is empty</h3><p>Add your first staff member to start scheduling, payroll, and CRM access control.</p><button type="button" className="clients-add" onClick={openCreate}><span className="clients-add__text">Add first staff</span></button></div> : null}

        {!isLoading && !error && data.length > 0 ? <div className="clients-table-card">
          <div className="clients-table-scroll">
            <table className="clients-table">
              <thead><tr><th>Staff</th><th>Branch</th><th>Roles</th><th>Specializations</th><th>Availability</th><th>Appointments</th><th>Compensation</th><th>Rating</th><th>Status</th><th /></tr></thead>
              <tbody>
                {data.map(item => <tr key={item.id}>
                  <td>
                    <div className="staff-person-cell">
                      <span className="clients-avatar">{initials(item.firstName, item.lastName)}</span>
                      <div>
                        <strong>{item.firstName} {item.lastName}</strong>
                        <div className="clients-client-meta">{item.phone ?? "—"} · {item.email ?? "—"}</div>
                      </div>
                    </div>
                  </td>
                  <td>{item.branchName ?? "—"}</td>
                  <td className="staff-chip-wrap">{item.roles.map(r => <span key={r.id} className="clients-status">{r.name}</span>)}</td>
                  <td className="staff-chip-wrap">{item.specializations.slice(0, 3).map(s => <span key={s.id} className="clients-status">{s.name}</span>)}{item.specializations.length > 3 && <span className="clients-status">+{item.specializations.length - 3}</span>}</td>
                  <td>{item.todaySchedule ?? "No schedule"}</td>
                  <td>{item.appointmentsToday} today / {item.appointmentsWeek} week</td>
                  <td>{item.currentCompensation?.compensationType ?? "—"}</td>
                  <td>{item.ratingAverage.toFixed(1)} ({item.ratingCount})</td>
                  <td><span className={`clients-status clients-status--${item.employmentStatus.toLowerCase()}`}>{item.employmentStatus}{item.hasCrmAccess ? " · CRM" : ""}</span></td>
                  <td><button className="clients-secondary" onClick={() => openEdit(item)}>Edit</button></td>
                </tr>)}
              </tbody>
            </table>
          </div>
        </div> : null}
      </section>
    </main>

    {open ? <div className="clients-modal" role="dialog" aria-modal="true">
      <div className="clients-modal__backdrop" onClick={() => setOpen(false)} />
      <section className="clients-modal__content">
        <div className="clients-modal__header">
          <div><h2>{editing ? "Edit staff" : "Add staff"}</h2><p>Profile, roles, compensation, and CRM access</p></div>
          <button className="clients-modal__close" onClick={() => setOpen(false)}>✕</button>
        </div>

        <div className="clients-form-grid">
          <label>First name<input value={form.firstName} onChange={e => setForm(f => ({ ...f, firstName: e.target.value }))} /></label>
          <label>Last name<input value={form.lastName} onChange={e => setForm(f => ({ ...f, lastName: e.target.value }))} /></label>
          <label>Phone<input value={form.phone ?? ""} onChange={e => setForm(f => ({ ...f, phone: e.target.value }))} /></label>
          <label>Email<input value={form.email ?? ""} onChange={e => setForm(f => ({ ...f, email: e.target.value }))} /></label>
          <label>Status<select value={form.employmentStatus} onChange={e => setForm(f => ({ ...f, employmentStatus: e.target.value }))}><option value="Active">Active</option><option value="OnLeave">On leave</option><option value="Terminated">Terminated</option></select></label>
          {shouldShowLocationField ? <label>Location<select value={form.branchId ?? ""} onChange={e => setForm(f => ({ ...f, branchId: e.target.value || null }))}><option value="">Not selected</option>{catalog.branches.map(b => <option key={b.id} value={b.id}>{b.name}</option>)}</select></label> : <label>Location<input value={singleLocation?.name ?? "Main location"} readOnly /></label>}
          <label className="clients-field-wide">Notes<textarea value={form.notes ?? ""} onChange={e => setForm(f => ({ ...f, notes: e.target.value }))} /></label>
        </div>

        <div><strong>Roles</strong><div className="clients-segments">{catalog.roles.map(r => <button key={r.id} type="button" className={`clients-segment ${form.roleIds.includes(r.id) ? "is-active" : ""}`} onClick={() => setForm(f => ({ ...f, roleIds: f.roleIds.includes(r.id) ? f.roleIds.filter(x => x !== r.id) : [...f.roleIds, r.id] }))}>{r.name}</button>)}</div></div>
        <div><strong>Specializations</strong><div className="clients-segments">{catalog.specializations.map(s => <button key={s.id} type="button" className={`clients-segment ${form.specializationIds.includes(s.id) ? "is-active" : ""}`} onClick={() => setForm(f => ({ ...f, specializationIds: f.specializationIds.includes(s.id) ? f.specializationIds.filter(x => x !== s.id) : [...f.specializationIds, s.id] }))}>{s.name}</button>)}</div></div>

        <div className="clients-form-grid">
          <label className="clients-field-wide staff-toggle-row"><span>Allow CRM access</span><button type="button" role="switch" aria-checked={form.hasCrmAccess} className={`staff-toggle ${form.hasCrmAccess ? "is-on" : ""}`} onClick={() => setForm(f => ({ ...f, hasCrmAccess: !f.hasCrmAccess, userId: !f.hasCrmAccess ? f.userId : null }))}><span /></button></label>
          {form.hasCrmAccess ? <label className="clients-field-wide">Linked user<select value={form.userId ?? ""} onChange={e => setForm(f => ({ ...f, userId: e.target.value || null }))}><option value="">Link existing user (optional)</option>{catalog.users.map(u => <option key={u.code} value={u.code}>{u.name}</option>)}</select></label> : null}
          <label>Compensation type<select value={form.compensation?.compensationType ?? "Fixed"} onChange={e => setForm(f => ({ ...f, compensation: { ...f.compensation, compensationType: e.target.value } }))}><option value="Fixed">Fixed salary</option><option value="Hourly">Hourly rate</option><option value="Commission">Commission</option></select></label>
          {form.compensation?.compensationType === "Hourly" ? <label>Hourly rate<input type="number" value={form.compensation?.hourlyRate ?? ""} onChange={e => setForm(f => ({ ...f, compensation: { ...f.compensation, hourlyRate: Number(e.target.value) } }))} /></label> : null}
          {form.compensation?.compensationType === "Commission" ? <label>Commission %<input type="number" value={form.compensation?.commissionPercent ?? ""} onChange={e => setForm(f => ({ ...f, compensation: { ...f.compensation, commissionPercent: Number(e.target.value) } }))} /></label> : null}
          {(!form.compensation?.compensationType || form.compensation.compensationType === "Fixed") ? <label>Monthly salary<input type="number" value={form.compensation?.fixedSalary ?? ""} onChange={e => setForm(f => ({ ...f, compensation: { ...f.compensation, fixedSalary: Number(e.target.value) } }))} /></label> : null}
        </div>

        <footer className="clients-modal__footer">
          <button className="clients-secondary" onClick={() => setOpen(false)}>Cancel</button>
          <button className="clients-primary" onClick={() => void onSave()}>Save</button>
        </footer>
      </section>
    </div> : null}
  </ThemeProvider>;
}
