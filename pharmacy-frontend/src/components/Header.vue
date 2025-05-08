<template>
  <header class="sticky top-0 z-50 bg-white/80 backdrop-blur-md border-b border-gray-200">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex items-center h-16 gap-6">
        <!-- Логотип и навигация -->
        <div class="flex items-center flex-shrink-0 space-x-6">
          <router-link to="/" class="flex items-center">
            <i class="fas fa-pills text-primary-600 text-2xl mr-2"></i>
            <span class="text-2xl font-bold text-gray-900">MediCare</span>
          </router-link>

          <nav class="hidden md:flex space-x-8">
            <router-link
              to="/"
              class="text-base font-medium"
              :class="$route.path === '/' ? 'text-primary-600 border-b-2 border-primary-500' : 'text-gray-500 hover:text-gray-700'"
            >
              Главная
            </router-link>

            <router-link
              to="/products"
              class="text-base font-medium"
              :class="$route.path.startsWith('/products') ? 'text-primary-600 border-b-2 border-primary-500' : 'text-gray-500 hover:text-gray-700'"
            >
              Каталог
            </router-link>

            <router-link
              to="/about"
              class="text-base font-medium"
              :class="$route.path === '/about' ? 'text-primary-600 border-b-2 border-primary-500' : 'text-gray-500 hover:text-gray-700'"
            >
              О нас
            </router-link>
          </nav>
        </div>

        <!-- Поиск -->
        <div
          class="relative flex-grow hidden md:block"
          @mouseenter="dropdownHovered = true"
          @mouseleave="dropdownHovered = false"
        >
          <el-input
            v-model="searchQuery"
            placeholder="Поиск товаров..."
            clearable
            @input="onSearchInput"
            @keyup.enter="goToSearch"
            @focus="onFocus"
            @blur="onBlur"
            class="w-full"
          >
            <template #prefix>
              <i class="fas fa-search text-gray-400 ml-2"></i>
            </template>
          </el-input>
          <ul
            v-if="showDropdown && searchResults.length > 0"
            class="absolute z-10 bg-white border rounded-md mt-1 w-full shadow-md max-h-60 overflow-auto"
          >
            <li
              v-for="(result, index) in searchResults"
              :key="index"
              @click="goToSearch(result)"
              class="px-4 py-2 hover:bg-gray-100 cursor-pointer text-sm text-gray-700"
            >
              {{ result }}
            </li>
          </ul>
        </div>

        <!-- Иконки и профиль -->
        <div class="flex items-center flex-shrink-0 space-x-6">
          <template v-if="isAuthenticated">
            <router-link
              to="/favorites"
              class="text-xl"
              :class="$route.path === '/favorites' ? 'text-primary-600' : 'text-gray-600 hover:text-primary-600'"
            >
              <i class="fas fa-heart"></i>
            </router-link>

            <router-link
              to="/cart"
              class="text-xl"
              :class="$route.path === '/cart' ? 'text-primary-600' : 'text-gray-600 hover:text-primary-600'"
            >
              <i class="fas fa-shopping-cart"></i>
            </router-link>

            <el-dropdown>
              <template #default>
                <button
                  class="bg-primary-600 hover:bg-primary-700 text-white text-sm font-medium px-4 py-2 rounded flex items-center gap-2 transition focus:outline-none"
                >
                  <i class="fas fa-user"></i>
                  Профиль
                  <i class="fas fa-chevron-down text-xs"></i>
                </button>
              </template>
              <template #dropdown>
                <el-dropdown-menu>
                  <el-dropdown-item><router-link to="/account">Личный кабинет</router-link></el-dropdown-item>
                  <el-dropdown-item><router-link to="/orders">История заказов</router-link></el-dropdown-item>
                  <el-dropdown-item @click="handleLogout">Выйти</el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </template>

          <template v-else>
            <el-button
              type="primary"
              class="!bg-primary-600 hover:!bg-primary-700"
              @click="showAuthModal = true"
            >
              <i class="fas fa-user mr-2"></i> Войти
            </el-button>
          </template>
        </div>
      </div>
    </div>
  </header>
  <LoginRegisterModal v-model:visible="showAuthModal" />
</template>

<script setup>
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { storeToRefs } from 'pinia'
import LoginRegisterModal from './LoginRegisterModal.vue'
import { useAuthStore } from '../store/AuthStore'
import api from '../utils/axios'

const auth = useAuthStore()
const { isAuthenticated } = storeToRefs(auth)

const $route = useRoute()
const router = useRouter()
const searchQuery = ref('')
const searchResults = ref([])
const showDropdown = ref(false)
const debounceTimer = ref(null)
const showAuthModal = ref(false)

const fetchSearchSuggestions = async (query) => {
  if (!query.trim() || query.length < 2) {
    searchResults.value = []
    showDropdown.value = false
    return
  }
  try {
    const response = await api.get(`/products/search-suggestions?query=${encodeURIComponent(query)}`)
    searchResults.value = response.data
    showDropdown.value = true
  } catch (err) {
    console.error('Ошибка при получении подсказок:', err)
  }
}

const onSearchInput = () => {
  clearTimeout(debounceTimer.value)
  debounceTimer.value = setTimeout(() => {
    fetchSearchSuggestions(searchQuery.value)
  }, 500)
}

const onFocus = () => {
  if (searchResults.value.length > 0) {
    showDropdown.value = true
  }
}

const onBlur = () => {
  // Позволяет завершить клик по результату перед скрытием
  requestAnimationFrame(() => {
    showDropdown.value = false
  })
}

const goToSearch = (value) => {
  const query = typeof value === 'string' ? value : searchQuery.value
  if (query.trim()) {
    showDropdown.value = false
    router.push({ path: '/products', query: { search: query.trim() } })
  }
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
