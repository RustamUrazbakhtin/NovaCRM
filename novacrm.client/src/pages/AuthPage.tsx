import axios from "axios";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { authApi } from "../app/auth";
import { TextInput } from "../components/TextInput";
import Button from "../components/Button";

const signInSchema = z.object({
    email: z.string().trim().email("Enter a valid email"),
    password: z.string().min(6, "Min 6 characters")
});

const signUpSchema = z.object({
    companyName: z.string().trim().min(1, "Required"),
    industry: z.string().trim().optional(),
    country: z.string().trim().min(1, "Required"),
    timezone: z.string().trim().min(1, "Required"),
    businessEmail: z.string().trim().email("Enter a valid email"),
    companyPhone: z.string().trim().optional(),
    branchName: z.string().trim().min(1, "Required"),
    branchAddress: z.string().trim().min(1, "Required"),
    branchCity: z.string().trim().min(1, "Required"),
    ownerFullName: z.string().trim().min(1, "Required"),
    ownerEmail: z.string().trim().email("Enter a valid email"),
    ownerPassword: z.string().min(6, "Min 6 characters")
});

type SignInForm = z.infer<typeof signInSchema>;
type SignUpForm = z.infer<typeof signUpSchema>;

export default function AuthPage() {
    const [tab, setTab] = useState<"sign-in" | "sign-up">("sign-in");
    const signInForm = useForm<SignInForm>({ resolver: zodResolver(signInSchema) });
    const signUpForm = useForm<SignUpForm>({ resolver: zodResolver(signUpSchema) });

    const handleSignIn = signInForm.handleSubmit(async data => {
        await authApi.login(data.email, data.password);
        location.href = "/";
    });

    const handleSignUp = signUpForm.handleSubmit(async data => {
        signUpForm.clearErrors("root");
        try {
            const payload = {
                companyName: data.companyName.trim(),
                industry: data.industry?.trim() || undefined,
                country: data.country.trim(),
                timezone: data.timezone.trim(),
                businessEmail: data.businessEmail.trim(),
                companyPhone: data.companyPhone?.trim() || undefined,
                branchName: data.branchName.trim(),
                branchAddress: data.branchAddress.trim(),
                branchCity: data.branchCity.trim(),
                ownerFullName: data.ownerFullName.trim(),
                ownerEmail: data.ownerEmail.trim(),
                ownerPassword: data.ownerPassword
            };

            await authApi.register(payload);

            const ownerEmail = payload.ownerEmail;
            signUpForm.reset();
            signInForm.reset({ email: ownerEmail, password: "" });
            setTab("sign-in");
            alert("Account created. Please sign in.");
        } catch (error) {
            if (axios.isAxiosError(error) && error.response?.status === 400 && error.response.data?.errors) {
                const fieldErrors = error.response.data.errors as Record<string, string[]>;
                signUpForm.clearErrors();
                const general: string[] = [];
                const values = signUpForm.getValues() as Record<string, unknown>;

                Object.entries(fieldErrors).forEach(([key, messages]) => {
                    const message = messages.join(" ");
                    const normalizedKey = key
                        .replace(/^\$\./, "")
                        .replace(/^registrationRequest\./i, "");

                    if (!normalizedKey) {
                        general.push(message);
                        return;
                    }

                    const camelKey = normalizedKey.length > 0
                        ? normalizedKey[0].toLowerCase() + normalizedKey.slice(1)
                        : normalizedKey;

                    if (camelKey in values) {
                        signUpForm.setError(camelKey as keyof SignUpForm, { type: "server", message });
                    } else {
                        general.push(message);
                    }
                });

                if (general.length) {
                    signUpForm.setError("root", { type: "server", message: general.join(" ") });
                }
            } else {
                signUpForm.setError("root", { type: "server", message: "Registration failed. Please try again." });
            }
        }
    });

    const showSignIn = () => {
        setTab("sign-in");
        signUpForm.clearErrors();
    };

    const showSignUp = () => {
        setTab("sign-up");
        signInForm.clearErrors();
    };

    return (
        <div className="centered">
            <div className="panel">
                <div className="stack" style={{ gap: 16 }}>
                    <div className="tabs">
                        <button
                            type="button"
                            className={`tab ${tab === "sign-in" ? "active" : ""}`}
                            onClick={showSignIn}
                        >
                            Sign In
                        </button>
                        <button
                            type="button"
                            className={`tab ${tab === "sign-up" ? "active" : ""}`}
                            onClick={showSignUp}
                        >
                            Create Account
                        </button>
                    </div>

                    <form className="stack" onSubmit={tab === "sign-in" ? handleSignIn : handleSignUp}>
                        {tab === "sign-in" ? (
                            <>
                                <TextInput
                                    type="email"
                                    placeholder="Email"
                                    {...signInForm.register("email")}
                                    error={signInForm.formState.errors.email?.message}
                                />
                                <TextInput
                                    type="password"
                                    placeholder="Password"
                                    {...signInForm.register("password")}
                                    error={signInForm.formState.errors.password?.message}
                                />
                            </>
                        ) : (
                            <>
                                <TextInput
                                    placeholder="Company name"
                                    {...signUpForm.register("companyName")}
                                    error={signUpForm.formState.errors.companyName?.message}
                                />
                                <TextInput
                                    placeholder="Industry"
                                    {...signUpForm.register("industry")}
                                    error={signUpForm.formState.errors.industry?.message}
                                />
                                <TextInput
                                    placeholder="Country"
                                    {...signUpForm.register("country")}
                                    error={signUpForm.formState.errors.country?.message}
                                />
                                <TextInput
                                    placeholder="Timezone"
                                    {...signUpForm.register("timezone")}
                                    error={signUpForm.formState.errors.timezone?.message}
                                />
                                <TextInput
                                    type="email"
                                    placeholder="Business email"
                                    {...signUpForm.register("businessEmail")}
                                    error={signUpForm.formState.errors.businessEmail?.message}
                                />
                                <TextInput
                                    placeholder="Company phone"
                                    {...signUpForm.register("companyPhone")}
                                    error={signUpForm.formState.errors.companyPhone?.message}
                                />
                                <TextInput
                                    placeholder="Branch name"
                                    {...signUpForm.register("branchName")}
                                    error={signUpForm.formState.errors.branchName?.message}
                                />
                                <TextInput
                                    placeholder="Branch address"
                                    {...signUpForm.register("branchAddress")}
                                    error={signUpForm.formState.errors.branchAddress?.message}
                                />
                                <TextInput
                                    placeholder="Branch city"
                                    {...signUpForm.register("branchCity")}
                                    error={signUpForm.formState.errors.branchCity?.message}
                                />
                                <TextInput
                                    placeholder="Owner full name"
                                    {...signUpForm.register("ownerFullName")}
                                    error={signUpForm.formState.errors.ownerFullName?.message}
                                />
                                <TextInput
                                    type="email"
                                    placeholder="Owner email"
                                    {...signUpForm.register("ownerEmail")}
                                    error={signUpForm.formState.errors.ownerEmail?.message}
                                />
                                <TextInput
                                    type="password"
                                    placeholder="Owner password"
                                    {...signUpForm.register("ownerPassword")}
                                    error={signUpForm.formState.errors.ownerPassword?.message}
                                />
                            </>
                        )}
                        <Button disabled={tab === "sign-in" ? signInForm.formState.isSubmitting : signUpForm.formState.isSubmitting}>
                            {tab === "sign-in" ? "Sign In" : "Create Account"}
                        </Button>
                        {tab === "sign-up" && signUpForm.formState.errors.root?.message && (
                            <div className="helper" role="alert">{signUpForm.formState.errors.root.message}</div>
                        )}
                        <div className="helper" style={{ textAlign: "center" }}>
                            {tab === "sign-in" ? "Forgot password? (add later)" : "By continuing you agree to the Terms"}
                        </div>
                    </form>
                </div>
            </div>
        </div>
    );
}
