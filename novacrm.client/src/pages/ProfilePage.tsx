import { useCallback, useEffect, useMemo, useRef, useState, type ChangeEvent, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import Header from "../layout/Header";
import ThemeProvider from "../providers/ThemeProvider";
import { authApi } from "../app/auth";
import {
    deleteAvatar,
    getProfile,
    getProfileRoles,
    isAxiosValidationError,
    updateProfile,
    uploadAvatar,
    type ProfileRole,
    type UpdateProfilePayload,
    type UserProfile,
} from "../api/profile";
import "../styles/dashboard/index.css";
import "../styles/profile/profile.css";

const MAX_AVATAR_SIZE_BYTES = 2 * 1024 * 1024;
const ACCEPTED_AVATAR_TYPES = ["image/jpeg", "image/png", "image/jpg"];

const TIMEZONE_OPTIONS = [
    "UTC",
    "America/New_York",
    "America/Los_Angeles",
    "Europe/London",
    "Europe/Paris",
    "Europe/Berlin",
    "Europe/Moscow",
    "Asia/Tokyo",
    "Asia/Dubai",
];

const LOCALE_OPTIONS = [
    "en-US",
    "en-GB",
    "fr-FR",
    "de-DE",
    "ru-RU",
    "es-ES",
];

type ProfileFieldKey =
    | "firstName"
    | "lastName"
    | "email"
    | "phone"
    | "roleId"
    | "timezone"
    | "locale"
    | "address"
    | "notes";

type FieldErrors = Partial<Record<ProfileFieldKey, string>>;

function normalize(value: string | null | undefined) {
    return value?.trim() ?? "";
}

function buildUpdatePayload(profile: UserProfile): UpdateProfilePayload {
    const payload: UpdateProfilePayload = {
        firstName: profile.firstName.trim(),
        lastName: profile.lastName.trim(),
        email: profile.email.trim(),
    };

    const optionalFields: Array<[keyof UpdateProfilePayload, string | null | undefined]> = [
        ["phone", profile.phone ?? null],
        ["roleId", profile.roleId ?? null],
        ["timezone", profile.timezone ?? null],
        ["locale", profile.locale ?? null],
        ["address", profile.address ?? null],
        ["notes", profile.notes ?? null],
    ];

    for (const [key, value] of optionalFields) {
        const trimmed = value?.trim();
        payload[key] = trimmed ? trimmed : null;
    }

    return payload;
}

export default function ProfilePage() {
    const navigate = useNavigate();
    const [isLoading, setIsLoading] = useState(true);
    const [isSaving, setIsSaving] = useState(false);
    const [avatarUploading, setAvatarUploading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [toast, setToast] = useState<string | null>(null);
    const [avatarError, setAvatarError] = useState<string | null>(null);
    const [apiFieldErrors, setApiFieldErrors] = useState<FieldErrors>({});
    const [profile, setProfile] = useState<UserProfile | null>(null);
    const [editedProfile, setEditedProfile] = useState<UserProfile | null>(null);
    const [roles, setRoles] = useState<ProfileRole[]>([]);
    const [areRolesLoading, setAreRolesLoading] = useState(true);
    const [localAvatarPreview, setLocalAvatarPreview] = useState<string | null>(null);
    const avatarInputRef = useRef<HTMLInputElement>(null);

    useEffect(() => {
        const profileController = new AbortController();
        const rolesController = new AbortController();

        setIsLoading(true);
        getProfile(profileController.signal)
            .then(result => {
                setProfile(result);
                setEditedProfile({ ...result });
                setError(null);
            })
            .catch(err => {
                if (axios.isCancel(err)) return;
                setProfile(null);
                setEditedProfile(null);
                setError("Unable to load profile. Please try again later.");
            })
            .finally(() => setIsLoading(false));

        setAreRolesLoading(true);
        getProfileRoles(rolesController.signal)
            .then(result => {
                setRoles(result);
            })
            .catch(err => {
                if (axios.isCancel(err)) return;
                setRoles([]);
            })
            .finally(() => setAreRolesLoading(false));

        return () => {
            profileController.abort();
            rolesController.abort();
        };
    }, []);

    useEffect(() => {
        if (!toast) return undefined;
        const timeout = window.setTimeout(() => setToast(null), 4000);
        return () => window.clearTimeout(timeout);
    }, [toast]);

    useEffect(() => {
        return () => {
            if (localAvatarPreview) {
                URL.revokeObjectURL(localAvatarPreview);
            }
        };
    }, [localAvatarPreview]);

    const validationErrors: FieldErrors = useMemo(() => {
        if (!editedProfile) return {};
        const next: FieldErrors = {};
        if (!editedProfile.firstName || !editedProfile.firstName.trim()) {
            next.firstName = "First name is required.";
        }
        if (!editedProfile.lastName || !editedProfile.lastName.trim()) {
            next.lastName = "Last name is required.";
        }
        if (!editedProfile.email || !editedProfile.email.trim()) {
            next.email = "Email is required.";
        } else if (!/^\S+@\S+\.\S+$/.test(editedProfile.email.trim())) {
            next.email = "Enter a valid email address.";
        }
        const phone = editedProfile.phone?.trim();
        if (phone && !/^\+?[\d\s().-]{7,}$/.test(phone)) {
            next.phone = "Enter a valid phone number.";
        }
        return next;
    }, [editedProfile]);

    const combinedErrors: FieldErrors = useMemo(() => {
        return { ...validationErrors, ...apiFieldErrors };
    }, [apiFieldErrors, validationErrors]);

    const hasErrors = useMemo(() => Object.values(combinedErrors).some(Boolean), [combinedErrors]);

    const isDirty = useMemo(() => {
        if (!profile || !editedProfile) return false;
        const keys: ProfileFieldKey[] = [
            "firstName",
            "lastName",
            "email",
            "phone",
            "roleId",
            "timezone",
            "locale",
            "address",
            "notes",
        ];

        return keys.some(key => {
            const typedKey = key as keyof UserProfile;
            return normalize(profile[typedKey]) !== normalize(editedProfile[typedKey]);
        });
    }, [profile, editedProfile]);

    const displayName = useMemo(() => {
        if (!editedProfile) return "";
        const parts = [editedProfile.firstName, editedProfile.lastName].map(part => part?.trim()).filter(Boolean);
        return parts.length ? parts.join(" ") : "Your profile";
    }, [editedProfile]);

    const resolvedRoleName = useMemo(() => {
        if (!editedProfile) return "";
        if (editedProfile.roleName?.trim()) {
            return editedProfile.roleName.trim();
        }
        if (editedProfile.roleId) {
            const match = roles.find(role => role.id === editedProfile.roleId);
            return match?.name ?? "";
        }
        return "";
    }, [editedProfile, roles]);

    const fallbackRoleOption = useMemo(() => {
        if (!editedProfile?.roleId) {
            return null;
        }

        const existsInList = roles.some(role => role.id === editedProfile.roleId);
        if (existsInList) {
            return null;
        }

        return {
            id: editedProfile.roleId,
            name: editedProfile.roleName ?? "Current role",
        };
    }, [editedProfile, roles]);

    const avatarUrl = localAvatarPreview ?? editedProfile?.avatarUrl ?? null;

    const handleLogout = useCallback(() => {
        authApi.logout();
        navigate("/auth", { replace: true });
    }, [navigate]);

    const handleFieldChange = useCallback(
        (field: Exclude<ProfileFieldKey, "roleId">) =>
            (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
                const value = event.target.value;
                setEditedProfile(prev => {
                    if (!prev) return prev;
                    return { ...prev, [field]: value };
                });
                setApiFieldErrors(prev => ({ ...prev, [field]: undefined }));
            },
        []
    );

    const handleRoleChange = useCallback(
        (event: ChangeEvent<HTMLSelectElement>) => {
            const value = event.target.value.trim();
            setEditedProfile(prev => {
                if (!prev) return prev;
                if (!value) {
                    return { ...prev, roleId: undefined, roleName: undefined };
                }

                const match = roles.find(role => role.id === value);
                return {
                    ...prev,
                    roleId: value,
                    roleName: match?.name ?? prev.roleName,
                };
            });
            setApiFieldErrors(prev => ({ ...prev, roleId: undefined }));
        },
        [roles]
    );

    const handleCancel = useCallback(() => {
        if (!profile) return;
        setEditedProfile({ ...profile });
        setApiFieldErrors({});
        setError(null);
        setAvatarError(null);
        if (localAvatarPreview) {
            URL.revokeObjectURL(localAvatarPreview);
            setLocalAvatarPreview(null);
        }
    }, [profile, localAvatarPreview]);

    const handleSubmit = useCallback(
        async (event: FormEvent<HTMLFormElement>) => {
            event.preventDefault();
            if (!editedProfile) return;
            if (Object.keys(validationErrors).length) {
                setError("Please fix the highlighted fields before saving.");
                return;
            }

            setIsSaving(true);
            setError(null);
            setApiFieldErrors({});

            try {
                const updated = await updateProfile(buildUpdatePayload(editedProfile));
                setProfile(updated);
                setEditedProfile({ ...updated });
                setToast("Profile saved");
            } catch (err) {
                if (isAxiosValidationError(err)) {
                    const payloadErrors: FieldErrors = {};
                    const responseErrors = err.response?.data?.errors;
                    if (responseErrors) {
                        for (const [key, messages] of Object.entries(responseErrors)) {
                            if (Array.isArray(messages) && messages.length > 0) {
                                const camelKey = key.charAt(0).toLowerCase() + key.slice(1);
                                payloadErrors[camelKey as ProfileFieldKey] = messages[0];
                            }
                        }
                    }
                    setApiFieldErrors(payloadErrors);
                    setError(err.response?.data?.detail || err.response?.data?.message || "Unable to save profile.");
                } else {
                    setError("Unable to save profile. Please try again.");
                }
            } finally {
                setIsSaving(false);
            }
        },
        [editedProfile, validationErrors]
    );

    const openFileDialog = useCallback(() => {
        avatarInputRef.current?.click();
    }, []);

    const handleAvatarChange = useCallback(
        async (event: ChangeEvent<HTMLInputElement>) => {
            const file = event.target.files?.[0];
            if (!file) return;

            if (!ACCEPTED_AVATAR_TYPES.includes(file.type)) {
                setAvatarError("Please choose a JPG or PNG image.");
                event.target.value = "";
                return;
            }

            if (file.size > MAX_AVATAR_SIZE_BYTES) {
                setAvatarError("Avatar must be 2 MB or smaller.");
                event.target.value = "";
                return;
            }

            setAvatarError(null);
            setError(null);

            if (localAvatarPreview) {
                URL.revokeObjectURL(localAvatarPreview);
            }
            const previewUrl = URL.createObjectURL(file);
            setLocalAvatarPreview(previewUrl);
            setAvatarUploading(true);

            try {
                const updated = await uploadAvatar(file);
                setProfile(updated);
                setEditedProfile({ ...updated });
                setToast("Avatar updated");
                setLocalAvatarPreview(null);
            } catch (err) {
                setLocalAvatarPreview(null);
                if (isAxiosValidationError(err)) {
                    setAvatarError(err.response?.data?.detail || err.response?.data?.message || "Unable to upload avatar.");
                } else {
                    setAvatarError("Unable to upload avatar. Please try again.");
                }
            } finally {
                setAvatarUploading(false);
                event.target.value = "";
            }
        },
        [localAvatarPreview]
    );

    const handleAvatarRemove = useCallback(async () => {
        setAvatarError(null);
        setError(null);
        setAvatarUploading(true);
        try {
            const updated = await deleteAvatar();
            setProfile(updated);
            setEditedProfile({ ...updated });
            setLocalAvatarPreview(null);
            setToast("Avatar removed");
        } catch (err) {
            if (isAxiosValidationError(err)) {
                setAvatarError(err.response?.data?.detail || err.response?.data?.message || "Unable to remove avatar.");
            } else {
                setAvatarError("Unable to remove avatar. Please try again.");
            }
        } finally {
            setAvatarUploading(false);
        }
    }, []);

    return (
        <ThemeProvider>
            <div className="profile-shell">
                <Header
                    breadcrumb="Profile"
                    onLogout={handleLogout}
                    userName={displayName || "Your profile"}
                    userEmail={editedProfile?.email || ""}
                />
                <main className="profile-page" aria-labelledby="profile-title">
                    <section className="profile-sheet">
                        <form className="profile-form" onSubmit={handleSubmit} noValidate>
                        <div className="profile-content">
                            <header className="profile-header">
                                <div className="profile-avatar">
                                    <button
                                        type="button"
                                        className="profile-avatar-button"
                                        onClick={openFileDialog}
                                        aria-label="Change avatar"
                                    >
                                        {avatarUrl ? (
                                            <img src={avatarUrl} alt="Current avatar" className="profile-avatar-image" />
                                        ) : (
                                            <span className="profile-avatar-initials">
                                                {displayName
                                                    .split(" ")
                                                    .filter(Boolean)
                                                    .map(part => part[0]?.toUpperCase())
                                                    .join("") || "U"}
                                            </span>
                                        )}
                                        {avatarUploading && <span className="profile-avatar-badge">...</span>}
                                    </button>
                                    <input
                                        ref={avatarInputRef}
                                        type="file"
                                        accept=".jpg,.jpeg,.png"
                                        className="visually-hidden"
                                        onChange={handleAvatarChange}
                                    />
                                    <div className="profile-avatar-actions">
                                        <button
                                            type="button"
                                            className="profile-avatar-action"
                                            onClick={openFileDialog}
                                        >
                                            Change photo
                                        </button>
                                        {(editedProfile?.avatarUrl || localAvatarPreview) && (
                                            <button
                                                type="button"
                                                className="profile-avatar-action"
                                                onClick={handleAvatarRemove}
                                            >
                                                Remove
                                            </button>
                                        )}
                                    </div>
                                    {avatarError && <div className="profile-error">{avatarError}</div>}
                                </div>
                                <div className="profile-meta">
                                    <h1 className="profile-title" id="profile-title">
                                        {displayName || "Your profile"}
                                    </h1>
                                    <p className="profile-subtitle">
                                        {resolvedRoleName || editedProfile?.email || "Update your personal details"}
                                    </p>
                                </div>
                            </header>

                            {isLoading ? (
                                <div className="profile-loading" aria-live="polite">
                                    Loading profile…
                                </div>
                            ) : (
                                <>
                                    <section className="profile-section" aria-labelledby="profile-section-primary">
                                        <div>
                                            <h2 className="profile-section-title" id="profile-section-primary">
                                                Primary information
                                            </h2>
                                            <p className="profile-section-caption">
                                                Keep your name and role current so teammates recognize you.
                                            </p>
                                        </div>
                                        <div className="profile-grid two-column">
                                            <div className="profile-field">
                                                <label className="profile-label" htmlFor="firstName">
                                                    First name
                                                </label>
                                                <input
                                                    id="firstName"
                                                    name="firstName"
                                                    className="profile-input"
                                                    value={editedProfile?.firstName ?? ""}
                                                    onChange={handleFieldChange("firstName")}
                                                    aria-invalid={Boolean(combinedErrors.firstName)}
                                                    required
                                                    autoComplete="given-name"
                                                />
                                                {combinedErrors.firstName && (
                                                    <div className="profile-error">{combinedErrors.firstName}</div>
                                                )}
                                            </div>
                                            <div className="profile-field">
                                                <label className="profile-label" htmlFor="lastName">
                                                    Last name
                                                </label>
                                                <input
                                                    id="lastName"
                                                    name="lastName"
                                                    className="profile-input"
                                                    value={editedProfile?.lastName ?? ""}
                                                    onChange={handleFieldChange("lastName")}
                                                    aria-invalid={Boolean(combinedErrors.lastName)}
                                                    required
                                                    autoComplete="family-name"
                                                />
                                                {combinedErrors.lastName && (
                                                    <div className="profile-error">{combinedErrors.lastName}</div>
                                                )}
                                            </div>
                                            <div className="profile-field">
                                                <label className="profile-label" htmlFor="role">
                                                    Role
                                                </label>
                                                <select
                                                    id="role"
                                                    name="role"
                                                    className="profile-select"
                                                    value={editedProfile?.roleId ?? ""}
                                                    onChange={handleRoleChange}
                                                    aria-invalid={Boolean(combinedErrors.roleId)}
                                                    disabled={areRolesLoading && roles.length === 0}
                                                >
                                                    {areRolesLoading && roles.length === 0 && (
                                                        <option value="" disabled>
                                                            Loading roles...
                                                        </option>
                                                    )}
                                                    {!areRolesLoading && (
                                                        <option value="">Select a role</option>
                                                    )}
                                                    {fallbackRoleOption && (
                                                        <option value={fallbackRoleOption.id}>
                                                            {fallbackRoleOption.name}
                                                        </option>
                                                    )}
                                                    {roles.map(role => (
                                                        <option key={role.id} value={role.id}>
                                                            {role.name}
                                                        </option>
                                                    ))}
                                                </select>
                                                {combinedErrors.roleId && (
                                                    <div className="profile-error">{combinedErrors.roleId}</div>
                                                )}
                                            </div>
                                            <div className="profile-field">
                                                <label className="profile-label" htmlFor="company">
                                                    Company
                                                </label>
                                                <input
                                                    id="company"
                                                    name="company"
                                                    className="profile-input"
                                                    value={editedProfile?.companyName ?? ""}
                                                    readOnly
                                                    aria-readonly="true"
                                                    autoComplete="organization"
                                                />
                                                <p className="profile-helper">Company can be edited in Organization settings.</p>
                                            </div>
                                        </div>
                                    </section>

                                    <section className="profile-section" aria-labelledby="profile-section-contact">
                                        <div>
                                            <h2 className="profile-section-title" id="profile-section-contact">
                                                Contact details
                                            </h2>
                                            <p className="profile-section-caption">
                                                These details are used for notifications and account recovery.
                                            </p>
                                        </div>
                                        <div className="profile-grid two-column">
                                            <div className="profile-field">
                                                <label className="profile-label" htmlFor="email">
                                                    Email
                                                </label>
                                                <input
                                                    id="email"
                                                    name="email"
                                                    className="profile-input"
                                                    type="email"
                                                    value={editedProfile?.email ?? ""}
                                                    onChange={handleFieldChange("email")}
                                                    aria-invalid={Boolean(combinedErrors.email)}
                                                    required
                                                    autoComplete="email"
                                                />
                                                {combinedErrors.email && <div className="profile-error">{combinedErrors.email}</div>}
                                            </div>
                                            <div className="profile-field">
                                                <label className="profile-label" htmlFor="phone">
                                                    Phone
                                                </label>
                                                <input
                                                    id="phone"
                                                    name="phone"
                                                    className="profile-input"
                                                    type="tel"
                                                    value={editedProfile?.phone ?? ""}
                                                    onChange={handleFieldChange("phone")}
                                                    aria-invalid={Boolean(combinedErrors.phone)}
                                                    autoComplete="tel"
                                                />
                                                {combinedErrors.phone && <div className="profile-error">{combinedErrors.phone}</div>}
                                            </div>
                                        </div>
                                    </section>

                                    <section className="profile-section" aria-labelledby="profile-section-preferences">
                                        <div>
                                            <h2 className="profile-section-title" id="profile-section-preferences">
                                                Preferences
                                            </h2>
                                            <p className="profile-section-caption">
                                                Tailor NovaCRM to your schedule and language.
                                            </p>
                                        </div>
                                        <div className="profile-grid two-column">
                                            <div className="profile-field">
                                                <label className="profile-label" htmlFor="timezone">
                                                    Timezone
                                                </label>
                                                <select
                                                    id="timezone"
                                                    name="timezone"
                                                    className="profile-select"
                                                    value={editedProfile?.timezone ?? ""}
                                                    onChange={handleFieldChange("timezone")}
                                                    aria-invalid={Boolean(combinedErrors.timezone)}
                                                >
                                                    <option value="">Select a timezone</option>
                                                    {TIMEZONE_OPTIONS.map(option => (
                                                        <option key={option} value={option}>
                                                            {option}
                                                        </option>
                                                    ))}
                                                </select>
                                                {combinedErrors.timezone && (
                                                    <div className="profile-error">{combinedErrors.timezone}</div>
                                                )}
                                            </div>
                                            <div className="profile-field">
                                                <label className="profile-label" htmlFor="locale">
                                                    Locale
                                                </label>
                                                <select
                                                    id="locale"
                                                    name="locale"
                                                    className="profile-select"
                                                    value={editedProfile?.locale ?? ""}
                                                    onChange={handleFieldChange("locale")}
                                                    aria-invalid={Boolean(combinedErrors.locale)}
                                                >
                                                    <option value="">Select a language</option>
                                                    {LOCALE_OPTIONS.map(option => (
                                                        <option key={option} value={option}>
                                                            {option}
                                                        </option>
                                                    ))}
                                                </select>
                                                {combinedErrors.locale && <div className="profile-error">{combinedErrors.locale}</div>}
                                            </div>
                                        </div>
                                    </section>

                                    <section className="profile-section" aria-labelledby="profile-section-other">
                                        <div>
                                            <h2 className="profile-section-title" id="profile-section-other">
                                                Other details
                                            </h2>
                                            <p className="profile-section-caption">
                                                Share anything your teammates should know.
                                            </p>
                                        </div>
                                        <div className="profile-grid single-column">
                                            <div className="profile-field">
                                                <label className="profile-label" htmlFor="address">
                                                    Address
                                                </label>
                                                <textarea
                                                    id="address"
                                                    name="address"
                                                    className="profile-textarea"
                                                    rows={3}
                                                    value={editedProfile?.address ?? ""}
                                                    onChange={handleFieldChange("address")}
                                                    aria-invalid={Boolean(combinedErrors.address)}
                                                />
                                                {combinedErrors.address && (
                                                    <div className="profile-error">{combinedErrors.address}</div>
                                                )}
                                            </div>
                                            <div className="profile-field">
                                                <label className="profile-label" htmlFor="notes">
                                                    Notes
                                                </label>
                                                <textarea
                                                    id="notes"
                                                    name="notes"
                                                    className="profile-textarea"
                                                    rows={4}
                                                    value={editedProfile?.notes ?? ""}
                                                    onChange={handleFieldChange("notes")}
                                                    aria-invalid={Boolean(combinedErrors.notes)}
                                                />
                                                {combinedErrors.notes && <div className="profile-error">{combinedErrors.notes}</div>}
                                            </div>
                                        </div>
                                    </section>
                                </>
                            )}
                        </div>

                        <div className="profile-actions">
                            <div className="profile-status" aria-live="polite">
                                {error && <div className="profile-message">{error}</div>}
                                {!error && toast && (
                                    <div className="profile-toast" role="status">
                                        {toast}
                                    </div>
                                )}
                                {isDirty && !error && !toast && (
                                    <span className="profile-unsaved">Unsaved changes</span>
                                )}
                            </div>
                            <div className="profile-buttons">
                                <button
                                    type="button"
                                    className="profile-button secondary"
                                    onClick={handleCancel}
                                    disabled={isSaving || avatarUploading || !isDirty}
                                >
                                    Cancel
                                </button>
                                <button
                                    type="submit"
                                    className="profile-button primary"
                                    disabled={isSaving || avatarUploading || hasErrors || !isDirty}
                                >
                                    {isSaving ? "Saving…" : "Save"}
                                </button>
                            </div>
                        </div>
                    </form>
                </section>
            </main>
        </div>
    </ThemeProvider>
    );
}
