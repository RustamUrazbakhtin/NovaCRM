import { useEffect, useMemo, useState } from "react";
import Header from "../layout/Header";
import ThemeProvider from "../providers/ThemeProvider";
import { authApi } from "../app/auth";
import { useNavigate } from "react-router-dom";
import { createStaff, getStaffById, getStaffCatalog, getStaffList, type StaffCatalog, type StaffItem, updateStaff, type UpsertStaffPayload } from "../api/staff";
import "../styles/dashboard/index.css";
import "../styles/clients/index.css";
import "../styles/staff/index.css";

const FILTERS = [
  ["all", "All"], ["active", "Active"], ["available", "Available"], ["busy", "Busy"], ["on-leave", "On leave"], ["admin", "Admin"], ["specialist", "Specialist"], ["owner", "Owner"], ["manager", "Manager"], ["top-rated", "Top rated"], ["upcoming", "Has upcoming appointments"]
] as const;

type CompensationType = "FixedSalary" | "HourlyRate" | "Commission";
const STATUS_OPTIONS = ["Active", "Vacation", "Terminated"] as const;

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
  compensationType: "FixedSalary",
  fixedSalary: null,
  hourlyRate: null,
  commissionPercent: null,
});

const normalizeCompensationType = (type?: string | null): CompensationType => {
  if (type === "Hourly" || type === "HourlyRate") return "HourlyRate";
  if (type === "Commission") return "Commission";
  return "FixedSalary";
};
const compensationTypeLabel = (type?: string | null): string => {
  const normalized = normalizeCompensationType(type);
  if (normalized === "FixedSalary") return "Fixed salary";
  if (normalized === "HourlyRate") return "Hourly rate";
  return "Commission";
};

const normalizeEmploymentStatus = (status?: string | null): typeof STATUS_OPTIONS[number] => {
  if (status === "OnLeave") return "Vacation";
  if (status === "Vacation" || status === "Terminated") return status;
  return "Active";
};

const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

const validateForm = (form: UpsertStaffPayload): string | null => {
  if (!form.firstName.trim()) return "First name is required.";
  if (!form.lastName.trim()) return "Last name is required.";

  if (form.email?.trim() && !emailRegex.test(form.email.trim())) {
    return "Enter a valid email address.";
  }

  const compensationType = normalizeCompensationType(form.compensationType);
  if (compensationType === "FixedSalary") {
    if (form.fixedSalary === null || form.fixedSalary === undefined || Number.isNaN(form.fixedSalary)) return "Monthly salary is required.";
    if (form.fixedSalary < 0) return "Monthly salary cannot be negative.";
  }

  if (compensationType === "HourlyRate") {
    if (form.hourlyRate === null || form.hourlyRate === undefined || Number.isNaN(form.hourlyRate)) return "Hourly rate is required.";
    if (form.hourlyRate < 0) return "Hourly rate cannot be negative.";
  }

  if (compensationType === "Commission") {
    if (form.commissionPercent === null || form.commissionPercent === undefined || Number.isNaN(form.commissionPercent)) return "Commission percent is required.";
    if (form.commissionPercent < 0 || form.commissionPercent > 100) return "Commission percent must be between 0 and 100.";
  }

  return null;
};

const buildPayload = (form: UpsertStaffPayload): UpsertStaffPayload => {
  const compensationType = normalizeCompensationType(form.compensationType);
  return {
    branchId: form.branchId,
    hasCrmAccess: form.hasCrmAccess,
    userId: form.hasCrmAccess ? (form.userId?.trim() || null) : null,
    firstName: form.firstName.trim(),
    lastName: form.lastName.trim(),
    phone: form.phone?.trim() || null,
    email: form.email?.trim() || null,
    notes: form.notes?.trim() || null,
    isActive: form.isActive,
    employmentStatus: normalizeEmploymentStatus(form.employmentStatus),
    ratingAverage: form.ratingAverage ?? null,
    ratingCount: form.ratingCount ?? null,
    roleIds: [...form.roleIds],
    specializationIds: [...form.specializationIds],
    compensationType,
    fixedSalary: compensationType === "FixedSalary" ? form.fixedSalary ?? null : null,
    hourlyRate: compensationType === "HourlyRate" ? form.hourlyRate ?? null : null,
    commissionPercent: compensationType === "Commission" ? form.commissionPercent ?? null : null,
  };
};

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
  const [isSaving, setIsSaving] = useState(false);
  const [saveMessage, setSaveMessage] = useState<string | null>(null);

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
    const validationError = validateForm(form);
    if (validationError) {
      setError(validationError);
      return;
    }
    setIsSaving(true);
    setError(null);
    try {
      const payload = buildPayload(form);

      if (editing) await updateStaff(editing.id, payload); else await createStaff(payload);
      setSaveMessage("Staff profile saved.");
      setOpen(false); setEditing(null); setForm(blankForm());
      await load();
    } catch {
      setError("Failed to save staff profile. Please try again.");
    } finally {
      setIsSaving(false);
    }
  };

  const openCreate = () => { setEditing(null); setForm(blankForm()); setSaveMessage(null); setOpen(true); };
  const openEdit = async (item: StaffItem) => {
    setEditing(item);
    try {
      const details = await getStaffById(item.id);
      const compensationType = normalizeCompensationType(details.currentCompensation?.compensationType);
      setForm({
        branchId: details.branchId ?? null,
        hasCrmAccess: details.hasCrmAccess,
        userId: details.userId,
        firstName: details.firstName,
        lastName: details.lastName,
        phone: details.phone,
        email: details.email,
        notes: details.notes ?? "",
        isActive: details.isActive,
        employmentStatus: normalizeEmploymentStatus(details.employmentStatus),
        roleIds: details.roles.map(r => r.id),
        specializationIds: details.specializations.map(s => s.id),
        compensationType,
        fixedSalary: compensationType === "FixedSalary" ? details.currentCompensation?.fixedSalary ?? null : null,
        hourlyRate: compensationType === "HourlyRate" ? details.currentCompensation?.hourlyRate ?? null : null,
        commissionPercent: compensationType === "Commission" ? details.currentCompensation?.commissionPercent ?? null : null,
      });
      setOpen(true);
      setSaveMessage(null);
    } catch {
      setError("Failed to load full staff details.");
      setEditing(null);
    }
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
        {!isLoading && !error && saveMessage ? <div className="clients-empty"><p>{saveMessage}</p></div> : null}
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
                  <td>{item.currentCompensation ? compensationTypeLabel(item.currentCompensation.compensationType) : "—"}</td>
                  <td>{item.ratingAverage.toFixed(1)} ({item.ratingCount})</td>
                  <td><span className={`clients-status clients-status--${item.employmentStatus.toLowerCase()}`}>{item.employmentStatus}{item.hasCrmAccess ? " · CRM" : ""}</span></td>
                  <td><button className="clients-secondary" onClick={() => void openEdit(item)}>Edit</button></td>
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
          <label>Status<select value={normalizeEmploymentStatus(form.employmentStatus)} onChange={e => setForm(f => ({ ...f, employmentStatus: e.target.value }))}><option value="Active">Active</option><option value="Vacation">Vacation</option><option value="Terminated">Terminated</option></select></label>
          {shouldShowLocationField ? <label>Location<select value={form.branchId ?? ""} onChange={e => setForm(f => ({ ...f, branchId: e.target.value || null }))}><option value="">Not selected</option>{catalog.branches.map(b => <option key={b.id} value={b.id}>{b.name}</option>)}</select></label> : <label>Location<input value={singleLocation?.name ?? "Main location"} readOnly /></label>}
          <label className="clients-field-wide">Notes<textarea value={form.notes ?? ""} onChange={e => setForm(f => ({ ...f, notes: e.target.value }))} /></label>
        </div>

        <div><strong>Roles</strong><div className="clients-segments">{catalog.roles.map(r => <button key={r.id} type="button" className={`clients-segment ${form.roleIds.includes(r.id) ? "is-active" : ""}`} onClick={() => setForm(f => ({ ...f, roleIds: f.roleIds.includes(r.id) ? f.roleIds.filter(x => x !== r.id) : [...f.roleIds, r.id] }))}>{r.name}</button>)}</div></div>
        <div><strong>Specializations</strong><div className="clients-segments">{catalog.specializations.map(s => <button key={s.id} type="button" className={`clients-segment ${form.specializationIds.includes(s.id) ? "is-active" : ""}`} onClick={() => setForm(f => ({ ...f, specializationIds: f.specializationIds.includes(s.id) ? f.specializationIds.filter(x => x !== s.id) : [...f.specializationIds, s.id] }))}>{s.name}</button>)}</div></div>

        <div className="clients-form-grid">
          <label className="clients-field-wide staff-toggle-row"><span>Allow CRM access</span><button type="button" role="switch" aria-checked={form.hasCrmAccess} className={`staff-toggle ${form.hasCrmAccess ? "is-on" : ""}`} onClick={() => setForm(f => ({ ...f, hasCrmAccess: !f.hasCrmAccess, userId: !f.hasCrmAccess ? f.userId : null }))}><span /></button></label>
          {form.hasCrmAccess ? <label className="clients-field-wide">Linked user<select value={form.userId ?? ""} onChange={e => setForm(f => ({ ...f, userId: e.target.value || null }))}><option value="">Link existing user (optional)</option>{catalog.users.map(u => <option key={u.code} value={u.code}>{u.name}</option>)}</select></label> : null}
          <label>Compensation type<select value={normalizeCompensationType(form.compensationType)} onChange={e => setForm(f => {
            const compensationType = normalizeCompensationType(e.target.value);
            return {
              ...f,
              compensationType,
              fixedSalary: compensationType === "FixedSalary" ? f.fixedSalary : null,
              hourlyRate: compensationType === "HourlyRate" ? f.hourlyRate : null,
              commissionPercent: compensationType === "Commission" ? f.commissionPercent : null,
            };
          })}><option value="FixedSalary">Fixed salary</option><option value="HourlyRate">Hourly rate</option><option value="Commission">Commission</option></select></label>
          {normalizeCompensationType(form.compensationType) === "HourlyRate" ? <label>Hourly rate<input type="number" value={form.hourlyRate ?? ""} onChange={e => setForm(f => ({ ...f, hourlyRate: e.target.value === "" ? null : Number(e.target.value) }))} /></label> : null}
          {normalizeCompensationType(form.compensationType) === "Commission" ? <label>Commission %<input type="number" value={form.commissionPercent ?? ""} onChange={e => setForm(f => ({ ...f, commissionPercent: e.target.value === "" ? null : Number(e.target.value) }))} /></label> : null}
          {normalizeCompensationType(form.compensationType) === "FixedSalary" ? <label>Monthly salary<input type="number" value={form.fixedSalary ?? ""} onChange={e => setForm(f => ({ ...f, fixedSalary: e.target.value === "" ? null : Number(e.target.value) }))} /></label> : null}
        </div>

        <footer className="clients-modal__footer">
          <button className="clients-secondary" onClick={() => setOpen(false)} disabled={isSaving}>Cancel</button>
          <button className="clients-primary" onClick={() => void onSave()} disabled={isSaving}>{isSaving ? "Saving…" : "Save"}</button>
        </footer>
      </section>
    </div> : null}
  </ThemeProvider>;
}
