import { createRouter, createWebHistory, RouteRecordRaw } from "vue-router";
import Login from "../views/Login.vue";
import Dashboard from "../views/Dashboard.vue";
import { store } from "@/store";

const routes: Array<RouteRecordRaw> = [  
  {
    path: "/login",
    name: "Login",
    component: Login,
    meta: { guest: true }
  },
  {
    path: "/",
    name: "Dashboard",
    component: Dashboard,
    meta: { requiresAuth: true }
  },
  {
    path: "/reports",
    name: "Reports",
    // route level code-splitting
    // this generates a separate chunk (reports.[hash].js) for this route
    // which is lazy-loaded when the route is visited.
    component: () => import(/* webpackChunkName: "reports" */ "@/views/Reports.vue"),
    meta: { requiresAuth: true }
  },
  {
    path: "/users",
    name: "Users",
    component: () => import("@/views/Users.vue"),
    meta: { requiresAuth: true }
  },
];

// create router singleton
const router = createRouter({
  history: createWebHistory(process.env.BASE_URL),
  routes
});

router.beforeEach((to, from, next) => {
  // auth guard
  if (to.matched.some(record => record.meta.requiresAuth) && !store.getters.isAuthenticated) {
    next('/login');
    return;
  }  
  // login redirect
  if (to.matched.some(record => record.meta.guest) && store.getters.isAuthenticated) {
    next('/');
    return;
  }
  next();
})

export default router;
