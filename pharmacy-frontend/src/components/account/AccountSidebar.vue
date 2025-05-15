<template>
  <aside class="w-full md:w-64 py-4 bg-white border-r shadow-sm rounded-r-xl">
    <nav class="space-y-2">
      <RouterLink
        to="/account"
        :class="navLinkClass('/account')"
      >
        <i class="fas fa-user mr-3"></i>
        Личный кабинет
      </RouterLink>

      <RouterLink
        to="/account/orders"
        :class="navLinkClass('/account/orders')"
      >
        <i class="fas fa-box mr-3"></i>
        История заказов
      </RouterLink>

      <RouterLink
        to="/account/favorites"
        :class="navLinkClass('/account/favorites')"
      >
        <i class="fas fa-heart mr-3"></i>
        Избранное
      </RouterLink>

      <RouterLink
        to="/cart"
        :class="navLinkClass('/cart')"
      >
        <i class="fas fa-shopping-cart mr-3"></i>
        Корзина
      </RouterLink>

      <button
        @click="logout"
        class="w-full flex items-center gap-3 text-red-600 hover:text-red-700 font-semibold px-4 py-2 rounded-xl transition"
      >
        <i class="fas fa-sign-out-alt"></i> Выйти
      </button>
    </nav>
  </aside>
</template>


<script setup>
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '/src/stores/AuthStore'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()

const navLinkClass = (path) => {
  const isActive =
    path === '/account'
      ? route.path === '/account'
      : route.path.startsWith(path)

  return (
    'w-full flex items-center px-4 py-2 rounded-xl transition font-semibold text-base ' +
    (isActive
      ? 'bg-primary-50 text-primary-700'
      : 'text-gray-600 hover:bg-gray-100 hover:text-primary-600')
  )
}


const logout = async () => {
  await auth.logout()
  router.push('/')
}
</script>
