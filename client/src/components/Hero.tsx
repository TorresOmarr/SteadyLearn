import { Badge } from './ui/badge'
import { Button } from './ui/button'
import { type Locale, getCopy } from '../lib/i18n'
import { cn } from '../lib/utils'
import { Zap, BarChart3, ShieldCheck } from 'lucide-react'

const icons = [Zap, BarChart3, ShieldCheck]

export type HeroProps = {
  locale: Locale
  onStart: () => void
}

export function Hero({ locale, onStart }: HeroProps) {
  const t = getCopy(locale)
  return (
    <section className="relative overflow-hidden rounded-3xl border border-border/60 bg-gradient-to-br from-indigo-600/20 via-slate-100 to-blue-500/15 px-6 py-20 sm:px-12 sm:py-28 shadow-lg">
      {/* decorative blobs */}
      <div className="pointer-events-none absolute inset-0">
        <div className="absolute -left-10 top-10 h-44 w-44 rounded-full bg-indigo-500/20 blur-3xl" />
        <div className="absolute -right-10 bottom-10 h-52 w-52 rounded-full bg-blue-500/20 blur-3xl" />
      </div>
      <div className="relative mx-auto flex max-w-4xl flex-col items-center gap-8 text-center">
        <Badge variant="secondary" className="shadow-sm">
          SteadyLearn
        </Badge>
        <div className="space-y-5">
          <h1 className="text-4xl font-extrabold leading-tight tracking-tight text-foreground sm:text-6xl">
            {t.heroTitle}
          </h1>
          <p className="mx-auto max-w-2xl text-lg text-muted-foreground sm:text-xl">
            {t.heroSubtitle}
          </p>
        </div>
        <Button size="lg" className="px-8 py-6 text-base shadow-md" onClick={onStart}>
          {t.startNow}
        </Button>
        <div className="mt-6 grid w-full max-w-3xl gap-5 sm:grid-cols-3">
          {t.bullets.map((item, idx) => {
            const Icon = icons[idx]
            return (
              <div
                key={item}
                className={cn(
                  'group rounded-2xl border border-border/70 bg-card/90 p-5 shadow-sm backdrop-blur transition hover:-translate-y-1 hover:shadow-lg',
                  idx === 0 ? 'sm:translate-y-2' : idx === 2 ? 'sm:-translate-y-2' : ''
                )}
              >
                <div className="mb-3 inline-flex h-10 w-10 items-center justify-center rounded-xl bg-indigo-500/15 text-indigo-600 transition group-hover:bg-indigo-500/25">
                  <Icon className="h-5 w-5" />
                </div>
                <p className="text-sm leading-relaxed text-foreground/90">{item}</p>
              </div>
            )
          })}
        </div>
      </div>
    </section>
  )
}
