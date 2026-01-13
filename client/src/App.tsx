import { useState } from 'react'
import './App.css'
import { Layout } from './components/Layout'
import { Hero } from './components/Hero'
import { Footer } from './components/Footer'
import { LoginDialog } from './features/auth/LoginDialog'
import { RegisterDialog } from './features/auth/RegisterDialog'
import { Value } from './features/landing/Value'
import { type Locale } from './lib/i18n'

function App() {
  const [locale, setLocale] = useState<Locale>('es')
  const [loginOpen, setLoginOpen] = useState(false)
  const [registerOpen, setRegisterOpen] = useState(false)

  const toggleLocale = () => setLocale((prev) => (prev === 'es' ? 'en' : 'es'))
  const openLogin = () => setLoginOpen(true)
  const openRegister = () => setRegisterOpen(true)

  return (
    <Layout locale={locale} onToggleLocale={toggleLocale} onLogin={openLogin} onRegister={openRegister}>
      <div className="flex flex-col gap-16">
        <Hero locale={locale} onStart={openRegister} />
        <Value locale={locale} />
      </div>
      <LoginDialog locale={locale} open={loginOpen} onOpenChange={setLoginOpen} />
      <RegisterDialog locale={locale} open={registerOpen} onOpenChange={setRegisterOpen} />
      <Footer locale={locale} />
    </Layout>
  )
}

export default App
