import { createRouter, createWebHistory } from 'vue-router';
import { storage } from '../lib/index.js';

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/login', component: () => import('../views/Login.vue') },
    {
      path: '/', component: () => import('../views/Index.vue'), meta: { requiresAuth: true },
      children: [
        { path: 'home', component: () => import('../views/Home.vue') },
        { path: 'string-types', component: () => import('../views/StringTypes.vue') },
        { path: 'string-types/new', component: () => import('../views/StringTypeForm.vue') },
        { path: 'string-types/:id/edit', component: () => import('../views/StringTypeForm.vue'), props: true },
        { path: 'date-types', component: () => import('../views/DateTypes.vue') },
        { path: 'date-types/new', component: () => import('../views/DateTypeForm.vue') },
        { path: 'date-types/:id/edit', component: () => import('../views/DateTypeForm.vue'), props: true },
        { path: 'numeric-types', component: () => import('../views/NumericTypes.vue') },
        { path: 'numeric-types/new', component: () => import('../views/NumericTypeForm.vue') },
        { path: 'numeric-types/:id/edit', component: () => import('../views/NumericTypeForm.vue'), props: true },
        { path: '', redirect: 'string-types' }
      ]
    },
    { path: '/:pathMatch(.*)*', redirect: '/' } // Catch-all route (must be LAST)
  ],
})

router.beforeEach((to) => {
  if (to.meta.requiresAuth && !storage.token) {
    return '/login'
  }
})

export default router
