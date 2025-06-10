import { createRouter, createWebHistory } from "vue-router";
import HomePage from "../pages/HomePage.vue";
import AboutPage from "../pages/AboutPage.vue";
import AccountLayout from "../layouts/AccountLayout.vue";
import AccountPage from "../pages/account/AccountPage.vue";
import CartPage from "../pages/CartPage.vue";
import OrderCheckoutPage from "../pages/order/OrderCheckoutPage.vue";
import OrderSummaryPage from "../pages/order/OrderSummaryPage.vue";
import OrderHistoryPage from "../pages/account/OrderHistoryPage.vue";
import SavedAddressesPage from "../pages/account/SavedAddressesPage.vue";
import OrderDetailsPage from "../pages/account/OrderDetailsPage.vue";
import FavoritesPage from "../pages/account/FavoritesPage.vue";
import ProductsMainPage from "../pages/product/ProductsMainPage.vue";
import ProductsLayout from "../layouts/ProductsLayout.vue";
import ProductDetailsPage from "../pages/product/ProductDetailsPage.vue";
import AdminLayout from "../layouts/AdminLayout.vue";
import AdminProfilePage from "../pages/admin/AdminProfilePage.vue";
import AdminUsersPage from "../pages/admin/user/AdminUsersPage.vue";
import AdminUserDetailsPage from "../pages/admin/user/AdminUserDetailsPage.vue";
import AdminCreateUserPage from "../pages/admin/user/AdminCreateUserPage.vue";
import AdminOrdersPage from "../pages/admin/order/AdminOrdersPage.vue";
import AdminOrderDetailsPage from "../pages/admin/order/AdminOrderDetailsPage.vue";
import AdminDeliveriesPage from "../pages/admin/delivery/AdminDeliveriesPage.vue";
import AdminDeliveryDetailsPage from "../pages/admin/delivery/AdminDeliveryDetailsPage.vue";
import AdminReferencesPage from "../pages/admin/reference/AdminReferencesPage.vue";
import AdminCategoriesPage from "../pages/admin/reference/categories/AdminCategoriesPage.vue";
import AdminManufacturersPage from "../pages/admin/reference/manufacturers/AdminManufacturersPage.vue";
import AdminOrderStatusesPage from "../pages/admin/reference/orderStatuses/AdminOrderStatusesPage.vue";
import AdminPaymentMethodsPage from "../pages/admin/reference/paymentMethods/AdminPaymentMethodsPage.vue";
import AdminPaymentStatusesPage from "../pages/admin/reference/paymentStatuses/AdminPaymentStatusesPage.vue";
import AdminPharmaciesPage from "../pages/admin/pharmacy/AdminPharmaciesPage.vue";
import AdminPharmacyDetailsPage from "../pages/admin/pharmacy/AdminPharmacyDetailsPage.vue";
import AdminPharmacyWarehousePage from "../pages/admin/pharmacy/AdminPharmacyWarehousePage.vue";
import AdminPaymentsPage from "../pages/admin/payment/AdminPaymentsPage.vue";
import AdminPaymentDetailsPage from "../pages/admin/payment/AdminPaymentDetailsPage.vue";
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
    meta: {},
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
    path: "/account",
    component: AccountLayout,
    meta: {},
    children: [
      { path: "", component: AccountPage },
      { path: "orders", name: "OrderHistory", component: OrderHistoryPage },
      { path: "orders/:id", name: "OrderDetails", component: OrderDetailsPage },
      { path: "favorites", component: FavoritesPage },
      {
        path: "addresses",
        name: "SavedAddresses",
        component: SavedAddressesPage,
      },
    ],
  },
  {
    path: "/admin",
    component: AdminLayout,
    meta: { requiresAuth: true, roles: ["Admin"], layout: "admin" },
    children: [
      { path: "", name: "AdminProfile", component: AdminProfilePage },
      { path: "users", name: "AdminUsers", component: AdminUsersPage },
      {
        path: "users/create",
        name: "AdminUserCreate",
        component: AdminCreateUserPage,
      },
      {
        path: "users/:id",
        name: "AdminUserDetails",
        component: AdminUserDetailsPage,
      },
      { path: "orders", name: "AdminOrders", component: AdminOrdersPage },
      {
        path: "orders/:id",
        name: "AdminOrderDetails",
        component: AdminOrderDetailsPage,
      },
      {
        path: "deliveries",
        name: "AdminDeliveries",
        component: AdminDeliveriesPage,
      },
      {
        path: "deliveries/:orderId",
        name: "AdminDeliveryDetails",
        component: AdminDeliveryDetailsPage,
      },
      {
        path: "payments",
        name: "AdminPayments",
        component: AdminPaymentsPage,
      },
      {
        path: "payments/:id",
        name: "AdminPaymentDetails",
        component: AdminPaymentDetailsPage,
      },
      {
        path: "pharmacies",
        name: "AdminPharmacies",
        component: AdminPharmaciesPage,
      },
      {
        path: "pharmacies/:id",
        name: "AdminPharmacyDetails",
        component: AdminPharmacyDetailsPage,
      },
      {
        path: "pharmacies/:id/warehouse",
        name: "AdminPharmacyWarehouse",
        component: AdminPharmacyWarehousePage,
      },
      {
        path: "references",
        component: AdminReferencesPage,
        children: [
          { path: "", redirect: { name: "AdminCategories" } },
          {
            path: "categories",
            name: "AdminCategories",
            component: AdminCategoriesPage,
          },
          {
            path: "manufacturers",
            name: "AdminManufacturers",
            component: AdminManufacturersPage,
          },
          {
            path: "order-statuses",
            name: "AdminOrderStatuses",
            component: AdminOrderStatusesPage,
          },
          {
            path: "payment-methods",
            name: "AdminPaymentMethods",
            component: AdminPaymentMethodsPage,
          },
          {
            path: "payment-statuses",
            name: "AdminPaymentStatuses",
            component: AdminPaymentStatusesPage,
          },
        ],
      },
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

  if (to.meta.roles && !to.meta.roles.includes(auth.role)) {
    return next("/");
  }

  const fromOrderPages = ["/order/checkout", "/order/summary"];
  const toOrderPages = ["/order/checkout", "/order/summary"];

  const isLeavingCheckoutFlow = fromOrderPages.includes(from.path);
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
  if (to.name === "OrderCheckout") {
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
      return next({ name: "OrderCheckout" });
    }
  }

  next();
});

export default router;
