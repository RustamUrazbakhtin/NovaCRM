import { api } from "../app/auth";

export interface DashboardTodaySummary {
    appointmentsToday: number;
    newClientsThisWeek: number;
    noShows: number;
    completedVisitsToday: number;
}

export interface DashboardUpcomingAppointment {
    id: string;
    startTime: string;
    clientName: string;
    staffName?: string | null;
    status: string;
    date: string;
}

export interface DashboardAnalyticsRing {
    key: string;
    label: string;
    valuePercent: number;
    currentValue: number;
    targetValue?: number | null;
    description: string;
}

export interface DashboardAnalyticsSummary {
    rings: DashboardAnalyticsRing[];
    isPlaceholder: boolean;
}

export interface DashboardAccountingPreview {
    revenueThisMonth: number;
    payrollThisMonth?: number | null;
    pendingAmount?: number | null;
    formsConfiguredCount?: number | null;
    statusNote: string;
}

export interface DashboardStaffMember {
    id: string;
    name: string;
    status: string;
}

export interface DashboardStaffSummary {
    total: number;
    inService: number;
    onBreak: number;
    available: number;
    members: DashboardStaffMember[];
}

export interface DashboardCalendarCount {
    date: string;
    count: number;
}

export interface DashboardRecentClient {
    id: string;
    name: string;
    phone: string;
    createdAt: string;
}

export interface DashboardTagSummary {
    id: string;
    name: string;
    count: number;
}

export interface DashboardReviewSummary {
    averageRating: number;
    recentCount: number;
    previousPeriodAverage: number;
    trend: string;
}

export interface DashboardOverview {
    todaySummary: DashboardTodaySummary;
    upcomingSoon: DashboardUpcomingAppointment[];
    analyticsSummary: DashboardAnalyticsSummary;
    accountingPreview: DashboardAccountingPreview;
    staffSummary: DashboardStaffSummary;
    calendarCounts: DashboardCalendarCount[];
    recentClients: DashboardRecentClient[];
    clientSegments: DashboardTagSummary[];
    reviewsSummary: DashboardReviewSummary;
}

const emptyOverview: DashboardOverview = {
    todaySummary: { appointmentsToday: 0, newClientsThisWeek: 0, noShows: 0, completedVisitsToday: 0 },
    upcomingSoon: [],
    analyticsSummary: { rings: [], isPlaceholder: true },
    accountingPreview: { revenueThisMonth: 0, payrollThisMonth: null, pendingAmount: null, formsConfiguredCount: null, statusNote: "Accounting data not configured yet." },
    staffSummary: { total: 0, inService: 0, onBreak: 0, available: 0, members: [] },
    calendarCounts: [],
    recentClients: [],
    clientSegments: [],
    reviewsSummary: { averageRating: 0, recentCount: 0, previousPeriodAverage: 0, trend: "flat" },
};

export async function getDashboardOverview(signal?: AbortSignal): Promise<DashboardOverview> {
    const { data } = await api.get<DashboardOverview>("/dashboard/overview", { signal });
    return {
        ...emptyOverview,
        ...data,
        todaySummary: { ...emptyOverview.todaySummary, ...(data?.todaySummary ?? {}) },
        analyticsSummary: {
            ...emptyOverview.analyticsSummary,
            ...(data?.analyticsSummary ?? {}),
            rings: data?.analyticsSummary?.rings ?? [],
        },
        accountingPreview: { ...emptyOverview.accountingPreview, ...(data?.accountingPreview ?? {}) },
        staffSummary: {
            ...emptyOverview.staffSummary,
            ...(data?.staffSummary ?? {}),
            members: data?.staffSummary?.members ?? [],
        },
        reviewsSummary: { ...emptyOverview.reviewsSummary, ...(data?.reviewsSummary ?? {}) },
        upcomingSoon: data?.upcomingSoon ?? [],
        calendarCounts: data?.calendarCounts ?? [],
        recentClients: data?.recentClients ?? [],
        clientSegments: data?.clientSegments ?? [],
    };
}
