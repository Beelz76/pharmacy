<template>
  <aside
    class="w-full md:w-64 flex-shrink-0 flex flex-col md:h-screen md:sticky md:top-0 bg-white border-r shadow-sm"
  >
    <div
      class="px-4 py-5 text-lg text-center font-semibold text-secondary-700 border-b"
    >
      {{ pharmacyName }}
    </div>
    <nav class="flex-1 space-y-1 px-4 py-4">
      <RouterLink
        to="/employee/profile"
        :class="navLinkClass('/employee/profile')"
      >
        <i class="fas fa-user text-base w-5 text-center mr-3"></i>
        Профиль
      </RouterLink>

      <RouterLink
        to="/employee/orders"
        :class="navLinkClass('/employee/orders')"
      >
        <i class="fas fa-shopping-basket text-base w-5 text-center mr-3"></i>
        Заказы
      </RouterLink>

      <RouterLink
        to="/employee/warehouse"
        :class="navLinkClass('/employee/warehouse')"
      >
        <i class="fas fa-boxes text-base w-5 text-center mr-3"></i>
        Склад
      </RouterLink>

      <RouterLink
        to="/employee/pharmacy"
        :class="navLinkClass('/employee/pharmacy')"
      >
        <i class="fas fa-clinic-medical text-base w-5 text-center mr-3"></i>
        Аптека
      </RouterLink>
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
import { useAccountStore } from "/src/stores/AccountStore";
import { ref, computed } from "vue";

const route = useRoute();
const router = useRouter();
const auth = useAuthStore();

const accountStore = useAccountStore();
accountStore.fetchProfile();
const pharmacyName = computed(
  () => accountStore.account?.pharmacy?.name || "Аптека"
);

const navLinkClass = (path) => {
  const isActive = route.path.startsWith(path);
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
