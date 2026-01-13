type ApiResult<T> = { success: true; data: T } | { success: false; error: { code: string; message: string } }

async function request<T>(path: string, init?: RequestInit): Promise<ApiResult<T>> {
  try {
    const response = await fetch(path, {
      headers: {
        'Content-Type': 'application/json',
        ...init?.headers,
      },
      ...init,
    })

    const json = await response.json().catch(() => null)

    if (!response.ok) {
      return {
        success: false,
        error: {
          code: json?.error?.code ?? 'UNKNOWN_ERROR',
          message: json?.error?.message ?? 'Unexpected error',
        },
      }
    }

    return json as ApiResult<T>
  } catch (error) {
    return {
      success: false,
      error: { code: 'NETWORK_ERROR', message: (error as Error).message },
    }
  }
}

export type LoginResponse = {
  accessToken: string
  expiresIn: number
  user: {
    id: string
    email: string
    name: string
  }
}

export type RegisterResponse = {
  userId: string
  email: string
  name: string
}

export async function login(body: { email: string; password: string }) {
  return request<LoginResponse>('/api/auth/login', {
    method: 'POST',
    body: JSON.stringify(body),
  })
}

export async function register(body: { name: string; email: string; password: string }) {
  return request<RegisterResponse>('/api/auth/register', {
    method: 'POST',
    body: JSON.stringify(body),
  })
}
