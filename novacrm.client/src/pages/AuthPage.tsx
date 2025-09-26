import { useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { authApi } from "../app/auth";
import { TextInput } from "../components/TextInput";
import Button from "../components/Button";

const schema = z.object({
    email: z.string().email("Enter a valid email"),
    password: z.string().min(6, "Min 6 characters")
});
type Form = z.infer<typeof schema>;

export default function AuthPage() {
    const [tab, setTab] = useState<"sign-in" | "sign-up">("sign-in");
    const { register, handleSubmit, formState: { errors, isSubmitting }, reset } =
        useForm<Form>({ resolver: zodResolver(schema) });

    const onSubmit = async (data: Form) => {
        if (tab === "sign-in") {
            await authApi.login(data.email, data.password);
            location.href = "/";
        } else {
            await authApi.register(data.email, data.password);
            setTab("sign-in");
            reset({ email: data.email, password: "" });
            alert("Account created. Please sign in.");
        }
    };

    return (
        <div className="centered">
            <div className="panel">
                <div className="stack" style={{ gap: 16 }}>
                    <div className="tabs">
                        <button className={`tab ${tab === "sign-in" ? "active" : ""}`} onClick={() => setTab("sign-in")}>Sign In</button>
                        <button className={`tab ${tab === "sign-up" ? "active" : ""}`} onClick={() => setTab("sign-up")}>Create Account</button>
                    </div>

                    <form className="stack" onSubmit={handleSubmit(onSubmit)}>
                        <TextInput type="email" placeholder="Email" {...register("email")} error={errors.email?.message} />
                        <TextInput type="password" placeholder="Password" {...register("password")} error={errors.password?.message} />
                        <Button disabled={isSubmitting}>{tab === "sign-in" ? "Sign In" : "Create Account"}</Button>
                        <div className="helper" style={{ textAlign: "center" }}>
                            {tab === "sign-in" ? "Forgot password? (add later)" : "By continuing you agree to the Terms"}
                        </div>
                    </form>
                </div>
            </div>
        </div>
    );
}
