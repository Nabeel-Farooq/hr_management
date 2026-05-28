import { route } from 'quasar/wrappers'
import {
  createMemoryHistory,
  createRouter,
  createWebHashHistory,
  createWebHistory,
  type RouteLocationNormalized
} from 'vue-router'

import routes from './routes'

const isServer = process.env.SERVER
const isHistoryMode = process.env.VUE_ROUTER_MODE === 'history'
const routerBase = process.env.MODE === 'ssr'
  ? undefined
  : process.env.VUE_ROUTER_BASE

const historyFactory = isServer
  ? createMemoryHistory
  : isHistoryMode
    ? createWebHistory
    : createWebHashHistory

function requiresAuth(to: RouteLocationNormalized): boolean {
  return to.matched.some(route => route.meta.requireLogin)
}

export default route(({ store }) => {
  const router = createRouter({
    history: historyFactory(routerBase),

    routes,

    scrollBehavior() {
      return {
        left: 0,
        top: 0
      }
    }
  })

  router.beforeEach((to) => {
    const isAuthenticated = store.state.auth?.isAuthenticated

    if (requiresAuth(to) && !isAuthenticated) {
      return {
        name: 'Login',
        query: {
          to: to.fullPath
        }
      }
    }

    return true
  })

  return router
})
