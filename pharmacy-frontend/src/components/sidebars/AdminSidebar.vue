<template>
  <aside class="w-64 h-screen py-4 px-4 bg-white border-r shadow-sm">
    <nav class="space-y-2">
      <RouterLink to="/admin" :class="navLinkClass('/admin')">
        <i class="fas fa-user text-base w-5 text-center mr-3"></i>
        Профиль
      </RouterLink>
    </nav>
    <div class="border-t mt-6 pt-4">
      <button
        @click="logout"
        class="w-full flex items-center gap-3 px-4 py-2 rounded-lg text-red-600 hover:text-red-700 hover:bg-red-50 font-medium transition"
      >
        <i class="fas fa-sign-out-alt text-base w-5 text-center"></i>
        Выйти
      </button>
    </div>
  </aside>
</template>

<script setup>
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '/src/stores/AuthStore'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()

const navLinkClass = (path) => {
  const isActive = path === '/admin' ? route.path === '/admin' : route.path.startsWith(path)
  return (
    'w-full flex items-center px-4 py-2 rounded-lg transition text-sm font-medium ' +
    (isActive
      ? 'bg-primary-100 text-primary-700 font-semibold'
      : 'text-gray-700 hover:text-primary-600 hover:bg-gray-50')
  )
}

const logout = async () => {
  await auth.logout()
  router.push('/')
}
</script>