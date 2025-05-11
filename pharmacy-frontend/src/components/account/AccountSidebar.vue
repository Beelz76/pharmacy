<template>
  <aside class="w-full md:w-64 p-4 border-r bg-slate-50">
    <nav class="space-y-4">
      <RouterLink
        to="/account"
        class="block text-base font-medium"
        :class="linkClass('/account')"
      >
        <i class="fas fa-user mr-2"></i> Личный кабинет
      </RouterLink>
      <RouterLink
        to="/account/orders"
        class="block text-base font-medium"
        :class="linkClass('/account/orders')"
      >
        <i class="fas fa-box mr-2"></i> История заказов
      </RouterLink>
      <RouterLink
        to="/account/favorites"
        class="block text-base font-medium"
        :class="linkClass('/account/favorites')"
      >
        <i class="fas fa-heart mr-2"></i> Избранное
      </RouterLink>
    </nav>
  </aside>
</template>

<script setup>
import { useRoute } from 'vue-router'
const route = useRoute()

const linkClass = (path) =>
  route.path === path ? 'text-primary-600' : 'text-gray-600 hover:text-primary-600'
</script>