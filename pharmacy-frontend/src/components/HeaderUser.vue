<template>
  <header class="sticky top-0 z-50 bg-slate-50/80 backdrop-blur-md border-b border-gray-200">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex items-center h-16 gap-6">
        <!-- Логотип -->
        <div class="flex items-center flex-shrink-0 space-x-6">
          <router-link to="/" class="flex items-center">
            <i class="fas fa-pills text-primary-600 text-2xl mr-2"></i>
            <span class="text-2xl font-bold text-gray-900">MediCare</span>
          </router-link>

          <!-- Навигация -->
          <nav class="hidden md:flex space-x-8 relative">
            <router-link
              to="/"
              class="text-base font-medium"
              :class="$route.path === '/' ? 'text-primary-600 border-b-2 border-primary-500' : 'text-gray-500 hover:text-gray-700'"
            >
              Главная
            </router-link>

            <!-- Каталог -->
            <el-dropdown trigger="hover" @visible-change="val => { if (!val) activeCategory = null }">
              <span
                :class="[
                  'text-base font-medium cursor-pointer flex items-center focus:outline-none',
                  $route.path.startsWith('/products')
                    ? 'text-primary-600 border-b-2 border-primary-500'
                    : 'text-gray-500 hover:text-gray-700'
                ]"
                @click="goToProducts"
              >
                Каталог <i class="fas fa-chevron-down text-xs ml-1"></i>
              </span>
              <template #dropdown>
                <div class="inline-flex rounded-lg shadow-lg z-50 overflow-hidden">
                  <ul class="w-64 border-r divide-y">
                    <li
                      v-for="cat in categories"
                      :key="cat.id"
                      @mouseenter="activeCategory = cat"
                      class="px-4 py-3 text-sm hover:bg-primary-50 cursor-pointer font-medium text-gray-800 whitespace-nowrap"
                    >
                      {{ cat.name }}
                    </li>
                  </ul>
                  <ul
                    v-if="activeCategory?.subcategories?.length"
                    class="divide-y"
                    :style="{ minWidth: '20rem' }"
                  >
                    <li
                      v-for="sub in activeCategory.subcategories"
                      :key="sub.id"
                      @click="goToCategory(sub)"
                      class="px-4 py-3 text-sm hover:bg-primary-50 cursor-pointer text-gray-800 whitespace-nowrap"
                    >
                      {{ sub.name }}
                    </li>
                  </ul>
                </div>
              </template>
            </el-dropdown>

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
            class="w-full !h-10 !text-sm !rounded-md"
          >
            <template #prefix>
              <i class="fas fa-search text-gray-400 ml-2"></i>
            </template>
          </el-input>
          <ul
            v-if="showDropdown && searchResults.length > 0"
            class="absolute z-10 bg-slate-50 border rounded-md mt-1 w-full shadow-md max-h-60 overflow-auto"
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
            <div class="relative">
              <router-link to="/account/favorites" class="text-xl text-gray-600 hover:text-primary-600">
                <i class="fas fa-heart"></i>
              </router-link>
              <span
                v-if="favoritesCount > 0"
                class="absolute -top-1 -right-2 bg-red-500 text-white text-xs font-semibold rounded-full w-5 h-5 flex items-center justify-center"
              >
                {{ favoritesCount }}
              </span>
            </div>

            <div class="relative">
              <router-link
                to="/cart"
                class="text-xl"
                :class="$route.path === '/cart' ? 'text-primary-600' : 'text-gray-600 hover:text-primary-600'"
              >
                <i class="fas fa-shopping-cart"></i>
              </router-link>
              <span
                v-if="cartCount > 0"
                class="absolute -top-1 -right-2 bg-red-500 text-white text-xs font-semibold rounded-full w-5 h-5 flex items-center justify-center"
              >
                {{ cartCount }}
              </span>
            </div>

            <!-- Профиль с dropdown -->
            <el-dropdown>
              <template #default>
                <button
                  @click="goToAccount"
                  class="bg-primary-600 hover:bg-primary-700 text-white text-sm font-medium px-4 py-2 rounded flex items-center gap-2 transition focus:outline-none"
                >
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
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { storeToRefs } from 'pinia'
import { useAuthStore } from '../store/AuthStore'
import { useFavoritesStore } from '../store/FavoritesStore'
import { useCartStore } from '../store/CartStore'
import { useCategoryStore } from '../store/CategoryStore'
import api from '../utils/axios'

const auth = useAuthStore()
const favorites = useFavoritesStore()
const cart = useCartStore()
const categoryStore = useCategoryStore()

const { isAuthenticated } = storeToRefs(auth)
const { favoritesCount } = storeToRefs(favorites)
const { cartCount } = storeToRefs(cart)
const { categories } = storeToRefs(categoryStore)

const showCatalog = ref(false)
const activeCategory = ref(null)

const $route = useRoute()
const router = useRouter()
const searchQuery = ref('')
const searchResults = ref([])
const showDropdown = ref(false)
const dropdownHovered = ref(false)
const debounceTimer = ref(null)
const showAuthModal = ref(false)

onMounted(() => {
  if (isAuthenticated.value) {
    favorites.fetchCount()
    cart.fetchCartCount()
  }
  categoryStore.fetchCategories()
})

function openAuthModal() {
  auth.setReturnUrl(router.currentRoute.value.fullPath)
  window.dispatchEvent(new Event('unauthorized'))
}

function goToProducts() {
  router.push('/products')
}

function goToAccount() {
  router.push('/account')
}

function goToCategory(category) {
  showCatalog.value = false
  router.push({ path: '/products', query: { categoryId: category.id } })
}

const fetchSearchSuggestions = async (query) => {
  const trimmed = query.trim()
  if (!trimmed || trimmed.length < 2) {
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
  requestAnimationFrame(() => {
    if (!dropdownHovered.value) {
      showDropdown.value = false
    }
  })
}

function goToSearch(value) {
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
