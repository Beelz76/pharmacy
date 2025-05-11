<template>
  <aside class="w-full md:w-64 py-4 border-r bg-white">
    <nav class="space-y-4">
      <RouterLink
        to="/account"
        class="block font-medium text-base sm:text-lg md:text-lg"
        :class="linkClass('/account')"
      >
        <i class="fas fa-user mr-2"></i> Личный кабинет
      </RouterLink>

      <RouterLink
        to="/account/orders"
        class="block font-medium text-base sm:text-lg md:text-lg"
        :class="linkClass('/account/orders')"
      >
        <i class="fas fa-box mr-2"></i> История заказов
      </RouterLink>

      <RouterLink
        to="/account/favorites"
        class="block font-medium text-base sm:text-lg md:text-lg"
        :class="linkClass('/account/favorites')"
      >
        <i class="fas fa-heart mr-2"></i> Избранное
      </RouterLink>

      <RouterLink
        to="/cart"
        class="block font-medium text-base sm:text-lg md:text-lg"
        :class="linkClass('/cart')"
      >
        <i class="fas fa-shopping-cart mr-2"></i> Корзина
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
