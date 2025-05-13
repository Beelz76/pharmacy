import { createRouter, createWebHistory } from "vue-router";
import HomePage from "../pages/HomePage.vue";
import ProductsPage from "../pages/ProductsPage.vue";
import AccountLayout from "../layouts/AccountLayout.vue";
import AccountPage from "../pages/account/AccountPage.vue";
import CartPage from '../pages/CartPage.vue'
import CheckoutPage from "../pages/CheckoutPage.vue"
import OrderSummaryPage from "../pages/OrderSummaryPage.vue"
import OrderHistoryPage from "../pages/account/OrderHistoryPage.vue";
import FavoritesPage from "../pages/account/FavoritesPage.vue";
import { useAuthStore } from '../stores/AuthStore'
import { useOrderStore } from '../stores/OrderStore'

const routes = [
  { path: "/", name: "Home", component: HomePage },
  { path: "/products", name: "Products", component: ProductsPage },
  { path: "/cart", name: "Cart", component: CartPage, meta: { requiresAuth: true } },
  { path: "/cart/checkout", name: "Checkout", component: CheckoutPage, meta: { requiresAuth: true }  },
  { path: "/cart/summary", name: "OrderSummary", component: OrderSummaryPage, meta: { requiresAuth: true } },
  {
    path: '/account',
    component: AccountLayout,
    meta: { requiresAuth: true },
    children: [
      { path: '', component: AccountPage },
      { path: 'orders', component: OrderHistoryPage },
      { path: 'favorites', component: FavoritesPage }
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

  const fromOrderPages = ['/cart/checkout', '/cart/summary']
  const toOrderPages = ['/cart/checkout', '/cart/summary']

  if (fromOrderPages.includes(from.path) && !toOrderPages.includes(to.path)) {
    const order = useOrderStore()
    order.resetOrder()
  }

  next()
})

export default router;
