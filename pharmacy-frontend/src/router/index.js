import { createRouter, createWebHistory } from "vue-router";
import HomePage from "../pages/HomePage.vue";
import AboutPage from "../pages/AboutPage.vue";
import AccountLayout from "../layouts/AccountLayout.vue";
import AccountPage from "../pages/account/AccountPage.vue";
import CartPage from "../pages/CartPage.vue";
import OrderCheckoutPage from "../pages/order/OrderCheckoutPage.vue";
import OrderSummaryPage from "../pages/order/OrderSummaryPage.vue";
import OrderPaymentPage from "../pages/order/OrderPaymentPage.vue";
import OrderHistoryPage from "../pages/account/OrderHistoryPage.vue";
import OrderDetailsPage from "../pages/account/OrderDetailsPage.vue";
import FavoritesPage from "../pages/account/FavoritesPage.vue";
import ProductsMainPage from "../pages/product/ProductsMainPage.vue";
import ProductsLayout from "../layouts/ProductsLayout.vue";
import ProductDetailsPage from "../pages/product/ProductDetailsPage.vue";
import { useAuthStore } from "../stores/AuthStore";
import { useOrderStore } from "../stores/OrderStore";
import { useCartStore } from "../stores/CartStore";
import { ElMessage } from "element-plus";

const routes = [
  { path: "/", name: "Home", component: HomePage },
  { path: "/about", name: "About", component: AboutPage },
  {
    path: "/products/catalog",
    component: ProductsLayout,
    children: [
      {
        path: "",
        name: "Products",
        component: ProductsMainPage,
      },
      {
        path: ":slug",
        name: "ProductsByCategory",
        component: ProductsMainPage,
      },
    ],
  },
  {
    path: "/products/:id-:slug",
    name: "ProductDetails",
    component: ProductDetailsPage,
    props: (route) => ({ id: Number(route.params.id) }),
  },
  {
    path: "/cart",
    name: "Cart",
    component: CartPage,
    meta: { requiresAuth: true },
  },
  {
    path: "/order/checkout",
    name: "OrderCheckout",
    component: OrderCheckoutPage,
    meta: { requiresAuth: true },
  },
  {
    path: "/order/summary",
    name: "OrderSummary",
    component: OrderSummaryPage,
    meta: { requiresAuth: true },
  },
  {
    path: "/order/payment",
    name: "OrderPayment",
    component: OrderPaymentPage,
    meta: { requiresAuth: true },
  },
  {
    path: "/account",
    component: AccountLayout,
    meta: { requiresAuth: true },
    children: [
      { path: "", component: AccountPage },
      { path: "orders", name: "OrderHistory", component: OrderHistoryPage },
      { path: "orders/:id", name: "OrderDetails", component: OrderDetailsPage },
      {
        path: "orders/:id/payment",
        name: "OrderPaymentAccount",
        component: OrderPaymentPage,
      },
      { path: "favorites", component: FavoritesPage },
    ],
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach((to, from, next) => {
  const auth = useAuthStore();

  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    auth.setReturnUrl(to.fullPath);
    window.dispatchEvent(new Event("unauthorized"));
    return next(false);
  }

  const fromOrderPages = ["/order/checkout", "/order/summary"];
  const toOrderPages = ["/order/checkout", "/order/summary", "/order/payment"];

  const isFromAccountPaymentPage =
    from.path.startsWith("/account/order/") && from.path.endsWith("/payment");
  const isLeavingCheckoutFlow =
    fromOrderPages.includes(from.path) || isFromAccountPaymentPage;
  const isToCheckoutFlow = toOrderPages.includes(to.path);

  if (isLeavingCheckoutFlow && !isToCheckoutFlow) {
    const order = useOrderStore();
    order.resetCheckout();
  }

  const cart = useCartStore();
  const order = useOrderStore();

  const cartIsEmpty = cart.items.length === 0;
  const hasUnavailableItems = cart.items.some((item) => !item.isAvailable);

  // Защита для страницы Checkout
  if (to.name === "Checkout") {
    if (cartIsEmpty) {
      ElMessage.warning("Корзина пуста");
      return next({ name: "Cart" });
    }
    if (hasUnavailableItems) {
      ElMessage.warning("Некоторые товары в корзине недоступны");
      return next({ name: "Cart" });
    }
  }

  // Защита для страницы OrderSummary
  if (to.name === "OrderSummary") {
    if (cartIsEmpty) {
      ElMessage.warning("Корзина пуста");
      return next({ name: "Cart" });
    }
    if (hasUnavailableItems) {
      ElMessage.warning("Некоторые товары в корзине недоступны");
      return next({ name: "Cart" });
    }
    const invalid =
      (!order.isDelivery && !order.selectedPharmacy) ||
      (order.isDelivery && !order.selectedAddressId) ||
      !order.paymentMethod;
    if (invalid) {
      ElMessage.warning(
        order.isDelivery
          ? "Выберите адрес и способ оплаты"
          : "Выберите аптеку и способ оплаты"
      );
      return next({ name: "Checkout" });
    }
  }

  next();
});

export default router;
