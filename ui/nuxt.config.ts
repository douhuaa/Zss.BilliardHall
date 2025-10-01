// Minimal Nuxt config for ABP Vue integration
export default defineNuxtConfig({
  devtools: { enabled: true },
  typescript: { strict: true },
  runtimeConfig: {
    public: {
      authorityUrl: process.env.NUXT_AUTHORITY_URL,
      clientId: process.env.NUXT_CLIENT_ID,
      clientSecret: process.env.NUXT_CLIENT_SECRET,
      scope: process.env.NUXT_SCOPE,
      apiEndpoint: process.env.NUXT_ABP_API_ENDPOINT,
      origin: process.env.NUXT_ORIGIN
    },
    sessionSecret: process.env.NUXT_SESSION_SECRET
  }
});
