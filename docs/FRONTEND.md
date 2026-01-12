# ⚛️ FRONTEND.md - React Architecture & Structure

**Status**: Foundation Phase
**Last Updated**: January 10, 2025

---

## 📌 Overview

The frontend is a React 19 + TypeScript application using:
- **State Management**: Zustand (lightweight, not Redux)
- **HTTP Client**: Axios
- **Styling**: TailwindCSS (utility-first)
- **Build Tool**: Vite
- **Package Manager**: pnpm (faster, more efficient)

---

## 📁 Folder Structure

```
/client
├── src/
│   ├── features/
│   │   ├── auth/
│   │   │   ├── components/
│   │   │   │   ├── LoginForm.tsx
│   │   │   │   ├── RegisterForm.tsx
│   │   │   │   └── VerifyEmailForm.tsx
│   │   │   ├── hooks/
│   │   │   │   ├── useAuth.ts
│   │   │   │   ├── useLogin.ts
│   │   │   │   └── useRegister.ts
│   │   │   ├── services/
│   │   │   │   └── authService.ts
│   │   │   ├── types/
│   │   │   │   └── auth.types.ts
│   │   │   └── AuthPage.tsx
│   │   │
│   │   ├── courses/
│   │   │   ├── components/
│   │   │   │   ├── CourseBuilder.tsx
│   │   │   │   ├── CourseList.tsx
│   │   │   │   ├── TopicForm.tsx
│   │   │   │   ├── SubtopicForm.tsx
│   │   │   │   ├── SessionForm.tsx
│   │   │   │   ├── TreeView.tsx (hierarchy)
│   │   │   │   └── ConfirmDelete.tsx
│   │   │   ├── hooks/
│   │   │   │   ├── useCourses.ts
│   │   │   │   ├── useTopics.ts
│   │   │   │   └── usePublish.ts
│   │   │   ├── services/
│   │   │   │   └── courseService.ts
│   │   │   ├── types/
│   │   │   │   └── course.types.ts
│   │   │   └── CoursesPage.tsx
│   │   │
│   │   └── dashboard/
│   │       ├── components/
│   │       │   ├── DashboardLayout.tsx
│   │       │   ├── Navigation.tsx
│   │       │   └── Sidebar.tsx
│   │       └── DashboardPage.tsx
│   │
│   ├── stores/
│   │   ├── authStore.ts
│   │   ├── courseStore.ts
│   │   ├── uiStore.ts (loading, notifications, etc.)
│   │   └── index.ts (barrel export)
│   │
│   ├── shared/
│   │   ├── components/
│   │   │   ├── Layout.tsx
│   │   │   ├── Header.tsx
│   │   │   ├── LoadingSpinner.tsx
│   │   │   ├── ErrorAlert.tsx
│   │   │   ├── Button.tsx
│   │   │   ├── Input.tsx
│   │   │   ├── Modal.tsx
│   │   │   └── Card.tsx
│   │   ├── hooks/
│   │   │   ├── useApi.ts (axios wrapper)
│   │   │   ├── useTranslate.ts (i18n)
│   │   │   ├── useNotification.ts (toast, alerts)
│   │   │   └── useLocalStorage.ts
│   │   ├── utils/
│   │   │   ├── apiClient.ts (axios instance)
│   │   │   ├── i18n.ts (translation loader)
│   │   │   ├── validation.ts
│   │   │   └── formatters.ts
│   │   └── types/
│   │       ├── api.types.ts
│   │       ├── error.types.ts
│   │       └── common.types.ts
│   │
│   ├── styles/
│   │   └── globals.css (TailwindCSS imports)
│   │
│   ├── types/
│   │   └── index.ts (global types)
│   │
│   ├── App.tsx
│   ├── App.css
│   ├── main.tsx
│   └── index.css
│
├── public/
│   └── locales/
│       ├── en/
│       │   ├── common.json
│       │   ├── auth.json
│       │   └── courses.json
│       └── es/
│           ├── common.json
│           ├── auth.json
│           └── courses.json
│
├── index.html
├── package.json
├── tsconfig.json
├── vite.config.ts
└── tailwind.config.ts
```

---

## 🎯 State Management with Zustand

### Auth Store
```typescript
// stores/authStore.ts
import { create } from 'zustand';
import { persist } from 'zustand/middleware';

export interface AuthState {
  // State
  user: User | null;
  accessToken: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;

  // Actions
  login: (email: string, password: string) => Promise<void>;
  register: (data: RegisterData) => Promise<void>;
  logout: () => Promise<void>;
  refreshToken: () => Promise<void>;
  clearError: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      // Initial state
      user: null,
      accessToken: null,
      isAuthenticated: false,
      isLoading: false,
      error: null,

      // Actions
      login: async (email: string, password: string) => {
        set({ isLoading: true, error: null });
        try {
          const response = await authService.login(email, password);
          set({
            user: response.user,
            accessToken: response.accessToken,
            isAuthenticated: true,
            isLoading: false,
          });
        } catch (error) {
          set({
            error: error.message,
            isLoading: false,
          });
          throw error;
        }
      },

      logout: async () => {
        try {
          await authService.logout();
        } finally {
          set({
            user: null,
            accessToken: null,
            isAuthenticated: false,
          });
        }
      },

      clearError: () => set({ error: null }),
    }),
    {
      name: 'auth-store',
      partialize: (state) => ({
        user: state.user,
        accessToken: state.accessToken,
      }),
    }
  )
);
```

### Course Store
```typescript
// stores/courseStore.ts
import { create } from 'zustand';

export interface CourseState {
  // State
  courses: Course[];
  currentCourse: Course | null;
  isLoading: boolean;
  error: string | null;

  // Actions
  fetchCourses: () => Promise<void>;
  fetchCourse: (id: string) => Promise<void>;
  createCourse: (data: CreateCourseData) => Promise<Course>;
  updateCourse: (id: string, data: UpdateCourseData) => Promise<void>;
  publishCourse: (id: string) => Promise<void>;
  deleteCourse: (id: string) => Promise<void>;
  clearError: () => void;
}

export const useCourseStore = create<CourseState>((set) => ({
  // Initial state
  courses: [],
  currentCourse: null,
  isLoading: false,
  error: null,

  // Actions
  fetchCourses: async () => {
    set({ isLoading: true });
    try {
      const response = await courseService.getAllCourses();
      set({ courses: response.data, isLoading: false });
    } catch (error) {
      set({ error: error.message, isLoading: false });
    }
  },

  createCourse: async (data) => {
    set({ isLoading: true });
    try {
      const response = await courseService.createCourse(data);
      set((state) => ({
        courses: [...state.courses, response.data],
        isLoading: false,
      }));
      return response.data;
    } catch (error) {
      set({ error: error.message, isLoading: false });
      throw error;
    }
  },

  clearError: () => set({ error: null }),
}));
```

---

## 🪝 Custom Hooks

### useApi (Axios Wrapper)
```typescript
// shared/hooks/useApi.ts
import { useState, useCallback } from 'react';
import { apiClient } from '../utils/apiClient';

export const useApi = <T,>(url: string) => {
  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetch = useCallback(async () => {
    setLoading(true);
    try {
      const response = await apiClient.get<T>(url);
      setData(response.data);
      setError(null);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }, [url]);

  return { data, loading, error, fetch };
};
```

### useAuth (Auth Hook)
```typescript
// features/auth/hooks/useAuth.ts
import { useAuthStore } from '../../../stores/authStore';

export const useAuth = () => {
  const { user, isAuthenticated, logout } = useAuthStore();

  const isAdmin = user?.role === 'Admin';
  const isStudent = user?.role === 'Student';

  return {
    user,
    isAuthenticated,
    isAdmin,
    isStudent,
    logout,
  };
};
```

### useTranslate (i18n Hook)
```typescript
// shared/hooks/useTranslate.ts
import { useEffect, useState } from 'react';

export const useTranslate = () => {
  const [language, setLanguage] = useState<'en' | 'es'>('en');
  const [translations, setTranslations] = useState<Record<string, any>>({});

  useEffect(() => {
    const lang = localStorage.getItem('language') || 'en';
    setLanguage(lang as 'en' | 'es');

    // Load translations
    loadTranslations(lang);
  }, []);

  const loadTranslations = async (lang: string) => {
    try {
      const response = await fetch(`/locales/${lang}/common.json`);
      const data = await response.json();
      setTranslations(data);
    } catch (error) {
      console.error('Failed to load translations:', error);
    }
  };

  const t = (key: string, defaultValue?: string) => {
    const keys = key.split('.');
    let value = translations;
    for (const k of keys) {
      value = value?.[k];
    }
    return value || defaultValue || key;
  };

  const switchLanguage = (lang: 'en' | 'es') => {
    setLanguage(lang);
    localStorage.setItem('language', lang);
    loadTranslations(lang);

    // Set Accept-Language header for API calls
    apiClient.defaults.headers.common['Accept-Language'] = lang;
  };

  return { language, t, switchLanguage };
};
```

---

## 🔌 API Service Pattern

### Auth Service
```typescript
// features/auth/services/authService.ts
import { apiClient } from '../../../shared/utils/apiClient';

export const authService = {
  async login(email: string, password: string) {
    const response = await apiClient.post('/api/auth/login', {
      email,
      password,
    });
    return response.data.data;
  },

  async register(data: RegisterData) {
    const response = await apiClient.post('/api/auth/register', data);
    return response.data.data;
  },

  async logout() {
    await apiClient.post('/api/auth/logout');
  },

  async verifyEmail(token: string) {
    const response = await apiClient.post('/api/auth/verify-email', {
      token,
    });
    return response.data.data;
  },

  async refreshToken() {
    const response = await apiClient.post('/api/auth/refresh');
    return response.data.data;
  },
};
```

### Course Service
```typescript
// features/courses/services/courseService.ts
import { apiClient } from '../../../shared/utils/apiClient';

export const courseService = {
  async getAllCourses(page = 1, pageSize = 10) {
    const response = await apiClient.get('/api/courses', {
      params: { page, pageSize },
    });
    return response.data.data;
  },

  async getCourse(courseId: string) {
    const response = await apiClient.get(`/api/courses/${courseId}`);
    return response.data.data;
  },

  async createCourse(data: CreateCourseData) {
    const response = await apiClient.post('/api/courses', data);
    return response.data.data;
  },

  async updateCourse(courseId: string, data: UpdateCourseData) {
    const response = await apiClient.put(`/api/courses/${courseId}`, data);
    return response.data.data;
  },

  async publishCourse(courseId: string) {
    const response = await apiClient.patch(`/api/courses/${courseId}/publish`);
    return response.data.data;
  },

  async deleteSession(sessionId: string) {
    await apiClient.delete(`/api/sessions/${sessionId}`);
  },
};
```

---

## 🎨 Component Examples

### Login Form
```typescript
// features/auth/components/LoginForm.tsx
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../../../stores/authStore';
import { useTranslate } from '../../../shared/hooks/useTranslate';

export const LoginForm = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');

  const navigate = useNavigate();
  const { login, isLoading } = useAuthStore();
  const { t } = useTranslate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    try {
      await login(email, password);
      navigate('/dashboard');
    } catch (err) {
      setError(err.message || 'Login failed');
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <input
        type="email"
        placeholder={t('auth.email')}
        value={email}
        onChange={(e) => setEmail(e.target.value)}
        className="w-full px-4 py-2 border rounded"
        required
      />
      <input
        type="password"
        placeholder={t('auth.password')}
        value={password}
        onChange={(e) => setPassword(e.target.value)}
        className="w-full px-4 py-2 border rounded"
        required
      />
      {error && <div className="text-red-600">{error}</div>}
      <button
        type="submit"
        disabled={isLoading}
        className="w-full px-4 py-2 bg-blue-600 text-white rounded disabled:opacity-50"
      >
        {isLoading ? 'Loading...' : t('auth.login')}
      </button>
    </form>
  );
};
```

### Course Builder
```typescript
// features/courses/components/CourseBuilder.tsx
import { useState, useEffect } from 'react';
import { useCourseStore } from '../../../stores/courseStore';
import { useTranslate } from '../../../shared/hooks/useTranslate';
import { TreeView } from './TreeView';
import { TopicForm } from './TopicForm';

export const CourseBuilder = ({ courseId }: { courseId: string }) => {
  const { currentCourse, fetchCourse, isLoading } = useCourseStore();
  const { t } = useTranslate();
  const [showAddTopic, setShowAddTopic] = useState(false);

  useEffect(() => {
    fetchCourse(courseId);
  }, [courseId]);

  if (isLoading) return <div>{t('common.loading')}</div>;
  if (!currentCourse) return <div>{t('common.notFound')}</div>;

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold">{currentCourse.title}</h1>
      
      <div className="flex justify-between items-center">
        <h2 className="text-xl">{t('courses.topics')}</h2>
        <button
          onClick={() => setShowAddTopic(true)}
          className="px-4 py-2 bg-green-600 text-white rounded"
        >
          {t('courses.addTopic')}
        </button>
      </div>

      {showAddTopic && (
        <TopicForm
          courseId={courseId}
          onClose={() => setShowAddTopic(false)}
        />
      )}

      <TreeView course={currentCourse} />
    </div>
  );
};
```

---

## 🛣️ Routing Structure

### React Router Setup
```typescript
// App.tsx
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { PrivateRoute } from './shared/components/PrivateRoute';
import { LoginPage } from './features/auth/AuthPage';
import { DashboardPage } from './features/dashboard/DashboardPage';
import { CoursesPage } from './features/courses/CoursesPage';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<LoginPage isRegister />} />
        
        <Route element={<PrivateRoute />}>
          <Route path="/dashboard" element={<DashboardPage />} />
          <Route path="/courses" element={<CoursesPage />} />
          <Route path="/courses/:id" element={<CoursesPage />} />
        </Route>

        <Route path="/" element={<Navigate to="/dashboard" />} />
      </Routes>
    </BrowserRouter>
  );
}
```

---

## 🔐 Protected Routes
```typescript
// shared/components/PrivateRoute.tsx
import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../../features/auth/hooks/useAuth';

export const PrivateRoute = () => {
  const { isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" />;
  }

  return <Outlet />;
};

// Admin Only Route
export const AdminRoute = () => {
  const { isAdmin } = useAuth();

  if (!isAdmin) {
    return <Navigate to="/dashboard" />;
  }

  return <Outlet />;
};
```

---

## 📡 API Client Configuration
```typescript
// shared/utils/apiClient.ts
import axios from 'axios';
import { useAuthStore } from '../../stores/authStore';

export const apiClient = axios.create({
  baseURL: process.env.REACT_APP_API_URL || 'http://localhost:5000',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor
apiClient.interceptors.request.use((config) => {
  const { accessToken } = useAuthStore.getState();
  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`;
  }
  return config;
});

// Response interceptor
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const { refreshToken, logout } = useAuthStore.getState();

    if (error.response?.status === 401 && refreshToken) {
      try {
        await refreshToken();
        return apiClient(error.config);
      } catch {
        logout();
      }
    }

    return Promise.reject(error);
  }
);
```

---

## 🎯 Performance Best Practices

```
✓ Use React.memo for expensive components
✓ Use useCallback for event handlers
✓ Use useMemo for expensive calculations
✓ Lazy load routes with React.lazy
✓ Optimize images
✓ Use debouncing for search inputs
✓ Avoid inline objects/functions in render
✓ Use virtualization for long lists
```

---

## 🔗 Related Documents

- **AGENTS.md** - Overall vision
- **API_DESIGN.md** - API endpoints
- **AUTH_IMPLEMENTATION.md** - Auth flow
- **I18N_STRATEGY.md** - Multilenguaje in frontend

---

*Frontend architecture should be simple, scalable, and predictable.*
