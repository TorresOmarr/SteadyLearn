import { type Locale } from '../lib/i18n'
import { Button } from './ui/button'

type Props = {
  locale: Locale
  onToggle: () => void
}

export function LanguageSwitcher({ locale, onToggle }: Props) {
  return (
    <Button variant="ghost" size="sm" onClick={onToggle} className="text-sm font-semibold">
      {locale === 'es' ? 'ES' : 'EN'}
    </Button>
  )
}
