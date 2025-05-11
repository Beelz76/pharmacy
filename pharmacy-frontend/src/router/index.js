import { createRouter, createWebHistory } from "vue-router";
import HomePage from "../pages/HomePage.vue";
import ProductsPage from "../pages/ProductsPage.vue";
import AccountLayout from "../layouts/AccountLayout.vue";
import AccountPage from "../pages/account/AccountPage.vue";
import OrderHistoryPage from "../pages/account/OrderHistoryPage.vue";
import FavoritesPage from "../pages/account/FavoritesPage.vue";
import { useAuthStore } from '../store/AuthStore'

const routes = [
  { path: "/", name: "Home", component: HomePage },
  { path: "/products", name: "Products", component: ProductsPage },
  {
    path: '/account',
    component: AccountLayout,
    meta: { requiresAuth: true },
    children: [
      { path: '', component: AccountPage },
      { path: 'orders', component: OrderHistoryPage },
      { path: 'favorites', component: FavoritesPage },
    ]
  }
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach((to, from, next) => {
  const auth = useAuthStore()

  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    auth.setReturnUrl(to.fullPath)
    window.dispatchEvent(new Event('unauthorized'))
    return next(false)
  }

  next()
})

export default router;
