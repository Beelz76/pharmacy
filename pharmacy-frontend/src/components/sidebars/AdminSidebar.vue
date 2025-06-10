<template>
  <aside
    class="w-full md:w-64 flex-shrink-0 flex flex-col md:h-screen md:sticky md:top-0 bg-white border-r shadow-sm"
  >
    <div
      class="px-4 py-5 text-lg text-center font-semibold text-secondary-700 border-b"
    >
      Админ панель
    </div>
    <nav class="flex-1 space-y-1 px-4 py-4">
      <RouterLink to="/admin" :class="navLinkClass('/admin')">
        <i class="fas fa-user text-base w-5 text-center mr-3"></i>
        Профиль
      </RouterLink>

      <RouterLink to="/admin/users" :class="navLinkClass('/admin/users')">
        <i class="fas fa-users text-base w-5 text-center mr-3"></i>
        Пользователи
      </RouterLink>

      <RouterLink to="/admin/orders" :class="navLinkClass('/admin/orders')">
        <i class="fas fa-shopping-basket text-base w-5 text-center mr-3"></i>
        Заказы
      </RouterLink>

      <RouterLink
        to="/admin/deliveries"
        :class="navLinkClass('/admin/deliveries')"
      >
        <i class="fas fa-truck text-base w-5 text-center mr-3"></i>
        Доставки
      </RouterLink>

      <RouterLink
        to="/admin/pharmacies"
        :class="navLinkClass('/admin/pharmacies')"
      >
        <i class="fas fa-clinic-medical text-base w-5 text-center mr-3"></i>
        Аптеки
      </RouterLink>

      <RouterLink to="/admin/products" :class="navLinkClass('/admin/products')">
        <i class="fas fa-box text-base w-5 text-center mr-3"></i>
        Товары
      </RouterLink>

      <RouterLink to="/admin/payments" :class="navLinkClass('/admin/payments')">
        <i class="fas fa-credit-card text-base w-5 text-center mr-3"></i>
        Платежи
      </RouterLink>

      <button
        @click="showRefs = !showRefs"
        :class="
          navLinkClass('/admin/references') + ' w-full flex items-center gap-3'
        "
      >
        <span class="flex-1 text-left">Справочники</span>
        <i :class="'fas fa-chevron-' + (showRefs ? 'down' : 'right')"></i>
      </button>
      <div v-show="showRefs" class="ml-6 space-y-1">
        <RouterLink
          to="/admin/references/categories"
          :class="navLinkClass('/admin/references/categories')"
        >
          Категории
        </RouterLink>
        <RouterLink
          to="/admin/references/manufacturers"
          :class="navLinkClass('/admin/references/manufacturers')"
        >
          Производители
        </RouterLink>
        <RouterLink
          to="/admin/references/order-statuses"
          :class="navLinkClass('/admin/references/order-statuses')"
        >
          Статусы заказа
        </RouterLink>

        <RouterLink
          to="/admin/references/payment-methods"
          :class="navLinkClass('/admin/references/payment-methods')"
        >
          Методы оплаты
        </RouterLink>
        <RouterLink
          to="/admin/references/payment-statuses"
          :class="navLinkClass('/admin/references/payment-statuses')"
        >
          Статусы оплаты
        </RouterLink>
      </div>
    </nav>
    <div class="border-t px-4 py-4">
      <button
        @click="logout"
        class="w-full flex items-center gap-3 px-3 py-2 rounded-md text-secondary-700 hover:bg-secondary-100 transition"
      >
        <i class="fas fa-sign-out-alt text-base w-5 text-center"></i>
        Выйти
      </button>
    </div>
  </aside>
</template>

<script setup>
import { useRoute, useRouter } from "vue-router";
import { useAuthStore } from "/src/stores/AuthStore";
import { ref } from "vue";

const route = useRoute();
const router = useRouter();
const auth = useAuthStore();

const showRefs = ref(route.path.startsWith("/admin/references"));

const navLinkClass = (path) => {
  const isActive =
    path === "/admin" ? route.path === "/admin" : route.path.startsWith(path);
  return (
    "w-full flex items-center px-3 py-2 rounded-md transition text-sm font-medium " +
    (isActive
      ? "bg-secondary-100 text-secondary-700 font-semibold"
      : "text-gray-700 hover:text-secondary-700 hover:bg-secondary-50")
  );
};

const logout = async () => {
  await auth.logout();
  router.push("/");
};
</script>
