import { api } from "../app/auth";

export interface StaffLookup { id: string; name: string; code: string; }
export interface EnumOption { id: number; name: string; }
export interface StaffCompensation { compensationType: number; compensationTypeName: string; fixedSalary?: number | null; hourlyRate?: number | null; commissionPercent?: number | null; perServiceAmount?: number | null; effectiveFrom: string; effectiveTo?: string | null; notes?: string | null; }
export interface StaffDetails extends StaffItem {
  notes?: string | null;
}
export interface StaffItem {
  id: string; firstName: string; lastName: string; phone?: string | null; email?: string | null; isActive: boolean; employmentStatus: number; employmentStatusName: string;
  ratingAverage: number; ratingCount: number; branchId?: string | null; branchName?: string | null; todaySchedule?: string | null; appointmentsToday: number; appointmentsWeek: number;
  hasCrmAccess: boolean; userId?: string | null;
  roles: StaffLookup[]; specializations: StaffLookup[]; currentCompensation?: StaffCompensation | null;
}
export interface StaffOverview { totalStaff: number; activeToday: number; bookedToday: number; availableToday: number; avgRating: number; payrollThisMonth: number; }
export interface StaffListResponse { overview: StaffOverview; items: StaffItem[]; }
export interface StaffCatalog { roles: StaffLookup[]; specializations: StaffLookup[]; branches: StaffLookup[]; users: StaffLookup[]; statuses: EnumOption[]; compensationTypes: EnumOption[]; }
export interface UpsertStaffPayload {
  branchId?: string | null; hasCrmAccess: boolean; userId?: string | null; firstName: string; lastName: string; phone?: string | null; email?: string | null; notes?: string | null;
  isActive: boolean; status: number; ratingAverage?: number | null; ratingCount?: number | null; roleIds: string[]; specializationIds: string[];
  compensationType: number; fixedSalary?: number | null; hourlyRate?: number | null; commissionPercent?: number | null;
}
export async function getStaffList(search?: string, filter?: string) { const { data } = await api.get<StaffListResponse>("/staff", { params: { search, filter } }); return data; }
export async function getStaffCatalog() { const { data } = await api.get<StaffCatalog>("/staff/catalog"); return data; }
export async function getStaffById(id: string) { const { data } = await api.get<StaffDetails>(`/staff/${id}`); return data; }
export async function createStaff(payload: UpsertStaffPayload) { const { data } = await api.post("/staff", payload); return data; }
export async function updateStaff(id: string, payload: UpsertStaffPayload) { const { data } = await api.put(`/staff/${id}`, payload); return data; }
