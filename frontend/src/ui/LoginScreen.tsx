import { useState } from "react";
import { login, register, AuthError } from "../api/auth";

interface Props {
  onLogin: (token: string) => void;
}

export default function LoginScreen({ onLogin }: Props) {
  const [isRegister, setIsRegister] = useState(false);
  const [username, setUsername] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      if (isRegister) {
        await register(username, email, password);
        // auto-login after successful registration
        const loginRes = await login(username, password);
        onLogin(loginRes.token);
      } else {
        const loginRes = await login(username, password);
        onLogin(loginRes.token);
      }
    } catch (err) {
      if (err instanceof AuthError) {
        setError(err.message);
      } else {
        setError("Connection failed");
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={styles.container}>
      <h1 style={styles.title}>conquerio</h1>
      <form onSubmit={handleSubmit} style={styles.form}>
        <input
          type="text"
          placeholder="username"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          style={styles.input}
          required
        />
        {isRegister && (
          <input
            type="email"
            placeholder="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            style={styles.input}
            required
          />
        )}
        <input
          type="password"
          placeholder="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          style={styles.input}
          required
        />
        {error && <div style={styles.error}>{error}</div>}
        <button type="submit" style={styles.button} disabled={loading}>
          {loading ? "..." : isRegister ? "register" : "play"}
        </button>
        <button
          type="button"
          style={styles.toggle}
          onClick={() => setIsRegister(!isRegister)}
        >
          {isRegister ? "have an account? login" : "no account? register"}
        </button>
      </form>
    </div>
  );
}

const styles: Record<string, React.CSSProperties> = {
  container: {
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    justifyContent: "center",
    height: "100vh",
    background: "#111",
    color: "#fff",
    fontFamily: "monospace",
  },
  title: {
    fontSize: "48px",
    marginBottom: "40px",
    letterSpacing: "4px",
  },
  form: {
    display: "flex",
    flexDirection: "column",
    gap: "12px",
    width: "280px",
  },
  input: {
    padding: "12px",
    background: "#222",
    border: "1px solid #333",
    color: "#fff",
    fontSize: "16px",
    fontFamily: "monospace",
    outline: "none",
  },
  button: {
    padding: "12px",
    background: "#fff",
    color: "#111",
    border: "none",
    fontSize: "16px",
    fontFamily: "monospace",
    cursor: "pointer",
    fontWeight: "bold",
  },
  toggle: {
    background: "none",
    border: "none",
    color: "#666",
    cursor: "pointer",
    fontSize: "13px",
    fontFamily: "monospace",
  },
  error: {
    color: "#e74c3c",
    fontSize: "13px",
  },
};
