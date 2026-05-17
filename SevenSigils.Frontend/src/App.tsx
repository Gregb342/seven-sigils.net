import reactLogo from './assets/react.svg'
import viteLogo from './assets/vite.svg'
import heroImg from './assets/hero.png'
import { getResource } from './resources'
import './App.css'

function App() {
  const copy = getResource('fr')

  return (
    <>
      <section id="center">
        <div className="hero">
          <img src={heroImg} className="base" width="170" height="179" alt="" />
          <img src={reactLogo} className="framework" alt="React logo" />
          <img src={viteLogo} className="vite" alt="Vite logo" />
        </div>
        <div>
          <p className="eyebrow">Front de démonstration</p>
          <h1>{copy.hero.title}</h1>
          <p>{copy.hero.subtitle}</p>
          <p className="status">{copy.hero.status}</p>
        </div>
        <div className="cta-group">
          <a className="counter" href="http://localhost:5000/swagger" target="_blank" rel="noreferrer">
            {copy.hero.primaryCta}
          </a>
          <a className="counter secondary" href="/blazons" target="_blank" rel="noreferrer">
            {copy.hero.secondaryCta}
          </a>
        </div>
      </section>

      <div className="ticks"></div>

      <section id="next-steps">
        <div id="docs">
          <svg className="icon" role="presentation" aria-hidden="true">
            <use href="/icons.svg#documentation-icon"></use>
          </svg>
          <h2>{copy.sections.api.title}</h2>
          <p>{copy.sections.api.description}</p>
          <ul>
            {copy.sections.api.links.map((link) => (
              <li key={link.href}>
                <a href={link.href} target="_blank" rel="noreferrer">
                  <img className="button-icon" src={link.href.includes('swagger') ? viteLogo : reactLogo} alt="" />
                  {link.label}
                </a>
              </li>
            ))}
          </ul>
        </div>
        <div id="social">
          <svg className="icon" role="presentation" aria-hidden="true">
            <use href="/icons.svg#social-icon"></use>
          </svg>
          <h2>{copy.sections.roadmap.title}</h2>
          <p>{copy.sections.roadmap.description}</p>
          <ul>
            {copy.sections.roadmap.items.map((item) => (
              <li key={item}>
                <span className="roadmap-item">{item}</span>
              </li>
            ))}
          </ul>
        </div>
      </section>

      <div className="ticks"></div>
      <section id="spacer"></section>
    </>
  )
}

export default App
