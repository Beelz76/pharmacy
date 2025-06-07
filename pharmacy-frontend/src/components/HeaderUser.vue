<template>
  <header class="sticky top-0 z-50 bg-white/90 backdrop-blur-md border-b border-gray-200">
    <div class="max-w-7xl mx-auto px-2">
      <div class="flex items-center h-16 justify-between gap-6">
        <!-- Логотип и навигация -->
        <div class="flex items-center gap-6">
          <router-link to="/" class="flex items-center">
            <i class="fas fa-pills text-primary-600 text-2xl mr-2"></i>
            <span class="text-2xl font-bold text-gray-900">MediCare</span>
          </router-link>

          <nav class="hidden md:flex space-x-6">
            <router-link
              to="/"
              class="text-base font-medium"
              :class="$route.path === '/' ? 'text-primary-600 border-b-2 border-primary-500' : 'text-gray-600 hover:text-primary-600'"
            >
              Главная
            </router-link>
            <router-link
              to="/about"
              class="text-base font-medium"
              :class="$route.path === '/about' ? 'text-primary-600 border-b-2 border-primary-500' : 'text-gray-600 hover:text-primary-600'"
            >
              О нас
            </router-link>
          </nav>
        </div>

        <!-- Поиск и категории -->
        <div class="flex-1">
          <SearchBarWithCategory />
        </div>

        <!-- Иконки -->
        <div class="flex items-center space-x-6">
          <!-- Избранное -->
          <div class="relative flex flex-col items-center">
            <router-link to="/account/favorites" class="text-2xl text-gray-600 hover:text-primary-600">
              <i class="fas fa-heart"></i>
            </router-link>
            <span
              v-if="favoritesCount > 0"
              class="absolute top-6 text-[10px] text-gray-700 font-semibold"
            >
              {{ favoritesCount }}
            </span>
          </div>

          <!-- Корзина -->
          <div class="relative flex flex-col items-center">
            <router-link
              to="/cart"
              class="text-2xl"
              :class="$route.path === '/cart' ? 'text-primary-600' : 'text-gray-600 hover:text-primary-600'"
            >
              <i class="fas fa-shopping-cart"></i>
            </router-link>
            <span
              v-if="cartCount > 0"
              class="absolute top-6 text-[10px] text-gray-700 font-semibold"
            >
              {{ cartCount }}
            </span>
          </div>
          <template v-if="isAuthenticated">
            <el-dropdown>
              <template #default>
                <button class="bg-primary-600 hover:bg-primary-700 text-white px-4 py-2 rounded text-sm flex items-center gap-2">
                  <i class="fas fa-user"></i>
                  Профиль
                  <i class="fas fa-chevron-down text-xs"></i>
                </button>
              </template>
              <template #dropdown>
                <el-dropdown-menu>
                  <el-dropdown-item>
                    <router-link to="/account">Личный кабинет</router-link>
                  </el-dropdown-item>
                  <el-dropdown-item>
                    <router-link to="/account/orders">История заказов</router-link>
                  </el-dropdown-item>
                  <el-dropdown-item @click="handleLogout">Выйти</el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </template>

          <template v-else>
            <el-button
              type="primary"
              class="!bg-primary-600 !h-10 hover:!bg-primary-700"
              @click="openAuthModal"
            >
              <i class="fas fa-user mr-2"></i> Войти
            </el-button>
          </template>
        </div>
      </div>
    </div>
  </header>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { storeToRefs } from 'pinia'
import { useAuthStore } from '../stores/AuthStore'
import { useFavoritesStore } from '../stores/FavoritesStore'
import { useCartStore } from '../stores/CartStore'
import { useCategoryStore } from '../stores/CategoryStore'
import api from '../utils/axios'
import SearchBarWithCategory from '../components/SearchBarWithCategory.vue'

const auth = useAuthStore()
const favorites = useFavoritesStore()
const cart = useCartStore()
const categoryStore = useCategoryStore()

const { isAuthenticated } = storeToRefs(auth)
const { favoritesCount } = storeToRefs(favorites)
const { cartCount } = storeToRefs(cart)

const activeCategory = ref(null)
const categoryDropdownVisible = ref(false)

const $route = useRoute()
const router = useRouter()

watch(
  () => $route.path,
  (path) => {
    if (!path.includes('/products/catalog')) {
      categoryStore.resetCategory()
    }
  }
)

watch(
  () => auth.role,
  (newRole) => {
    if (isAuthenticated.value && newRole === 'User') {
      favorites.fetchCount()
      cart.fetchCartCount()
    }
  },
  { immediate: true }
)

onMounted(() => {
  categoryStore.fetchCategories()
})

function openAuthModal() {
  auth.setReturnUrl(router.currentRoute.value.fullPath)
  window.dispatchEvent(new Event('unauthorized'))
}

function handleLogout() {
  auth.logout()
  router.push('/')
}
</script>

<style scoped>
ul::-webkit-scrollbar {
  width: 4px;
}
ul::-webkit-scrollbar-thumb {
  background-color: #ccc;
  border-radius: 2px;
}
</style>
