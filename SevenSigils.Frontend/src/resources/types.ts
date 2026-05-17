export type ResourceLink = {
  href: string
  label: string
}

export type AppResource = {
  hero: {
    title: string
    subtitle: string
    primaryCta: string
    secondaryCta: string
    status: string
  }
  sections: {
    api: {
      title: string
      description: string
      links: ResourceLink[]
    }
    roadmap: {
      title: string
      description: string
      items: string[]
    }
  }
}