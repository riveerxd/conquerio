const API = "/api/auth";

export class AuthError extends Error {
  constructor(message: string) {
    super(message);
    this.name = "AuthError";
  }
}

export async function login(
  username: string,
  password: string
): Promise<{ token: string }> {
  const res = await fetch(`${API}/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ username, password }),
  });

  if (res.status === 401) {
    throw new AuthError("Wrong username or password");
  }
  if (!res.ok) {
    throw new AuthError("Server error, please try again later");
  }

  return res.json();
}

export async function register(
  username: string,
  email: string,
  password: string
): Promise<{ message: string }> {
  const res = await fetch(`${API}/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ username, email, password }),
  });

  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    const first: string = body?.errors?.[0] ?? "Registration failed";
    throw new AuthError(first);
  }

  return res.json();
}
