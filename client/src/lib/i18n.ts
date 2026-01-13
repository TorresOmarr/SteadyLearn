export type Locale = 'es' | 'en'

export const copy: Record<Locale, {
  heroTitle: string
  heroSubtitle: string
  startNow: string
  loginCta: string
  registerCta: string
  bulletTitle: string
  bullets: string[]
  footer: string
  footerRights: string
  login: {
    title: string
    submit: string
    email: string
    password: string
    remember: string
    error: string
    loading: string
  }
  register: {
    title: string
    submit: string
    name: string
    email: string
    password: string
    confirm: string
    error: string
    loading: string
  }
}> = {
  es: {
    heroTitle: 'Aprende con ritmo estable y resultados medibles',
    heroSubtitle: 'Una plataforma moderna para formación modular, seguimiento y analítica de progreso.',
    startNow: 'Empieza ahora',
    loginCta: 'Iniciar sesión',
    registerCta: 'Crear cuenta',
    bulletTitle: 'Diseñada para escalar tu aprendizaje',
    bullets: [
      'Rutas curadas y sesiones enfocadas en hábitos',
      'Seguimiento claro con métricas accionables',
      'Onboarding seguro con autenticación moderna',
    ],
    footer: 'SteadyLearn',
    footerRights: 'Construyendo hábitos de aprendizaje sostenibles',
    login: {
      title: 'Accede a tu cuenta',
      submit: 'Entrar',
      email: 'Correo electrónico',
      password: 'Contraseña',
      remember: 'Recordarme',
      error: 'No pudimos validar tus credenciales.',
      loading: 'Ingresando...',
    },
    register: {
      title: 'Crea tu cuenta',
      submit: 'Registrarme',
      name: 'Nombre completo',
      email: 'Correo electrónico',
      password: 'Contraseña',
      confirm: 'Confirmar contraseña',
      error: 'No pudimos crear tu cuenta.',
      loading: 'Creando cuenta...',
    },
  },
  en: {
    heroTitle: 'Learn steadily with measurable outcomes',
    heroSubtitle: 'A modern platform for modular learning, tracking, and progress insights.',
    startNow: 'Start now',
    loginCta: 'Log in',
    registerCta: 'Sign up',
    bulletTitle: 'Built to scale your learning',
    bullets: [
      'Curated paths and habit-focused sessions',
      'Clear tracking with actionable metrics',
      'Secure onboarding with modern auth',
    ],
    footer: 'SteadyLearn',
    footerRights: 'Building sustainable learning habits',
    login: {
      title: 'Access your account',
      submit: 'Log in',
      email: 'Email',
      password: 'Password',
      remember: 'Remember me',
      error: 'We could not validate your credentials.',
      loading: 'Signing in...',
    },
    register: {
      title: 'Create your account',
      submit: 'Sign up',
      name: 'Full name',
      email: 'Email',
      password: 'Password',
      confirm: 'Confirm password',
      error: 'We could not create your account.',
      loading: 'Creating account...',
    },
  },
}

export function getCopy(locale: Locale) {
  return copy[locale]
}
