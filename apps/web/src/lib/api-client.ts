let cachedCsrfToken: string | null = null;

async function getCsrfToken(): Promise<string> {
  if (cachedCsrfToken) return cachedCsrfToken;

  const response = await fetch("/api/csrf");
  const { token } = (await response.json()) as { token: string };
  cachedCsrfToken = token;
  return token;
}

/**
 * ASP.NET Core's antiforgery tokens are bound to authentication state — a
 * token issued while signed out is rejected once the session is
 * authenticated, and vice versa. Call this right after login/logout so the
 * next apiMutate() call fetches a token valid for the new auth state,
 * instead of replaying a now-stale cached one.
 */
export function invalidateCsrfToken(): void {
  cachedCsrfToken = null;
}

/** fetch wrapper for mutating requests — attaches the CSRF token the API's antiforgery middleware requires. */
export async function apiMutate(path: string, init: RequestInit = {}): Promise<Response> {
  const token = await getCsrfToken();
  return fetch(path, {
    ...init,
    headers: {
      ...(init.headers ?? {}),
      "X-CSRF-TOKEN": token,
    },
  });
}
